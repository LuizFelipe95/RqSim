using RqSimForms.Forms.Interfaces.AutoTuning;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RqSimForms;

/// <summary>
/// Auto-tuning UI controls and logic for Form_Main.
/// Located in splitMain.Panel2 on tabPage_Summary.
/// </summary>
public partial class Form_Main
{
    // ============================================================
    // AUTO-TUNING UI CONTROLS (created in Designer or dynamically)
    // ============================================================

    private GroupBox? grpAutoTuning;
    private Label? lblAutoTuneStatus;
    private Label? lblAutoTuneSpectralDim;
    private Label? valAutoTuneSpectralDim;
    private Label? lblAutoTuneConfidence;
    private Label? valAutoTuneConfidence;
    private Label? lblAutoTuneGravity;
    private Label? valAutoTuneGravity;
    private Label? lblAutoTuneDecoherence;
    private Label? valAutoTuneDecoherence;
    private Label? lblAutoTuneEnergy;
    private Label? valAutoTuneEnergy;
    private Label? lblAutoTuneCluster;
    private Label? valAutoTuneCluster;

    private NumericUpDown? numAutoTuneTargetDim;
    private NumericUpDown? numAutoTuneTolerance;
    private NumericUpDown? numAutoTuneInterval;

    private CheckBox? chkAutoTuneHybridSpectral;
    private CheckBox? chkAutoTuneManageEnergy;
    private CheckBox? chkAutoTuneAllowInjection;

    private ComboBox? cmbAutoTunePreset;

    // ============================================================
    // INITIALIZATION
    // ============================================================

    /// <summary>
    /// Initializes the auto-tuning UI controls in splitMain.Panel2.
    /// Call this from Form_Main_Load or Form_Main_Shown.
    /// </summary>
    private void InitializeAutoTuningUI()
    {
        // Create the main GroupBox
        grpAutoTuning = new GroupBox
        {
            Text = "Auto-Tuning (d_S ? 4D)",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(3),
            Padding = new Padding(8, 24, 8, 8) // Increased Top padding to avoid text overlap
        };

        // Create TableLayoutPanel for organized layout
        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            RowCount = 15,
            Margin = new Padding(4, 8, 4, 8), // Top/bottom margin inside GroupBox
            Padding = new Padding(2)
        };

        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

        for (int i = 0; i < 15; i++)
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));

        int row = 0;

        // === Status Display ===
        lblAutoTuneStatus = new Label { Text = "Status:", Anchor = AnchorStyles.Left };
        valAutoTuneSpectralDim = new Label { Text = "Disabled", Anchor = AnchorStyles.Left, ForeColor = Color.Gray };
        tlp.Controls.Add(lblAutoTuneStatus, 0, row);
        tlp.Controls.Add(valAutoTuneSpectralDim, 1, row);
        row++;

        // Spectral Dimension
        lblAutoTuneSpectralDim = new Label { Text = "Spectral Dim (d_S):", Anchor = AnchorStyles.Left };
        valAutoTuneSpectralDim = new Label { Text = "—", Anchor = AnchorStyles.Left, Font = new Font(Font, FontStyle.Bold) };
        tlp.Controls.Add(lblAutoTuneSpectralDim, 0, row);
        tlp.Controls.Add(valAutoTuneSpectralDim, 1, row);
        row++;

        // Confidence
        lblAutoTuneConfidence = new Label { Text = "Confidence:", Anchor = AnchorStyles.Left };
        valAutoTuneConfidence = new Label { Text = "—", Anchor = AnchorStyles.Left };
        tlp.Controls.Add(lblAutoTuneConfidence, 0, row);
        tlp.Controls.Add(valAutoTuneConfidence, 1, row);
        row++;

        // Gravity
        lblAutoTuneGravity = new Label { Text = "Gravity (G):", Anchor = AnchorStyles.Left };
        valAutoTuneGravity = new Label { Text = "—", Anchor = AnchorStyles.Left };
        tlp.Controls.Add(lblAutoTuneGravity, 0, row);
        tlp.Controls.Add(valAutoTuneGravity, 1, row);
        row++;

        // Decoherence
        lblAutoTuneDecoherence = new Label { Text = "Decoherence:", Anchor = AnchorStyles.Left };
        valAutoTuneDecoherence = new Label { Text = "—", Anchor = AnchorStyles.Left };
        tlp.Controls.Add(lblAutoTuneDecoherence, 0, row);
        tlp.Controls.Add(valAutoTuneDecoherence, 1, row);
        row++;

        // Energy Status
        lblAutoTuneEnergy = new Label { Text = "Energy Status:", Anchor = AnchorStyles.Left };
        valAutoTuneEnergy = new Label { Text = "—", Anchor = AnchorStyles.Left };
        tlp.Controls.Add(lblAutoTuneEnergy, 0, row);
        tlp.Controls.Add(valAutoTuneEnergy, 1, row);
        row++;

        // Cluster Status
        lblAutoTuneCluster = new Label { Text = "Cluster Status:", Anchor = AnchorStyles.Left };
        valAutoTuneCluster = new Label { Text = "—", Anchor = AnchorStyles.Left };
        tlp.Controls.Add(lblAutoTuneCluster, 0, row);
        tlp.Controls.Add(valAutoTuneCluster, 1, row);
        row++;

        // === Separator ===
        var separator = new Label { BorderStyle = BorderStyle.Fixed3D, Height = 2, Dock = DockStyle.Fill };
        tlp.Controls.Add(separator, 0, row);
        tlp.SetColumnSpan(separator, 2);
        row++;

        // === Configuration ===
        // Target Dimension
        var lblTargetDim = new Label { Text = "Target d_S:", Anchor = AnchorStyles.Left };
        numAutoTuneTargetDim = new NumericUpDown
        {
            Minimum = 2.0m,
            Maximum = 6.0m,
            DecimalPlaces = 1,
            Increment = 0.5m,
            Value = 4.0m,
            Dock = DockStyle.Fill
        };
        numAutoTuneTargetDim.ValueChanged += NumAutoTuneTargetDim_ValueChanged;
        tlp.Controls.Add(lblTargetDim, 0, row);
        tlp.Controls.Add(numAutoTuneTargetDim, 1, row);
        row++;

        // Tolerance
        var lblTolerance = new Label { Text = "Tolerance:", Anchor = AnchorStyles.Left };
        numAutoTuneTolerance = new NumericUpDown
        {
            Minimum = 0.1m,
            Maximum = 2.0m,
            DecimalPlaces = 1,
            Increment = 0.1m,
            Value = 0.5m,
            Dock = DockStyle.Fill
        };
        numAutoTuneTolerance.ValueChanged += NumAutoTuneTolerance_ValueChanged;
        tlp.Controls.Add(lblTolerance, 0, row);
        tlp.Controls.Add(numAutoTuneTolerance, 1, row);
        row++;

        // Interval
        var lblInterval = new Label { Text = "Tune Interval:", Anchor = AnchorStyles.Left };
        numAutoTuneInterval = new NumericUpDown
        {
            Minimum = 10,
            Maximum = 1000,
            Increment = 50,
            Value = 100,
            Dock = DockStyle.Fill
        };
        numAutoTuneInterval.ValueChanged += NumAutoTuneInterval_ValueChanged;
        tlp.Controls.Add(lblInterval, 0, row);
        tlp.Controls.Add(numAutoTuneInterval, 1, row);
        row++;

        // Preset
        var lblPreset = new Label { Text = "Preset:", Anchor = AnchorStyles.Left };
        cmbAutoTunePreset = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill
        };
        cmbAutoTunePreset.Items.AddRange(["Default", "Aggressive", "Conservative", "Long Run"]);
        cmbAutoTunePreset.SelectedIndex = 0;
        cmbAutoTunePreset.SelectedIndexChanged += CmbAutoTunePreset_SelectedIndexChanged;
        tlp.Controls.Add(lblPreset, 0, row);
        tlp.Controls.Add(cmbAutoTunePreset, 1, row);
        row++;

        // === Checkboxes ===
        chkAutoTuneHybridSpectral = new CheckBox
        {
            Text = "Hybrid Spectral Method",
            Checked = true,
            Anchor = AnchorStyles.Left
        };
        chkAutoTuneHybridSpectral.CheckedChanged += ChkAutoTuneHybridSpectral_CheckedChanged;
        tlp.Controls.Add(chkAutoTuneHybridSpectral, 0, row);
        tlp.SetColumnSpan(chkAutoTuneHybridSpectral, 2);
        row++;

        chkAutoTuneManageEnergy = new CheckBox
        {
            Text = "Manage Vacuum Energy",
            Checked = true,
            Anchor = AnchorStyles.Left
        };
        chkAutoTuneManageEnergy.CheckedChanged += ChkAutoTuneManageEnergy_CheckedChanged;
        tlp.Controls.Add(chkAutoTuneManageEnergy, 0, row);
        tlp.SetColumnSpan(chkAutoTuneManageEnergy, 2);
        row++;

        // Auto-inject energy when low (prevents simulation from stopping)
        chkAutoTuneAllowInjection = new CheckBox
        {
            Text = "Auto-Inject Energy (Small Graphs)",
            Checked = true,
            Anchor = AnchorStyles.Left
        };
        chkAutoTuneAllowInjection.CheckedChanged += ChkAutoTuneAllowInjection_CheckedChanged;
        tlp.Controls.Add(chkAutoTuneAllowInjection, 0, row);
        tlp.SetColumnSpan(chkAutoTuneAllowInjection, 2);

        grpAutoTuning.Controls.Add(tlp);

        // Add to splitMain.Panel2
        splitpanels_Add.Panel2.Controls.Add(grpAutoTuning);

        // Initially disabled until auto-tuning is enabled
        grpAutoTuning.Enabled = false;
    }

    // ============================================================
    // EVENT HANDLERS
    // ============================================================

    /// <summary>
    /// Handles the main auto-tuning enable/disable checkbox.
    /// </summary>
    private void CheckBox_AutoTuning_CheckedChanged(object? sender, EventArgs e)
    {
        bool enabled = checkBox_AutoTuning.Checked;
        _simApi.AutoTuningEnabled = enabled;

        if (grpAutoTuning != null)
        {
            grpAutoTuning.Enabled = enabled;
        }

        if (enabled)
        {
            // Sync current settings to AutoTuningConfig
            SyncLiveConfigToAutoTuning();

            // Initialize auto-tuning system
            _simApi.InitializeAutoTuning();

            AppendSimConsole("[AUTO-TUNE] Auto-tuning ENABLED. Target d_S = 4.0\n");
        }
        else
        {
            AppendSimConsole("[AUTO-TUNE] Auto-tuning DISABLED.\n");
        }

        UpdateAutoTuningStatusDisplay();
    }

    private void NumAutoTuneTargetDim_ValueChanged(object? sender, EventArgs e)
    {
        if (numAutoTuneTargetDim != null)
        {
            _simApi.LiveConfig.AutoTuneTargetDimension = (double)numAutoTuneTargetDim.Value;
            _simApi.AutoTuningConfig.TargetSpectralDimension = (double)numAutoTuneTargetDim.Value;
        }
    }

    private void NumAutoTuneTolerance_ValueChanged(object? sender, EventArgs e)
    {
        if (numAutoTuneTolerance != null)
        {
            _simApi.LiveConfig.AutoTuneDimensionTolerance = (double)numAutoTuneTolerance.Value;
            _simApi.AutoTuningConfig.SpectralDimensionTolerance = (double)numAutoTuneTolerance.Value;
        }
    }

    private void NumAutoTuneInterval_ValueChanged(object? sender, EventArgs e)
    {
        if (numAutoTuneInterval != null)
        {
            _simApi.AutoTuneInterval = (int)numAutoTuneInterval.Value;
            _simApi.AutoTuningConfig.TuningInterval = (int)numAutoTuneInterval.Value;
        }
    }

    private void CmbAutoTunePreset_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbAutoTunePreset == null) return;

        AutoTuningConfig newConfig = cmbAutoTunePreset.SelectedIndex switch
        {
            0 => AutoTuningConfig.CreateDefault(),
            1 => AutoTuningConfig.CreateAggressive(),
            2 => AutoTuningConfig.CreateConservative(),
            3 => AutoTuningConfig.CreateLongRun(),
            _ => AutoTuningConfig.CreateDefault()
        };

        _simApi.AutoTuningConfig = newConfig;

        // Update UI controls to reflect new preset
        UpdateAutoTuningControlsFromConfig(newConfig);

        AppendSimConsole($"[AUTO-TUNE] Preset changed to: {cmbAutoTunePreset.SelectedItem}\n");
    }

    private void ChkAutoTuneHybridSpectral_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkAutoTuneHybridSpectral != null)
        {
            _simApi.LiveConfig.AutoTuneUseHybridSpectral = chkAutoTuneHybridSpectral.Checked;
            _simApi.AutoTuningConfig.UseHybridSpectralComputation = chkAutoTuneHybridSpectral.Checked;
        }
    }

    private void ChkAutoTuneManageEnergy_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkAutoTuneManageEnergy != null)
        {
            _simApi.LiveConfig.AutoTuneManageVacuumEnergy = chkAutoTuneManageEnergy.Checked;
            _simApi.AutoTuningConfig.EnableVacuumEnergyManagement = chkAutoTuneManageEnergy.Checked;
        }
    }

    private void ChkAutoTuneAllowInjection_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkAutoTuneAllowInjection != null)
        {
            _simApi.AutoTuningConfig.AllowEmergencyEnergyInjection = chkAutoTuneAllowInjection.Checked;
            _simApi.AutoTuningConfig.EnableProactiveEnergyInjection = chkAutoTuneAllowInjection.Checked;
        }
    }

    // ============================================================
    // STATUS UPDATES
    // ============================================================

    /// <summary>
    /// Updates the auto-tuning status display in the UI.
    /// Call this from the UI update timer.
    /// </summary>
    private void UpdateAutoTuningStatusDisplay()
    {
        if (!_simApi.AutoTuningEnabled)
        {
            if (valAutoTuneSpectralDim != null)
                valAutoTuneSpectralDim.Text = "Disabled";
            return;
        }

        // Spectral dimension with color coding
        double dS = _simApi.CachedSpectralDimension;
        double conf = _simApi.SpectralConfidence;

        if (valAutoTuneSpectralDim != null)
        {
            valAutoTuneSpectralDim.Text = $"{dS:F2}";
            valAutoTuneSpectralDim.ForeColor = GetSpectralDimensionColor(dS);
        }

        if (valAutoTuneConfidence != null)
        {
            valAutoTuneConfidence.Text = $"{conf:P0}";
            valAutoTuneConfidence.ForeColor = conf >= 0.7 ? Color.Green :
                                               conf >= 0.4 ? Color.Orange : Color.Red;
        }

        // Gravity
        if (valAutoTuneGravity != null)
        {
            valAutoTuneGravity.Text = $"{_simApi.LiveConfig.GravitationalCoupling:F4}";
            if (_simApi.GravityController.InEmergencyMode)
            {
                valAutoTuneGravity.ForeColor = Color.Red;
                valAutoTuneGravity.Text += " [EMERG]";
            }
            else
            {
                valAutoTuneGravity.ForeColor = Color.Black;
            }
        }

        // Decoherence
        if (valAutoTuneDecoherence != null)
        {
            valAutoTuneDecoherence.Text = $"{_simApi.LiveConfig.DecoherenceRate:F4}";
        }

        // Energy status
        if (valAutoTuneEnergy != null)
        {
            var status = _simApi.VacuumManager.Status;
            valAutoTuneEnergy.Text = status.ToString();
            valAutoTuneEnergy.ForeColor = status switch
            {
                EnergyStatus.Healthy => Color.Green,
                EnergyStatus.Low => Color.Orange,
                EnergyStatus.Warning => Color.OrangeRed,
                EnergyStatus.Critical => Color.Red,
                _ => Color.Black
            };
        }

        // Cluster status
        if (valAutoTuneCluster != null)
        {
            var status = _simApi.ClusterController.Status;
            valAutoTuneCluster.Text = status.ToString();
            valAutoTuneCluster.ForeColor = status switch
            {
                ClusterStatus.Healthy => Color.Green,
                ClusterStatus.Giant => Color.Orange,
                ClusterStatus.Emergency => Color.OrangeRed,
                ClusterStatus.Extreme => Color.Red,
                ClusterStatus.TooFew => Color.Blue,
                _ => Color.Black
            };
        }
    }

    /// <summary>
    /// Gets color based on spectral dimension health.
    /// </summary>
    private static Color GetSpectralDimensionColor(double dS)
    {
        if (dS >= 3.5 && dS <= 4.5)
            return Color.Green; // Healthy 4D

        if (dS >= 2.5 && dS <= 5.5)
            return Color.Orange; // Warning zone

        return Color.Red; // Critical
    }

    // ============================================================
    // SYNC HELPERS
    // ============================================================

    /// <summary>
    /// Syncs LiveConfig auto-tuning parameters to AutoTuningConfig.
    /// </summary>
    private void SyncLiveConfigToAutoTuning()
    {
        _simApi.LiveConfig.ApplyToAutoTuningConfig(_simApi.AutoTuningConfig);
    }

    /// <summary>
    /// Updates UI controls to reflect an AutoTuningConfig.
    /// </summary>
    private void UpdateAutoTuningControlsFromConfig(AutoTuningConfig config)
    {
        if (numAutoTuneTargetDim != null)
            numAutoTuneTargetDim.Value = (decimal)config.TargetSpectralDimension;

        if (numAutoTuneTolerance != null)
            numAutoTuneTolerance.Value = (decimal)config.SpectralDimensionTolerance;

        if (numAutoTuneInterval != null)
            numAutoTuneInterval.Value = config.TuningInterval;

        if (chkAutoTuneHybridSpectral != null)
            chkAutoTuneHybridSpectral.Checked = config.UseHybridSpectralComputation;

        if (chkAutoTuneManageEnergy != null)
            chkAutoTuneManageEnergy.Checked = config.EnableVacuumEnergyManagement;

        if (chkAutoTuneAllowInjection != null)
            chkAutoTuneAllowInjection.Checked = config.AllowEmergencyEnergyInjection || config.EnableProactiveEnergyInjection;
    }
}
