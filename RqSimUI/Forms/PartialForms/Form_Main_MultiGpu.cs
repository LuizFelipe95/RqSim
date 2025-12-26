using ComputeSharp;
using RqSimForms.Forms.Interfaces;
using RQSimulation.Core.Infrastructure;
using RQSimulation.Core.Scheduler;

namespace RqSimForms;

/// <summary>
/// Multi-GPU cluster UI integration for Form_Main.
/// Handles GPU enumeration, role assignment, and cluster lifecycle via UI controls.
/// </summary>
partial class Form_Main
{
    /// <summary>
    /// All enumerated hardware-accelerated GPUs.
    /// </summary>
    private List<GpuDeviceInfo> _availableGpus = [];

    /// <summary>
    /// Initialize Multi-GPU UI controls after device enumeration.
    /// Call after InitializeGpuControls().
    /// </summary>
    private void InitializeMultiGpuControls()
    {
        EnumerateGpuDevices();
        PopulateMultiGpuLists();
        WireMultiGpuEvents();
        UpdateMultiGpuSettingsState();

        // Initialize background plugins UI (depends on GPU enumeration)
        InitializeBackgroundPluginsControls();
    }

    /// <summary>
    /// Enumerate all hardware-accelerated GPUs.
    /// </summary>
    private void EnumerateGpuDevices()
    {
        _availableGpus.Clear();

        try
        {
            int index = 0;
            foreach (var device in GraphicsDevice.EnumerateDevices())
            {
                if (device.IsHardwareAccelerated)
                {
                    _availableGpus.Add(new GpuDeviceInfo
                    {
                        Index = index,
                        Name = device.Name,
                        SupportsDoublePrecision = device.IsDoublePrecisionSupportAvailable()
                    });
                    index++;
                }
            }

            AppendSysConsole($"[MultiGPU] Found {_availableGpus.Count} hardware-accelerated GPU(s)\n");
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[MultiGPU] GPU enumeration failed: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Populate all Multi-GPU combo boxes and checked list boxes.
    /// </summary>
    private void PopulateMultiGpuLists()
    {
        // Physics GPU selector
        comboBox_MultiGpu_PhysicsGPU.Items.Clear();
        foreach (var gpu in _availableGpus)
        {
            string suffix = gpu.SupportsDoublePrecision ? " [FP64]" : "";
            comboBox_MultiGpu_PhysicsGPU.Items.Add($"GPU {gpu.Index}: {gpu.Name}{suffix}");
        }

        if (comboBox_MultiGpu_PhysicsGPU.Items.Count > 0)
        {
            comboBox_MultiGpu_PhysicsGPU.SelectedIndex = 0;
        }


    }

    /// <summary>
    /// Wire up event handlers for Multi-GPU controls.
    /// </summary>
    private void WireMultiGpuEvents()
    {
        checkBox_UseMultiGPU.CheckedChanged += CheckBox_UseMultiGPU_CheckedChanged;
        checkBox_EnableGPU.CheckedChanged += CheckBox_EnableGPU_MultiGpu_CheckedChanged;
        comboBox_MultiGpu_PhysicsGPU.SelectedIndexChanged += ComboBox_MultiGpu_PhysicsGPU_SelectedIndexChanged;
    }

    /// <summary>
    /// Update enabled state of Multi-GPU controls based on current settings.
    /// </summary>
    private void UpdateMultiGpuSettingsState()
    {
        bool gpuEnabled = checkBox_EnableGPU.Checked && _availableGpus.Count > 0;
        bool multiGpuEnabled = gpuEnabled && checkBox_UseMultiGPU.Checked && _availableGpus.Count >= 2;

        // Multi-GPU checkbox
        checkBox_UseMultiGPU.Enabled = gpuEnabled && _availableGpus.Count >= 2;

        // Physics GPU selector
        comboBox_MultiGpu_PhysicsGPU.Enabled = gpuEnabled;


        // Show warning if only 1 GPU
        if (_availableGpus.Count == 1)
        {
            checkBox_UseMultiGPU.Text = "Multi GPU (requires 2+ GPUs)";
        }
        else
        {
            checkBox_UseMultiGPU.Text = $"Multi GPU Cluster ({_availableGpus.Count} GPUs)";
        }

        // Update background plugins state (if initialized)
        UpdateBackgroundPluginsState();
    }

    /// <summary>
    /// Apply Multi-GPU settings from UI to RqSimEngineApi.
    /// </summary>
    private void ApplyMultiGpuSettings()
    {
        if (!checkBox_EnableGPU.Checked || !checkBox_UseMultiGPU.Checked)
        {
            return;
        }

        // Get selected physics GPU
        int physicsGpuIndex = comboBox_MultiGpu_PhysicsGPU.SelectedIndex;
        if (physicsGpuIndex < 0)
        {
            physicsGpuIndex = 0;
        }
        /*


                // Apply to RqSimEngineApi
                _simApi.MultiGpuSettings.Enabled = true;
                _simApi.MultiGpuSettings.McmcWorkerCount = mcmcWorkerCount;
                _simApi.MultiGpuSettings.SpectralWorkerCount = spectralWorkerCount;
                _simApi.MultiGpuSettings.SpectralWalkers = (int)numericUpDown_MultiGpu_SpectralWalkers.Value;

                AppendSysConsole($"[MultiGPU] Settings applied: Physics=GPU{physicsGpuIndex}, " +
                              $"Spectral={spectralWorkerCount}, MCMC={mcmcWorkerCount}, " +
                              $"Walkers={(int)numericUpDown_MultiGpu_SpectralWalkers.Value}\n");*/
    }

    /// <summary>
    /// Initialize Multi-GPU cluster before simulation starts.
    /// </summary>
    /// <returns>True if Multi-GPU mode was successfully initialized.</returns>
    private bool TryInitializeMultiGpuCluster()
    {
        if (!checkBox_EnableGPU.Checked || !checkBox_UseMultiGPU.Checked)
        {
            return false;
        }

        if (_availableGpus.Count < 2)
        {
            AppendSysConsole("[MultiGPU] Cannot initialize: requires 2+ GPUs\n");
            return false;
        }

        // Apply UI settings
        ApplyMultiGpuSettings();

        // Initialize cluster via RqSimEngineApi
        bool success = _simApi.InitializeMultiGpuCluster();

        if (success)
        {
            AppendSysConsole("[MultiGPU] Cluster initialized successfully\n");
        }

        return success;
    }

    private void button_AddGpuBackgroundPluginToPipeline_Click(object sender, EventArgs e)
    {

    }

    private void button_RemoveGpuBackgroundPluginToPipeline_Click(object sender, EventArgs e)
    {

    }

    private void comboBox_BackgroundPipelineGPU_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
    /// <summary>
    /// Dispose Multi-GPU cluster on simulation stop or form close.
    /// </summary>
    private void DisposeMultiGpuCluster()
    {
        _simApi.DisposeMultiGpuCluster();
    }

    // === Event Handlers ===

    private void CheckBox_UseMultiGPU_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateMultiGpuSettingsState();

        if (checkBox_UseMultiGPU.Checked)
        {
            AppendSysConsole("[MultiGPU] Multi-GPU mode enabled\n");
        }
        else
        {
            AppendSysConsole("[MultiGPU] Multi-GPU mode disabled\n");
        }
    }

    private void CheckBox_EnableGPU_MultiGpu_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateMultiGpuSettingsState();
    }

    private void ComboBox_MultiGpu_PhysicsGPU_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int selectedIndex = comboBox_MultiGpu_PhysicsGPU.SelectedIndex;
        if (selectedIndex < 0 || selectedIndex >= _availableGpus.Count)
        {
            return;
        }


        AppendSysConsole($"[MultiGPU] Physics GPU set to: GPU {selectedIndex}\n");
    }

    /// <summary>
    /// Get Multi-GPU cluster status summary for dashboard display.
    /// </summary>
    public string GetMultiGpuStatusSummary()
    {
        if (!_simApi.IsMultiGpuActive)
        {
            return "Inactive";
        }

        var status = _simApi.GetMultiGpuStatus();
        if (status == null)
        {
            return "N/A";
        }

        return $"S={status.SpectralWorkerCount}, M={status.McmcWorkerCount}, " +
               $"Busy={status.BusySpectralWorkers + status.BusyMcmcWorkers}";
    }

    /// <summary>
    /// GPU device information for UI display.
    /// </summary>
    private sealed class GpuDeviceInfo
    {
        public int Index { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool SupportsDoublePrecision { get; init; }
    }
}
