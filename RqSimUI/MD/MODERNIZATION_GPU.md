# План Модернизации GPU - RQSimulation

## Обзор

Данный документ описывает пошаговый план переноса алгоритмов на GPU (ComputeSharp) с обеспечением double-precision (64-bit) вычислений. Учитывается физическая корректность распараллеливания и выбор между CSR (Compressed Sparse Row) и Dense форматами хранения данных.
 
**Целевые директории:**
- `RQSimulation/GPUOptimized/` - Dense формат, небольшие графы
- `RQSimulation/GPUCompressedSparseRow/` - CSR формат, большие разреженные графы

**Принципы:**
1. **Double precision (64-bit)** - обязательно для физически критичных операций
2. **Распараллеливание по узлам/рёбрам** - независимые операции на GPU
3. **CSR для больших графов (N > 10⁴)** - экономия памяти и bandwidth
4. **Dense для малых графов (N < 10⁴)** - простота и минимальный overhead
5. **Обратная совместимость** - CPU fallback если GPU недоступен

---

## Рекомендуемый порядок реализации этапов

| # | Этап | Описание | Сложность | Распараллеливаемость | Статус |
|---|------|----------|-----------|---------------------|--------|
| 1 | **GPU Wheeler-DeWitt** | Constraint violation на GPU | Средняя | По узлам | ✅ Завершено |
| 2 | **GPU Spectral Action** | Спектральное действие на GPU | Средняя | По рёбрам | ✅ Завершено |
| 3 | **GPU Quantum Edges** | Квантовые амплитуды рёбер на GPU | Низкая | По рёбрам | ✅ Завершено |
| 4 | **GPU MCMC** | MCMC сэмплирование на GPU | Высокая | Параллельные proposals | ✅ Завершено |
| 5 | **GPU Internal Observer** | Измерения через GPU | Средняя | По узлам наблюдателя | ✅ Завершено |
| 6 | **CSR Unified Engine** | Объединённый CSR движок | Высокая | SpMV операции | ✅ Завершено |

---

## Этап 1: GPU Wheeler-DeWitt Constraint

### 1.1 Цель
Перенести вычисление Wheeler-DeWitt constraint violation на GPU.

### 1.2 Физика распараллеливания

**Полностью распараллеливаемо:**
- Каждый узел независимо вычисляет свой локальный constraint
- H_geom = GetLocalCurvature(i) - зависит только от соседей
- H_matter = mass[i] - локальное значение
- constraint[i] = (H_geom - κ * H_matter)²

**Итоговая редукция:**
- total_violation = Σᵢ constraint[i] / N

### 1.3 Файлы для создания

#### 1.3.1 `GPUOptimized/Constraint/ConstraintShadersDouble.cs`

```csharp
namespace RQSimulation.GPUOptimized.Constraint
{
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct LocalCurvatureKernelDouble : IComputeShader
    {
        public readonly ReadOnlyBuffer<int> adjOffsets;
        public readonly ReadOnlyBuffer<int> adjNeighbors;
        public readonly ReadOnlyBuffer<double> weights;
        public readonly ReadWriteBuffer<double> curvature;
        public readonly double triangleBonus;
        public readonly double degreePenalty;
        
        public void Execute()
        {
            int i = ThreadIds.X;
            // Forman-Ricci curvature calculation per node
        }
    }
    
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct WheelerDeWittConstraintKernelDouble : IComputeShader
    {
        public readonly ReadOnlyBuffer<double> curvature;
        public readonly ReadOnlyBuffer<double> mass;
        public readonly ReadWriteBuffer<double> violation;
        public readonly double kappa;
        
        public void Execute()
        {
            int i = ThreadIds.X;
            double H_geom = curvature[i];
            double H_matter = mass[i];
            double constraint = H_geom - kappa * H_matter;
            violation[i] = constraint * constraint;
        }
    }
}
```

#### 1.3.2 `GPUOptimized/Constraint/GpuConstraintEngine.cs`

```csharp
namespace RQSimulation.GPUOptimized.Constraint
{
    /// <summary>
    /// GPU-accelerated Wheeler-DeWitt constraint computation.
    /// Uses double precision for physical accuracy.
    /// </summary>
    public sealed class GpuConstraintEngine : IDisposable
    {
        public void Initialize(int nodeCount, int edgeCount);
        public void UploadTopology(RQGraph graph);
        public void UploadMass(double[] mass);
        public double ComputeTotalConstraintViolation();
        public void DownloadCurvature(double[] output);
    }
}
```

### 1.4 Тестирование
- Тест: GPU constraint == CPU constraint (tolerance 1e-10)
- Тест: Производительность > 10x для N > 1000

---

## Этап 2: GPU Spectral Action

### 2.1 Цель
Перенести вычисление спектрального действия Chamseddine-Connes на GPU.

### 2.2 Физика распараллеливания

**Параллельно по рёбрам:**
- Эффективный объём: V = Σᵢⱼ wᵢⱼ

**Параллельно по узлам:**
- Средняя кривизна: R_avg = (Σᵢ Rᵢ) / N
- Дисперсия кривизны (Weyl²): Var(R) = Σᵢ (Rᵢ - R_avg)²

### 2.3 Файлы для создания

#### 2.3.1 `GPUOptimized/SpectralAction/SpectralActionShadersDouble.cs`

```csharp
namespace RQSimulation.GPUOptimized.SpectralAction
{
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct EdgeVolumeKernelDouble : IComputeShader
    {
        public readonly ReadOnlyBuffer<double> weights;
        public readonly ReadWriteBuffer<double> volumeContributions;
        
        public void Execute()
        {
            int e = ThreadIds.X;
            volumeContributions[e] = weights[e];
        }
    }
    
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct CurvatureVarianceKernelDouble : IComputeShader
    {
        public readonly ReadOnlyBuffer<double> curvature;
        public readonly ReadWriteBuffer<double> variance;
        public readonly double avgCurvature;
        
        public void Execute()
        {
            int i = ThreadIds.X;
            double diff = curvature[i] - avgCurvature;
            variance[i] = diff * diff;
        }
    }
}
```

#### 2.3.2 `GPUOptimized/SpectralAction/GpuSpectralActionEngine.cs`

```csharp
namespace RQSimulation.GPUOptimized.SpectralAction
{
    /// <summary>
    /// GPU-accelerated Spectral Action computation.
    /// S = f₀Λ⁴V + f₂Λ²∫R + f₄∫C² + S_dimension
    /// </summary>
    public sealed class GpuSpectralActionEngine : IDisposable
    {
        public void Initialize(int nodeCount, int edgeCount);
        public double ComputeSpectralAction(double spectralDimension);
        public double ComputeEffectiveVolume();
        public double ComputeAverageCurvature();
        public double ComputeWeylSquared();
    }
}
```

### 2.4 Тестирование
- Тест: GPU action == CPU action (tolerance 1e-8)

---

## Этап 3: GPU Quantum Edges

### 3.1 Цель
Перенести операции с квантовыми амплитудами рёбер на GPU.

### 3.2 Физика распараллеливания

**Полностью распараллеливаемо:**
- Унитарная эволюция амплитуд: α_new = exp(-iHdt) α
- Вычисление вероятностей: P = |α|²
- Вычисление чистоты: purity = Σ P² / (Σ P)²

### 3.3 Файлы для создания

#### 3.3.1 `GPUOptimized/QuantumEdges/QuantumEdgeShadersDouble.cs`

```csharp
namespace RQSimulation.GPUOptimized.QuantumEdges
{
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct UnitaryEvolutionKernelDouble : IComputeShader
    {
        public readonly ReadWriteBuffer<Double2> amplitudes; // (real, imag)
        public readonly ReadOnlyBuffer<double> hamiltonianDiag;
        public readonly double dt;
        
        public void Execute()
        {
            int e = ThreadIds.X;
            Double2 alpha = amplitudes[e];
            double H = hamiltonianDiag[e];
            
            // exp(-i H dt) = cos(H*dt) - i*sin(H*dt)
            double phase = H * dt;
            double cosP = Hlsl.Cos((float)phase);
            double sinP = Hlsl.Sin((float)phase);
            
            double newReal = alpha.X * cosP + alpha.Y * sinP;
            double newImag = alpha.Y * cosP - alpha.X * sinP;
            
            amplitudes[e] = new Double2(newReal, newImag);
        }
    }
    
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct ExistenceProbabilityKernelDouble : IComputeShader
    {
        public readonly ReadOnlyBuffer<Double2> amplitudes;
        public readonly ReadWriteBuffer<double> probabilities;
        
        public void Execute()
        {
            int e = ThreadIds.X;
            Double2 alpha = amplitudes[e];
            probabilities[e] = alpha.X * alpha.X + alpha.Y * alpha.Y;
        }
    }
}
```

#### 3.3.2 `GPUOptimized/QuantumEdges/GpuQuantumEdgeEngine.cs`

```csharp
namespace RQSimulation.GPUOptimized.QuantumEdges
{
    /// <summary>
    /// GPU-accelerated quantum edge operations.
    /// Edge amplitudes evolve unitarily: |edge⟩ = α|exists⟩ + β|not-exists⟩
    /// </summary>
    public sealed class GpuQuantumEdgeEngine : IDisposable
    {
        public void Initialize(int edgeCount);
        public void UploadAmplitudes(ComplexEdge[,] edges);
        public void EvolveUnitary(double dt);
        public void CollapseAllEdges();
        public double ComputePurity();
        public void DownloadAmplitudes(ComplexEdge[,] output);
    }
}
```

### 3.4 Тестирование
- Тест: Унитарность сохраняется (||α||² = const)
- Тест: Collapse statistics match P = |α|²

---

## Этап 4: GPU MCMC Sampling

### 4.1 Цель
Перенести вычисление Euclidean Action и Metropolis-Hastings сэмплирование на GPU.

### 4.2 Физика распараллеливания

**Можно распараллелить:**
- Вычисление H_links (сумма по рёбрам) - reduction на GPU
- Вычисление H_nodes (сумма по узлам) - reduction на GPU
- Вычисление constraint violation - параллельно по узлам

**Нельзя распараллелить напрямую:**
- Последовательное применение moves (нарушает детальный баланс)

**Решение: Parallel Tempering / Replica Exchange**
- K независимых реплик с разными температурами
- Каждая реплика делает независимый MCMC шаг на GPU
- Периодически обмениваемся состояниями

### 4.3 Файлы для создания

#### 4.3.1 `GPUOptimized/MCMC/MCMCShadersDouble.cs`

```csharp
namespace RQSimulation.GPUOptimized.MCMC
{
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct EdgeActionKernelDouble : IComputeShader
    {
        public readonly ReadOnlyBuffer<int> edgeI;
        public readonly ReadOnlyBuffer<int> edgeJ;
        public readonly ReadOnlyBuffer<double> weights;
        public readonly ReadWriteBuffer<double> edgeActions;
        public readonly double linkCostCoeff;
        
        public void Execute()
        {
            int e = ThreadIds.X;
            double w = weights[e];
            edgeActions[e] = linkCostCoeff * (1.0 - w);
        }
    }
    
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct SumReductionKernelDouble : IComputeShader
    {
        public readonly ReadWriteBuffer<double> data;
        public readonly int stride;
        
        public void Execute()
        {
            int i = ThreadIds.X;
            int target = i * stride * 2;
            int source = target + stride;
            if (source < data.Length)
                data[target] += data[source];
        }
    }
}
```

#### 4.3.2 `GPUOptimized/MCMC/GpuMCMCEngine.cs`

```csharp
namespace RQSimulation.GPUOptimized.MCMC
{
    /// <summary>
    /// GPU-accelerated MCMC engine using Parallel Tempering.
    /// </summary>
    public sealed class GpuMCMCEngine : IDisposable
    {
        public void Initialize(int nodeCount, int edgeCount, int replicaCount);
        public void UploadGraph(RQGraph graph);
        public double ComputeEuclideanActionGpu();
        public void SampleConfigurationSpaceGpu(int samples, Action<int>? onSample = null);
        
        public int AcceptedMoves { get; }
        public int RejectedMoves { get; }
        public double AcceptanceRate { get; }
    }
}
```

### 4.4 Тестирование
- Тест: GPU action == CPU action (tolerance 1e-10)
- Тест: Acceptance rate в диапазоне 20-40%

---

## Этап 5: GPU Internal Observer

### 5.1 Цель
Перенести операции внутреннего наблюдателя на GPU.

### 5.2 Физика распараллеливания

**Параллельно:**
- Вычисление корреляций observer-target
- Сдвиг фаз в подсистеме наблюдателя
- Вычисление взаимной информации

### 5.3 Файлы для создания

#### 5.3.1 `GPUOptimized/Observer/ObserverShadersDouble.cs`

```csharp
namespace RQSimulation.GPUOptimized.Observer
{
    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    [RequiresDoublePrecisionSupport]
    public readonly partial struct PhaseShiftKernelDouble : IComputeShader
    {
        public readonly ReadWriteBuffer<Double2> wavefunction;
        public readonly ReadOnlyBuffer<int> observerNodes;
        public readonly ReadOnlyBuffer<double> phaseShifts;
        
        public void Execute()
        {
            int idx = ThreadIds.X;
            int node = observerNodes[idx];
            double shift = phaseShifts[idx];
            
            Double2 psi = wavefunction[node];
            double cosS = Hlsl.Cos((float)shift);
            double sinS = Hlsl.Sin((float)shift);
            
            double newReal = psi.X * cosS - psi.Y * sinS;
            double newImag = psi.X * sinS + psi.Y * cosS;
            
            wavefunction[node] = new Double2(newReal, newImag);
        }
    }
}
```

#### 5.3.2 `GPUOptimized/Observer/GpuObserverEngine.cs`

```csharp
namespace RQSimulation.GPUOptimized.Observer
{
    /// <summary>
    /// GPU-accelerated internal observer operations.
    /// </summary>
    public sealed class GpuObserverEngine : IDisposable
    {
        public void Initialize(int nodeCount, int observerSize);
        public void SetObserverNodes(int[] nodes);
        public void MeasureSweepGpu();
        public double ComputeMutualInformation();
        public void ApplyPhaseShifts(double[] shifts);
    }
}
```

### 5.4 Тестирование
- Тест: Mutual information ≥ 0
- Тест: Phase shifts preserve normalization

---

## Этап 6: CSR Unified Engine

### 6.1 Цель
Создать объединённый CSR движок для всех операций на больших разреженных графах.

### 6.2 Преимущества CSR

| Аспект | Dense | CSR |
|--------|-------|-----|
| Память | O(N²) | O(N + E) |
| SpMV | O(N²) | O(E) |
| Cache locality | Плохая | Хорошая |
| Порог | N < 10⁴ | N > 10⁴ |

### 6.3 Файлы для создания

#### 6.3.1 `GPUCompressedSparseRow/Unified/CsrUnifiedEngine.cs`

```csharp
namespace RQSimulation.GPUCompressedSparseRow.Unified
{
    /// <summary>
    /// Unified CSR engine for large sparse graphs.
    /// Combines all GPU operations with shared topology.
    /// </summary>
    public sealed class CsrUnifiedEngine : IDisposable
    {
        private CsrTopology _topology;
        private GpuBiCGStabSolverCsr _biCGStab;
        
        // Sub-engines using shared CSR topology
        private CsrConstraintEngine _constraint;
        private CsrSpectralActionEngine _spectralAction;
        private CsrQuantumEdgeEngine _quantumEdges;
        private CsrMCMCEngine _mcmc;
        
        public void Initialize(RQGraph graph);
        public void UpdateTopology(RQGraph graph);
        
        // Unified physics step
        public void PhysicsStepGpu(double dt);
        
        // Individual operations
        public double ComputeConstraintViolation();
        public double ComputeSpectralAction();
        public void EvolveCayley(double dt);
        public void MCMCSample(int samples);
    }
}
```

### 6.4 Тестирование
- Тест: CSR results == Dense results
- Тест: Memory usage < O(N²) для sparse graphs

---

## Архитектурные Решения

### Double vs Float

| Операция | Precision | Причина |
|----------|-----------|---------|
| Euclidean Action | Double | Накопление ошибок |
| Constraint Violation | Double | Физическая точность |
| Spectral Action | Double | Энергетические расчёты |
| Quantum Amplitudes | Double | Унитарность |
| Random Walk | Float | Статистика, не критично |
| Visualization | Float | Скорость, не критично |

### Dense vs CSR

| Размер графа | Формат | Движок |
|--------------|--------|--------|
| N < 1,000 | Dense | GPUOptimized/* |
| 1,000 ≤ N < 10,000 | Выбор | Оба работают |
| N ≥ 10,000 | CSR | GPUCompressedSparseRow/* |

### HLSL Intrinsics Limitation

ComputeSharp HLSL intrinsics принимают только `float`:
- `Hlsl.Sin`, `Hlsl.Cos`, `Hlsl.Tan`
- `Hlsl.Exp`, `Hlsl.Log`, `Hlsl.Pow`
- `Hlsl.Sqrt`

**Решение:** Cast to float для трансцендентных функций:
```csharp
double cosVal = Hlsl.Cos((float)angle);  // Returns float, promoted to double
```

---

## Оценка Трудозатрат

| Этап | Сложность | Оценка (часы) | Риск |
|------|-----------|---------------|------|
| Этап 1 (Wheeler-DeWitt) | Средняя | 8-12 | Средний |
| Этап 2 (Spectral Action) | Средняя | 8-12 | Средний |
| Этап 3 (Quantum Edges) | Низкая | 6-8 | Низкий |
| Этап 4 (GPU MCMC) | Высокая | 16-24 | Высокий |
| Этап 5 (Observer) | Средняя | 8-12 | Низкий |
| Этап 6 (CSR Unified) | Высокая | 16-24 | Средний |
| **Итого** | | **62-92** | |

---

## Проверка После Каждого Этапа

```bash
dotnet build RqSimGraphEngine/RqSimGraphEngine.csproj
dotnet test RqSimGPUCPUTests/RqSimGPUCPUTests.csproj
```

**Критерии успеха:**
- [ ] Нет ошибок компиляции
- [ ] GPU результаты совпадают с CPU (tolerance зависит от операции)
- [ ] Производительность GPU > 10x CPU для N > 1000
- [ ] Double precision сохраняется где требуется
