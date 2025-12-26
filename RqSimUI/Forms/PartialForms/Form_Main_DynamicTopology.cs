using System;
using System.Windows.Forms;
using RQSimulation.GPUCompressedSparseRow;

namespace RqSimForms;

/// <summary>
/// Partial class for Dynamic Topology UI controls and settings.
/// Handles configuration of DynamicHardRewiring mode for CSR engine.
/// </summary>
partial class Form_Main
{
    // Dynamic topology settings fields
    private int _dynamicTopologyRebuildInterval = 10;
    private double _dynamicTopologyDeletionThreshold = 0.001;
    private double _dynamicTopologyBeta = 1.0;
    private double _dynamicTopologyInitialWeight = 0.5;

    // UI Controls (will be created dynamically or added to designer)
    private NumericUpDown? _nudDynamicRebuildInterval;
    private NumericUpDown? _nudDynamicDeletionThreshold;
    private NumericUpDown? _nudDynamicBeta;
    private Panel? _pnlDynamicTopologySettings;

    /// <summary>
    /// Initializes dynamic topology UI controls.
    /// Call this after InitializeUniPipelineTab.
    /// </summary>
    private void InitializeDynamicTopologyControls()
    {
        // Create panel for dynamic topology settings
        _pnlDynamicTopologySettings = new Panel
        {
            Name = "_pnlDynamicTopologySettings",
            AutoSize = true,
            Visible = false, // Only show when DynamicHardRewiring is selected
            Padding = new Padding(5)
        };

        var flp = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        // Rebuild Interval
        var lblInterval = new Label
        {
            Text = "Rebuild Interval:",
            AutoSize = true,
            Margin = new Padding(3, 6, 3, 3)
        };
        _nudDynamicRebuildInterval = new NumericUpDown
        {
            Name = "_nudDynamicRebuildInterval",
            Minimum = 1,
            Maximum = 1000,
            Value = _dynamicTopologyRebuildInterval,
            Width = 60,
            Margin = new Padding(3)
        };
        _nudDynamicRebuildInterval.ValueChanged += NudDynamicRebuildInterval_ValueChanged;

        // Deletion Threshold
        var lblThreshold = new Label
        {
            Text = "Del. Threshold:",
            AutoSize = true,
            Margin = new Padding(8, 6, 3, 3)
        };
        _nudDynamicDeletionThreshold = new NumericUpDown
        {
            Name = "_nudDynamicDeletionThreshold",
            Minimum = 0.0001M,
            Maximum = 1.0M,
            DecimalPlaces = 4,
            Increment = 0.001M,
            Value = (decimal)_dynamicTopologyDeletionThreshold,
            Width = 80,
            Margin = new Padding(3)
        };
        _nudDynamicDeletionThreshold.ValueChanged += NudDynamicDeletionThreshold_ValueChanged;

        // Beta (inverse temperature)
        var lblBeta = new Label
        {
            Text = "Beta:",
            AutoSize = true,
            Margin = new Padding(8, 6, 3, 3)
        };
        _nudDynamicBeta = new NumericUpDown
        {
            Name = "_nudDynamicBeta",
            Minimum = 0.01M,
            Maximum = 100.0M,
            DecimalPlaces = 2,
            Increment = 0.1M,
            Value = (decimal)_dynamicTopologyBeta,
            Width = 60,
            Margin = new Padding(3)
        };
        _nudDynamicBeta.ValueChanged += NudDynamicBeta_ValueChanged;

        // Add to flow panel
        flp.Controls.Add(lblInterval);
        flp.Controls.Add(_nudDynamicRebuildInterval);
        flp.Controls.Add(lblThreshold);
        flp.Controls.Add(_nudDynamicDeletionThreshold);
        flp.Controls.Add(lblBeta);
        flp.Controls.Add(_nudDynamicBeta);

        _pnlDynamicTopologySettings.Controls.Add(flp);

        // Try to add panel to GPU/Topology settings area
        TryAddDynamicTopologyPanel();
    }

    /// <summary>
    /// Attempts to add dynamic topology panel to the GPU settings area.
    /// </summary>
    private void TryAddDynamicTopologyPanel()
    {
        if (_pnlDynamicTopologySettings is null) return;

        try
        {
            // Find the flow panel containing GPU/Topology controls
            var flpGpuTopologySettings = FindControlRecursive(this, "_flpGpuTopologySettings") as FlowLayoutPanel;
            if (flpGpuTopologySettings is not null)
            {
                flpGpuTopologySettings.Controls.Add(_pnlDynamicTopologySettings);
                return;
            }

            // Alternative: find by comboBox_TopologyMode's parent
            if (comboBox_TopologyMode?.Parent is FlowLayoutPanel parentFlp)
            {
                parentFlp.Controls.Add(_pnlDynamicTopologySettings);
                return;
            }

            // Fallback: add to tabPageUniPipeline's parent if available
            if (comboBox_TopologyMode?.Parent?.Parent is Control container)
            {
                _pnlDynamicTopologySettings.Dock = DockStyle.Bottom;
                container.Controls.Add(_pnlDynamicTopologySettings);
            }
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[UI] Could not add dynamic topology panel: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Recursively finds a control by name.
    /// </summary>
    private static Control? FindControlRecursive(Control parent, string name)
    {
        if (parent.Name == name) return parent;

        foreach (Control child in parent.Controls)
        {
            var found = FindControlRecursive(child, name);
            if (found is not null) return found;
        }
        return null;
    }

    /// <summary>
    /// Updates visibility of dynamic topology settings panel based on topology mode selection.
    /// </summary>
    private void UpdateDynamicTopologyPanelVisibility()
    {
        if (_pnlDynamicTopologySettings is null) return;

        bool show = _topologyModeSelection == "Dynamic Hard Rewiring";
        _pnlDynamicTopologySettings.Visible = show;
    }

    private void NudDynamicRebuildInterval_ValueChanged(object? sender, EventArgs e)
    {
        if (_nudDynamicRebuildInterval is null) return;
        _dynamicTopologyRebuildInterval = (int)_nudDynamicRebuildInterval.Value;
        SaveDynamicTopologySettings();
        ApplyDynamicTopologySettingsInternal();
    }

    private void NudDynamicDeletionThreshold_ValueChanged(object? sender, EventArgs e)
    {
        if (_nudDynamicDeletionThreshold is null) return;
        _dynamicTopologyDeletionThreshold = (double)_nudDynamicDeletionThreshold.Value;
        SaveDynamicTopologySettings();
        ApplyDynamicTopologySettingsInternal();
    }

    private void NudDynamicBeta_ValueChanged(object? sender, EventArgs e)
    {
        if (_nudDynamicBeta is null) return;
        _dynamicTopologyBeta = (double)_nudDynamicBeta.Value;
        SaveDynamicTopologySettings();
        ApplyDynamicTopologySettingsInternal();
    }

    /// <summary>
    /// Saves dynamic topology settings to persistent storage.
    /// </summary>
    private void SaveDynamicTopologySettings()
    {
        try
        {
            var settings = new DynamicTopologyUiSettings
            {
                RebuildInterval = _dynamicTopologyRebuildInterval,
                DeletionThreshold = _dynamicTopologyDeletionThreshold,
                Beta = _dynamicTopologyBeta,
                InitialWeight = _dynamicTopologyInitialWeight
            };

            var path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RqSim",
                "dynamic_topology_settings.json");

            var dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            string json = System.Text.Json.JsonSerializer.Serialize(settings, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(path, json);
        }
        catch
        {
            // Non-fatal
        }
    }

    /// <summary>
    /// Loads dynamic topology settings from persistent storage.
    /// </summary>
    private void LoadDynamicTopologySettings()
    {
        try
        {
            var path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RqSim",
                "dynamic_topology_settings.json");

            if (!System.IO.File.Exists(path)) return;

            string json = System.IO.File.ReadAllText(path);
            var settings = System.Text.Json.JsonSerializer.Deserialize<DynamicTopologyUiSettings>(json);

            if (settings is not null)
            {
                _dynamicTopologyRebuildInterval = settings.RebuildInterval;
                _dynamicTopologyDeletionThreshold = settings.DeletionThreshold;
                _dynamicTopologyBeta = settings.Beta;
                _dynamicTopologyInitialWeight = settings.InitialWeight;

                // Update UI controls if they exist
                if (_nudDynamicRebuildInterval is not null)
                    _nudDynamicRebuildInterval.Value = _dynamicTopologyRebuildInterval;
                if (_nudDynamicDeletionThreshold is not null)
                    _nudDynamicDeletionThreshold.Value = (decimal)_dynamicTopologyDeletionThreshold;
                if (_nudDynamicBeta is not null)
                    _nudDynamicBeta.Value = (decimal)_dynamicTopologyBeta;
            }
        }
        catch
        {
            // Non-fatal
        }
    }

    /// <summary>
    /// Applies current dynamic topology settings to CSR engine.
    /// </summary>
    private void ApplyDynamicTopologySettingsInternal()
    {
        try
        {
            var csrEngine = _simApi?.CsrCayleyEngine;
            if (csrEngine is null || !csrEngine.IsInitialized) return;

            if (csrEngine.CurrentTopologyMode != GpuCayleyEvolutionEngineCsr.TopologyMode.DynamicHardRewiring)
                return;

            csrEngine.ConfigureDynamicTopology(config =>
            {
                config.RebuildInterval = _dynamicTopologyRebuildInterval;
                config.DeletionThreshold = _dynamicTopologyDeletionThreshold;
                config.Beta = _dynamicTopologyBeta;
                config.InitialWeight = _dynamicTopologyInitialWeight;
            });

            AppendSysConsole($"[DynamicTopology] Settings applied: Interval={_dynamicTopologyRebuildInterval}, Threshold={_dynamicTopologyDeletionThreshold:F4}, Beta={_dynamicTopologyBeta:F2}\n");
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[DynamicTopology] Failed to apply settings: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Settings class for dynamic topology persistence.
    /// </summary>
    private sealed class DynamicTopologyUiSettings
    {
        public int RebuildInterval { get; set; } = 10;
        public double DeletionThreshold { get; set; } = 0.001;
        public double Beta { get; set; } = 1.0;
        public double InitialWeight { get; set; } = 0.5;
    }
}
