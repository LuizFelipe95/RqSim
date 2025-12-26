# Dynamic Topology — Migration & Integration Plan

> Status: In progress — implementing GPU Top-K selection. Current step: added block-local Top-M shader and GPU-assisted two-stage selection; next: integrated block-top-M selection into MarkLowWeightEdges with safe CPU fallback and upgraded shaders for parallel local Top-M.

Документ описывает следующие шаги, проверку интеграции и детализированные задачи для завершения миграции динамической топологии на чисто GPU-пайплайн, а также интеграцию как плагинов в UniPipeline/UI.

Содержание:
- Краткое резюме
- Текущее состояние интеграции (результаты быстрого аудита)
- Приоритетный чек-лист задач (технические и интеграционные)
- Детализированные задачи (шаги реализации)
- Тесты и критерии приёмки
- Риски и рекомендации

---

## 1. Краткое резюме

Цель: полностью убрать крупные блокирующие CPU-копирования при реконструкции CSR-структуры и переместить все этапы (mark, degree, scan, scatter, swap) на GPU. При этом обеспечить корректность (сохранение тяжёлых рёбер, симметрию, валидность CSR) и интеграцию в существующую систему плагинов и UI (UniPipeline).

Ранее реализованы: 
- GPU шейдеры версии Blelloch/KoggeStone и scatter/compact (файлы: `PrefixScanShaders.cs`, `StreamCompactionKernel.cs`, `ScatterRebuildShaders.cs`).
- Класс `GpuStreamCompactionEngine` с методами `CompactTopologyFullGpu` и BlellochScan.
- В `GpuDynamicTopologyEngine` заменён CPU-процесс на вызовы `GpuStreamCompactionEngine.CompactTopologyFullGpu` и добавлены проверки `VerifyCsrKernel`, `ReplaceGpuBuffers` в `DynamicCsrTopology` и top-K защита на CPU.

---

## 2. Быстрый аудит интеграции (файловый обзор)

Найденные места/модули:
- `RqSim.PluginManager.UI\IncludedPlugins\IncludedPluginsRegistry.cs` — реестр включённых плагинов содержит GPU-модули: `GravityShaderFormanModule`, `GpuMCMCEngineModule`, `TopologicalInvariantsGpuModule`, `PotentialLinkActivatorGpuModule`, `GaussLawMonitorGpuModule`, `RenderDataMapperGpuModule`.
- `RqSim.PluginManager.UI\IncludedPlugins\GPUOptimizedCSR\PotentialLinkActivatorGpuModule.cs` — пример GPU модуля интегрирован в pipeline и UI.
- `RqSimGraphEngine\RQSimulation\GPUCompressedSparseRow\GpuStreamCompactionEngine.cs` — реализует GPU-скан/компактацию. Добавлена CPU QuickSelect fallback `GetTopKIndices` и GPU-assisted two-stage `GetTopKIndicesTwoStage`.
- `RqSimGraphEngine\RQSimulation\GPUCompressedSparseRow\DynamicTopology\ScatterRebuildShaders.cs` — содержит `VerifyCsrKernel` и scatter-шейдеры.
- `RqSimUI\Forms\UniPipelineForm\Form_Main.cs` — UI: комбобоксы для выбора GPU-движка и TopologyMode уже присутствуют и применяют настройки в SimAPI (`ApplyTopologyModeToSimApi`). Добавлена панель логов топологии и кнопка Force CPU Fallback.

Вывод: базовая интеграция присутствует — плагины и UI уже рассчитаны на GPU модули/режимы. Однако есть технические места, требующие доработки и формализации (см. задачи).

---

## 3. Приоритетный чек-лист задач

1) Проверки/включение валидаторов
- Запуск `VerifyCsrKernel` после компакции (выполнено в `GpuDynamicTopologyEngine`) и обработка ошибок
- Проверка, что `VerifyCsrKernel` действительно экспортирован (файл `ScatterRebuildShaders.cs`) — выполнено

2) Swap / Ownership
- Добавить/документировать контракт `ReplaceGpuBuffers` в `DynamicCsrTopology` (выполнено)
- Убедиться, что после передачи буферов вызывающая сторона не вызывает `Dispose` на них

3) Top-K защита
- Краткосрочный: CPU QuickSelect fallback (`GetTopKIndices`) — реализован
- Среднесрочный: GPU-assisted two-stage Top-K (`GetTopKIndicesTwoStage`) — реализована первая версия (TopBlockMaxKernel + CPU refine)
- Долгосрочный: полноценный GPU-only Top-K (partial sort / block-level heaps + reduce)

4) UI / Pipeline интеграция
- Убедиться, что все включённые плагины, которые используют новые GPU-топологии (TopologyMode), доступны в `IncludedPluginsRegistry` и корректно регистрируются в pipeline
- В UI: добавлена визуализация логов топологии и кнопка «Force CPU fallback» (UniPipeline tab)

5) Тесты
- Unit: CPU vs GPU parity (краевые веса, симметрия, суммарная энергия)
- Integration: серия rebuild шагов — проверка отсутствия утечек буферов и корректности версий топологии
- Performance tests: время шага топологии на реальных данных

---

## 4. Детализированные задачи (с шагами)

### A. Полностью GPU top-K (опционально — сложная задача)
Цель: реализовать выбор k самых больших весов без скачивания всего буфера на CPU.

Шаги:
1. Исследовать доступные алгоритмы в шейдерах: selection by histogram/radix or partial sort using block-level top-K and reduce-of-blocks.
2. Добавить новый шейдер `TopKSelectionKernel`:
   - Фаза 1: разбить массив на блоки, в каждом блоке выбрать локальный top-K (min-heap в shared memory simulated via registers), записать локальные top-K в промежуточный буфер.
   - Фаза 2: редуцировать локальные топ-K в глобальный top-K (cpu или второй шейдер).
3. API: `GpuStreamCompactionEngine.GetTopKIndices(ReadOnlyBuffer<double> weights, int k)` возвращает `ReadOnlyBuffer<int>` с индексами.
4. Заменить CPU-пассинг в `MarkLowWeightEdges` вызовом GPU TopK.

Критерии приёмки:
- Результат совпадает с CPU top-K для тестовых массивов.
- Уменьшение времени при больших nnz (профилировать).

Прогресс:
- Создан шейдер `TopBlockMaxKernel` (первичная стадия — per-block max index).
- Добавлен `GetTopKIndicesTwoStage` в `GpuStreamCompactionEngine`: выполняет TopBlockMaxKernel, копирует кандидатов и уточняет top-K на CPU через QuickSelect.

Оценка: 2–5 дней (исходя из тестирования на разных устройствах).


### B. VerifyCsr integration (короткий и критичный)
Шаги:
1. В `GpuDynamicTopologyEngine` непосредственно после `CompactTopologyFullGpu` запускать `VerifyCsrKernel` (уже сделано).
2. Собирать `errorFlags` и в случае ошибки логировать и переключать режим восстановления (например, откат на старую топологию или запуск CPU-рефита как fallback).
3. UI: при ошибке отображать диалог/лог в `UniPipeline` (в `Form_Main`) и опцию «Rebuild with CPU fallback».

Критерии приёмки:
- Любая проблема CSR ставит шаг в failed и выдаёт читаемую трассу ошибок.

Оценка: 1 день.


### C. ReplaceGpuBuffers контракт и lifecycle
Шаги:
1. Документировать контракт: кто владеет переданными ReadOnlyBuffer-объектами после `ReplaceGpuBuffers`.
2. Проверить все вызовы `UploadToGpu()` и места, где ранее ожидалось, что GPU буферы будут пересоздаваться; обновить их при необходимости.
3. Добавить `Dispose` и defensive checks (например, флаги `bufferReplaced`), логгирование при попытках повторного освобождения чужого буфера.

Критерии приёмки:
- Нет двойного `Dispose` / use-after-dispose.
- Версии топологии корректно инкрементируются.

Оценка: 0.5–1 день.


### D. UI / Pipeline module integration
Шаги:
1. В `IncludedPluginsRegistry` проверить, что все модули для топологии (GPU) присутствуют — выполнено.
2. В `Form_Main.InitializeUniPipelineTab` убедиться, что элементы выбора `TopologyMode`/`GpuEngine` корректно отображают текущие опции и применяют их к SimAPI — контролируется `ApplyTopologyModeToSimApi` и `ApplyGpuEngineSelectionToSimApi` (в коде уже есть).
3. Добавить в UI: индикатор состояния компакта (OK/Fail), кнопку «Force CPU fallback», лог ошибок компакта (реализовано в UniPipeline tab).

Критерии приёмки:
- UI отображает режимы и позволяет переключать их, SimAPI получает значение.
- При ошибке компакта UI показывает опцию восстановить CPU-рефит.

Оценка: 1 день.


### E. Тесты (обязательные)
- Unit parity tests: малые графы (N<=32), edge cases (weights == threshold), top-K preservation
- Integration: несколько итераций EvolveTopology на случайных графах и фиксация версий/валидности
- Performance: измерение времени шага и профилирование времени Alloc/Dispatch/Copy

Оценка: 1–2 дня в зависимости от покрытия.

---

## 5. Тесты и критерии приёмки

- Функциональная корректность: GPU-результат == CPU-референс на наборе тестов (парность, симметрия, sum(weights) invariants)
- Надёжность: VerifyCsrKernel не возвращает ошибки на корректных данных
- Производительность: шаг топологии значительно быстрее CPU (цель < 5 ms для целевых конфигураций)
- Ресурсы: нет утечек GPU-буферов при многократных rebuilds

---

## 6. Риски

- GPU double-precision support отсутствует на некоторых устройствах (шэйдеры помечены `RequiresDoublePrecisionSupport`) — нужно fallback или ранняя проверка.
- Top-K на CPU создаёт копию весов; при больших nnz это дорого — рекомендуется реализовать GPU-версию (в процессе).
- Swap-ownership model может конфликтовать с местами, ожидающими `UploadToGpu()`.

---

## 7. Резюме: recommended immediate tasks (с приоритетами)

P0 (Critical)
- Добавить UI-лог / fail handling для `VerifyCsrKernel` и CPU-fallback.
- Завершить `ReplaceGpuBuffers` контракт и документировать ownership.

P1 (High)
- Реализовать протестированные unit/integration тесты (паритет CPU/GPU).
- Добавить VerifyCsrKernel вызов во всех местах, где выполняется полно-GPU компактация.

P2 (Medium)
- Завершить GPU top-K выборку (long-term).
- Добавить опциональные защитные механизмы (e.g., protect top-% by weight per-node).

---

Файл обновлён автоматически агентом. Если требуется, могу:
- продолжить реализацию GPU top-K (фулл-шейдерная версия),
- добавить UI-элементы в `Form_Main` (лог/кнопки) и связать с SimAPI,
- подготовить набор unit/integration тестов.
