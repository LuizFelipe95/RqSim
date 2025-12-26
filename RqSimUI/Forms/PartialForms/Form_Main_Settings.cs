using RqSimForms.Forms.Interfaces;
using RQSimulation;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{

    /// <summary>
    /// Connects ValueChanged handlers to all NumericUpDown controls in grpSimParams and grpPhysicsConstants
    /// for live parameter updates during simulation run.
    /// </summary>
    private void WireLiveParameterHandlers()
    {
        // grpPhysicsConstants - NumericUpDown controls
        numInitialEdgeProb.ValueChanged += OnLiveParameterChanged;
        numGravitationalCoupling.ValueChanged += OnLiveParameterChanged;
        numVacuumEnergyScale.ValueChanged += OnLiveParameterChanged;
        numDecoherenceRate.ValueChanged += OnLiveParameterChanged;
        numHotStartTemperature.ValueChanged += OnLiveParameterChanged;
        numAdaptiveThresholdSigma.ValueChanged += OnLiveParameterChanged;
        numWarmupDuration.ValueChanged += OnLiveParameterChanged;
        numGravityTransitionDuration.ValueChanged += OnLiveParameterChanged;

        // grpSimParams - NumericUpDown controls
        numNodeCount.ValueChanged += OnLiveParameterChanged;
        numTargetDegree.ValueChanged += OnLiveParameterChanged;
        numInitialExcitedProb.ValueChanged += OnLiveParameterChanged;
        numLambdaState.ValueChanged += OnLiveParameterChanged;
        numTemperature.ValueChanged += OnLiveParameterChanged;
        numEdgeTrialProb.ValueChanged += OnLiveParameterChanged;
        numMeasurementThreshold.ValueChanged += OnLiveParameterChanged;
        numTotalSteps.ValueChanged += OnLiveParameterChanged;
        numFractalLevels.ValueChanged += OnLiveParameterChanged;
        numFractalBranchFactor.ValueChanged += OnLiveParameterChanged;

        // RQ-Hypothesis checklist controls (live updates)
        numEdgeWeightQuantum.ValueChanged += OnRQChecklistParameterChanged;
        numRngStepCost.ValueChanged += OnRQChecklistParameterChanged;
        numEdgeCreationCost.ValueChanged += OnRQChecklistParameterChanged;
        numInitialVacuumEnergy.ValueChanged += OnRQChecklistParameterChanged;
    }

    /// <summary>
    /// Handler for live parameter changes - updates LiveConfig for running simulation.
    /// Thread-safe: writes to fields that calculation thread reads.
    /// </summary>
    private void OnLiveParameterChanged(object? sender, EventArgs e)
    {
        // Skip if controls not yet initialized
        if (numInitialEdgeProb is null) return;

        var liveConfig = _simApi.LiveConfig;

        // grpPhysicsConstants values
        liveConfig.InitialEdgeProb = (double)numInitialEdgeProb.Value;
        liveConfig.GravitationalCoupling = (double)numGravitationalCoupling.Value;
        liveConfig.VacuumEnergyScale = (double)numVacuumEnergyScale.Value;
        liveConfig.DecoherenceRate = (double)numDecoherenceRate.Value;
        liveConfig.HotStartTemperature = (double)numHotStartTemperature.Value;
        liveConfig.AdaptiveThresholdSigma = (double)numAdaptiveThresholdSigma.Value;
        liveConfig.WarmupDuration = (double)numWarmupDuration.Value;
        liveConfig.GravityTransitionDuration = (double)numGravityTransitionDuration.Value;

        // grpSimParams values
        liveConfig.TargetDegree = (int)numTargetDegree.Value;
        liveConfig.InitialExcitedProb = (double)numInitialExcitedProb.Value;
        liveConfig.LambdaState = (double)numLambdaState.Value;
        liveConfig.Temperature = (double)numTemperature.Value;
        liveConfig.EdgeTrialProb = (double)numEdgeTrialProb.Value;
        liveConfig.MeasurementThreshold = (double)numMeasurementThreshold.Value;
        liveConfig.FractalLevels = (int)numFractalLevels.Value;
        liveConfig.FractalBranchFactor = (int)numFractalBranchFactor.Value;

        // Mirror RQ-Hypothesis checklist controls into simulation API RQChecklist (if initialized)
        if (numEdgeWeightQuantum is not null)
        {
            var rq = _simApi.RQChecklist;
            rq.EdgeWeightQuantum = (double)numEdgeWeightQuantum.Value;
            rq.RngStepCost = (double)numRngStepCost.Value;
            rq.EdgeCreationCost = (double)numEdgeCreationCost.Value;
            rq.InitialVacuumEnergy = (double)numInitialVacuumEnergy.Value;
            rq.MarkUpdated();
        }

        liveConfig.MarkUpdated();

        // Log if simulation is running
        if (_isModernRunning && sender is NumericUpDown num)
        {
            AppendSimConsole($"[Live] {num.Name}: {num.Value}\n");
        }
    }

    // В методе OnRQChecklistParameterChanged замените строки с _simApi.RQChecklist на _rqChecklist
    private void OnRQChecklistParameterChanged(object? sender, EventArgs e)
    {
        // Skip if controls not yet initialized
        if (numEdgeWeightQuantum is null) return;

        var rq = _simApi.RQChecklist;

        rq.EdgeWeightQuantum = (double)numEdgeWeightQuantum.Value;
        rq.RngStepCost = (double)numRngStepCost.Value;
        rq.EdgeCreationCost = (double)numEdgeCreationCost.Value;
        rq.InitialVacuumEnergy = (double)numInitialVacuumEnergy.Value;
        rq.MarkUpdated();
        if (_isModernRunning && sender is NumericUpDown num)
        {
            AppendSimConsole($"[RQ Checklist] {num.Name}: {num.Value} (live update applied)\n");
        }
    }

    /// <summary>
    /// Handler for Graph Health parameter changes.
    /// Updates GraphHealthLive in _simApi for auto-tuning to use.
    /// </summary>
    private void OnGraphHealthParameterChanged(object? sender, EventArgs e)
    {
        // Skip if controls not yet initialized
        if (numGiantClusterThreshold is null) return;

        // Update GraphHealthLive config for auto-tuning
        _simApi.GraphHealthLive.GiantClusterThreshold = (double)numGiantClusterThreshold.Value;
        _simApi.GraphHealthLive.EmergencyGiantClusterThreshold = (double)numEmergencyGiantClusterThreshold.Value;
        _simApi.GraphHealthLive.GiantClusterDecoherenceRate = (double)numGiantClusterDecoherenceRate.Value;
        _simApi.GraphHealthLive.MaxDecoherenceEdgesFraction = (double)numMaxDecoherenceEdgesFraction.Value;
        _simApi.GraphHealthLive.CriticalSpectralDimension = (double)numCriticalSpectralDimension.Value;
        _simApi.GraphHealthLive.WarningSpectralDimension = (double)numWarningSpectralDimension.Value;

        if (_isModernRunning && sender is NumericUpDown num)
        {
            AppendSimConsole($"[GraphHealth] {num.Name}: {num.Value} (live update applied)\n");
        }
    }

    /// <summary>
    /// Initializes Graph Health UI controls on the Settings tab.
    /// These controls allow runtime configuration of fragmentation detection
    /// and giant cluster decoherence parameters (RQ-Hypothesis compliance).
    /// </summary>
    private void InitializeGraphHealthControls()
    {
        // Expand tlpPhysicsConstants to add Graph Health section
        // Current row count is 10, we need 6 more rows (1 header + 6 params)
        int startRow = tlpPhysicsConstants.RowCount;
        tlpPhysicsConstants.RowCount = startRow + 7;

        // Add row styles for new rows
        for (int i = 0; i < 7; i++)
        {
            tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        }

        // === Header Label ===
        var lblGraphHealthHeader = new Label
        {
            Text = "??? Graph Health (RQ) ???",
            AutoSize = true,
            ForeColor = Color.DarkBlue,
            Font = new Font(Font, FontStyle.Bold),
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        tlpPhysicsConstants.Controls.Add(lblGraphHealthHeader, 0, startRow);
        tlpPhysicsConstants.SetColumnSpan(lblGraphHealthHeader, 2);

        // === Giant Cluster Threshold ===
        var lblGiantClusterThreshold = new Label
        {
            Text = "Giant Cluster (% of N):",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numGiantClusterThreshold = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.05m,
            Minimum = 0.10m,
            Maximum = 0.90m,
            Value = (decimal)PhysicsConstants.GiantClusterThreshold,
            Dock = DockStyle.Fill
        };
        numGiantClusterThreshold.ValueChanged += OnGraphHealthParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblGiantClusterThreshold, 0, startRow + 1);
        tlpPhysicsConstants.Controls.Add(numGiantClusterThreshold, 1, startRow + 1);

        // === Emergency Giant Cluster Threshold ===
        var lblEmergencyThreshold = new Label
        {
            Text = "Emergency Cluster (% of N):",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numEmergencyGiantClusterThreshold = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.05m,
            Minimum = 0.20m,
            Maximum = 0.95m,
            Value = (decimal)PhysicsConstants.EmergencyGiantClusterThreshold,
            Dock = DockStyle.Fill
        };
        numEmergencyGiantClusterThreshold.ValueChanged += OnGraphHealthParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblEmergencyThreshold, 0, startRow + 2);
        tlpPhysicsConstants.Controls.Add(numEmergencyGiantClusterThreshold, 1, startRow + 2);

        // === Decoherence Rate ===
        var lblDecoherenceRate = new Label
        {
            Text = "Cluster Decoherence Rate:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numGiantClusterDecoherenceRate = new NumericUpDown
        {
            DecimalPlaces = 3,
            Increment = 0.01m,
            Minimum = 0.01m,
            Maximum = 0.50m,
            Value = (decimal)PhysicsConstants.GiantClusterDecoherenceRate,
            Dock = DockStyle.Fill
        };
        numGiantClusterDecoherenceRate.ValueChanged += OnGraphHealthParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblDecoherenceRate, 0, startRow + 3);
        tlpPhysicsConstants.Controls.Add(numGiantClusterDecoherenceRate, 1, startRow + 3);

        // === Max Decoherence Edges Fraction ===
        var lblMaxEdgesFraction = new Label
        {
            Text = "Max Edges Weakened (%):",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numMaxDecoherenceEdgesFraction = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.02m,
            Minimum = 0.02m,
            Maximum = 0.50m,
            Value = (decimal)PhysicsConstants.MaxDecoherenceEdgesFraction,
            Dock = DockStyle.Fill
        };
        numMaxDecoherenceEdgesFraction.ValueChanged += OnGraphHealthParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblMaxEdgesFraction, 0, startRow + 4);
        tlpPhysicsConstants.Controls.Add(numMaxDecoherenceEdgesFraction, 1, startRow + 4);

        // === Critical Spectral Dimension ===
        var lblCriticalSpectralDim = new Label
        {
            Text = "Critical d_S (fragmentation):",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numCriticalSpectralDimension = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.1m,
            Minimum = 0.5m,
            Maximum = 2.0m,
            Value = (decimal)PhysicsConstants.CriticalSpectralDimension,
            Dock = DockStyle.Fill
        };
        numCriticalSpectralDimension.ValueChanged += OnGraphHealthParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblCriticalSpectralDim, 0, startRow + 5);
        tlpPhysicsConstants.Controls.Add(numCriticalSpectralDimension, 1, startRow + 5);

        // === Warning Spectral Dimension ===
        var lblWarningSpectralDim = new Label
        {
            Text = "Warning d_S (correction):",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numWarningSpectralDimension = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.1m,
            Minimum = 1.0m,
            Maximum = 3.0m,
            Value = (decimal)PhysicsConstants.WarningSpectralDimension,
            Dock = DockStyle.Fill
        };
        numWarningSpectralDimension.ValueChanged += OnGraphHealthParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblWarningSpectralDim, 0, startRow + 6);
        tlpPhysicsConstants.Controls.Add(numWarningSpectralDimension, 1, startRow + 6);

        // === RQ-Hypothesis Checklist Constants (Energy/Quantization) ===
        // Добавляем разделитель для группировки контролов
        var lblChecklistHeader = new Label
        {
            Text = "??? RQ-Hypothesis Checklist ???",
            AutoSize = true,
            ForeColor = Color.DarkGreen,
            Font = new Font(Font, FontStyle.Bold),
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        tlpPhysicsConstants.Controls.Add(lblChecklistHeader, 0, startRow + 7);
        tlpPhysicsConstants.SetColumnSpan(lblChecklistHeader, 2);

        // === Edge Weight Quantum ===
        var lblEdgeWeightQuantum = new Label
        {
            Text = "Edge Weight Quantum:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numEdgeWeightQuantum = new NumericUpDown
        {
            DecimalPlaces = 4,
            Increment = 0.0001m,
            Minimum = 0.0001m,
            Maximum = 0.1000m,
            Value = (decimal)PhysicsConstants.EdgeWeightQuantum,
            Dock = DockStyle.Fill
        };
        numEdgeWeightQuantum.ValueChanged += OnRQChecklistParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblEdgeWeightQuantum, 0, startRow + 8);
        tlpPhysicsConstants.Controls.Add(numEdgeWeightQuantum, 1, startRow + 8);

        // === RNG Step Cost ===
        var lblRngStepCost = new Label
        {
            Text = "RNG Step Cost:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numRngStepCost = new NumericUpDown
        {
            DecimalPlaces = 7,
            Increment = 0.000001m,
            Minimum = 0.0000001m,
            Maximum = 0.0100m,
            Value = (decimal)PhysicsConstants.RngStepCost,
            Dock = DockStyle.Fill
        };
        numRngStepCost.ValueChanged += OnRQChecklistParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblRngStepCost, 0, startRow + 9);
        tlpPhysicsConstants.Controls.Add(numRngStepCost, 1, startRow + 9);

        // === Edge Creation Cost ===
        var lblEdgeCreationCost = new Label
        {
            Text = "Edge Creation Cost:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numEdgeCreationCost = new NumericUpDown
        {
            DecimalPlaces = 4,
            Increment = 0.0001m,
            Minimum = 0.0001m,
            Maximum = 0.1000m,
            Value = (decimal)PhysicsConstants.EdgeCreationCost,
            Dock = DockStyle.Fill
        };
        numEdgeCreationCost.ValueChanged += OnRQChecklistParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblEdgeCreationCost, 0, startRow + 10);
        tlpPhysicsConstants.Controls.Add(numEdgeCreationCost, 1, startRow + 10);

        // === Initial Vacuum Energy ===
        var lblInitialVacuumEnergy = new Label
        {
            Text = "Initial Vacuum Energy:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        numInitialVacuumEnergy = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 10.0m,
            Minimum = 0.0001m,
            Maximum = 10000.0m,
            Value = (decimal)PhysicsConstants.InitialVacuumEnergy,
            Dock = DockStyle.Fill
        };
        numInitialVacuumEnergy.ValueChanged += OnRQChecklistParameterChanged;
        tlpPhysicsConstants.Controls.Add(lblInitialVacuumEnergy, 0, startRow + 11);
        tlpPhysicsConstants.Controls.Add(numInitialVacuumEnergy, 1, startRow + 11);
    }

    /// <summary>
    /// Initializes Advanced Physics Controls for runtime adjustable parameters.
    /// These controls affect simulation behavior in real-time when simulation is running.
    /// </summary>


    /// <summary>
    /// Initializes Spectral Action Controls for NCG-based dimension stabilization.
    /// </summary>
    private void InitializeSpectralActionControls()
    {
        int startRow = tlpPhysicsConstants.RowCount;
        tlpPhysicsConstants.RowCount = startRow + 4;
        for (int i = 0; i < 4; i++)
        {
            tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        }

        // === Header ===
        var lblSpectralHeader = new Label
        {
            Text = "📊 Spectral Action (NCG) 📊",
            AutoSize = true,
            ForeColor = Color.DarkCyan,
            Font = new Font(Font, FontStyle.Bold),
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        tlpPhysicsConstants.Controls.Add(lblSpectralHeader, 0, startRow);
        tlpPhysicsConstants.SetColumnSpan(lblSpectralHeader, 2);

        // === Lambda Cutoff ===
        AddAdvancedControl(startRow + 1, "Λ Cutoff:",
            ref numSpectralLambdaCutoff, 1, 1.0m, 1.0m, 100.0m,
            (decimal)PhysicsConstants.SpectralActionConstants.LambdaCutoff, OnAdvancedPhysicsParameterChanged);

        // === Target Spectral Dimension ===
        AddAdvancedControl(startRow + 2, "Target d_S:",
            ref numSpectralTargetDimension, 1, 0.5m, 1.0m, 8.0m,
            (decimal)PhysicsConstants.SpectralActionConstants.TargetSpectralDimension, OnAdvancedPhysicsParameterChanged);

        // === Dimension Potential Strength ===
        AddAdvancedControl(startRow + 3, "Dim. Potential Strength:",
            ref numSpectralDimensionPotentialStrength, 3, 0.01m, 0.001m, 1.0m,
            (decimal)PhysicsConstants.SpectralActionConstants.DimensionPotentialStrength, OnAdvancedPhysicsParameterChanged);
    }

    /// <summary>
    /// Initializes AutoTuning parameter controls for runtime adjustment.
    /// </summary>
    private void InitializeAutoTuningControls()
    {
        int startRow = tlpPhysicsConstants.RowCount;
        tlpPhysicsConstants.RowCount = startRow + 6;
        for (int i = 0; i < 6; i++)
        {
            tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        }

        // === Header ===
        var lblAutoTuneHeader = new Label
        {
            Text = "🎯 AutoTuning Parameters 🎯",
            AutoSize = true,
            ForeColor = Color.Teal,
            Font = new Font(Font, FontStyle.Bold),
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        tlpPhysicsConstants.Controls.Add(lblAutoTuneHeader, 0, startRow);
        tlpPhysicsConstants.SetColumnSpan(lblAutoTuneHeader, 2);

        // === Target Dimension ===
        AddAdvancedControl(startRow + 1, "Target Dimension:",
            ref numAutoTuneTargetDimension, 1, 0.5m, 1.0m, 8.0m,
            (decimal)_simApi.LiveConfig.AutoTuneTargetDimension, OnAutoTuningParameterChanged);

        // === Dimension Tolerance ===
        AddAdvancedControl(startRow + 2, "Dimension Tolerance:",
            ref numAutoTuneDimensionTolerance, 2, 0.1m, 0.1m, 2.0m,
            (decimal)_simApi.LiveConfig.AutoTuneDimensionTolerance, OnAutoTuningParameterChanged);

        // === Energy Recycling Rate ===
        AddAdvancedControl(startRow + 3, "Energy Recycling Rate:",
            ref numAutoTuneEnergyRecyclingRate, 2, 0.1m, 0.0m, 1.0m,
            (decimal)_simApi.LiveConfig.AutoTuneEnergyRecyclingRate, OnAutoTuningParameterChanged);

        // === Gravity Adjustment Rate ===
        AddAdvancedControl(startRow + 4, "Gravity Adj. Rate:",
            ref numAutoTuneGravityAdjustmentRate, 2, 0.1m, 0.0m, 1.0m,
            (decimal)_simApi.LiveConfig.AutoTuneGravityAdjustmentRate, OnAutoTuningParameterChanged);

        // === Exploration Probability ===
        AddAdvancedControl(startRow + 5, "Exploration Prob:",
            ref numAutoTuneExplorationProb, 3, 0.01m, 0.0m, 0.5m,
            (decimal)_simApi.LiveConfig.AutoTuneExplorationProb, OnAutoTuningParameterChanged);
    }

    /// <summary>
    /// Helper method to add a NumericUpDown control with label to tlpPhysicsConstants.
    /// </summary>
    private void AddAdvancedControl(int row, string labelText,
        ref NumericUpDown numControl, int decimalPlaces, decimal increment,
        decimal minimum, decimal maximum, decimal value, EventHandler handler)
    {
        var lbl = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };

        numControl = new NumericUpDown
        {
            DecimalPlaces = decimalPlaces,
            Increment = increment,
            Minimum = minimum,
            Maximum = maximum,
            Value = Math.Max(minimum, Math.Min(maximum, value)),
            Dock = DockStyle.Fill
        };
        numControl.ValueChanged += handler;

        tlpPhysicsConstants.Controls.Add(lbl, 0, row);
        tlpPhysicsConstants.Controls.Add(numControl, 1, row);
    }

    /// <summary>
    /// Handler for Advanced Physics parameter changes - updates LiveConfig for running simulation.
    /// </summary>
    private void OnAdvancedPhysicsParameterChanged(object? sender, EventArgs e)
    {
        // Skip if controls not yet initialized
        if (numLapseFunctionAlpha is null) return;

        var liveConfig = _simApi.LiveConfig;

        // Update all advanced physics parameters
        liveConfig.LapseFunctionAlpha = (double)numLapseFunctionAlpha.Value;
        liveConfig.TimeDilationAlpha = (double)numTimeDilationAlpha.Value;
        liveConfig.WilsonParameter = (double)numWilsonParameter.Value;
        liveConfig.TopologyDecoherenceInterval = (int)numTopologyDecoherenceInterval.Value;
        liveConfig.TopologyDecoherenceTemperature = (double)numTopologyDecoherenceTemperature.Value;
        liveConfig.GaugeTolerance = (double)numGaugeTolerance.Value;
        liveConfig.MaxRemovableFlux = (double)numMaxRemovableFlux.Value;
        liveConfig.GeometryInertiaMass = (double)numGeometryInertiaMass.Value;
        liveConfig.GaugeFieldDamping = (double)numGaugeFieldDamping.Value;
        liveConfig.PairCreationMassThreshold = (double)numPairCreationMassThreshold.Value;
        liveConfig.PairCreationEnergy = (double)numPairCreationEnergy.Value;

        // Spectral action parameters (may be null if not yet initialized)
        if (numSpectralLambdaCutoff is not null)
            liveConfig.SpectralLambdaCutoff = (double)numSpectralLambdaCutoff.Value;
        if (numSpectralTargetDimension is not null)
            liveConfig.SpectralTargetDimension = (double)numSpectralTargetDimension.Value;
        if (numSpectralDimensionPotentialStrength is not null)
            liveConfig.SpectralDimensionPotentialStrength = (double)numSpectralDimensionPotentialStrength.Value;

        liveConfig.MarkUpdated();

        if (_isModernRunning && sender is NumericUpDown num)
        {
            AppendSimConsole($"[AdvancedPhysics] {num.Name}: {num.Value}\n");
        }


    }



    /// <summary>
    /// Handler for advanced physics parameter changes.
    /// Updates LiveConfig for running simulation.
    /// </summary>
    /*
    private void OnAdvancedPhysicsParameterChanged(object? sender, EventArgs e)
    {
        if (_eventsSupressed) return;

        var liveConfig = _simApi.LiveConfig;

        if (numLapseFunctionAlpha != null)
            liveConfig.LapseFunctionAlpha = (double)numLapseFunctionAlpha.Value;
        if (numTimeDilationAlpha != null)
            liveConfig.TimeDilationAlpha = (double)numTimeDilationAlpha.Value;
        if (numWilsonParameter != null)
            liveConfig.WilsonParameter = (double)numWilsonParameter.Value;
        if (numTopologyDecoherenceInterval != null)
            liveConfig.TopologyDecoherenceInterval = (int)numTopologyDecoherenceInterval.Value;
        if (numTopologyDecoherenceTemperature != null)
            liveConfig.TopologyDecoherenceTemperature = (double)numTopologyDecoherenceTemperature.Value;
        if (numGaugeTolerance != null)
            liveConfig.GaugeTolerance = (double)numGaugeTolerance.Value;
        if (numMaxRemovableFlux != null)
            liveConfig.MaxRemovableFlux = (double)numMaxRemovableFlux.Value;
        if (numGeometryInertiaMass != null)
            liveConfig.GeometryInertiaMass = (double)numGeometryInertiaMass.Value;
        if (numGaugeFieldDamping != null)
            liveConfig.GaugeFieldDamping = (double)numGaugeFieldDamping.Value;
        if (numPairCreationMassThreshold != null)
            liveConfig.PairCreationMassThreshold = (double)numPairCreationMassThreshold.Value;
        if (numPairCreationEnergy != null)
            liveConfig.PairCreationEnergy = (double)numPairCreationEnergy.Value;
        if (numSpectralLambdaCutoff != null)
            liveConfig.SpectralLambdaCutoff = (double)numSpectralLambdaCutoff.Value;
        if (numSpectralTargetDimension != null)
            liveConfig.SpectralTargetDimension = (double)numSpectralTargetDimension.Value;
        if (numSpectralDimensionPotentialStrength != null)
            liveConfig.SpectralDimensionPotentialStrength = (double)numSpectralDimensionPotentialStrength.Value;

        liveConfig.MarkUpdated();

        if (_isModernRunning && sender is NumericUpDown num)
        {
            AppendSimConsole($"[AdvPhysics] {num.Name}: {num.Value} (live update applied)\n");
        }
    }*/


    /// <summary>
    /// Handler for AutoTuning parameter changes - updates LiveConfig for running simulation.
    /// </summary>
    private void OnAutoTuningParameterChanged(object? sender, EventArgs e)
    {
        // Skip if controls not yet initialized
        if (numAutoTuneTargetDimension is null) return;

        var liveConfig = _simApi.LiveConfig;

        liveConfig.AutoTuneTargetDimension = (double)numAutoTuneTargetDimension.Value;
        liveConfig.AutoTuneDimensionTolerance = (double)numAutoTuneDimensionTolerance.Value;
        liveConfig.AutoTuneEnergyRecyclingRate = (double)numAutoTuneEnergyRecyclingRate.Value;
        liveConfig.AutoTuneGravityAdjustmentRate = (double)numAutoTuneGravityAdjustmentRate.Value;
        liveConfig.AutoTuneExplorationProb = (double)numAutoTuneExplorationProb.Value;

        liveConfig.MarkUpdated();

        if (_isModernRunning && sender is NumericUpDown num)
        {
            AppendSimConsole($"[AutoTuning] {num.Name}: {num.Value}\n");
        }
    }

    /// <summary>
    /// Handler for MaxFPS value change - updates UI timer interval
    /// </summary>
    private void numericUpDown_MaxFPS_ValueChanged(object? sender, EventArgs e)
    {
        int fps = (int)numericUpDown_MaxFPS.Value;
        if (fps <= 0) fps = 1;

        int intervalMs = 1000 / fps;

        if (_uiUpdateTimer != null)
        {
            _uiUpdateTimer.Interval = intervalMs;
        }

        AppendSysConsole($"[UI] FPS установлен: {fps} ({intervalMs}ms интервал)\n");
    }

    private void checkBox_AutoTuning_CheckedChanged(object sender, EventArgs e)
    {
        _simApi.AutoTuningEnabled = checkBox_AutoTuning.Checked;
        AppendSimConsole($"[AutoTuning] {(checkBox_AutoTuning.Checked ? "включен" : "выключен")}\n");

        if (checkBox_AutoTuning.Checked)
        {
            AppendSimConsole("[AutoTuning] Критерии: d_S, кластеры, excited ratio, giant cluster\n");
            AppendSimConsole("[AutoTuning] Интервал: каждые 100 шагов (быстрая реакция)\n");
        }
    }
}
