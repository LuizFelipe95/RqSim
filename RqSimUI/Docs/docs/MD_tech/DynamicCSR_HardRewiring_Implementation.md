# Dynamic CSR (Roadmap) / (Hard Rewiring) & UI Implementation

## Overview
This document details the implementation of the **Dynamic Hard Rewiring** topology mode for the CSR (Compressed Sparse Row) GPU engine. This feature allows the graph topology to evolve dynamically on the GPU, removing edges that fall below a certain weight threshold and potentially adding new edges (in future phases).

## UI Implementation
A new UI panel has been added to the **UniPipeline** tab to configure Dynamic Topology settings.

### New Controls
- **Rebuild Interval**: Number of steps between topology rebuilds (default: 10).
- **Deletion Threshold**: Weight threshold below which edges are removed (default: 0.001).
- **Beta**: Inverse temperature for Metropolis-Hastings acceptance (default: 1.0).

### Files
- `RqSimUI/Forms/PartialForms/Form_Main_DynamicTopology.cs`: Handles UI creation, event wiring, and settings persistence.
- `RqSimUI/Forms/PartialForms/Form_Main_UniPipelineDynamicTopology.cs`: Extends the main form to handle the "Dynamic Hard Rewiring" topology mode selection.
- `ui_settings.json` & `dynamic_topology_settings.json`: Persistence for UI state.

## GPU Implementation Details

### 1. Stream Compaction (Prefix Sum & Scatter)
To efficiently remove edges in parallel on the GPU without leaving "holes" in the CSR arrays, we implemented a Stream Compaction pipeline.

#### Scan Kernels (`ScanKernels.cs`)
Implemented parallel Prefix Sum (Scan) to recalculate `RowOffsets` based on active edges.

```csharp
// [NEW FILE]
// Реализация параллельного Scan (Prefix Sum) для пересчета RowOffsets
[AutoConstructor]
internal readonly partial struct PrefixSumKernel : IComputeShader
{
    public readonly ReadWriteBuffer<int> InputValues;  // Количество активных связей у узла
    public readonly ReadWriteBuffer<int> OutputOffsets; // Результат: новые RowOffsets
    public readonly int N;
    public readonly int Step; // Шаг алгоритма (1, 2, 4, 8...)

    public void Execute()
    {
        int i = ThreadIds.X;
        if (i >= N) return;

        if (i >= Step)
            OutputOffsets[i] = InputValues[i] + InputValues[i - Step];
        else
            OutputOffsets[i] = InputValues[i];
    }
}
```

#### Stream Compaction Kernel (`StreamCompactionKernel.cs`)
Implemented the scatter phase to pack active edges into new dense buffers.

```csharp
// [NEW FILE]
// "Сжатие" разреженного массива на основе рассчитанных оффсетов
[AutoConstructor]
internal readonly partial struct CompactCsrKernel : IComputeShader
{
    public readonly ReadOnlyBuffer<int> OldRowOffsets;
    public readonly ReadOnlyBuffer<int> OldColIndices;
    public readonly ReadOnlyBuffer<double> OldWeights;
    
    public readonly ReadOnlyBuffer<int> NewRowOffsets; // Рассчитано через PrefixSum
    public readonly ReadWriteBuffer<int> NewColIndices;
    public readonly ReadWriteBuffer<double> NewWeights;

    public void Execute()
    {
        int row = ThreadIds.X;
        // Логика: идем по старому ряду, если ребро живо (weight > threshold),
        // пишем его в позицию, определенную NewRowOffsets[row] + localCounter
        // ... (требует атомика или локального скана внутри варпа)
    }
}
```

### 2. Evolution Engine Integration (`GpuCayleyEvolutionEngineCsr.cs`)
The evolution cycle now includes a compaction phase.

```csharp
public void EvolveTopology()
{
    // 1. Mark Phase: Шейдер помечает ребра с весом < epsilon как "0", остальные "1"
    // 2. Scan Phase: Запуск PrefixSumKernel для получения новых индексов
    // 3. Compact Phase: Запуск CompactCsrKernel для создания плотных массивов
    
    // CRITICAL FIX: Swap buffers
    this.topology.RowOffsets = newRowOffsetsBuffer;
    this.topology.ColIndices = newColIndicesBuffer;
    // Без этого граф статичен!
}
```

## Scientific Mode & Safety Removal

### Scientific Mode Flag
Added `ScientificMode` to `PhysicsConstants.Simulation.cs` to toggle between "safe" visualization mode and "raw" scientific simulation.

```csharp
public static class SimulationConstants 
{
    // [ADD THIS]
    // Если true, отключает все искусственные лимиты.
    // Если false, работает в режиме "красивой скринсейвер-физики".
    public static bool ScientificMode = true; 
}
```

### Ricci Flow Kernel Update (`GravityShadersDouble.cs`)
Updated `RicciFlowKernelDouble` to respect `ScientificMode`.

```csharp
[AutoConstructor]
internal readonly partial struct RicciFlowKernelDouble : IComputeShader
{
    // ... buffers
    public readonly int IsScientificMode; // 1 = true, 0 = false

    public void Execute()
    {
        // ... calculation of curvature (kappa) ...
        
        double flow = -kappa * learningRate;

        // [MODIFIED BLOCK]
        if (IsScientificMode == 0) 
        {
            // Старая логика с "подушками безопасности"
            if (flow > maxFlow) flow = maxFlow;
            if (flow < -maxFlow) flow = -maxFlow;
            
            double newW = currentW + flow;
            if (newW < weightMin) newW = weightMin; // Искусственное спасение жизни ребра
        }
        else 
        {
            // [SCIENTIFIC MODE]
            // Let it explode. Let it die.
            // Если newW уйдет в минус или NaN — это результат эксперимента.
            double newW = currentW + flow; 
            
            // Здесь можно добавить только проверку на NaN для логирования,
            // но не для предотвращения.
        }
        
        EdgeWeights[index] = newW;
    }
}
```

## Strict Hamiltonian Constraint

### Metropolis Kernel Update (`CsrMCMCShaders.cs`)
Added strict Hamiltonian constraint check to `MetropolisKernel`.

```csharp
[AutoConstructor]
internal readonly partial struct MetropolisKernel : IComputeShader
{
    // ... params
    public readonly double HamiltonianTolerance; // e.g., 1e-6

    public void Execute()
    {
        // ... расчет proposedState ...
        
        // Вычисление Гамильтониана для предлагаемого состояния
        // H = sum(C_ijk * ...) - N_tet * Lambda ... (упрощенно)
        double proposedH = CalculateHamiltonian(proposedState);

        // [CRITICAL CHECK: HAMILTONIAN CONSTRAINT]
        // В LQG/CDT физические состояния должны удовлетворять H|psi> = 0.
        // Если H отклоняется от нуля сильнее, чем допуск численного метода — это не физическое состояние.
        
        if (abs(proposedH) > HamiltonianTolerance)
        {
            // REJECT MOVE IMMEDIATELY
            // Не применяем изменения, даже если энтропия/действие благоприятны.
            return; 
        }

        // ... далее стандартная логика Action-based acceptance (exp(-DeltaS))...
    }
}
```
