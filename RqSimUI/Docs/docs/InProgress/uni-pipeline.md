# Dynamic Physics Configuration - Implementation Plan

## Статус: ? Phase 1-4 COMPLETED (All Build Errors Fixed)

**Цель:** Обеспечить сквозную передачу настроек из UI в шейдеры GPU без перезапуска симуляции.

**Проблема:** UI живет своей жизнью, а GPU считает физику по константам, зашитым в код при компиляции.

---

## Архитектурный Обзор

### Текущее состояние (Broken Chain) ? Целевое (Unified Pipeline)

```
БЫЛО:                                    СТАЛО:
???????????????                          ???????????????
?   UI        ?                          ?   UI        ?
? WinForms    ? ??X GPU игнорирует       ? WinForms    ? ??> ToGpuParameters()
? Sliders     ?                          ? Sliders     ?
???????????????                          ???????????????
                                              ?
                                              ?
                                         ?????????????????????????
                                         ? SimulationParameters  ? (blittable)
                                         ?????????????????????????
                                              ?
                                              ?
???????????????????                      ???????????????????
? PhysicsPipeline ?                      ? PhysicsPipeline ?
? const values    ? ??X                  ? + context.Params? ??> Modules
???????????????????                      ???????????????????
                                              ?
                                              ?
????????????????????                     ????????????????????
?  GPU Shaders     ?                     ?  GPU Shaders     ?
?  const double    ? ?                  ?  readonly fields ? ?
????????????????????                     ????????????????????
```

---

## ? Phase 1: Core Infrastructure - COMPLETED

### Выполнено:
- [x] `RqSimEngineApi/Contracts/SimulationParameters.cs` - GPU-compatible blittable struct (~250 bytes)
- [x] `RqSimEngineApi/Contracts/SimulationParameters.Defaults.cs` - Default, FastPreview, Scientific presets
- [x] `RqSimEngineApi/Contracts/SimulationContext.cs` - Extended with Params field
- [x] `RqSim.PluginManager.UI/Configuration/PhysicsSettingsConfig.cs` - ToGpuParameters() & FromGpuParameters()
- [x] Unit tests: 8 tests passing

---

## ? Phase 2: Shader Refactoring - COMPLETED

### Выполнено:
- [x] `SinkhornOllivierRicciKernel` - добавлен `lazyWalkAlpha` параметр
- [x] `OllivierRicciCurvature.cs` (CPU) - параметризован с `lazyWalkAlpha`  
- [x] `SimulationParameters` - добавлено поле `LazyWalkAlpha`
- [x] Все presets обновлены (Default, FastPreview, Scientific)
- [x] `PhysicsSettingsConfig.ToGpuParameters()` - включает LazyWalkAlpha

---

## ? Phase 3: Pipeline & Engines - COMPLETED

### Выполнено:
- [x] `PhysicsPipeline.DynamicParams.cs` - ExecuteFrameWithParams(), UpdateParameters(), ApplyPreset()
- [x] `DynamicPhysicsParams.cs` - локальная копия структуры параметров для GraphEngine
- [x] `IDynamicPhysicsModule` - интерфейс для модулей с поддержкой динамических параметров
- [x] `FunctionalPhysicsModule` - добавлена поддержка IDynamicPhysicsModule + callback delegate
- [x] `PhysicsModuleBase` - добавлена поддержка IDynamicPhysicsModule + CurrentParams field

---

## ? Phase 4: UI Binding - COMPLETED

Подробный расширенный чеклист/план находится в:
- `RqSimUI/Docs/docs/InProgress/uni-pipeline-ui.md`

### Выполненные задачи:
- [x] `button_ApplyPipelineConfSet_Click(...)` реально применяет параметры в `PhysicsPipeline.UpdateParameters(...)`
  - Реализовано в: `Form_Main_DynamicPhysicsParams.cs`, `Form_Main_ApplyPipelineEnhanced.cs`
- [x] `checkBox_ScienceSimMode` отключает `trackEdgeThreshold` (Science mode -> disabled)
  - Реализовано в: `Form_Main_EdgeThresholdGating.cs`, `Form_Main_ScienceMode.cs`
- [x] `trackEdgeThreshold` работает одинаково в embedded и standalone DX12 окне
  - Реализовано в: `PartialForm3D_CSR_EdgeThresholdSync.cs`
- [x] Встроенное 3D-окно: добавлен отдельный Render mode "чистый DX12" (в дополнение к IMGUI)
  - Реализовано в: `PartialForm3D_CSR_Core.cs` (CsrRenderMode3D enum), `PartialForm3D_CSR_UI.cs`
- [x] Встроенное 3D-окно: GPU 3D контролы (Node Radius, Edge Thickness, Edge Quads, Occlusion Culling)
  - Реализовано в: `PartialForm3D_CSR_Core.cs`, `PartialForm3D_CSR_UI.cs`
- [x] Встроенное 3D-окно: estado "waiting for simulating data" при terminate/отсутствии данных
  - Реализовано в: `PartialForm3D_CSR_WaitingOverlay.cs`
- [x] Конвертация `SimulationParameters` -> `DynamicPhysicsParams` на границе API
  - Уже существовало в: `SimulationParametersConverter.cs`, `FormSimAPI_DynamicPhysics.cs`

### Новые файлы Phase 4:
- `RqSimUI/Forms/PartialForms/Form_Main_DynamicPhysicsParams.cs` - Dynamic physics UI panel
  - Поля: `_currentPhysicsConfig`, `_trkLazyWalkAlpha`, `_lblLazyWalkAlphaValue`, `_btnApplyPhysics`, `_lblPhysicsApplyStatus`
  - Методы: `InitializeDynamicPhysicsControls()`, `ApplyPhysicsParametersToPipeline()`, `SyncPhysicsConfigFromUI()`
- `RqSimUI/Forms/PartialForms/Form_Main_EdgeThresholdGating.cs` - Science mode gating logic
  - Поля: `_edgeThresholdValue`, `_trkEdgeThreshold`, `_lblEdgeThresholdValue`
  - Методы: `InitializeEdgeThresholdControls()`, `UpdateEdgeThresholdAvailability()`, `SetEdgeThreshold()`, `GetEdgeThreshold()`
- `RqSimUI/Forms/PartialForms/Form_Main_ApplyPipelineEnhanced.cs` - Enhanced Apply button
  - Методы: `ApplyDynamicPhysicsParametersToReport()`, `GetCurrentPhysicsParametersFromUI()`
- `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_Core.cs` - GPU 3D rendering fields (updated)
  - Enum: `CsrRenderMode3D { ImGui2D, Gpu3D }`
  - Поля: `_csrRenderMode3D`, `_csrRenderModeComboBox`, `_csrEdgeQuadsCheckBox`, `_csrOcclusionCullingCheckBox`
  - Поля: `_csrNodeRadiusTrackBar`, `_csrEdgeThicknessTrackBar`, `_csrNodeRadius`, `_csrUseEdgeQuads`, `_csrUseOcclusionCulling`
- `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_UI.cs` - GPU 3D rendering controls (updated)
  - Методы: `CsrRenderModeComboBox_SelectedIndexChanged()`, `CsrEdgeQuadsCheckBox_CheckedChanged()`
  - Методы: `CsrOcclusionCullingCheckBox_CheckedChanged()`, `CsrNodeRadiusTrackBar_ValueChanged()`
  - Методы: `CsrEdgeThicknessTrackBar_ValueChanged()`, `UpdateCsrGpuRenderingControls()`
- `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_EdgeThresholdSync.cs` - CSR sync
  - Поля: `_csrTrackEdgeThreshold`, `_csrWaitingLabel`, `_csrIsWaitingForData`
  - Методы: `StoreCsrEdgeThresholdReference()`, `SyncEdgeThresholdToCsrWindow()`, `SyncEdgeThresholdFromCsrWindow()`
- `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_WaitingOverlay.cs` - Waiting state overlay

---

## Риски и Ограничения

### Compile-Time Constraints
```csharp
private const int MaxNeighbors = 32;  // Cannot change at runtime
```
**Решение:** Оставить как есть, задокументировать. Для изменения требуется ребилд.

### Two Param Structures
- `SimulationParameters` (RqSimEngineApi) - UI layer
- `DynamicPhysicsParams` (RqSimGraphEngine) - core layer

**Почему:** Избежать циклической зависимости. Конвертация на границе API.