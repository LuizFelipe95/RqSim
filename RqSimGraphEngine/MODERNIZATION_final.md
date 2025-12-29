# Modernization Checklist


\RqSim3DForm\ - Stadnalone DX12 WinForm - \RqSimulator\RqSim3DForm\RqSim3DForm.csproj

RqSimUI\Forms\3DVisual\GDI+\    - GDI+ 3D Visualization Forms

RqSimUI\Forms\3DVisual\Dx12Vulkan\ - DX12/Vulkan 3D Visualization Forms


## DX12 Rendering & Visualization

- [x] **Fix Ghost Simulation / Overlay Issue**
    - *Status:* ✅ FIXED. Ghost overlay eliminated via `ClearCachedGraph()` + buffer reset.
    - *Notes:*
        - Standalone DX12 form now supports `ClearData()` (added in `RqSim3DForm\Form_Rsim3DForm.Rendering.cs`).
        - CSR overlay was shifted right to avoid left panel scrollbar overlap.
        - **NEW FIX:** Added `_simApi.ClearCachedGraph()` call in `ClearCsrVisualizationData()` to clear `_lastActiveGraph` from previous session.
        - **NEW FIX:** Reset manifold embedding state on new simulation start.
    - *Files:*
        - `RqSimUI\Forms\3DVisual\Dx12Vulkan\PartialForm3D_CSR_Rendering.cs`
        - `RqSimUI\Forms\3DVisual\Dx12Vulkan\PartialForm3D_CSR_WaitingOverlay.cs`
        - `RqSimRenderingEngine\Rendering\Backend\DX12\Dx12RenderHost.cs`
        - `RqSim3DForm\Form_Rsim3DForm.UI.cs`
        - `RqSim3DForm\Form_Rsim3DForm.Rendering.cs`
        - `RqSimEngineApi\RqSimEngineApi\FormSimAPI_Core.cs` (added `ClearCachedGraph()`)

- [x] **Stop Simulation Behavior**
    - *Status:* Implemented. Manifold dynamics continue after simulation stop (matching GDI+ behavior).
    - *Notes:*
        - `IsSimulationRunning` is updated by `Form_Main_Simulation` on start/stop.
        - `ApplyManifoldEmbedding()` now runs whenever `_enableManifoldEmbedding = true`, **NOT** gated by `IsSimulationRunning`.
        - This matches GDI+ behavior where the graph "breathes" even after simulation stops.
        - Embedded CSR: `Terminate` stops `_timerCsr3D` + clears CSR cache; new simulation start restarts `_timerCsr3D`.
    - *Files:*
        - `RqSimUI\Forms\PartialForms\Form_Main_Simulation.cs`
        - `RqSimUI\Forms\PartialForms\Form_Main_RqConsole.cs`
        - `RqSimRenderingEngine\Rendering\Backend\DX12\Rendering\Dx12SceneRenderer.cs`
        - `RqSim3DForm\Form_Rsim3DForm.UI.cs`
        - `RqSim3DForm\Form_Rsim3DForm.Rendering.cs`

- [x] **Target Metrics Display**
    - *Status:* Implemented (GDI+ and standalone DX12 now update target metrics).
    - *Work done:*
        - GDI+: added `"Target"` visualization mode so target metrics update condition can be satisfied.
        - GDI+: metric computation no longer swallows exceptions; failures are logged and statuses are set to `Failed` instead of silently staying `---`.
        - Standalone DX12: `UpdateTargetMetrics` runs only when `_showTargetOverlay = true` (matching CSR behavior).
        - Standalone DX12: uses `SmoothedSpectralDimension` only (no expensive `ComputeSpectralDimension` fallback).
        - Standalone DX12: stabilized heuristic proxy values (clamped) to avoid negative/unstable target numbers.
    - *Files:*
        - `RqSimUI\Forms\3DVisual\GDI+\PartialForm3D.cs`
        - `RqSimUI\Forms\3DVisual\GDI+\PartialForm3D.Targets.cs`
        - `RqSimUI\Forms\PartialForms\Form_Main_Standalone3dForm.cs`
        - `RqSim3DForm\Form_Rsim3DForm.Rendering.cs`
        - `RqSim3DForm\Form_Rsim3DForm.Targets.cs`
        - `RqSim3DForm\Form_Rsim3DForm.UI.cs`

- [x] **Metrics Layout Fix**
    - *Status:* Implemented for embedded CSR overlay positioning.
    - *Work done:*
        - CSR ImGui overlay moved right and now uses `_csrControlsHostPanel.Width` to avoid overlap with left controls/metrics area.
    - *Files:*
        - `RqSimUI\Forms\3DVisual\Dx12Vulkan\PartialForm3D_CSR_UI.cs`
        - `RqSimUI\Forms\3DVisual\Dx12Vulkan\PartialForm3D_CSR_Rendering.cs`
        - `RqSim3DForm\Form_Rsim3DForm.UI.cs`
        - `RqSim3DForm\Form_Rsim3DForm.Rendering.cs`

- [x] **Edge Threshold Slider**
    - *Status:* ✅ Implemented (GDI+/Main ↔ CSR bidirectional sync, Science Mode disables both).
    - *Clarification:* **This is UI-only, NOT physics!**
        - `_edgeThresholdValue` / `_displayWeightThreshold` only filters which edges are **displayed**.
        - Does NOT affect simulation physics in the engine process.
        - In **Science Mode**, slider is disabled to prevent visual confusion.
    - *Work done:*
        - Main slider syncs to CSR via `SyncEdgeThresholdToCsrWindow()`.
        - CSR slider syncs back to main via `SyncEdgeThresholdFromCsrWindow(...)`.
        - CSR trackbar reference stored for Science Mode gating.
    - *Files:*
        - `RqSimUI\Forms\PartialForms\Form_Main_EdgeThresholdGating.cs`
        - `RqSimUI\Forms\3DVisual\Dx12Vulkan\PartialForm3D_CSR_EdgeThresholdSync.cs`
        - `RqSimUI\Forms\3DVisual\Dx12Vulkan\PartialForm3D_CSR_UI.cs`

- [x] **Standalone DX12 Performance Optimization**
    - *Status:* ✅ FIXED. FPS increased from 2-4 to 28+.
    - *Root causes found:*
        1. `ComputeSpectralDimension()` called every frame when `SmoothedSpectralDimension` unavailable (O(n²) operation).
        2. Force arrays and filtered edges list allocated every frame in manifold embedding.
        3. `ApplyManifoldEmbedding()` gated by `IsSimulationRunning` — caused manifold to freeze after sim stop.
    - *Fixes applied:*
        - Removed `ComputeSpectralDimension()` fallback — use `SmoothedSpectralDimension` only.
        - Cached force arrays (`_forceX`, `_forceY`, `_forceZ`) and filtered edges list.
        - Throttled manifold physics to every 2 frames.
        - Added reentry guard `_isRendering` to prevent overlapping tick handlers.
        - Manifold now runs when `_enableManifoldEmbedding = true` (not gated by `IsSimulationRunning`).
        - **NEW FIX:** Manifold now blends 80% embedding + 20% fresh data for stable animation with graph updates.
    - *Files:*
        - `RqSimUI\Forms\PartialForms\Form_Main_Standalone3dForm.cs`
        - `RqSim3DForm\Form_Rsim3DForm.Rendering.cs`
        - `RqSim3DForm\Form_Rsim3DForm.Manifold.cs`

## Physics Pipeline & Settings

- [x] **Real-time Physics Tuning**
    - *Status:* ✅ Implemented. Confirmation dialogs added for module changes and auto-tuning during running simulation.
    - *Work done:*
        - Added confirmation dialogs when toggling physics modules during simulation.
        - Added confirmation dialogs for Auto-tuning enable/disable.
        - Added batch module change confirmation for presets.
        - Created helper methods for mode and parameter change confirmations.
    - *Files:*
        - `RqSimUI\Forms\PartialForms\Form_Main_ModuleCheckboxes_New.cs` (new)
        - `RqSimUI\Forms\PartialForms\Form_Main_Confirmations.cs` (new)
        - `RqSimUI\Forms\PartialForms\Form_Main_AutoTuning.cs`

- [x] **Physics Settings UI**
    - *Status:* ✅ Already comprehensive. All engine physics settings available in `tabPage_Settings`.
    - *Notes:*
        - `PhysicsSettingsConfig` contains all runtime-adjustable physics parameters.
        - `Form_Main_PhysicsInfo.cs` displays read-only reference of all PhysicsConstants.
    - *Files:*
        - `RqSimUI\Forms\PartialForms\Form_Main_PhysicsInfo.cs`
        - `RqSim.PluginManager.UI\Configuration\PhysicsSettingsConfig.cs`

- [x] **Settings Persistence**
    - *Status:* ✅ Implemented. All settings (system, physics, simulation, modes) saved/loaded to JSON.
    - *Work done:*
        - `PhysicsSettingsSerializer` handles JSON save/load with backup support.
        - `PhysicsSettingsManager` captures/applies settings to LiveConfig and RQFlags.
        - Science Mode and Auto-tuning states now persisted.
    - *Files:*
        - `RqSimUI\Forms\PartialForms\Form_Main_SettingsPersistence.cs`
        - `RqSimUI\Forms\Interfaces\PhysicsSettingsManager.cs`
        - `RqSim.PluginManager.UI\Configuration\PhysicsSettingsSerializer.cs`

- [x] **Science Mode Persistence**
    - *Status:* ✅ FIXED. Science Mode checkbox state NOW saved/restored via `FormSettings`.
    - *Work done:*
        - Added `ScienceMode`, `UseOllivierRicciCurvature`, `EnableConservationValidation`, `UseGpuAnisotropy` to `FormSettings.cs`.
        - Added save logic in `SaveCurrentSettings()` in `Form_Main_Settings.cs`.
        - Added load logic in `LoadAndApplySettings()` in `Form_Main_Settings.cs`.
        - `checkBox_ScienceSimMode.Checked` is restored on form load.
    - *Files:*
        - `RqSimUI\Forms\MainForm\FormSettings.cs` (added Mode Settings section)
        - `RqSimUI\Forms\MainForm\MainCore\Form_Main_Settings.cs` (load/save wiring)
        - `RqSim.PluginManager.UI\Configuration\PhysicsSettingsConfig.cs`

- [x] **Mode Change Confirmation**
    - *Status:* ✅ Implemented. Yes/No prompts for Auto-tuning and Science Mode changes.
    - *Work done:*
        - Added confirmation for Auto-tuning toggle during simulation.
        - Added confirmation for preset changes during simulation.
        - Created reusable confirmation helper methods in `Form_Main_Confirmations.cs`.
    - *Files:*
        - `RqSimUI\Forms\PartialForms\Form_Main_Confirmations.cs` (new)
        - `RqSimUI\Forms\PartialForms\Form_Main_AutoTuning.cs`

- [x] **Restore Missing Settings Panel**
    - *Status:* ✅ Already exists. Science Mode settings panel is created dynamically via `CreateScienceModeSettingsPanel()`.
    - *Notes:*
        - Panel includes: Ollivier-Ricci toggle, Conservation validation toggle, GPU Anisotropy toggle.
        - Panel is added to UniPipeline tab or Settings tab automatically.
    - *Files:*
        - `RqSimUI\Forms\PartialForms\Form_Main_ScienceMode.cs`

---

## Summary of Latest Session Fixes

### Build Issues Fixed
1. ✅ `_edgeThreshold` → `_edgeWeightThreshold` in `Form_Rsim3DForm.Manifold.cs`
2. ✅ `Dx12EdgeVertex` → `Dx12LineVertex` in `PartialForm3D_CSR_Rendering.cs`
3. ✅ Added `using RqSimRenderingEngine.Rendering.Backend.DX12.Rendering;` for types

### Key Clarifications
- **Edge Threshold** is **UI-only** - filters displayed edges, does NOT affect physics
- **Science Mode** disables Edge Threshold slider to prevent visual confusion
- **Manifold Embedding** is visualization-only force-directed layout, not physics simulation

### Files Modified This Session
- `RqSimUI\Forms\MainForm\FormSettings.cs` - Added Science Mode settings
- `RqSimUI\Forms\MainForm\MainCore\Form_Main_Settings.cs` - Added Science Mode load/save
- `RqSimUI\Forms\3DVisual\Dx12Vulkan\PartialForm3D_CSR_Rendering.cs` - Fixed ghost graph, added usings
- `RqSimEngineApi\RqSimEngineApi\FormSimAPI_Core.cs` - Added `ClearCachedGraph()`
- `RqSim3DForm\Form_Rsim3DForm.Manifold.cs` - Fixed field name, improved embedding blend
