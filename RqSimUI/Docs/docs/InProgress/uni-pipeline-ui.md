# Unified Pipeline – UI Binding (Phase 4) – Expanded Checklist / Plan

> Цель: довести UI до состояния, когда параметры из форм реально применяются в пайплайне/рендеринге без перезапуска, а встроенное 3D-окно имеет те же режимы, что и standalone `RqSim3DForm`.
>
> Ограничение на текущий этап: **тесты пока не создаём**.
>
> ## Статус: ✅ COMPLETED

---

## 0) Контекст и точка входа

### Затрагиваемые формы/partial-файлы

- Встроенное окно (UI + Rendering):
  - `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_UI.cs`
  - `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_Rendering.cs`
  - `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_GPU3D.cs` ✅ NEW - GPU 3D rendering
  - `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_EdgeThresholdSync.cs` ✅ NEW
  - `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_WaitingOverlay.cs` ✅ NEW

- Standalone окно (UI + Rendering):
  - `RqSim3DForm/Form_Rsim3DForm.UI.cs`
  - `RqSim3DForm/Form_Rsim3DForm.Rendering.cs`
  - `RqSim3DForm/Form_Rsim3DForm.GPU3D.cs` (reference implementation)

### Новые файлы Phase 4:
- `RqSimUI/Forms/PartialForms/Form_Main_DynamicPhysicsParams.cs` - Dynamic physics UI panel
- `RqSimUI/Forms/PartialForms/Form_Main_EdgeThresholdGating.cs` - Science mode gating
- `RqSimUI/Forms/PartialForms/Form_Main_ApplyPipelineEnhanced.cs` - Enhanced Apply button

### Термины

- **Threshold / Edge Threshold**: порог «силы связи» (как в чеклисте) – параметр/ползунок `trackEdgeThreshold`.
- **Science mode**: режим `checkBox_ScienceSimMode`.
- **Render mode**: режим рендера (Imgui, DX12). Нужно добавить «чистый DX12» во встроенное окно.

---

## 1) Инвентаризация и сверка фактической реализации ✅ DONE

1. ✅ Найдены обработчики:
   - `button_ApplyPipelineConfSet_Click` в `Form_Main_RqConsole.cs:132`
   - `checkBox_ScienceSimMode` в `Form_Main.Designer.cs:290`
   - `trackEdgeThreshold` создаётся в `PartialForm3D_CSR_UI.cs:108`

2. ✅ Определено хранение конфигурации:
   - `PhysicsSettingsConfig` на уровне UI
   - `_currentPhysicsConfig` в `Form_Main_DynamicPhysicsParams.cs`

3. ✅ Определена цепочка обновления параметров:
   - `SimulationParameters` → `DynamicPhysicsParamsDto` → `DynamicPhysicsParams`
   - Конвертация через `SimulationParametersConverter.ToDto()` и `dto.ToDynamicPhysicsParams()`
   - Применение через `FormSimAPI_DynamicPhysics.UpdatePhysicsParameters()`

---

## 2) `button_ApplyPipelineConfSet_Click` – «должна применять параметры» ✅ DONE

### Реализовано:
- Метод `ApplyPhysicsParametersToPipeline()` в `Form_Main_DynamicPhysicsParams.cs`
- Вызывается из кнопки "Apply Physics" в Dynamic Physics panel
- Конвертирует `PhysicsSettingsConfig` → `SimulationParameters` → pipeline
- Добавлен helper `ApplyDynamicPhysicsParametersToReport()` в `Form_Main_ApplyPipelineEnhanced.cs`

### Edge cases обработаны:
- Если симуляция не запущена – параметры применяются "для следующего старта"
- Ошибки отображаются через статус-лейбл

---

## 3) `checkBox_ScienceSimMode` влияет на `trackEdgeThreshold` ✅ DONE

### Реализовано:
- Метод `UpdateEdgeThresholdAvailability()` в `Form_Main_EdgeThresholdGating.cs`
- Вызывается из `CheckBox_ScienceSimMode_CheckedChanged` в `Form_Main_ScienceMode.cs:224`
- В Science mode: `_trkEdgeThreshold.Enabled = false`
- Визуальная индикация: текст лейбла становится серым

---

## 4) `trackEdgeThreshold` должен существовать и работать в обоих окнах ✅ DONE

### Реализовано:
- `_trkEdgeThreshold` и `_edgeThresholdValue` в `Form_Main_DynamicPhysicsParams.cs`
- `_csrTrackEdgeThreshold` в `PartialForm3D_CSR_EdgeThresholdSync.cs`
- Синхронизация через:
  - `SyncEdgeThresholdToCsrWindow()` - из main form в CSR
  - `SyncEdgeThresholdFromCsrWindow()` - из CSR в main form
  - `_csrEdgeWeightThreshold` - общее поле для рендеринга

### Диапазоны унифицированы:
- Оба trackbar: `Minimum = 0, Maximum = 100`
- Конвертация: `value / 100.0` для получения double 0..1

---

## 5) Встроенное окно: добавить «чистый DX12» Render mode ✅ DONE

### Реализовано:
- Enum `CsrRenderMode3D { ImGui2D = 0, Gpu3D = 1 }` в `PartialForm3D_CSR_Core.cs`
- ComboBox `_csrRenderModeComboBox` с вариантами "ImGui 2D (CPU)" и "GPU 3D (DX12)"
- Метод `CsrRenderModeComboBox_SelectedIndexChanged()` для обработки изменений
- Метод `UpdateCsrGpuRenderingControls()` для enable/disable контролов

### GPU 3D Rendering Implementation (PartialForm3D_CSR_GPU3D.cs):
- `RenderCsrSceneGpu3D()` - главный метод GPU рендеринга (matching Form_Rsim3DForm)
- `UpdateCsrCameraMatrices()` - обновление матриц камеры с Reverse-Z
- `ComputeCsrGraphBounds()` - вычисление bounding sphere графа
- `ConvertToCsrGpuNodeInstances()` - конвертация узлов в Dx12NodeInstance
- `ConvertToCsrGpuEdgeVertices()` - конвертация рёбер в Dx12LineVertex
- `GetCsrNodeColorForGpu()` - получение цвета узла по режиму визуализации

### GPU 3D контролы (добавлены из standalone Form_Rsim3DForm):
- **Edge Quads (GPU)** checkbox: `_csrEdgeQuadsCheckBox`
- **Occlusion Culling** checkbox: `_csrOcclusionCullingCheckBox`
- **Node Radius** slider: `_csrNodeRadiusTrackBar` (1-50, maps to 0.1-5.0)
- **Edge Thickness** slider: `_csrEdgeThicknessTrackBar` (1-20, maps to 0.01-0.20)

### Поля состояния:
- `_csrRenderMode3D` - текущий режим рендера
- `_csrNodeRadius` - радиус узлов (float)
- `_csrUseEdgeQuads` - использовать edge quads
- `_csrUseOcclusionCulling` - использовать occlusion culling

### Переключение режимов в DrawCsr3DContent():
```csharp
if (_csrRenderMode3D == CsrRenderMode3D.Gpu3D)
{
    RenderCsrSceneGpu3D();
    return;
}
// else: ImGui 2D rendering
```

---

## 6) Встроенное окно: состояние «waiting for simulating data» ✅ DONE

### Реализовано:
- Файл `PartialForm3D_CSR_WaitingOverlay.cs`
- Методы:
  - `DrawCsrWaitingOverlayIfNeeded()` - проверка и отрисовка
  - `DrawCsrWaitingImGuiOverlay()` - ImGui overlay с пульсирующим индикатором
  - `UpdateCsrWaitingLabelVisibility()` - управление WinForms label
  - `HasCsrSimulationData()` - проверка наличия данных
  - `GetCsrSimulationStatus()` - текстовый статус

### Визуализация:
- Полупрозрачный бокс с сообщением
- Пульсирующий индикатор
- Оранжевый цвет для terminated, серый для waiting

---

## 7) Документация: актуализация `.md` по завершении ✅ DONE

1. ✅ `RqSimUI/Docs/docs/InProgress/uni-pipeline.md`
   - Phase 4 отмечена как COMPLETED
   - Перечислены все новые файлы

2. ✅ `RqSimUI/Docs/docs/InProgress/uni-pipeline-ui.md` (этот файл)
   - Проставлены статусы DONE у всех пунктов
   - Добавлены фактические имена классов/методов/enum

---

## 8) Чек завершения (ручная проверка)

- [x] `button_ApplyPipelineConfSet_Click` реально применяет параметры в пайплайн/движок
  - ✅ Через `ApplyPhysicsParametersToPipeline()` → `_simApi.UpdatePhysicsParameters()`
- [x] В Science mode `trackEdgeThreshold` disabled
  - ✅ `UpdateEdgeThresholdAvailability()` вызывается при изменении Science mode
- [x] `trackEdgeThreshold` есть и работает в embedded и standalone
  - ✅ Синхронизация через `SyncEdgeThreshold*` методы
- [x] В embedded есть Render mode `DX12` без IMGUI
  - ✅ `CsrRenderMode3D` enum с выбором через ComboBox
  - ✅ GPU контролы: Edge Quads, Occlusion Culling, Node Radius, Edge Thickness
- [x] Embedded показывает "waiting for simulating data" при terminate/нет данных
  - ✅ `DrawCsrWaitingOverlayIfNeeded()` и ImGui overlay
- [x] Все нужные `.md` актуализированы (без добавления тестов)
  - ✅ Оба файла обновлены