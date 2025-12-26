# RqSim Scientific Validity Upgrade - Complete Change Log

## Обзор

Данный документ описывает все изменения, выполненные в рамках модернизации симулятора RqSim для обеспечения научной корректности согласно двум чек-листам:

1. **Roadmap to Q1** - Scientific Validity Upgrade
2. **Final Pre-Publication Punch List** - Terminological and Data Export fixes

---

## Чек-лист 1: Scientific Validity Upgrade (Roadmap to Q1)

### ? 1. Удаление «Мягких стен» (Soft Walls Removal)

**Цель:** Позволить весам рёбер достигать истинного нуля (образование горизонта событий).

**Файлы изменены:**
- `RqSimGraphEngine\RQSimulation\Core\Constants\PhysicsConstants.Simulation.cs`
- `RqSimGraphEngine\RQSimulation\GPUOptimized\Gravity\ImprovedNetworkGravity.cs`

**Изменения:**

```csharp
// PhysicsConstants.Simulation.cs - Новые флаги конфигурации

/// <summary>
/// RQ-MODERNIZATION: Allow edge weights to reach true zero.
/// When TRUE: Weights can reach 0.0, allowing physical horizon formation.
/// When FALSE: Legacy behavior with soft walls preventing zero weights.
/// </summary>
public const bool AllowZeroWeightEdges = false;

/// <summary>
/// RQ-MODERNIZATION: Enable/disable soft walls for edge weights.
/// When TRUE: Clamp weights to [WeightLowerSoftWall, WeightUpperSoftWall].
/// When FALSE: Allow weights to evolve freely (may require adaptive timestep).
/// </summary>
public const bool UseSoftWalls = true;
```

```csharp
// ImprovedNetworkGravity.cs - Модифицированный ApplySoftWalls

private static double ApplySoftWalls(double weight)
{
    // RQ-MODERNIZATION: Configurable soft wall behavior
    if (!PhysicsConstants.UseSoftWalls)
    {
        if (PhysicsConstants.AllowZeroWeightEdges)
        {
            // Allow true zero: only prevent negative weights
            return Math.Max(weight, 0.0);
        }
        else
        {
            // Prevent exact zero but no upper bound
            return Math.Max(weight, PhysicsConstants.WeightAbsoluteMinimum);
        }
    }
    
    // Legacy behavior: clamp to soft walls
    return Math.Clamp(weight, PhysicsConstants.WeightLowerSoftWall, PhysicsConstants.WeightUpperSoftWall);
}
```

---

### ? 2. Отключение защиты связности (Connectivity Protection Disable)

**Цель:** Если уравнения Эйнштейна-Редже предсказывают распад вселенной, симулятор не должен мешать.

**Файлы изменены:**
- `RqSimGraphEngine\RQSimulation\Core\Constants\PhysicsConstants.Simulation.cs`
- `RqSimGraphEngine\RQSimulation\GPUOptimized\Gravity\ImprovedNetworkGravity.cs`

**Изменения:**

```csharp
// PhysicsConstants.Simulation.cs

/// <summary>
/// RQ-MODERNIZATION: Enable/disable connectivity protection.
/// When TRUE: Suppress weight decreases when graph is at fragmentation risk.
/// When FALSE: Let physics equations determine graph fate.
/// </summary>
public const bool UseConnectivityProtection = true;
```

```csharp
// ImprovedNetworkGravity.cs - EvolveNetworkGeometryOllivierDynamic

bool protectConnectivity = false;
double connectivityProtectionFactor = 1.0;

if (PhysicsConstants.UseConnectivityProtection)
{
    double avgDegree = ComputeAverageDegree(graph);
    double minWeightSum = ComputeMinNodeWeightSum(graph);
    protectConnectivity = avgDegree < 4.0 || minWeightSum < 0.8;
    connectivityProtectionFactor = protectConnectivity ? 0.5 : 1.0;
}
```

---

### ? 3. Исправление точности FP64 (Double Precision Fix)

**Цель:** Убрать явное приведение к `float` внутри `double` шейдеров.

**Файл изменён:**
- `RqSimGraphEngine\RQSimulation\GPUOptimized\Gravity\GravityShadersDouble.cs`

**Изменения:**

```csharp
// БЫЛО (FP32 внутри FP64):
newW = Hlsl.Max((float)newW, 0.001f);
kappa = Hlsl.Clamp((float)kappa, -2.0f, 1.0f);

// СТАЛО (чистый FP64):
if (newW < weightMin) newW = weightMin;
if (newW > weightMax) newW = weightMax;

if (kappa < -2.0) kappa = -2.0;
if (kappa > 1.0) kappa = 1.0;
```

**RicciFlowKernelDouble** теперь принимает конфигурационные параметры:
- `weightMin` - минимальный вес (0.0 если AllowZeroWeightEdges)
- `weightMax` - максимальный вес
- `maxFlow` - ограничение скорости потока (0.0 = неограниченный)
- `useSoftWalls` - флаг использования soft walls

---

### ? 4. Замена Tanh на линейный интегратор

**Цель:** Убрать искусственное насыщение скорости эволюции.

**Файлы изменены:**
- `RqSimGraphEngine\RQSimulation\Core\Constants\PhysicsConstants.Simulation.cs`
- `RqSimGraphEngine\RQSimulation\GPUOptimized\Gravity\ImprovedNetworkGravity.cs`

**ВАЖНО: Терминологическое исправление (Чек-лист 2, пункт 1)**

Флаг переименован с `UseSymplecticGravityIntegrator` на `UseUnboundedFlow`:

```csharp
/// <summary>
/// RQ-MODERNIZATION: Use unbounded linear flow for gravity evolution.
/// When TRUE: Use linear Euler step without Tanh saturation (unbounded flow).
/// When FALSE: Use Tanh-bounded flow (legacy, more stable but artificially saturated).
/// 
/// TERMINOLOGICAL NOTE:
/// This is NOT a symplectic integrator. True symplectic integration requires:
/// - A momentum variable (? or p) in addition to position (w)
/// - Velocity Verlet or leapfrog scheme preserving phase space volume
/// 
/// This flag simply removes the Tanh() saturation from the flow equation.
/// </summary>
public const bool UseUnboundedFlow = false;
```

```csharp
// ImprovedNetworkGravity.cs

if (PhysicsConstants.UseUnboundedFlow)
{
    // Linear Euler step: unbounded flow without artificial saturation
    // NOTE: This is NOT symplectic - no momentum variable involved
    relativeChange = flowRate * learningRate;
}
else
{
    // Legacy: Tanh-bounded multiplicative update
    relativeChange = Math.Tanh(flowRate * 0.1) * learningRate;
}
```

---

### ? 5. Валидация «Смерти Вселенной» (Singularity Detection)

**Цель:** Обрабатывать сингулярности как научный результат, а не ошибку.

**Файлы изменены:**
- `RqSimGraphEngine\RQSimulation\Core\Infrastructure\RQGraph.GraphHealth.cs`
- `RqSimGraphEngine\RQSimulation\Core\SimulationConfig\SimulationEngine.cs`

**Новые типы:**

```csharp
public enum SingularityType
{
    None,           // Нет сингулярности
    Numerical,      // Численный сбой (NaN)
    Curvature,      // Сингулярность кривизны (бесконечность)
    Topological,    // Топологическая (распад графа)
    Horizon         // Формирование горизонта (w ? 0)
}

public readonly struct SingularityStatus
{
    public SingularityType Type { get; init; }
    public string Description { get; init; }
    public bool IsTerminal { get; init; }
    public int ConsecutiveSteps { get; init; }
    public int NaNEdgeCount { get; init; }
    public int InfinityEdgeCount { get; init; }
    public int ZeroWeightEdgeCount { get; init; }
    public bool HasSingularity => Type != SingularityType.None;
}

public enum SimulationState
{
    Running,
    SingularityForming,
    Finished,
    HorizonFormed,
    CurvatureSingularity,
    SpacetimeFragmented,
    NumericalBreakdown,
    Cancelled
}
```

**Новый метод:**

```csharp
public SingularityStatus CheckSingularityState()
{
    // Проверяет все рёбра на NaN, Infinity, нулевой вес
    // Определяет тип сингулярности
    // Отслеживает последовательные шаги с сингулярностью
    // Возвращает статус для обработки
}
```

---

### ? 6. Документация динамической топологии (Dynamic Topology)

**Цель:** Явно задокументировать ограничения текущей реализации.

**Файл изменён:**
- `RqSimGraphEngine\RQSimulation\GPUOptimized\Topology\PotentialLinkActivator.cs`

**Добавлен блок документации:**

```csharp
/// ============================================================
/// RQ-MODERNIZATION: IMPORTANT SCIENTIFIC LIMITATION
/// ============================================================
/// 
/// WARNING: Current implementation uses FIXED LATTICE BACKGROUND.
/// 
/// This is NOT true Quantum Graphity / Causal Dynamical Triangulations.
/// The current approach is equivalent to Lattice Field Theory on a 
/// pre-defined graph topology with variable edge weights.
/// 
/// WHAT THIS MEANS:
/// - Edges can become "sleeping" (weight ? 0) but are never truly removed
/// - New edges cannot be created - only pre-existing edges can be activated
/// - The fundamental graph structure is fixed at initialization
/// 
/// TODO for Q2 (True Dynamic Topology):
/// 1. Implement GPU Stream Compaction (Parallel Prefix Sum)
/// 2. Use atomic operations for thread-safe edge insertion/deletion
/// 3. Consider Causal Sets approach with partial order preservation
/// ============================================================
```

---

### ? 7. Разделение физики и визуализации

**Цель:** Убедиться, что координаты визуализации не влияют на физику.

**Файл изменён:**
- `RqSimGraphEngine\RQSimulation\GPUOptimized\Gravity\GravityShaders.cs`

**Добавлена документация:**

```csharp
/// ============================================================
/// RQ-MODERNIZATION: Physics-Visualization Separation (Checklist #7)
/// ============================================================
/// 
/// IMPORTANT: This shader computes PHYSICS quantities only.
/// 
/// INPUTS (Physics Data):
/// - weights: Edge weights (w_ij) - the metric tensor components
/// - edges: Topological connectivity (which nodes are connected)
/// - adjOffsets/adjData: CSR graph structure for neighbor enumeration
/// 
/// NOT USED:
/// - NodePositions (visualization coordinates)
/// - Any UI/rendering data
/// 
/// This maintains strict separation between:
/// - PHYSICS: Discrete geometry on the graph (weights, curvature, stress-energy)
/// - VISUALIZATION: Embedding coordinates for rendering (positions in ??)
/// ============================================================
```

---

## Чек-лист 2: Final Pre-Publication Punch List

### ? 1. Терминологическая честность (The "Symplectic" Trap)

**Проблема:** Флаг `UseSymplecticGravityIntegrator` использовал термин, зарезервированный для методов с сохранением фазового объёма.

**Решение:** Переименовано в `UseUnboundedFlow` с подробной документацией:

```csharp
/// TERMINOLOGICAL NOTE:
/// This is NOT a symplectic integrator. True symplectic integration requires:
/// - A momentum variable (? or p) in addition to position (w)
/// - Velocity Verlet or leapfrog scheme preserving phase space volume
/// 
/// This flag simply removes the Tanh() saturation from the flow equation,
/// allowing the physics equations to evolve without artificial velocity limits.
/// 
/// For true Hamiltonian gravity with momentum, see GeometryMomenta module
/// which implements second-order dynamics with EdgeMomentum buffer.
```

---

### ? 2. Экспорт данных при сингулярности (Crash Dump)

**Проблема:** При останове симуляции не сохранялось состояние для анализа.

**Решение:** Добавлен автоматический экспорт crash dump при терминальной сингулярности.

**Новые типы:**

```csharp
public class SingularitySnapshot
{
    public DateTime Timestamp { get; set; }
    public int SimulationStep { get; set; }
    public string SingularityType { get; set; }
    public string SingularityDescription { get; set; }
    public int NodeCount { get; set; }
    public int TotalEdgeCount { get; set; }
    public double SpectralDimension { get; set; }
    public double AverageDegree { get; set; }
    public int NaNEdgeCount { get; set; }
    public int InfinityEdgeCount { get; set; }
    public int ZeroWeightEdgeCount { get; set; }
    public double MeanCurvature { get; set; }
    public double MinCurvature { get; set; }
    public double MaxCurvature { get; set; }
    public double MeanNodeMass { get; set; }
    public double MaxNodeMass { get; set; }
    public List<CriticalEdgeInfo> CriticalEdges { get; set; }
}

public class CriticalEdgeInfo
{
    public int NodeA { get; set; }
    public int NodeB { get; set; }
    public double Weight { get; set; }
    public double Curvature { get; set; }
    public string Reason { get; set; }  // "NearZero", "NaN", "Infinity"
}
```

**Новый метод в RQGraph:**

```csharp
public void ExportSingularitySnapshot(
    string filePath,
    int step,
    SingularityStatus singularityStatus,
    double? spectralDimension = null)
```

**Интеграция в SimulationEngine:**

```csharp
public bool CheckAndHandleSingularity(int step, double? spectralDimension = null)
{
    // ... проверка сингулярности ...
    
    if (singularity.IsTerminal)
    {
        // RQ-MODERNIZATION: Export crash dump for scientific analysis
        ExportSingularityCrashDump(step, singularity, spectralDimension);
        return false; // Stop simulation
    }
}
```

**Формат файла:** `CRASH_DUMP_{SingularityType}_step{N}_{timestamp}.json`
**Расположение:** `%TEMP%` директория

---

## Новые конфигурационные опции

### В PhysicsConstants.Simulation.cs:

| Константа | Тип | Значение | Описание |
|-----------|-----|----------|----------|
| `AllowZeroWeightEdges` | bool | false | Разрешить w = 0 (горизонты) |
| `UseSoftWalls` | bool | true | Использовать мягкие ограничения весов |
| `UseConnectivityProtection` | bool | true | Защита от фрагментации графа |
| `UseUnboundedFlow` | bool | false | Линейный интегратор без Tanh |
| `AllowSingularityFormation` | bool | true | Сингулярности как результат, не ошибка |
| `SingularityGracePeriodSteps` | int | 5 | Шагов до терминальной сингулярности |

### В SimulationConfig:

| Свойство | Тип | Значение | Описание |
|----------|-----|----------|----------|
| `AllowZeroWeightEdges` | bool | false | UI-переключатель для AllowZeroWeightEdges |
| `UseSoftWalls` | bool | true | UI-переключатель для UseSoftWalls |
| `UseConnectivityProtection` | bool | true | UI-переключатель для UseConnectivityProtection |
| `UseUnboundedFlow` | bool | false | UI-переключатель для UseUnboundedFlow |
| `AllowSingularityFormation` | bool | true | UI-переключатель для AllowSingularityFormation |

---

## Файлы изменены

1. `RqSimGraphEngine\RQSimulation\Core\Constants\PhysicsConstants.Simulation.cs`
2. `RqSimGraphEngine\RQSimulation\GPUOptimized\Gravity\ImprovedNetworkGravity.cs`
3. `RqSimGraphEngine\RQSimulation\GPUOptimized\Gravity\GravityShadersDouble.cs`
4. `RqSimGraphEngine\RQSimulation\GPUOptimized\Gravity\GravityShaders.cs`
5. `RqSimGraphEngine\RQSimulation\GPUOptimized\Topology\PotentialLinkActivator.cs`
6. `RqSimGraphEngine\RQSimulation\Core\Infrastructure\RQGraph.GraphHealth.cs`
7. `RqSimGraphEngine\RQSimulation\Core\SimulationConfig\SimulationEngine.cs`

---

## Рекомендации по использованию

### Для физически строгих симуляций:

```csharp
// В PhysicsConstants или через UI:
AllowZeroWeightEdges = true;
UseSoftWalls = false;
UseConnectivityProtection = false;
UseUnboundedFlow = true;  // Требует меньший dt!
AllowSingularityFormation = true;
```

### Для стабильных исследовательских запусков:

```csharp
// Значения по умолчанию:
AllowZeroWeightEdges = false;
UseSoftWalls = true;
UseConnectivityProtection = true;
UseUnboundedFlow = false;
AllowSingularityFormation = true;
```

---

## TODO для Q2

1. **Истинный симплектический интегратор**
   - Добавить буфер `EdgeMomentum`
   - Реализовать Velocity Verlet схему
   - Проверить сохранение энергии

2. **Динамическая топология на GPU**
   - Реализовать GPU Stream Compaction
   - Атомарные операции для изменения CSR
   - Алгоритмы Causal Sets

3. **Адаптивный timestep**
   - CFL условие для стабильности
   - Автоматическое уменьшение dt при высокой кривизне

---

*Документ создан: $(date)*
*Версия: Scientific Validity Upgrade v1.0*
