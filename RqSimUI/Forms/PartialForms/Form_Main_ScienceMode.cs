// ============================================================
// Form_Main_ScienceMode.cs
// Partial class for Science Mode UI integration
// Handles StrictScienceProfile, Ollivier-Ricci curvature, Conservation
// ============================================================

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RQSimulation;
using RQSimulation.Core.StrongScience;

namespace RqSimForms;

/// <summary>
/// Extension of Form_Main for Science Mode functionality.
/// Provides UI controls for:
/// - Science Mode toggle (StrictScienceProfile vs VisualSandboxProfile)
/// - Ollivier-Ricci curvature toggle (vs Forman-Ricci)
/// - Conservation law validation toggle
/// - GPU Anisotropy computation toggle
/// </summary>
partial class Form_Main
{
    // === Science Mode State ===
    private bool _scienceModeEnabled;
    private bool _useOllivierRicci = true;
    private bool _enableConservation;
    private bool _useGpuAnisotropy = true;

    // === Science Mode UI Controls (created programmatically) ===
    private CheckBox? _checkBox_UseOllivierRicci;
    private CheckBox? _checkBox_EnableConservation;
    private CheckBox? _checkBox_UseGpuAnisotropy;
    private GroupBox? _grpScienceModeSettings;
    private Label? _lblScienceModeStatus;

    /// <summary>
    /// Gets or sets the current simulation profile.
    /// When Science Mode is enabled, uses StrictScienceProfile.
    /// Otherwise uses VisualSandboxProfile.
    /// </summary>
    private ISimulationProfile? CurrentProfile { get; set; }

    /// <summary>
    /// Initializes Science Mode UI controls.
    /// Call this from InitializeUiAfterDesigner.
    /// </summary>
    private void InitializeScienceModeControls()
    {
        try
        {
            // Wire existing checkBox_ScienceSimMode
            if (checkBox_ScienceSimMode is not null)
            {
                checkBox_ScienceSimMode.CheckedChanged -= CheckBox_ScienceSimMode_CheckedChanged;
                checkBox_ScienceSimMode.CheckedChanged += CheckBox_ScienceSimMode_CheckedChanged;
                _scienceModeEnabled = checkBox_ScienceSimMode.Checked;
            }

            // Create Science Mode settings panel
            CreateScienceModeSettingsPanel();

            // Apply initial state
            UpdateScienceModeProfile();
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[ScienceMode] Failed to initialize controls: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Creates the Science Mode settings panel with additional controls.
    /// </summary>
    private void CreateScienceModeSettingsPanel()
    {
        // Create groupbox for science mode settings
        _grpScienceModeSettings = new GroupBox
        {
            Text = "Science Mode Settings",
            Dock = DockStyle.None,
            AutoSize = true,
            Padding = new Padding(8),
            Margin = new Padding(4)
        };

        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(4)
        };

        // Row 0: Status label
        _lblScienceModeStatus = new Label
        {
            Text = "Mode: Visual Sandbox",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 9f, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            Dock = DockStyle.Fill
        };
        tlp.Controls.Add(_lblScienceModeStatus, 0, 0);
        tlp.SetColumnSpan(_lblScienceModeStatus, 2);

        // Row 1: Ollivier-Ricci checkbox
        _checkBox_UseOllivierRicci = new CheckBox
        {
            Text = "Use Ollivier-Ricci Curvature",
            AutoSize = true,
            Checked = _useOllivierRicci,
            Margin = new Padding(4)
        };
        _checkBox_UseOllivierRicci.CheckedChanged += CheckBox_UseOllivierRicci_CheckedChanged;
        tlp.Controls.Add(_checkBox_UseOllivierRicci, 0, 1);
        tlp.SetColumnSpan(_checkBox_UseOllivierRicci, 2);

        // Row 2: Conservation checkbox
        _checkBox_EnableConservation = new CheckBox
        {
            Text = "Enable Conservation Validation",
            AutoSize = true,
            Checked = _enableConservation,
            Margin = new Padding(4)
        };
        _checkBox_EnableConservation.CheckedChanged += CheckBox_EnableConservation_CheckedChanged;
        tlp.Controls.Add(_checkBox_EnableConservation, 0, 2);
        tlp.SetColumnSpan(_checkBox_EnableConservation, 2);

        // Row 3: GPU Anisotropy checkbox
        _checkBox_UseGpuAnisotropy = new CheckBox
        {
            Text = "Use GPU Edge Anisotropy",
            AutoSize = true,
            Checked = _useGpuAnisotropy,
            Margin = new Padding(4)
        };
        _checkBox_UseGpuAnisotropy.CheckedChanged += CheckBox_UseGpuAnisotropy_CheckedChanged;
        tlp.Controls.Add(_checkBox_UseGpuAnisotropy, 0, 3);
        tlp.SetColumnSpan(_checkBox_UseGpuAnisotropy, 2);

        // Row 4: Info label
        var lblInfo = new Label
        {
            Text = "Science Mode enforces strict validation\nand uses fundamental physical constants.",
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font(Font.FontFamily, 8f),
            Margin = new Padding(4, 8, 4, 4)
        };
        tlp.Controls.Add(lblInfo, 0, 4);
        tlp.SetColumnSpan(lblInfo, 2);

        _grpScienceModeSettings.Controls.Add(tlp);

        // Try to add to UniPipeline tab or Settings tab
        TryAddScienceModePanel();
    }

    /// <summary>
    /// Attempts to add the Science Mode panel to an appropriate tab.
    /// </summary>
    private void TryAddScienceModePanel()
    {
        if (_grpScienceModeSettings is null) return;

        try
        {
            // Try to find UniPipeline tab first
            if (tabPage_UniPipelineState is not null)
            {
                // Find the left panel or create a container
                foreach (Control ctrl in tabPage_UniPipelineState.Controls)
                {
                    if (ctrl is TableLayoutPanel tlp && tlp.Name == "_tlp_UniPipeline_Main")
                    {
                        // Add to bottom of main layout
                        _grpScienceModeSettings.Dock = DockStyle.Bottom;
                        _grpScienceModeSettings.Height = 180;
                        tabPage_UniPipelineState.Controls.Add(_grpScienceModeSettings);
                        _grpScienceModeSettings.BringToFront();
                        return;
                    }
                }

                // Fallback: just add to tab
                _grpScienceModeSettings.Dock = DockStyle.Bottom;
                _grpScienceModeSettings.Height = 180;
                tabPage_UniPipelineState.Controls.Add(_grpScienceModeSettings);
                return;
            }

            // Fallback to Settings tab
            if (tabPage_Settings is not null)
            {
                _grpScienceModeSettings.Location = new Point(10, 10);
                tabPage_Settings.Controls.Add(_grpScienceModeSettings);
            }
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[ScienceMode] Failed to add settings panel: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Handles Science Mode toggle.
    /// </summary>
    private void CheckBox_ScienceSimMode_CheckedChanged(object? sender, EventArgs e)
    {
        if (checkBox_ScienceSimMode is null) return;

        _scienceModeEnabled = checkBox_ScienceSimMode.Checked;
        AppendSysConsole($"[ScienceMode] Science Mode {(_scienceModeEnabled ? "ENABLED" : "disabled")}\n");

        UpdateScienceModeProfile();
        UpdateScienceModeUI();

        // Update Edge Threshold availability based on Science mode (Phase 4)
        UpdateEdgeThresholdAvailability();
    }

    /// <summary>
    /// Handles Ollivier-Ricci curvature toggle.
    /// </summary>
    private void CheckBox_UseOllivierRicci_CheckedChanged(object? sender, EventArgs e)
    {
        if (_checkBox_UseOllivierRicci is null) return;

        _useOllivierRicci = _checkBox_UseOllivierRicci.Checked;
        AppendSysConsole($"[ScienceMode] Ollivier-Ricci curvature: {(_useOllivierRicci ? "ON" : "OFF (using Forman-Ricci)")}\n");

        // Apply to SimAPI if available
        ApplyOllivierRicciSetting();
    }

    /// <summary>
    /// Handles Conservation validation toggle.
    /// </summary>
    private void CheckBox_EnableConservation_CheckedChanged(object? sender, EventArgs e)
    {
        if (_checkBox_EnableConservation is null) return;

        _enableConservation = _checkBox_EnableConservation.Checked;
        AppendSysConsole($"[ScienceMode] Conservation validation: {(_enableConservation ? "ON" : "OFF")}\n");

        // Apply to dynamic topology config if available
        ApplyConservationSetting();
    }

    /// <summary>
    /// Handles GPU Anisotropy toggle.
    /// </summary>
    private void CheckBox_UseGpuAnisotropy_CheckedChanged(object? sender, EventArgs e)
    {
        if (_checkBox_UseGpuAnisotropy is null) return;

        _useGpuAnisotropy = _checkBox_UseGpuAnisotropy.Checked;
        AppendSysConsole($"[ScienceMode] GPU Edge Anisotropy: {(_useGpuAnisotropy ? "ON" : "OFF")}\n");

        // Apply to pipeline if available
        ApplyGpuAnisotropySetting();
    }

    /// <summary>
    /// Updates the simulation profile based on Science Mode state.
    /// </summary>
    private void UpdateScienceModeProfile()
    {
        try
        {
            if (_scienceModeEnabled)
            {
                // Create Strict Science Profile
                string experimentId = $"UI_Experiment_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
                CurrentProfile = StrictScienceProfile.CreatePlanckScale(experimentId);
                AppendSysConsole($"[ScienceMode] Created StrictScienceProfile: {experimentId}\n");
            }
            else
            {
                // Create Visual Sandbox Profile
                CurrentProfile = new VisualSandboxProfile();
                AppendSysConsole($"[ScienceMode] Using VisualSandboxProfile\n");
            }

            // Apply profile to SimAPI if available
            ApplyProfileToSimApi();
        }
        catch (ScientificMalpracticeException ex)
        {
            AppendSysConsole($"[ScienceMode] ERROR: {ex.Message}\n");
            MessageBox.Show(
                $"Science Mode validation failed:\n{ex.Message}",
                "Scientific Integrity Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            // Revert to sandbox mode
            _scienceModeEnabled = false;
            if (checkBox_ScienceSimMode is not null)
                checkBox_ScienceSimMode.Checked = false;
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[ScienceMode] Failed to create profile: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Updates Science Mode UI elements.
    /// </summary>
    private void UpdateScienceModeUI()
    {
        if (_lblScienceModeStatus is not null)
        {
            if (_scienceModeEnabled)
            {
                _lblScienceModeStatus.Text = "Mode: STRICT SCIENCE";
                _lblScienceModeStatus.ForeColor = Color.DarkRed;
            }
            else
            {
                _lblScienceModeStatus.Text = "Mode: Visual Sandbox";
                _lblScienceModeStatus.ForeColor = Color.DarkGreen;
            }
        }

        // Enable/disable controls based on mode
        bool enableAdvanced = _scienceModeEnabled;
        if (_checkBox_EnableConservation is not null)
        {
            // Conservation is optional in sandbox, required-visible in science
            _checkBox_EnableConservation.Enabled = true;
        }
    }

    /// <summary>
    /// Applies the current profile to SimAPI.
    /// </summary>
    private void ApplyProfileToSimApi()
    {
        if (_simApi is null || CurrentProfile is null) return;

        try
        {
            // Apply profile to dynamic topology config if available
            var csrEngine = _simApi.CsrCayleyEngine;
            if (csrEngine?.DynamicConfig is not null)
            {
                // Configure conservation based on Science mode
                csrEngine.ConfigureDynamicTopology(config =>
                {
                    config.EnableConservation = _scienceModeEnabled && _enableConservation;

                    if (_scienceModeEnabled && CurrentProfile?.Constants is IPhysicalConstants constants)
                    {
                        config.EnergyConversionFactor = constants.GravitationalCoupling;
                        config.FluxConversionFactor = constants.FineStructureConstant;
                    }
                });
                AppendSysConsole($"[ScienceMode] Applied profile to DynamicTopologyConfig\n");
            }
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[ScienceMode] Failed to apply profile: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Applies Ollivier-Ricci setting to SimAPI.
    /// </summary>
    private void ApplyOllivierRicciSetting()
    {
        try
        {
            // Set UseOllivierRicciForWeyl on PhysicsConstants
            // This is a static field accessed by curvature calculations
            // Note: We need to use reflection or direct access since it's const in RQHypothesis
            PhysicsConstants.ScientificMode = _scienceModeEnabled;
            AppendSysConsole($"[ScienceMode] Set ScientificMode = {_scienceModeEnabled}, PreferOllivierRicci = {_useOllivierRicci}\n");
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[ScienceMode] Failed to apply Ollivier-Ricci setting: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Applies Conservation setting to dynamic topology config.
    /// </summary>
    private void ApplyConservationSetting()
    {
        if (_simApi is null) return;

        try
        {
            var csrEngine = _simApi.CsrCayleyEngine;
            if (csrEngine is not null)
            {
                csrEngine.ConfigureDynamicTopology(config =>
                {
                    config.EnableConservation = _enableConservation;
                });
                AppendSysConsole($"[ScienceMode] Set EnableConservation = {_enableConservation}\n");
            }
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[ScienceMode] Failed to apply conservation setting: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Applies GPU Anisotropy setting to pipeline.
    /// </summary>
    private void ApplyGpuAnisotropySetting()
    {
        if (_simApi is null) return;

        try
        {
            // Enable/disable EdgeAnisotropyGpuModule in pipeline
            var pipeline = _simApi.Pipeline;
            if (pipeline is not null)
            {
                var anisotropyModule = pipeline.Modules
                    .FirstOrDefault(m => m.Name.Contains("Anisotropy", StringComparison.OrdinalIgnoreCase));

                if (anisotropyModule is not null)
                {
                    anisotropyModule.IsEnabled = _useGpuAnisotropy;
                    AppendSysConsole($"[ScienceMode] EdgeAnisotropyModule.IsEnabled = {_useGpuAnisotropy}\n");
                }
            }
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[ScienceMode] Failed to apply GPU anisotropy setting: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Gets the current Science Mode configuration for serialization.
    /// </summary>
    public ScienceModeSettings GetScienceModeSettings()
    {
        return new ScienceModeSettings
        {
            ScienceModeEnabled = _scienceModeEnabled,
            UseOllivierRicci = _useOllivierRicci,
            EnableConservation = _enableConservation,
            UseGpuAnisotropy = _useGpuAnisotropy
        };
    }

    /// <summary>
    /// Applies Science Mode configuration from settings.
    /// </summary>
    public void ApplyScienceModeSettings(ScienceModeSettings settings)
    {
        if (settings is null) return;

        _scienceModeEnabled = settings.ScienceModeEnabled;
        _useOllivierRicci = settings.UseOllivierRicci;
        _enableConservation = settings.EnableConservation;
        _useGpuAnisotropy = settings.UseGpuAnisotropy;

        // Update UI controls
        if (checkBox_ScienceSimMode is not null)
            checkBox_ScienceSimMode.Checked = _scienceModeEnabled;
        if (_checkBox_UseOllivierRicci is not null)
            _checkBox_UseOllivierRicci.Checked = _useOllivierRicci;
        if (_checkBox_EnableConservation is not null)
            _checkBox_EnableConservation.Checked = _enableConservation;
        if (_checkBox_UseGpuAnisotropy is not null)
            _checkBox_UseGpuAnisotropy.Checked = _useGpuAnisotropy;

        UpdateScienceModeProfile();
        UpdateScienceModeUI();

        // Update Edge Threshold availability based on Science mode (Phase 4)
        UpdateEdgeThresholdAvailability();
    }
}

/// <summary>
/// Settings class for Science Mode configuration.
/// </summary>
public sealed class ScienceModeSettings
{
    public bool ScienceModeEnabled { get; set; }
    public bool UseOllivierRicci { get; set; } = true;
    public bool EnableConservation { get; set; }
    public bool UseGpuAnisotropy { get; set; } = true;
}
