using RqSimGraphEngine.Experiments;
using RQSimulation;
using System;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    // === Experiment Manager ===
    private IExperiment? _selectedExperiment;

    /// <summary>
    /// Initializes simulation presets in comboBox_Presets.
    /// Call this in constructor after InitializeComponent.
    /// </summary>
    private void InitializePresets()
    {
        comboBox_Presets.Items.Clear();
        comboBox_Presets.Items.Add("Custom");
        comboBox_Presets.Items.Add("Quick Test (100 nodes, 1K steps)");
        comboBox_Presets.Items.Add("Small (300 nodes, 5K steps)");
        comboBox_Presets.Items.Add("Medium (500 nodes, 10K steps)");
        comboBox_Presets.Items.Add("Large (1000 nodes, 20K steps)");
        comboBox_Presets.Items.Add("XL Research (2000 nodes, 50K steps)");
        comboBox_Presets.Items.Add("Stable Clustering (optimized for d_S)");
        comboBox_Presets.Items.Add("High Energy (strong gravity)");
        comboBox_Presets.Items.Add("Quantum Dominated");
        comboBox_Presets.SelectedIndex = 0;
        comboBox_Presets.SelectedIndexChanged += ComboBox_Presets_SelectedIndexChanged;
    }

    /// <summary>
    /// Handles preset selection and applies corresponding configuration to UI controls.
    /// </summary>
    private void ComboBox_Presets_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (comboBox_Presets.SelectedIndex <= 0) return; // "Custom" selected

        string preset = comboBox_Presets.SelectedItem?.ToString() ?? "";

        // Default base values
        int nodeCount = 300;
        int totalSteps = 5000;
        int targetDegree = 8;
        double initialEdgeProb = 0.08;
        double initialExcitedProb = 0.10;
        double gravitationalCoupling = 0.025;
        double hotStartTemperature = 5.0;
        double decoherenceRate = 0.005;
        double temperature = 10.0;
        double edgeTrialProb = 0.02;
        double vacuumEnergyScale = 0.0001;
        double annealingCoolingRate = 0.995;
        double adaptiveThresholdSigma = 1.5;
        double warmupDuration = 200;
        double gravityTransitionDuration = 137;

        // Physics modules defaults
        bool useSpectralGeometry = true;
        bool useNetworkGravity = true;
        bool useQuantumDriven = true;
        bool useSpinorField = false;
        bool useVacuumFluctuations = true;

        // Apply preset-specific values
        if (preset.Contains("Quick Test"))
        {
            nodeCount = 100;
            totalSteps = 1000;
            targetDegree = 6;
            warmupDuration = 50;
            gravityTransitionDuration = 50;
        }
        else if (preset.Contains("Small"))
        {
            nodeCount = 300;
            totalSteps = 5000;
            targetDegree = 8;
        }
        else if (preset.Contains("Medium"))
        {
            nodeCount = 500;
            totalSteps = 10000;
            targetDegree = 10;
            warmupDuration = 300;
            gravityTransitionDuration = 200;
        }
        else if (preset.Contains("Large"))
        {
            nodeCount = 1000;
            totalSteps = 20000;
            targetDegree = 12;
            warmupDuration = 500;
            gravityTransitionDuration = 300;
            gravitationalCoupling = 0.02; // Lower G for larger graphs
            decoherenceRate = 0.003;
        }
        else if (preset.Contains("XL Research"))
        {
            nodeCount = 2000;
            totalSteps = 50000;
            targetDegree = 15;
            warmupDuration = 1000;
            gravityTransitionDuration = 500;
            gravitationalCoupling = 0.015;
            decoherenceRate = 0.002;
            initialEdgeProb = 0.05; // Sparser initial graph
        }
        else if (preset.Contains("Stable Clustering"))
        {
            // Optimized for stable spectral dimension 2-4
            nodeCount = 500;
            totalSteps = 15000;
            targetDegree = 10;
            gravitationalCoupling = 0.02;
            hotStartTemperature = 3.0;
            decoherenceRate = 0.008;
            adaptiveThresholdSigma = 1.2; // Tighter threshold
            warmupDuration = 400;
            gravityTransitionDuration = 250;
            annealingCoolingRate = 0.998; // Slower cooling
        }
        else if (preset.Contains("High Energy"))
        {
            // Strong gravity, more structure formation
            nodeCount = 400;
            totalSteps = 10000;
            targetDegree = 12;
            gravitationalCoupling = 0.08; // Strong gravity
            hotStartTemperature = 8.0;
            decoherenceRate = 0.003;
            temperature = 15.0;
            useSpinorField = true;
        }
        else if (preset.Contains("Quantum Dominated"))
        {
            // Emphasize quantum effects
            nodeCount = 300;
            totalSteps = 8000;
            targetDegree = 8;
            gravitationalCoupling = 0.01; // Weak gravity
            hotStartTemperature = 2.0;
            decoherenceRate = 0.015; // Higher decoherence
            useQuantumDriven = true;
            useSpinorField = true;
            useVacuumFluctuations = true;
        }

        // Apply values to UI controls (suppress events temporarily)
        SuspendLayout();
        try
        {
            // Simulation Parameters
            numNodeCount.Value = nodeCount;
            numTotalSteps.Value = totalSteps;
            numTargetDegree.Value = targetDegree;
            numInitialExcitedProb.Value = (decimal)initialExcitedProb;
            numTemperature.Value = (decimal)temperature;
            numEdgeTrialProb.Value = (decimal)edgeTrialProb;

            // Physics Constants
            numInitialEdgeProb.Value = (decimal)initialEdgeProb;
            numGravitationalCoupling.Value = (decimal)gravitationalCoupling;
            numVacuumEnergyScale.Value = (decimal)vacuumEnergyScale;
            numDecoherenceRate.Value = (decimal)decoherenceRate;
            numHotStartTemperature.Value = (decimal)hotStartTemperature;
            numAdaptiveThresholdSigma.Value = (decimal)adaptiveThresholdSigma;
            numWarmupDuration.Value = (decimal)warmupDuration;
            numGravityTransitionDuration.Value = (decimal)gravityTransitionDuration;

            // Physics Modules
            chkSpectralGeometry.Checked = useSpectralGeometry;
            chkNetworkGravity.Checked = useNetworkGravity;
            chkQuantumDriven.Checked = useQuantumDriven;
            chkSpinorField.Checked = useSpinorField;
            chkVacuumFluctuations.Checked = useVacuumFluctuations;
            // Hot start annealing removed - controlled by simulation core
        }
        finally
        {
            ResumeLayout();
        }

        AppendSimConsole($"[Preset] Применён: {preset}\n");
        AppendSimConsole($"  Nodes={nodeCount}, Steps={totalSteps}, G={gravitationalCoupling}, T_hot={hotStartTemperature}\n");
    }

    /// <summary>
    /// Initializes experiment selection combobox with available experiments.
    /// </summary>
    private void InitializeExperiments()
    {
        comboBox_Experiments.Items.Clear();
        comboBox_Experiments.Items.Add("(Custom / Manual)");

        foreach (var experiment in ExperimentFactory.AvailableExperiments)
        {
            comboBox_Experiments.Items.Add(experiment.Name);
        }

        comboBox_Experiments.SelectedIndex = 0;
        comboBox_Experiments.SelectedIndexChanged += ComboBox_Experiments_SelectedIndexChanged;
    }

    /// <summary>
    /// Handles experiment selection change.
    /// Loads experiment configuration into UI controls.
    /// </summary>
    private void ComboBox_Experiments_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (comboBox_Experiments.SelectedIndex <= 0)
        {
            // Custom / Manual mode - clear experiment
            _selectedExperiment = null;
            _simApi.SetCustomInitializer(null);
            AppendSimConsole("[Experiment] Manual mode selected - using custom parameters\n");
            return;
        }

        string experimentName = comboBox_Experiments.SelectedItem?.ToString() ?? "";
        var experiment = ExperimentFactory.GetByName(experimentName);

        if (experiment != null)
        {
            LoadExperiment(experiment);
        }
    }

    /// <summary>
    /// Loads an experiment configuration into UI controls.
    /// 
    /// This method:
    /// 1. Gets the experiment's StartupConfig
    /// 2. Calls ApplyPhysicsOverrides() for any physics constant changes
    /// 3. Updates all UI fields to match the config
    /// 4. Sets up the custom initializer in the simulation API
    /// </summary>
    private void LoadExperiment(IExperiment experiment)
    {
        _selectedExperiment = experiment;

        var config = experiment.GetConfig();
        experiment.ApplyPhysicsOverrides();

        // Store custom initializer in simulation API
        _simApi.SetCustomInitializer(experiment.CustomInitializer);

        // Update UI fields with experiment configuration
        SuspendLayout();
        try
        {
            // === Simulation Parameters (grpSimParams) ===
            numNodeCount.Value = config.NodeCount;
            numTotalSteps.Value = config.TotalSteps;
            numTargetDegree.Value = config.TargetDegree;
            numInitialExcitedProb.Value = (decimal)config.InitialExcitedProb;
            numTemperature.Value = (decimal)config.Temperature;
            numEdgeTrialProb.Value = (decimal)config.EdgeTrialProbability;
            numLambdaState.Value = (decimal)config.LambdaState;
            numMeasurementThreshold.Value = (decimal)config.MeasurementThreshold;
            numFractalLevels.Value = config.FractalLevels;
            numFractalBranchFactor.Value = config.FractalBranchFactor;

            // === Physics Constants (grpPhysicsConstants) ===
            numInitialEdgeProb.Value = (decimal)config.InitialEdgeProb;
            numGravitationalCoupling.Value = (decimal)config.GravitationalCoupling;
            numVacuumEnergyScale.Value = (decimal)config.VacuumEnergyScale;
            numDecoherenceRate.Value = (decimal)config.DecoherenceRate;
            numHotStartTemperature.Value = (decimal)config.HotStartTemperature;
            numAdaptiveThresholdSigma.Value = (decimal)config.AdaptiveThresholdSigma;
            numWarmupDuration.Value = (decimal)config.WarmupDuration;
            numGravityTransitionDuration.Value = (decimal)config.GravityTransitionDuration;

            // === Physics Modules (checkboxes) ===
            chkSpectralGeometry.Checked = config.UseSpectralGeometry;
            chkNetworkGravity.Checked = config.UseNetworkGravity;
            chkQuantumDriven.Checked = config.UseQuantumDrivenStates;
            chkSpinorField.Checked = config.UseSpinorField;
            chkVacuumFluctuations.Checked = config.UseVacuumFluctuations;
            // Hot start annealing removed from UI-driven config
        }
        finally
        {
            ResumeLayout();
        }

        // Log experiment selection
        AppendSimConsole($"\n[Experiment] === {experiment.Name} ===\n");
        AppendSimConsole($"[Experiment] {experiment.Description}\n");
        AppendSimConsole($"[Experiment] Nodes={config.NodeCount}, Steps={config.TotalSteps}, " +
                     $"G={config.GravitationalCoupling}, T_hot={config.HotStartTemperature}\n");

        if (experiment.CustomInitializer != null)
        {
            AppendSimConsole($"[Experiment] Custom topology initializer will be applied\n");
        }

        // Reset preset selector to "Custom" since we're using experiment
        comboBox_Presets.SelectedIndex = 0;
    }
}
