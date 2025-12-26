using RqSim.PluginManager.UI.IncludedPlugins;
using RqSim.PluginManager.UI.IncludedPlugins.GPUOptimizedCSR;
using RQSimulation.Core.Plugins;

namespace RqSimForms;

/// <summary>
/// Background GPU Plugins UI integration for Form_Main.
/// Manages GPU plugin selection, assignment to background GPUs, and pipeline integration.
/// 
/// UI Controls:
/// - listView_AnaliticsGPU: displays active background plugins with GPU assignment
/// - comboBox_BackgroundPipelineGPU: selects target GPU for new plugin
/// - numericUpDown_BackgroundPluginGPUKernels: kernel/thread count for plugin
/// - button_AddGpuBackgroundPluginToPipeline: adds selected plugin to pipeline
/// - button_RemoveGpuBackgroundPluginToPipeline: removes selected plugin from pipeline
/// </summary>
partial class Form_Main
{
    /// <summary>
    /// Available background GPU plugins for selection.
    /// </summary>
    private readonly List<BackgroundPluginInfo> _availableBackgroundPlugins = [];
    
    /// <summary>
    /// Currently active background plugins in the pipeline.
    /// </summary>
    private readonly List<ActiveBackgroundPlugin> _activeBackgroundPlugins = [];

    /// <summary>
    /// Initialize background plugins UI controls.
    /// Call after InitializeMultiGpuControls().
    /// </summary>
    private void InitializeBackgroundPluginsControls()
    {
        PopulateAvailableBackgroundPlugins();
        PopulateBackgroundGpuComboBox();
        SetupBackgroundPluginsListView();
        WireBackgroundPluginsEvents();
        UpdateBackgroundPluginsState();
    }

    /// <summary>
    /// Populate the list of available background GPU plugins.
    /// </summary>
    private void PopulateAvailableBackgroundPlugins()
    {
        _availableBackgroundPlugins.Clear();
        
        // Add GPU plugins that can run on background GPUs
        foreach (Type pluginType in IncludedPluginsRegistry.GpuPluginTypes)
        {
            string name = pluginType.Name.Replace("Module", "").Replace("Gpu", " GPU");
            string category = GetPluginCategory(pluginType);
            
            _availableBackgroundPlugins.Add(new BackgroundPluginInfo
            {
                PluginType = pluginType,
                DisplayName = name,
                Category = category,
                DefaultKernels = GetDefaultKernelCount(pluginType)
            });
        }
        
        AppendSysConsole($"[BackgroundPlugins] Loaded {_availableBackgroundPlugins.Count} available GPU plugins\n");
    }

    /// <summary>
    /// Populate the background GPU combo box.
    /// </summary>
    private void PopulateBackgroundGpuComboBox()
    {
        comboBox_BackgroundPipelineGPU.Items.Clear();
        
        // Add "Auto" option
        comboBox_BackgroundPipelineGPU.Items.Add("Auto (next available)");
        
        // Add specific GPUs (skip GPU 0 which is typically physics/rendering)
        for (int i = 0; i < _availableGpus.Count; i++)
        {
            var gpu = _availableGpus[i];
            string suffix = gpu.SupportsDoublePrecision ? " [FP64]" : "";
            string role = i == 0 ? " (Physics)" : " (Background)";
            comboBox_BackgroundPipelineGPU.Items.Add($"GPU {i}: {gpu.Name}{suffix}{role}");
        }
        
        if (comboBox_BackgroundPipelineGPU.Items.Count > 0)
        {
            comboBox_BackgroundPipelineGPU.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Setup the ListView for displaying active background plugins.
    /// </summary>
    private void SetupBackgroundPluginsListView()
    {
        listView_AnaliticsGPU.Items.Clear();
        listView_AnaliticsGPU.FullRowSelect = true;
        listView_AnaliticsGPU.CheckBoxes = true;
        
        // Ensure columns are sized
        if (listView_AnaliticsGPU.Columns.Count >= 3)
        {
            listView_AnaliticsGPU.Columns[0].Width = 60;  // GPU
            listView_AnaliticsGPU.Columns[1].Width = 200; // Algorithm/Plugin
            listView_AnaliticsGPU.Columns[2].Width = 100; // Kernels/Threads
        }
        
        // Add available plugins as unchecked items
        foreach (var plugin in _availableBackgroundPlugins)
        {
            var item = new ListViewItem(["--", plugin.DisplayName, plugin.DefaultKernels.ToString()])
            {
                Tag = plugin,
                Checked = false
            };
            listView_AnaliticsGPU.Items.Add(item);
        }
    }

    /// <summary>
    /// Wire up event handlers for background plugins controls.
    /// </summary>
    private void WireBackgroundPluginsEvents()
    {
        button_AddGpuBackgroundPluginToPipeline.Click += Button_AddBackgroundPlugin_Click;
        button_RemoveGpuBackgroundPluginToPipeline.Click += Button_RemoveBackgroundPlugin_Click;
        listView_AnaliticsGPU.ItemChecked += ListView_AnaliticsGPU_ItemChecked;
        comboBox_BackgroundPipelineGPU.SelectedIndexChanged += ComboBox_BackgroundGPU_SelectedIndexChanged;
    }

    /// <summary>
    /// Update enabled state of background plugins controls based on Multi-GPU settings.
    /// </summary>
    private void UpdateBackgroundPluginsState()
    {
        bool gpuEnabled = checkBox_EnableGPU.Checked && _availableGpus.Count > 0;
        bool multiGpuEnabled = gpuEnabled && checkBox_UseMultiGPU.Checked && _availableGpus.Count >= 2;
        
        // Enable controls only when Multi-GPU is enabled
        listView_AnaliticsGPU.Enabled = multiGpuEnabled;
        comboBox_BackgroundPipelineGPU.Enabled = multiGpuEnabled;
        numericUpDown_BackgroundPluginGPUKernels.Enabled = multiGpuEnabled;
        button_AddGpuBackgroundPluginToPipeline.Enabled = multiGpuEnabled;
        button_RemoveGpuBackgroundPluginToPipeline.Enabled = multiGpuEnabled && 
            listView_AnaliticsGPU.SelectedItems.Count > 0;
        
        // Update label text based on state
        if (!gpuEnabled)
        {
            label_BackgroundPipelineGPU.Text = "Background Pipeline GPU: (GPU disabled)";
        }
        else if (!multiGpuEnabled)
        {
            label_BackgroundPipelineGPU.Text = "Background Pipeline GPU: (requires Multi-GPU)";
        }
        else
        {
            label_BackgroundPipelineGPU.Text = "Background Pipeline GPU:";
        }
    }

    /// <summary>
    /// Add selected plugin to the background pipeline.
    /// </summary>
    private void AddSelectedPluginToBackgroundPipeline()
    {
        if (listView_AnaliticsGPU.SelectedItems.Count == 0)
        {
            return;
        }
        
        var selectedItem = listView_AnaliticsGPU.SelectedItems[0];
        if (selectedItem.Tag is not BackgroundPluginInfo pluginInfo)
        {
            return;
        }
        
        // Get target GPU index
        int targetGpuIndex = comboBox_BackgroundPipelineGPU.SelectedIndex - 1; // -1 for "Auto"
        if (targetGpuIndex < 0)
        {
            // Auto: find first available background GPU (skip GPU 0)
            targetGpuIndex = _availableGpus.Count > 1 ? 1 : 0;
        }
        
        // Get kernel count
        int kernelCount = (int)numericUpDown_BackgroundPluginGPUKernels.Value;
        
        // Check if already active
        if (_activeBackgroundPlugins.Any(p => p.PluginType == pluginInfo.PluginType))
        {
            AppendSysConsole($"[BackgroundPlugins] Plugin '{pluginInfo.DisplayName}' is already active\n");
            return;
        }
        
        // Create plugin instance
        try
        {
            IPhysicsModule? module = CreatePluginInstance(pluginInfo.PluginType);
            if (module == null)
            {
                AppendSysConsole($"[BackgroundPlugins] Failed to create plugin instance: {pluginInfo.DisplayName}\n");
                return;
            }
            
            // Add to active list
            var activePlugin = new ActiveBackgroundPlugin
            {
                PluginType = pluginInfo.PluginType,
                DisplayName = pluginInfo.DisplayName,
                GpuIndex = targetGpuIndex,
                KernelCount = kernelCount,
                Module = module,
                IsEnabled = true
            };
            _activeBackgroundPlugins.Add(activePlugin);
            
            // Update ListView item
            selectedItem.Checked = true;
            selectedItem.SubItems[0].Text = $"GPU {targetGpuIndex}";
            selectedItem.SubItems[2].Text = kernelCount.ToString();
            
            // Register with physics pipeline if available
            var pipeline = _simApi?.Pipeline;
            if (pipeline != null)
            {
                pipeline.RegisterModule(module);
                AppendSysConsole($"[BackgroundPlugins] Added '{pluginInfo.DisplayName}' to pipeline on GPU {targetGpuIndex}\n");
            }
            else
            {
                AppendSysConsole($"[BackgroundPlugins] Queued '{pluginInfo.DisplayName}' for GPU {targetGpuIndex} (will activate on simulation start)\n");
            }
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[BackgroundPlugins] Error adding plugin: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Remove selected plugin from the background pipeline.
    /// </summary>
    private void RemoveSelectedPluginFromBackgroundPipeline()
    {
        if (listView_AnaliticsGPU.SelectedItems.Count == 0)
        {
            return;
        }
        
        var selectedItem = listView_AnaliticsGPU.SelectedItems[0];
        if (selectedItem.Tag is not BackgroundPluginInfo pluginInfo)
        {
            return;
        }
        
        // Find in active list
        var activePlugin = _activeBackgroundPlugins.FirstOrDefault(p => p.PluginType == pluginInfo.PluginType);
        if (activePlugin == null)
        {
            AppendSysConsole($"[BackgroundPlugins] Plugin '{pluginInfo.DisplayName}' is not active\n");
            return;
        }
        
        try
        {
            // Remove from physics pipeline
            var pipeline = _simApi?.Pipeline;
            if (pipeline != null && activePlugin.Module != null)
            {
                pipeline.RemoveModule(activePlugin.Module);
            }
            
            // Cleanup module
            activePlugin.Module?.Cleanup();
            
            // Remove from active list
            _activeBackgroundPlugins.Remove(activePlugin);
            
            // Update ListView item
            selectedItem.Checked = false;
            selectedItem.SubItems[0].Text = "--";
            
            AppendSysConsole($"[BackgroundPlugins] Removed '{pluginInfo.DisplayName}' from pipeline\n");
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[BackgroundPlugins] Error removing plugin: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Create plugin instance from type.
    /// </summary>
    private static IPhysicsModule? CreatePluginInstance(Type pluginType)
    {
        try
        {
            return Activator.CreateInstance(pluginType) as IPhysicsModule;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get plugin category from type.
    /// </summary>
    private static string GetPluginCategory(Type pluginType)
    {
        if (IncludedPluginsRegistry.TopologyPluginTypes.Contains(pluginType))
            return "Topology";
        if (IncludedPluginsRegistry.GaugePluginTypes.Contains(pluginType))
            return "Gauge";
        if (IncludedPluginsRegistry.RenderingPluginTypes.Contains(pluginType))
            return "Rendering";
        if (pluginType.Name.Contains("MCMC"))
            return "Sampling";
        if (pluginType.Name.Contains("Gravity"))
            return "Gravity";
        return "Physics";
    }

    /// <summary>
    /// Get default kernel count for plugin type.
    /// </summary>
    private static int GetDefaultKernelCount(Type pluginType)
    {
        // MCMC and spectral algorithms typically need more kernels
        if (pluginType.Name.Contains("MCMC"))
            return 100000;
        if (pluginType.Name.Contains("Spectral"))
            return 50000;
        // Standard physics plugins
        return 10000;
    }

    /// <summary>
    /// Register all queued background plugins to the physics pipeline.
    /// Call when simulation starts.
    /// </summary>
    public void RegisterBackgroundPluginsToPipeline()
    {
        var pipeline = _simApi?.Pipeline;
        if (pipeline == null)
        {
            return;
        }
        
        foreach (var activePlugin in _activeBackgroundPlugins.Where(p => p.IsEnabled && p.Module != null))
        {
            try
            {
                pipeline.RegisterModule(activePlugin.Module!);
                AppendSysConsole($"[BackgroundPlugins] Registered '{activePlugin.DisplayName}' on GPU {activePlugin.GpuIndex}\n");
            }
            catch (Exception ex)
            {
                AppendSysConsole($"[BackgroundPlugins] Failed to register '{activePlugin.DisplayName}': {ex.Message}\n");
            }
        }
    }

    /// <summary>
    /// Cleanup all active background plugins.
    /// Call when simulation stops or form closes.
    /// </summary>
    private void CleanupBackgroundPlugins()
    {
        foreach (var activePlugin in _activeBackgroundPlugins)
        {
            try
            {
                activePlugin.Module?.Cleanup();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        _activeBackgroundPlugins.Clear();
        
        // Reset ListView
        foreach (ListViewItem item in listView_AnaliticsGPU.Items)
        {
            item.Checked = false;
            item.SubItems[0].Text = "--";
        }
    }

    /// <summary>
    /// Restores background plugins from saved settings.
    /// Called from LoadAndApplySettings after InitializeBackgroundPluginsControls.
    /// </summary>
    private void RestoreBackgroundPluginsFromSettings()
    {
        if (_formSettings is null)
        {
            return;
        }

        // Skip if no saved plugins
        if (_formSettings.EnabledBackgroundPlugins.Count == 0)
        {
            return;
        }

        // Skip if Multi-GPU is not enabled
        if (!checkBox_UseMultiGPU.Checked || _availableGpus.Count < 2)
        {
            AppendSysConsole("[BackgroundPlugins] Saved plugins found but Multi-GPU is disabled - skipping restore\n");
            return;
        }

        int restoredCount = 0;
        foreach (string typeName in _formSettings.EnabledBackgroundPlugins)
        {
            try
            {
                // Find matching plugin in available list
                BackgroundPluginInfo? pluginInfo = null;
                ListViewItem? listViewItem = null;
                
                foreach (ListViewItem item in listView_AnaliticsGPU.Items)
                {
                    if (item.Tag is BackgroundPluginInfo info)
                    {
                        string infoTypeName = info.PluginType.FullName ?? info.PluginType.Name;
                        if (infoTypeName == typeName || info.PluginType.Name == typeName)
                        {
                            pluginInfo = info;
                            listViewItem = item;
                            break;
                        }
                    }
                }

                if (pluginInfo is null || listViewItem is null)
                {
                    AppendSysConsole($"[BackgroundPlugins] Could not find plugin type: {typeName}\n");
                    continue;
                }

                // Get saved GPU index and kernel count
                int gpuIndex = _formSettings.PluginGpuAssignments.TryGetValue(typeName, out int savedGpu) 
                    ? savedGpu 
                    : (_availableGpus.Count > 1 ? 1 : 0);
                int kernelCount = _formSettings.PluginKernelCounts.TryGetValue(typeName, out int savedKernels) 
                    ? savedKernels 
                    : pluginInfo.DefaultKernels;

                // Create plugin instance
                IPhysicsModule? module = CreatePluginInstance(pluginInfo.PluginType);
                if (module is null)
                {
                    AppendSysConsole($"[BackgroundPlugins] Failed to create instance: {pluginInfo.DisplayName}\n");
                    continue;
                }

                // Add to active list
                var activePlugin = new ActiveBackgroundPlugin
                {
                    PluginType = pluginInfo.PluginType,
                    DisplayName = pluginInfo.DisplayName,
                    GpuIndex = gpuIndex,
                    KernelCount = kernelCount,
                    Module = module,
                    IsEnabled = true
                };
                _activeBackgroundPlugins.Add(activePlugin);

                // Update ListView item
                listViewItem.Checked = true;
                listViewItem.SubItems[0].Text = $"GPU {gpuIndex}";
                listViewItem.SubItems[2].Text = kernelCount.ToString();

                restoredCount++;
            }
            catch (Exception ex)
            {
                AppendSysConsole($"[BackgroundPlugins] Error restoring plugin {typeName}: {ex.Message}\n");
            }
        }

        if (restoredCount > 0)
        {
            AppendSysConsole($"[BackgroundPlugins] Restored {restoredCount} plugin(s) from settings\n");
        }
    }

    // === Event Handlers ===

    private void Button_AddBackgroundPlugin_Click(object? sender, EventArgs e)
    {
        AddSelectedPluginToBackgroundPipeline();
    }

    private void Button_RemoveBackgroundPlugin_Click(object? sender, EventArgs e)
    {
        RemoveSelectedPluginFromBackgroundPipeline();
    }

    private void ListView_AnaliticsGPU_ItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        // Enable/disable remove button based on selection
        button_RemoveGpuBackgroundPluginToPipeline.Enabled = 
            checkBox_UseMultiGPU.Checked && 
            listView_AnaliticsGPU.SelectedItems.Count > 0 &&
            e.Item.Checked;
    }

    private void ComboBox_BackgroundGPU_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Update kernel count default based on selected GPU capabilities
        if (comboBox_BackgroundPipelineGPU.SelectedIndex > 0)
        {
            int gpuIndex = comboBox_BackgroundPipelineGPU.SelectedIndex - 1;
            if (gpuIndex < _availableGpus.Count && _availableGpus[gpuIndex].SupportsDoublePrecision)
            {
                // FP64 capable GPUs can handle more kernels
                numericUpDown_BackgroundPluginGPUKernels.Value = Math.Min(
                    numericUpDown_BackgroundPluginGPUKernels.Maximum,
                    50000);
            }
        }
    }

    /// <summary>
    /// Information about an available background GPU plugin.
    /// </summary>
    private sealed class BackgroundPluginInfo
    {
        public Type PluginType { get; init; } = typeof(object);
        public string DisplayName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public int DefaultKernels { get; init; } = 10000;
    }

    /// <summary>
    /// Information about an active background plugin in the pipeline.
    /// </summary>
    private sealed class ActiveBackgroundPlugin
    {
        public Type PluginType { get; init; } = typeof(object);
        public string DisplayName { get; init; } = string.Empty;
        public int GpuIndex { get; init; }
        public int KernelCount { get; init; }
        public IPhysicsModule? Module { get; init; }
        public bool IsEnabled { get; set; }
    }
}
