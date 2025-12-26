# GPU Porting Critical Simulation Modules (CSR + ComputeSharp)

## Обзор проекта

Портирование критических CPU-алгоритмов симуляции на GPU с использованием CSR (Compressed Sparse Row) формата и ComputeSharp для массивного параллелизма.

**Целевые модули:**
1. Black Hole Horizon Detection ?
2. Causal Discovery (Parallel BFS) ?
3. Gauge Invariant Checker (Wilson Loops) ?
4. Heavy Mass Analyzer ?
5. Hybrid Pipeline Coordinator ?
6. Integration Tests ?

---

## ? Завершённые модули

### 1. GPU Black Hole Horizon Detection Module

**Статус:** ? Полностью реализован

**Описание:** GPU-ускоренное обнаружение горизонтов событий чёрных дыр на основе критерия Шварцшильда.

**Физика (RQ-HYPOTHESIS):**
- Чёрные дыры возникают как области высокой плотности графа
- Горизонт определяется условием: `r_eff ? 2M` (радиус Шварцшильда)
- Lapse-функция замерзает на горизонте (время останавливается)
- Hawking radiation вызывает медленное испарение

**Файлы:**

| Файл | Назначение |
|------|------------|
| `GPUCompressedSparseRow/BlackHole/HorizonStateGpu.cs` | Структура данных состояния горизонта |
| `GPUCompressedSparseRow/BlackHole/HorizonShaders.cs` | 4 GPU шейдера: LocalMassKernel, EffectiveRadiusKernel, HorizonDetectionKernel, HawkingEvaporationKernel |
| `GPUCompressedSparseRow/BlackHole/GpuHorizonEngine.cs` | Движок координации GPU операций |
| `GPUCompressedSparseRow/BlackHole/GpuBlackHoleHorizonModule.cs` | IPhysicsModule интеграция |
| `Core/Infrastructure/RQGraph.HorizonIntegration.cs` | Расширение RQGraph |

**Алгоритм (O(N) параллельный):**
1. `LocalMassKernel`: M_i = ?_j w_ij * |?_j|? через CSR
2. `EffectiveRadiusKernel`: r_eff = mean(-ln(w_ij)) для соседей
3. `HorizonDetectionKernel`: Проверка r_eff ? r_s = 2M, установка флагов
4. `HawkingEvaporationKernel`: Применение потери массы для испаряющихся горизонтов

**Horizon Flags (битовое поле):**
- Bit 0: IsHorizon (узел на горизонте событий)
- Bit 1: IsSingularity (узел внутри горизонта)
- Bit 2: IsTrapped (свет не может выйти)
- Bit 3: IsEvaporating (теряет массу через Hawking radiation)

---

### 2. GPU Causal Discovery Module

**Статус:** ? Полностью реализован

**Описание:** GPU-ускоренное вычисление причинной структуры через параллельный BFS (Wavefront Propagation).

**Физика (RQ-HYPOTHESIS):**
- Причинность возникает из связности графа
- Световой конус = все достижимые узлы за proper time
- Скорость света = 1 ребро за единицу времени (настраиваемо)

**Файлы:**

| Файл | Назначение |
|------|------------|
| `GPUCompressedSparseRow/Causal/CausalShaders.cs` | 5 GPU шейдеров: CausalWavefrontKernel, CausalInitKernel, FrontierCheckKernel, CausalConeExtractKernel, MultiSourceInitKernel |
| `GPUCompressedSparseRow/Causal/GpuCausalEngine.cs` | Движок для параллельного BFS |
| `GPUCompressedSparseRow/Causal/GpuCausalDiscoveryModule.cs` | IPhysicsModule интеграция |

**Ключевые методы:**
- `ComputeCausalCone(sourceNode, maxDepth)` - вычислить световой конус
- `ComputeCausalConeMultiSource(sourceNodes[], maxDepth)` - множественные источники
- `AreCausallyConnected(nodeA, nodeB, maxDepth)` - проверка причинной связи
- `GetDistance(nodeA, nodeB)` - кратчайшее расстояние

---

### 3. GPU Gauge Invariant Checker (Wilson Loops)

**Статус:** ? Полностью реализован

**Описание:** GPU-ускоренная проверка калибровочной инвариантности через вычисление петель Вильсона.

**Физика:**
- Петля Вильсона = произведение фаз/матриц вдоль замкнутого цикла
- Проверяет калибровочную инвариантность: Tr(W) = const
- Вычисление кривизны через плакеты (минимальные циклы)

**Файлы:**

| Файл | Назначение |
|------|------------|
| `GPUCompressedSparseRow/Gauge/GaugeShaders.cs` | 5 GPU шейдеров: TriangleDetectionKernel, WilsonLoopKernel, TopologicalChargeKernel, LocalGaugeViolationKernel, BerryPhaseKernel |
| `GPUCompressedSparseRow/Gauge/GpuGaugeEngine.cs` | Движок Wilson loop computation |
| `GPUCompressedSparseRow/Gauge/GpuGaugeInvariantModule.cs` | IPhysicsModule интеграция |

**Алгоритм:**
1. `TriangleDetectionKernel` - поиск треугольников через CSR intersection
2. `WilsonLoopKernel` - вычисление W = exp(i??)
3. `TopologicalChargeKernel` - Chern number из arg(W)
4. Нарушение калибровки = отклонение |W| от 1

---

### 4. GPU Heavy Mass Analyzer

**Статус:** ? Полностью реализован

**Описание:** GPU-ускоренный анализ "тяжёлых" кластеров с высокой геометрической инерцией.

**Физика:**
- Correlation mass = сумма весов рёбер (connectivity strength)
- Geometry inertia = сопротивление изменениям топологии
- Heavy clusters = стабильные "барионные" структуры

**Файлы:**

| Файл | Назначение |
|------|------------|
| `GPUCompressedSparseRow/HeavyMass/HeavyMassShaders.cs` | 5 GPU шейдеров: CorrelationMassKernel, ClusterEnergyKernel, GeometryInertiaKernel, CenterOfMassKernel, HeavyClusterDetectionKernel |
| `GPUCompressedSparseRow/HeavyMass/GpuHeavyMassEngine.cs` | Движок mass analysis |
| `GPUCompressedSparseRow/HeavyMass/GpuHeavyMassModule.cs` | IPhysicsModule интеграция |

---

### 5. Hybrid Pipeline Coordinator

**Статус:** ? Полностью реализован

**Описание:** Координатор гибридного GPU-CPU конвейера для топологических изменений.

**Архитектура:**
1. **GPU Phase (Read-Only):** GPU модули читают CSR, генерируют рекомендации
2. **Collection Phase:** Coordinator собирает и приоритизирует рекомендации
3. **CPU Phase (Write):** Применяет изменения к RQGraph
4. **Sync Phase:** Ребилд CSR и загрузка на GPU

**Файлы:**

| Файл | Назначение |
|------|------------|
| `GPUCompressedSparseRow/Hybrid/RecommendationBuffer.cs` | Потокобезопасный буфер рекомендаций, типы рекомендаций |
| `GPUCompressedSparseRow/Hybrid/HybridPipelineCoordinator.cs` | Координатор сбора, валидации, применения изменений |

**Типы рекомендаций:**
- CreateEdge, RemoveEdge
- StrengthenEdge, WeakenEdge
- CreateNode, RemoveNode
- Rewire

---

### 6. Integration Tests

**Статус:** ? Полностью реализован

**Файл:** `Tests/RqSimGPUCPUTests/Tests/CSR/GpuCsrModules_Integration_Tests.cs`

**Тесты:**
- `GpuHorizonEngine_Initialize_ValidTopology_Succeeds`
- `GpuHorizonEngine_DetectHorizons_ProducesReasonableResults`
- `GpuBlackHoleHorizonModule_ExecuteStep_NoErrors`
- `GpuCausalEngine_Initialize_ValidTopology_Succeeds`
- `GpuCausalEngine_ComputeCausalCone_ReturnsReachableNodes`
- `GpuCausalEngine_AreCausallyConnected_SelfConnection_ReturnsTrue`
- `GpuCausalEngine_GetDistance_DirectNeighbors_ReturnsOne`
- `GpuCausalDiscoveryModule_ExecuteStep_NoErrors`
- `GpuGaugeEngine_Initialize_ValidTopology_Succeeds`
- `GpuGaugeEngine_DetectTriangles_FindsTriangles`
- `GpuGaugeEngine_ComputeWilsonLoops_ZeroPhases_MagnitudeOne`
- `GpuGaugeInvariantModule_ExecuteStep_NoErrors`
- `GpuHeavyMassEngine_Initialize_ValidTopology_Succeeds`
- `GpuHeavyMassEngine_ComputeCorrelationMass_PositiveValues`
- `GpuHeavyMassEngine_DetectHeavyNodes_ReturnsValidList`
- `GpuHeavyMassModule_ExecuteStep_NoErrors`
- `RecommendationBuffer_Add_IncreasesCount`
- `RecommendationBuffer_GetSorted_OrdersByPriority`
- `HybridPipelineCoordinator_Initialize_NoErrors`
- `HybridPipelineCoordinator_ProcessRecommendations_EmptyBuffer_NoChanges`

---

## ?? Полная структура файлов

```
RqSimGraphEngine/RQSimulation/
??? GPUCompressedSparseRow/
?   ??? BlackHole/                      # ? Horizon Detection
?   ?   ??? HorizonStateGpu.cs
?   ?   ??? HorizonShaders.cs
?   ?   ??? GpuHorizonEngine.cs
?   ?   ??? GpuBlackHoleHorizonModule.cs
?   ?
?   ??? Causal/                         # ? Causal Discovery
?   ?   ??? CausalShaders.cs
?   ?   ??? GpuCausalEngine.cs
?   ?   ??? GpuCausalDiscoveryModule.cs
?   ?
?   ??? Gauge/                          # ? Gauge Invariants
?   ?   ??? GaugeShaders.cs
?   ?   ??? GpuGaugeEngine.cs
?   ?   ??? GpuGaugeInvariantModule.cs
?   ?
?   ??? HeavyMass/                      # ? Heavy Mass
?   ?   ??? HeavyMassShaders.cs
?   ?   ??? GpuHeavyMassEngine.cs
?   ?   ??? GpuHeavyMassModule.cs
?   ?
?   ??? Hybrid/                         # ? Hybrid Pipeline
?       ??? RecommendationBuffer.cs
?       ??? HybridPipelineCoordinator.cs
?
??? Core/Infrastructure/
    ??? RQGraph.HorizonIntegration.cs   # ? Расширение RQGraph

Tests/RqSimGPUCPUTests/Tests/CSR/
??? GpuCsrModules_Integration_Tests.cs  # ? Integration Tests
```

---

## ?? Использование

### Black Hole Horizon Detection

```csharp
var module = new GpuBlackHoleHorizonModule
{
    DensityThreshold = 10.0,
    MinMassThreshold = 0.01,
    EnableEvaporation = true
};
pipeline.AddModule(module);
```

### Causal Discovery

```csharp
var module = new GpuCausalDiscoveryModule
{
    MaxCausalDepth = 10,
    SpeedOfLight = 1.0
};
pipeline.AddModule(module);

// Query
int coneSize = module.ComputeCausalCone(42, 5);
bool connected = module.AreCausallyConnected(10, 50, 5.0);
```

### Gauge Invariant Check

```csharp
var module = new GpuGaugeInvariantModule
{
    ViolationWarningThreshold = 0.01
};
pipeline.AddModule(module);

// Results
double chern = module.TotalTopologicalCharge;
bool valid = module.IsGaugeValid;
```

### Heavy Mass Analysis

```csharp
var module = new GpuHeavyMassModule
{
    HeavyMassThreshold = 1.0
};
pipeline.AddModule(module);

// Results
var heavyNodes = module.GetHeavyNodes();
double totalMass = module.TotalCorrelationMass;
```

### Hybrid Pipeline

```csharp
var coordinator = new HybridPipelineCoordinator
{
    MaxRecommendationsPerStep = 100,
    AutoRebuildCsr = true
};
coordinator.Initialize(graph);
coordinator.RegisterModule(hybridModule);

// After GPU step
coordinator.ProcessRecommendations(graph);
```

---

## ?? Ожидаемые улучшения производительности

| Модуль | CPU | GPU (ожидаемо) | Speedup |
|--------|-----|----------------|---------|
| Horizon Detection | O(N?) | O(N) parallel | 10-100x |
| Causal BFS | O(N log N) | O(D ? N/32) | 5-50x |
| Wilson Loops | O(N?) | O(triangles) | 50-500x |
| Heavy Mass | O(N?) | O(N) parallel | 10-100x |

---

## ?? Checklist - ВСЕ ЗАВЕРШЕНО

- [x] **High Priority:** `GpuBlackHoleHorizonModule` (GPU) ?
- [x] **Medium Priority:** `GpuCausalDiscoveryModule` (GPU) ?
- [x] **High Technical Value:** `GpuGaugeInvariantChecker` (GPU) ?
- [x] **Medium Priority:** `GpuHeavyMassAnalyzer` (GPU) ?
- [x] **Architecture:** Hybrid Pipeline Coordinator ?
- [x] **Testing:** Integration tests for all GPU modules ?
