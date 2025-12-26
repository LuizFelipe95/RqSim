# Plugin System Complete Integration Plan

## Current State Analysis

### Issues Found

1. **Missing Parameterless Constructors**: Several built-in modules require constructor parameters (e.g., `MexicanHatPotentialModule`, `SpinorFieldModule`, `KleinGordonModule`, `InternalTimeModule`, `QuantumGraphityModule`) - causes runtime error when dynamically creating instances.

2. **Empty Button Handlers**: `_btnOK_Click` and `_btnApply_Click` in `PhysxPluginsForm` are empty stubs.

3. **No Plugin Configuration Persistence**: No mechanism to save/load plugin configurations (enabled state, order, parameters).

4. **No Parameter UI**: Modules with configurable parameters have no UI to edit them.

5. **Background Plugins Not Fully Integrated**: `Form_Main_BackgroundPlugins.cs` needs cleanup integration and simulation start hooks.

6. **Missing ModuleGroup/Stage Columns in UI**: The DataGridView doesn't show new `Stage`, `ModuleGroup`, `GroupMode` properties.

---

## Implementation Plan

### Phase 1: Fix Module Constructors (Critical)

**Step 1.1**: Add parameterless constructors to all built-in modules
- Files: `RqSimGraphEngine\RQSimulation\Core\Plugins\Modules\BuiltInModules.cs`
- Modules needing fix:
  - `SpinorFieldModule` - add `SpinorFieldModule()`
  - `KleinGordonModule` - add `KleinGordonModule()`
  - `InternalTimeModule` - add `InternalTimeModule()`
  - `QuantumGraphityModule` - add `QuantumGraphityModule()`
  - `MexicanHatPotentialModule` - add `MexicanHatPotentialModule()`

### Phase 2: Complete PhysxPluginsForm Functionality

**Step 2.1**: Implement `_btnOK_Click` handler
- Mark dirty state as saved
- Close form with `DialogResult.OK`

**Step 2.2**: Implement `_btnApply_Click` handler
- Apply current changes
- Sort pipeline by priority
- Clear dirty flag

**Step 2.3**: Add Stage and ModuleGroup columns to DataGridView
- Add `_colStage` column
- Add `_colModuleGroup` column  
- Update `RefreshModuleList()` to populate new columns

**Step 2.4**: Add color coding for execution stage
- Preparation = Light Yellow
- Forces = White (default)
- Integration = Light Cyan
- PostProcess = Light Gray

### Phase 3: Plugin Configuration Persistence

**Step 3.1**: Create `PluginPipelineConfig` class
- File: `RqSim.PluginManager.UI\Configuration\PluginPipelineConfig.cs`
- Properties:
  - `List<PluginModuleConfig> Modules`
  - `DateTime LastModified`
  - `string Version`

**Step 3.2**: Create `PluginModuleConfig` class
- Properties:
  - `string TypeName` (fully qualified)
  - `string AssemblyPath` (for external DLLs)
  - `bool IsEnabled`
  - `int Priority`
  - `Dictionary<string, object> Parameters`

**Step 3.3**: Create `PluginConfigSerializer` class
- `Save(PluginPipelineConfig, string path)` - JSON serialization
- `Load(string path) -> PluginPipelineConfig`
- Default path: `%AppData%\RqSim\plugins.json`

**Step 3.4**: Add Save/Load menu items to PhysxPluginsForm
- "Save Configuration" button
- "Load Configuration" button
- Auto-save on OK click

### Phase 4: Module Parameter Editor

**Step 4.1**: Create `IConfigurableModule` interface
- File: `RqSimGraphEngine\RQSimulation\Core\Plugins\IConfigurableModule.cs`
- Methods:
  - `Dictionary<string, ParameterInfo> GetParameters()`
  - `void SetParameter(string name, object value)`
  - `object GetParameter(string name)`

**Step 4.2**: Create `ParameterInfo` class
- Properties:
  - `string Name`
  - `Type ValueType`
  - `object DefaultValue`
  - `object MinValue` (optional)
  - `object MaxValue` (optional)
  - `string Description`

**Step 4.3**: Implement `IConfigurableModule` on key modules
- `MexicanHatPotentialModule`
- `SpinorFieldModule`
- `KleinGordonModule`
- `InternalTimeModule`
- `QuantumGraphityModule`

**Step 4.4**: Add parameter editing panel to PhysxPluginsForm
- PropertyGrid or custom NumericUpDown controls
- Bind to selected module's parameters
- Save changes to module instance

### Phase 5: Background Plugins Integration

**Step 5.1**: Wire cleanup to simulation stop
- File: `Form_Main_Simulation.cs` or appropriate partial
- Call `CleanupBackgroundPlugins()` when simulation stops

**Step 5.2**: Wire registration to simulation start
- Call `RegisterBackgroundPluginsToPipeline()` when simulation starts

**Step 5.3**: Add status display
- Show active background plugin count in status bar
- Show GPU assignment status

### Phase 6: Settings Persistence

**Step 6.1**: Add plugin settings to `FormSettings`
- File: `RqSimUI\Forms\MainForm\MainCore\FormSettings.cs`
- New properties:
  - `List<string> EnabledBackgroundPlugins`
  - `Dictionary<string, int> PluginGpuAssignments`

**Step 6.2**: Update `LoadAndApplySettings()` and `SaveCurrentSettings()`
- Save/restore background plugin state
- Save/restore GPU assignments

---

## File Changes Summary

| File | Changes |
|------|---------|
| `BuiltInModules.cs` | Add parameterless constructors to 5 modules |
| `PhysxPluginsForm.cs` | Implement OK/Apply handlers, add columns |
| `PhysxPluginsForm.Designer.cs` | Add Stage/ModuleGroup columns |
| `PluginPipelineConfig.cs` | New file - configuration model |
| `PluginConfigSerializer.cs` | New file - JSON persistence |
| `IConfigurableModule.cs` | New file - parameter interface |
| `Form_Main_BackgroundPlugins.cs` | Add cleanup/registration integration |
| `Form_Main_Simulation.cs` | Wire plugin lifecycle |
| `FormSettings.cs` | Add plugin settings |

---

## Priority Order

1. **Phase 1** (Critical) - Fix constructors to unblock UI
2. **Phase 2** (High) - Complete form functionality
3. **Phase 5** (High) - Background plugins integration
4. **Phase 3** (Medium) - Configuration persistence
5. **Phase 6** (Medium) - Settings persistence  
6. **Phase 4** (Low) - Parameter editor (nice-to-have)

---

## Estimated Effort

- Phase 1: 15 minutes
- Phase 2: 30 minutes
- Phase 3: 45 minutes
- Phase 4: 60 minutes
- Phase 5: 20 minutes
- Phase 6: 20 minutes

**Total: ~3 hours**
