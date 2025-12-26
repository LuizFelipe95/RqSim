using System.Text.Json;
using System.Text.Json.Serialization;

namespace RqSimForms;

/// <summary>
/// Stores all Form_Main UI settings for persistence across sessions.
/// Saved to JSON file on form close, loaded on form load.
/// </summary>
public sealed class FormSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RqSim", "form_settings.json");

    // === Simulation Parameters ===
    public int NodeCount { get; set; } = 250;
    public int TargetDegree { get; set; } = 8;
    public double InitialExcitedProb { get; set; } = 0.10;
    public double LambdaState { get; set; } = 0.5;
    public double Temperature { get; set; } = 10.0;
    public double EdgeTrialProb { get; set; } = 0.02;
    public double MeasurementThreshold { get; set; } = 0.30;
    public int TotalSteps { get; set; } = 500000;
    public int FractalLevels { get; set; } = 0;
    public int FractalBranchFactor { get; set; } = 0;
    public bool AutoTuning { get; set; } = false;

    // === Physics Constants ===
    public double InitialEdgeProb { get; set; } = 0.035;
    public double GravitationalCoupling { get; set; } = 0.010;
    public double VacuumEnergyScale { get; set; } = 0.00005;
    public double DecoherenceRate { get; set; } = 0.005;
    public double HotStartTemperature { get; set; } = 6.0;
    public double AdaptiveThresholdSigma { get; set; } = 1.5;
    public int WarmupDuration { get; set; } = 200;
    public double GravityTransitionDuration { get; set; } = 137.0;

    // === Physics Modules (checkboxes) ===
    public bool UseQuantumDriven { get; set; } = true;
    public bool UseSpacetimePhysics { get; set; } = true;
    public bool UseSpinorField { get; set; } = true;
    public bool UseVacuumFluctuations { get; set; } = true;
    public bool UseBlackHolePhysics { get; set; } = true;
    public bool UseYangMillsGauge { get; set; } = true;
    public bool UseEnhancedKleinGordon { get; set; } = true;
    public bool UseInternalTime { get; set; } = true;
    public bool UseSpectralGeometry { get; set; } = true;
    public bool UseQuantumGraphity { get; set; } = true;
    public bool UseRelationalTime { get; set; } = true;
    public bool UseRelationalYangMills { get; set; } = true;
    public bool UseNetworkGravity { get; set; } = true;
    public bool UseUnifiedPhysicsStep { get; set; } = true;
    public bool UseEnforceGaugeConstraints { get; set; } = true;
    public bool UseCausalRewiring { get; set; } = true;
    public bool UseTopologicalProtection { get; set; } = true;
    public bool UseValidateEnergyConservation { get; set; } = true;
    public bool UseMexicanHatPotential { get; set; } = true;
    public bool UseGeometryMomenta { get; set; } = true;
    public bool UseTopologicalCensorship { get; set; } = true;

    // === GPU Settings ===
    public bool EnableGPU { get; set; } = true;
    public bool UseMultiGPU { get; set; } = false;
    public int GPUComputeEngineIndex { get; set; } = 0;
    public int GPUIndex { get; set; } = 0;
    public int MultiGpuSpectralWalkers { get; set; } = 10000;

    // === UI Settings ===
    public int MaxFPS { get; set; } = 10;
    public int CPUThreads { get; set; } = 8;
    public bool AutoScrollConsole { get; set; } = true;
    public bool ShowHeavyOnly { get; set; } = false;
    public int WeightThresholdIndex { get; set; } = 0;
    public int PresetIndex { get; set; } = 0;
    public int ExperimentIndex { get; set; } = 0;

    // === Window State ===
    public int WindowWidth { get; set; } = 1359;
    public int WindowHeight { get; set; } = 737;
    public int WindowX { get; set; } = 100;
    public int WindowY { get; set; } = 100;
    public bool IsMaximized { get; set; } = false;
    public int SelectedTabIndex { get; set; } = 0;

    // === Background Plugins Settings ===
    /// <summary>
    /// List of enabled background plugin type names.
    /// </summary>
    public List<string> EnabledBackgroundPlugins { get; set; } = [];
    
    /// <summary>
    /// GPU assignments for background plugins (plugin type name -> GPU index).
    /// </summary>
    public Dictionary<string, int> PluginGpuAssignments { get; set; } = [];
    
    /// <summary>
    /// Kernel counts for background plugins (plugin type name -> kernel count).
    /// </summary>
    public Dictionary<string, int> PluginKernelCounts { get; set; } = [];
    
    /// <summary>
    /// Path to last used plugin configuration file.
    /// </summary>
    public string? LastPluginConfigPath { get; set; }

    /// <summary>
    /// Load settings from file. Returns default settings if file doesn't exist.
    /// </summary>
    public static FormSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new FormSettings();

            string json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<FormSettings>(json) ?? new FormSettings();
        }
        catch
        {
            return new FormSettings();
        }
    }

    /// <summary>
    /// Save settings to file.
    /// </summary>
    public void Save()
    {
        try
        {
            string? directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail - settings are not critical
        }
    }
}
