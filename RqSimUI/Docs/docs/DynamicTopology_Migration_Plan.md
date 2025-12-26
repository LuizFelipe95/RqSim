# Dynamic Topology — Migration & Integration Plan

> Status: **Completed v3** — Added Conservation Kernel with Science Mode integration for energy/gauge conservation during topology changes.

Документ описывает следующие шаги, проверку интеграции и детализированные задачи для завершения миграции динамической топологии на чисто GPU-пайплайн, а также интеграцию как плагинов в UniPipeline/UI.

Содержание:
- Краткое резюме
- Текущее состояние интеграции (результаты быстрого аудита)
- Приоритетный чек-лист задач (технические и интеграционные)
- Детализированные задачи (шаги реализации)
- Тесты и критерии приёмки
- Риски и рекомендации
- **Новые возможности (v2)**
- **Conservation Kernel & Science Mode (v3)** — NEW

---

## 1. Краткое резюме

Цель: полностью убрать крупные блокирующие CPU-копирования при реконструкции CSR-структуры и переместить все этапы (mark, degree, scan, scatter, swap) на GPU. При этом обеспечить корректность (сохранение тяжёлых рёбер, симметрию, валидность CSR) и интеграцию в существующую систему плагинов и UI (UniPipeline).

**Реализовано:**
- GPU шейдеры версии Blelloch/KoggeStone и scatter/compact (файлы: `PrefixScanShaders.cs`, `StreamCompactionKernel.cs`, `ScatterRebuildShaders.cs`).
- Класс `GpuStreamCompactionEngine` с методами `CompactTopologyFullGpu` и BlellochScan.
- В `GpuDynamicTopologyEngine` заменён CPU-процесс на вызовы `GpuStreamCompactionEngine.CompactTopologyFullGpu` и добавлены проверки `VerifyCsrKernel`, `ReplaceGpuBuffers` в `DynamicCsrTopology`.
- **GPU Top-K selection**: три стратегии (`CpuOnly`, `BlockTopM`, `ParallelBlockTopM`) с автовыбором по размеру графа.
- **Метрики производительности**: `TopKSelectionMetrics` с детальной разбивкой времени GPU/CPU.
- **Unit tests**: 4 теста для проверки паритета CPU vs GPU и корректности метрик.
- **NEW (v3)**: Conservation Kernel для сохранения энергии/заряда при удалении рёбер.
- **NEW (v3)**: Интеграция с StrongScienceProfile для валидации законов сохранения.

---

## 2. Быстрый аудит интеграции (файловый обзор)

Найденные места/модули:
- `RqSim.PluginManager.UI\IncludedPlugins\IncludedPluginsRegistry.cs` — реестр включённых плагинов содержит GPU-модули: `GravityShaderFormanModule`, `GpuMCMCEngineModule`, `TopologicalInvariantsGpuModule`, `PotentialLinkActivatorGpuModule`, `GaussLawMonitorGpuModule`, `RenderDataMapperGpuModule`.
- `RqSim.PluginManager.UI\IncludedPlugins\GPUOptimizedCSR\PotentialLinkActivatorGpuModule.cs` — пример GPU модуля интегрирован в pipeline и UI.
- `RqSimGraphEngine\RQSimulation\GPUCompressedSparseRow\GpuStreamCompactionEngine.cs` — реализует GPU-скан/компактацию. **Добавлены**:
  - `SelectTopK()` — унифицированный API с выбором стратегии
  - `GetTopKIndicesBlockTopM()` — per-block top-M selection
  - `GetTopKIndicesParallel()` — parallel threads per block
  - `TopKSelectionMetrics` — класс метрик производительности
  - `LastTopKMetrics` — свойство для доступа к последним метрикам
- `RqSimGraphEngine\RQSimulation\GPUCompressedSparseRow\DynamicTopology\ScatterRebuildShaders.cs` — содержит `VerifyCsrKernel` и scatter-шейдеры.
- `RqSimUI\Forms\UniPipelineForm\Form_Main.cs` — UI: комбобоксы для выбора GPU-движка и TopologyMode уже присутствуют и применяют настройки в SimAPI (`ApplyTopologyModeToSimApi`). Добавлена панель логов топологии и кнопка Force CPU Fallback.

Вывод: базовая интеграция присутствует — плагины и UI уже рассчитаны на GPU модули/режимы. ?

---

## 3. Приоритетный чек-лист задач

1) ? Проверки/включение валидаторов
- Запуск `VerifyCsrKernel` после компакции (выполнено в `GpuDynamicTopologyEngine`) и обработка ошибок
- Проверка, что `VerifyCsrKernel` действительно экспортирован (файл `ScatterRebuildShaders.cs`) — выполнено

2) ? Swap / Ownership
- Добавить/документировать контракт `ReplaceGpuBuffers` в `DynamicCsrTopology` (выполнено)
- Убедиться, что после передачи буферов вызывающая сторона не вызывает `Dispose` на них

3) ? Top-K защита
- Краткосрочный: CPU QuickSelect fallback (`GetTopKIndices`) — реализован
- Среднесрочный: GPU-assisted two-stage Top-K (`GetTopKIndicesTwoStage`) — реализован
- **Реализовано**: Parallel Block Top-M (`GetTopKIndicesParallel`) с настраиваемым threadsPerBlock и M
- **Реализовано**: Unified API `SelectTopK()` с автовыбором стратегии

4) ? UI / Pipeline интеграция
- Убедиться, что все включённые плагины, которые используют новые GPU-топологии (TopologyMode), доступны в `IncludedPluginsRegistry` и корректно регистрируются в pipeline
- В UI: добавлена визуализация логов топологии и кнопка «Force CPU fallback» (UniPipeline tab)

5) ? Тесты
- Unit: CPU vs GPU parity — **4 новых теста** в `DynamicTopology_Tests.cs`:
  - `TopKSelection_CpuVsBlockTopM_ProducesSameResults`
  - `TopKSelection_CpuVsParallelBlockTopM_ProducesSameResults`
  - `TopKSelection_AutoStrategy_SelectsAppropriateMethod`
  - `TopKSelection_MetricsArePopulated`
- Integration: серия rebuild шагов — проверка отсутствия утечек буферов и корректности версий топологии

---

## 4. Детализированные задачи (с шагами)

### A. ? GPU top-K (ЗАВЕРШЕНО)

**Реализованные компоненты:**
1. **Шейдеры** (`PrefixScanShaders.cs`):
   - `TopBlockMaxKernel` — per-block maximum selection
   - `TopBlockTopMKernel` — per-block top-M selection (M up to 8)
   - `ParallelBlockTopMKernel` — parallel threads per block top-M

2. **API** (`GpuStreamCompactionEngine`):
   ```csharp
   // Unified API with strategy selection
   int[] SelectTopK(CsrTopology topology, int k, 
       TopKSelectionStrategy strategy = TopKSelectionStrategy.Auto, 
       int M = 4);
   
   // Individual methods
   int[] GetTopKIndices(CsrTopology topology, int k);           // CPU QuickSelect
   int[] GetTopKIndicesTwoStage(CsrTopology topology, int k);   // GPU block-max + CPU
   int[] GetTopKIndicesBlockTopM(CsrTopology topology, int k, int M);  // GPU per-block top-M
   int[] GetTopKIndicesParallel(CsrTopology topology, int k, int threadsPerBlock, int M);  // Parallel GPU
   ```

3. **Стратегии** (`TopKSelectionStrategy`):
   - `CpuOnly` — pure CPU QuickSelect (best for small graphs <10k edges)
   - `BlockTopM` — GPU per-block top-M + CPU refine (medium graphs 10k-100k)
   - `ParallelBlockTopM` — parallel GPU threads + CPU refine (large graphs >100k)
   - `Auto` — automatic selection based on nnz

4. **Метрики** (`TopKSelectionMetrics`):
   - `UsedStrategy` — actual strategy used
   - `TotalTimeMs`, `GpuTimeMs`, `CpuRefineTimeMs` — timing breakdown
   - `GpuCandidateCount`, `ResultCount`, `SourceNnz`, `RequestedK`
   - `UsedFallback`, `ErrorMessage` — fallback tracking

5. **Конфигурация** (`DynamicTopologyConfig`):
   - `TopKStrategy` — default strategy (Auto)
   - `TopKLocalM` — M parameter for GPU selection (default 4)

### B. ? VerifyCsr integration (ЗАВЕРШЕНО)

Шаги:
1. В `GpuDynamicTopologyEngine` непосредственно после `CompactTopologyFullGpu` запускать `VerifyCsrKernel` (уже сделано).
2. Собирать `errorFlags` и в случае ошибки логировать и переключать режим восстановления.
3. UI: при ошибке отображать диалог/лог в `UniPipeline`.

### C. ? ReplaceGpuBuffers контракт и lifecycle (ЗАВЕРШЕНО)

Контракт задокументирован. Ownership передаётся при вызове `ReplaceGpuBuffers`.

### D. ? UI / Pipeline module integration (ЗАВЕРШЕНО)

UI элементы добавлены в UniPipeline tab.

### E. ? Тесты (ЗАВЕРШЕНО)

Добавлены в `Tests\RqSimGPUCPUTests\Tests\DynamicTopology\DynamicTopology_Tests.cs`:
- `TopKSelection_CpuVsBlockTopM_ProducesSameResults` — проверка паритета CPU vs BlockTopM
- `TopKSelection_CpuVsParallelBlockTopM_ProducesSameResults` — проверка паритета CPU vs ParallelBlockTopM
- `TopKSelection_AutoStrategy_SelectsAppropriateMethod` — проверка автовыбора стратегии
- `TopKSelection_MetricsArePopulated` — проверка заполнения метрик

---

## 5. Тесты и критерии приёмки

- ? Функциональная корректность: GPU-результат == CPU-референс на наборе тестов (парность, симметрия, sum(weights) invariants)
- ? Надёжность: VerifyCsrKernel не возвращает ошибки на корректных данных
- ? Производительность: шаг топологии значительно быстрее CPU (цель < 5 ms для целевых конфигураций) — требует профилирования на реальных данных
- ? Ресурсы: нет утечек GPU-буферов при многократных rebuilds

---

## 6. Риски

- GPU double-precision support отсутствует на некоторых устройствах (шэйдеры помечены `RequiresDoublePrecisionSupport`) — нужно fallback или ранняя проверка.
- ? Top-K на CPU создаёт копию весов — **решено**: GPU-версии минимизируют CPU overhead.
- Swap-ownership model может конфликтовать с местами, ожидающими `UploadToGpu()`.

---

## 7. Новые возможности (v2)

### 7.1 Configurable Top-K Selection

В `DynamicTopologyConfig` добавлены параметры:
```csharp
TopKStrategy = TopKSelectionStrategy.Auto;  // Auto|CpuOnly|BlockTopM|ParallelBlockTopM
TopKLocalM = 4;  // 1-8, количество top-элементов на блок/поток
```

### 7.2 Performance Metrics

`DynamicTopologyStats` расширен полями:
- `TopKSelectionTimeMs` — время выбора top-K
- `TopKGpuCandidateCount` — количество GPU-кандидатов
- `TopKSourceNnz` — исходное количество рёбер
- `TopKUsedFallback` — использован ли fallback
- `TopKErrorMessage` — сообщение об ошибке

### 7.3 Logging

Debug-логирование в `SelectTopK`:
```
[TopK] Strategy=BlockTopM, k=100, nnz=50000, candidates=512, result=100, gpu=1.23ms, cpu=0.45ms, total=1.68ms
```

---

## 8. Резюме

**Статус: ЗАВЕРШЕНО** ?

Все основные задачи миграции GPU Top-K selection выполнены:
- ? Parallel Top-M shader с настраиваемым параллелизмом
- ? Unified `SelectTopK` API с автовыбором стратегии
- ? Интеграция в `MarkLowWeightEdges` с безопасным CPU fallback
- ? Метрики производительности для отладки и оптимизации
- ? Unit tests для проверки паритета CPU vs GPU

**Следующие шаги (опционально):**
- Performance profiling на больших графах (>1M edges)
- Дополнительная оптимизация GPU kernel launch overhead
- Добавление histogram-based top-K для extreme scales

---

## 9. Conservation Kernel & Science Mode (v3) — NEW

### 9.1 Обзор

При удалении рёбер в динамической топологии их физическое содержимое (метрическая энергия, калибровочный поток) должно быть сохранено путём переноса на узлы. Это реализовано через новую фазу CONSERVE в пайплайне.

### 9.2 Pipeline Integration

Обновлённый пайплайн `GpuDynamicTopologyEngine`:

```
1. PROPOSAL  ? Collect edge addition/deletion candidates
2. MARK      ? Identify edges for deletion (weight < threshold)
3. CONSERVE  ? Transfer dying edge content to nodes (NEW!)
4. DEGREE    ? Compute new degrees for all nodes
5. SCAN      ? Parallel prefix sum for new RowOffsets
6. SCATTER   ? Rebuild ColIndices and Weights arrays
7. SWAP      ? Replace old buffers with new ones
```

### 9.3 Science Mode Integration

```csharp
// Создание движка с научным профилем
var profile = StrictScienceProfile.CreatePlanckScale("TopologyExperiment_001");
var engine = new GpuDynamicTopologyEngine(device, profile);

// Включение консервации
engine.Config.EnableConservation = true;
engine.Config.ConservationTolerance = 1e-8;

// При нарушении консервации в Science mode — исключение
try 
{
    var newTopology = engine.EvolveTopology(topology, masses);
}
catch (ScientificMalpracticeException ex) when (ex.MalpracticeType == ScientificMalpracticeType.EnergyConservationViolation)
{
    // Нарушение закона сохранения энергии
    Console.WriteLine($"Conservation violated: {ex.Message}");
}
```

### 9.4 Physical Principles (RQ-Hypothesis)

Когда ребро "умирает" (удаляется), его физическое содержимое переносится:

1. **Метрическая энергия ? Масса узлов**:
   ```
   E_edge = EnergyConversionFactor ? weight
   m_A += E_edge / 2
   m_B += E_edge / 2
   ```

2. **Калибровочный поток ? Спинор/Заряд узлов**:
   ```
   ?_edge = FluxConversionFactor ? gaugePhase
   spin_A += ?_edge    // Сохранение заряда
   spin_B -= ?_edge    // Противоположный знак
   ```

3. **Валидация (Science Mode)**:
   ```
   |E_before - E_transferred| ? ConservationTolerance
   ```

### 9.5 Configuration Options

Добавлены в `DynamicTopologyConfig`:

| Параметр | Тип | Default | Описание |
|----------|-----|---------|----------|
| `EnableConservation` | bool | false | Включить фазу CONSERVE |
| `ConservationTolerance` | double | 1e-6 | Допуск для валидации |
| `EnergyConversionFactor` | double | 1.0 | E = factor ? weight |
| `FluxConversionFactor` | double | 1.0 | ? = factor ? gaugePhase |

В Science Mode (`StrictScienceProfile`):
- `EnergyConversionFactor` = `Constants.GravitationalCoupling`
- `FluxConversionFactor` = `Constants.FineStructureConstant`

### 9.6 Statistics

Новые поля в `DynamicTopologyStats`:

```csharp
public bool EnergyConserved { get; set; }      // Успешна ли консервация
public double ConservationError { get; set; }  // |E_before - E_transferred|
public double ConservationTimeMs { get; set; } // Время фазы CONSERVE
```

Детальная статистика в `ConservationStats`:

```csharp
public double EnergyBefore { get; set; }       // Сумма энергий умирающих рёбер
public double EnergyTransferred { get; set; }  // Перенесённая энергия
public int DyingEdgeCount { get; set; }        // Количество умирающих рёбер
```

### 9.7 Implementation Notes

**Текущая реализация (CPU)**:
- Используется CPU для double-precision вычислений
- Atomic operations для double не поддерживаются напрямую в HLSL
- Достаточно быстро для типичных размеров графов (<100k edges)

**GPU версия (будущее)**:
- Использовать fixed-point арифметику (int atomics)
- Или CAS-loop для double atomics
- Только когда CPU станет узким местом

### 9.8 Files

| Файл | Описание |
|------|----------|
| `ConservationShaders.cs` | GPU шейдеры (counting, summing) |
| `NodeStateGpu.cs` | Структуры `NodeConservationState`, `EdgeGaugeStateGpu` |
| `GpuDynamicTopologyEngine.cs` | Интеграция CONSERVE фазы |
| `DynamicTopologyConfig` | Параметры консервации |
| `StrictScienceProfile` | Валидация в Science mode |

---

## 10. Резюме v3

**Статус: ЗАВЕРШЕНО** ?

Все задачи Conservation Kernel выполнены:
- ? ConservationShaders.cs с поддержкой GPU counting
- ? NodeConservationState, EdgeGaugeStateGpu структуры
- ? CONSERVE фаза в GpuDynamicTopologyEngine
- ? Интеграция с StrongScienceProfile
- ? ConservationStats для отслеживания
- ? ScientificMalpracticeException при нарушениях

---

Файл обновлён 2025-12-25.
