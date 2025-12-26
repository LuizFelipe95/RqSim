using RQSimulation;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - Handles synchronization of UI settings with PhysicsConstants.
/// This file contains methods to initialize additional physics controls and sync UI with constants.
/// </summary>
public partial class Form_Main
{
    // === Additional RQ-Hypothesis Numeric Controls ===

    private NumericUpDown numTimeDilationAlpha = null!;

    private NumericUpDown numLapseFunctionAlpha = null!;
    private NumericUpDown numWilsonParameter = null!;
    private NumericUpDown numTopologyDecoherenceInterval = null!;
    private NumericUpDown numTopologyDecoherenceTemperature = null!;
    private NumericUpDown numGaugeTolerance = null!;
    private NumericUpDown numMaxRemovableFlux = null!;
    private NumericUpDown numGeometryInertiaMass = null!;
    private NumericUpDown numGaugeFieldDamping = null!;
    private NumericUpDown numPairCreationMassThreshold = null!;
    private NumericUpDown numPairCreationEnergy = null!;

    // === Spectral Action Controls ===
    private NumericUpDown numSpectralLambdaCutoff = null!;
    private NumericUpDown numSpectralTargetDimension = null!;
    private NumericUpDown numSpectralDimensionPotentialStrength = null!;

    // === Additional RQ-Hypothesis Checkbox Controls ===
    private CheckBox chkEnableSymplecticGaugeEvolution = null!;
    private CheckBox chkEnableAdaptiveTopologyDecoherence = null!;
    private CheckBox chkEnableWilsonLoopProtection = null!;
    private CheckBox chkEnableSpectralActionMode = null!;
    private CheckBox chkEnableWheelerDeWittStrictMode = null!;
    private CheckBox chkUseHamiltonianGravity = null!;
    private CheckBox chkEnableVacuumEnergyReservoir = null!;
    private CheckBox chkPreferOllivierRicciCurvature = null!;

    /// <summary>
    /// Initializes additional physics controls on tlpPhysicsConstants panel.
    /// Called after InitializeGraphHealthControls() to add more advanced parameters.
    /// </summary>
    private void InitializeAdvancedPhysicsControls()
    {
        int startRow = tlpPhysicsConstants.RowCount;

        // === Lapse Function Section ===
        AddPhysicsHeader("─── Lapse Function ───", ref startRow, Color.DarkSlateBlue);

        AddNumericControl("Lapse Function α:", ref numLapseFunctionAlpha,
            (decimal)PhysicsConstants.LapseFunctionAlpha, 0.01m, 0.0m, 10.0m, 2, ref startRow,
            "Controls gravitational time dilation: N = 1/(1 + α|R|)");

        AddNumericControl("Time Dilation α (entropy):", ref numTimeDilationAlpha,
            (decimal)PhysicsConstants.TimeDilationAlpha, 0.1m, 0.0m, 5.0m, 2, ref startRow,
            "Entropic time dilation: N = exp(-α·S)");

        // === Wilson Fermion Section ===
        AddPhysicsHeader("─── Wilson Fermion ───", ref startRow, Color.SteelBlue);

        AddNumericControl("Wilson Parameter (r):", ref numWilsonParameter,
            (decimal)PhysicsConstants.WilsonParameter, 0.1m, 0.0m, 2.0m, 2, ref startRow,
            "Wilson term coefficient for fermion doubling suppression");

        // === Topology Decoherence Section ===
        AddPhysicsHeader("─── Topology Decoherence ───", ref startRow, Color.DarkCyan);

        AddNumericControl("Topology Decoh. Interval:", ref numTopologyDecoherenceInterval,
            PhysicsConstants.TopologyDecoherenceInterval, 1, 1, 100, 0, ref startRow,
            "Steps between topology updates (Zeno effect prevention)");

        AddNumericControl("Topology Decoh. Temp:", ref numTopologyDecoherenceTemperature,
            (decimal)PhysicsConstants.TopologyDecoherenceTemperature, 0.1m, 0.01m, 10.0m, 2, ref startRow,
            "Base temperature for adaptive topology flip probability");

        // === Gauge Protection Section ===
        AddPhysicsHeader("─── Gauge Protection ───", ref startRow, Color.DarkMagenta);

        AddNumericControl("Gauge Tolerance (rad):", ref numGaugeTolerance,
            (decimal)PhysicsConstants.GaugeTolerance, 0.01m, 0.01m, 1.0m, 3, ref startRow,
            "Threshold for trivial gauge phase (Wilson loops)");

        AddNumericControl("Max Removable Flux (rad):", ref numMaxRemovableFlux,
            (decimal)PhysicsConstants.MaxRemovableFlux, 0.1m, 0.1m, 3.14m, 2, ref startRow,
            "Maximum flux for edge removal without redistribution");

        // === Geometry Inertia Section ===
        AddPhysicsHeader("─── Geometry Inertia ───", ref startRow, Color.DarkOliveGreen);

        AddNumericControl("Geometry Inertia Mass:", ref numGeometryInertiaMass,
            (decimal)PhysicsConstants.GeometryInertiaMass, 1.0m, 0.1m, 100.0m, 1, ref startRow,
            "Inertial mass of geometry (Hamiltonian gravity)");

        AddNumericControl("Gauge Field Damping:", ref numGaugeFieldDamping,
            (decimal)PhysicsConstants.GaugeFieldDamping, 0.0001m, 0.0m, 0.1m, 4, ref startRow,
            "Damping coefficient for gauge oscillations");

        // === Hawking Radiation Section ===
        AddPhysicsHeader("─── Hawking Radiation ───", ref startRow, Color.Crimson);

        AddNumericControl("Pair Creation Mass Thresh:", ref numPairCreationMassThreshold,
            (decimal)PhysicsConstants.PairCreationMassThreshold, 0.01m, 0.001m, 1.0m, 3, ref startRow,
            "Mass threshold for spontaneous pair creation");

        AddNumericControl("Pair Creation Energy:", ref numPairCreationEnergy,
            (decimal)PhysicsConstants.PairCreationEnergy, 0.001m, 0.001m, 0.1m, 4, ref startRow,
            "Energy extracted from geometry per pair creation");

        // === Spectral Action Section ===
        AddPhysicsHeader("─── Spectral Action ───", ref startRow, Color.Indigo);

        AddNumericControl("Spectral Λ Cutoff:", ref numSpectralLambdaCutoff,
            (decimal)PhysicsConstants.SpectralActionConstants.LambdaCutoff, 0.1m, 0.1m, 10.0m, 2, ref startRow,
            "UV cutoff scale for spectral action");

        AddNumericControl("Target Spectral Dim:", ref numSpectralTargetDimension,
            (decimal)PhysicsConstants.SpectralActionConstants.TargetSpectralDimension, 0.5m, 1.0m, 10.0m, 1, ref startRow,
            "Target dimension for spectral action minimum");

        AddNumericControl("Dim Potential Strength:", ref numSpectralDimensionPotentialStrength,
            (decimal)PhysicsConstants.SpectralActionConstants.DimensionPotentialStrength, 0.01m, 0.0m, 1.0m, 3, ref startRow,
            "Coupling for dimension stabilization potential");
    }


    /// <summary>
    /// Initializes additional RQ-Hypothesis experimental flag checkboxes.
    /// Called after InitializeRQExperimentalFlagsControls() to add more flags.
    /// </summary>
    private void InitializeAdditionalRQFlags()
    {
        // === Enable Symplectic Gauge Evolution ===
        chkEnableSymplecticGaugeEvolution = new CheckBox
        {
            AutoSize = true,
            Text = "Symplectic Gauge Evolution",
            Checked = PhysicsConstants.EnableSymplecticGaugeEvolution,
            Name = "chkEnableSymplecticGaugeEvolution",
            Margin = new Padding(3)
        };
        chkEnableSymplecticGaugeEvolution.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableSymplecticGaugeEvolution);

        // === Enable Adaptive Topology Decoherence ===
        chkEnableAdaptiveTopologyDecoherence = new CheckBox
        {
            AutoSize = true,
            Text = "Adaptive Topology Decoherence",
            Checked = PhysicsConstants.EnableAdaptiveTopologyDecoherence,
            Name = "chkEnableAdaptiveTopologyDecoherence",
            Margin = new Padding(3)
        };
        chkEnableAdaptiveTopologyDecoherence.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableAdaptiveTopologyDecoherence);

        // === Enable Wilson Loop Protection ===
        chkEnableWilsonLoopProtection = new CheckBox
        {
            AutoSize = true,
            Text = "Wilson Loop Protection",
            Checked = PhysicsConstants.EnableWilsonLoopProtection,
            Name = "chkEnableWilsonLoopProtection",
            Margin = new Padding(3)
        };
        chkEnableWilsonLoopProtection.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableWilsonLoopProtection);

        // === Enable Spectral Action Mode ===
        chkEnableSpectralActionMode = new CheckBox
        {
            AutoSize = true,
            Text = "Spectral Action Mode",
            Checked = PhysicsConstants.SpectralActionConstants.EnableSpectralActionMode,
            Name = "chkEnableSpectralActionMode",
            Margin = new Padding(3)
        };
        chkEnableSpectralActionMode.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableSpectralActionMode);

        // === Enable Wheeler-DeWitt Strict Mode ===
        chkEnableWheelerDeWittStrictMode = new CheckBox
        {
            AutoSize = true,
            Text = "Wheeler-DeWitt Strict Mode",
            Checked = PhysicsConstants.WheelerDeWittConstants.EnableStrictMode,
            Name = "chkEnableWheelerDeWittStrictMode",
            Margin = new Padding(3)
        };
        chkEnableWheelerDeWittStrictMode.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableWheelerDeWittStrictMode);

        // === Use Hamiltonian Gravity ===
        chkUseHamiltonianGravity = new CheckBox
        {
            AutoSize = true,
            Text = "Use Hamiltonian Gravity",
            Checked = PhysicsConstants.UseHamiltonianGravity,
            Name = "chkUseHamiltonianGravity",
            Margin = new Padding(3)
        };
        chkUseHamiltonianGravity.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkUseHamiltonianGravity);

        // === Enable Vacuum Energy Reservoir ===
        chkEnableVacuumEnergyReservoir = new CheckBox
        {
            AutoSize = true,
            Text = "Vacuum Energy Reservoir",
            Checked = PhysicsConstants.EnableVacuumEnergyReservoir,
            Name = "chkEnableVacuumEnergyReservoir",
            Margin = new Padding(3)
        };
        chkEnableVacuumEnergyReservoir.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableVacuumEnergyReservoir);

        // === Prefer Ollivier-Ricci Curvature ===
        chkPreferOllivierRicciCurvature = new CheckBox
        {
            AutoSize = true,
            Text = "Prefer Ollivier-Ricci Curvature",
            Checked = PhysicsConstants.PreferOllivierRicciCurvature,
            Name = "chkPreferOllivierRicciCurvature",
            Margin = new Padding(3)
        };
        chkPreferOllivierRicciCurvature.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkPreferOllivierRicciCurvature);
    }

    #region Helper Methods for Control Creation

    private void AddPhysicsHeader(string text, ref int row, Color color)
    {
        tlpPhysicsConstants.RowCount = row + 1;
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        var label = new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = color,
            Font = new Font(Font, FontStyle.Bold),
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(0, 8, 0, 2)
        };
        tlpPhysicsConstants.Controls.Add(label, 0, row);
        tlpPhysicsConstants.SetColumnSpan(label, 2);
        row++;
    }

    private void AddNumericControl(string labelText, ref NumericUpDown? control,
        decimal value, decimal increment, decimal min, decimal max, int decimals,
        ref int row, string? tooltip = null)
    {
        tlpPhysicsConstants.RowCount = row + 1;
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };

        control = new NumericUpDown
        {
            DecimalPlaces = decimals,
            Increment = increment,
            Minimum = min,
            Maximum = max,
            Value = Math.Clamp(value, min, max),
            Dock = DockStyle.Fill
        };
        control.ValueChanged += OnAdvancedPhysicsParameterChanged;

        if (tooltip != null)
        {
            var tt = new ToolTip();
            tt.SetToolTip(control, tooltip);
            tt.SetToolTip(label, tooltip);
        }

        tlpPhysicsConstants.Controls.Add(label, 0, row);
        tlpPhysicsConstants.Controls.Add(control, 1, row);
        row++;
    }

    #endregion



    // === Graph Health UI Controls (RQ-Hypothesis compliance) ===
    private NumericUpDown numGiantClusterThreshold = null!;
    private NumericUpDown numEmergencyGiantClusterThreshold = null!;
    private NumericUpDown numGiantClusterDecoherenceRate = null!;
    private NumericUpDown numMaxDecoherenceEdgesFraction = null!;
    private NumericUpDown numCriticalSpectralDimension = null!;
    private NumericUpDown numWarningSpectralDimension = null!;

    // === RQ-Hypothesis Checklist Constants (Energy/Quantization) ===
    private NumericUpDown numEdgeWeightQuantum = null!;
    private NumericUpDown numRngStepCost = null!;
    private NumericUpDown numEdgeCreationCost = null!;
    private NumericUpDown numInitialVacuumEnergy = null!;

    // === AutoTuning Controls ===
    private NumericUpDown numAutoTuneTargetDimension = null!;
    private NumericUpDown numAutoTuneDimensionTolerance = null!;
    private NumericUpDown numAutoTuneEnergyRecyclingRate = null!;
    private NumericUpDown numAutoTuneGravityAdjustmentRate = null!;
    private NumericUpDown numAutoTuneExplorationProb = null!;

    // === All Physics Constants Panel (read-only display) ===
    private Panel panelAllPhysicsConstants = null!;
    private TableLayoutPanel tlpAllPhysicsConstants = null!;


    /// <summary>
    /// Synchronizes all UI controls with current PhysicsConstants values.
    /// Call this to refresh UI after loading a preset or configuration.
    /// </summary>
    public void SyncUIWithPhysicsConstants()
    {
        // Disable event handlers temporarily
        SuspendControlEvents();

        try
        {
            // === tlpSimParams controls ===
            // (values from Designer defaults, keep as is)

            // === tlpPhysicsConstants controls ===
            SetNumericValueSafe(numInitialEdgeProb, 0.035m); // Default from Designer
            SetNumericValueSafe(numGravitationalCoupling, (decimal)PhysicsConstants.GravitationalCoupling);
            SetNumericValueSafe(numVacuumEnergyScale, (decimal)PhysicsConstants.VacuumFluctuationScale);
            SetNumericValueSafe(numDecoherenceRate, (decimal)PhysicsConstants.GiantClusterDecoherenceRate);
            SetNumericValueSafe(numAdaptiveThresholdSigma, (decimal)PhysicsConstants.AdaptiveThresholdSigma);
            SetNumericValueSafe(numWarmupDuration, PhysicsConstants.WarmupDuration);
            SetNumericValueSafe(numGravityTransitionDuration, PhysicsConstants.GravityTransitionDuration);

            // === Graph Health controls ===
            SetNumericValueSafe(numGiantClusterThreshold, (decimal)PhysicsConstants.GiantClusterThreshold);
            SetNumericValueSafe(numEmergencyGiantClusterThreshold, (decimal)PhysicsConstants.EmergencyGiantClusterThreshold);
            SetNumericValueSafe(numGiantClusterDecoherenceRate, (decimal)PhysicsConstants.GiantClusterDecoherenceRate);
            SetNumericValueSafe(numMaxDecoherenceEdgesFraction, (decimal)PhysicsConstants.MaxDecoherenceEdgesFraction);
            SetNumericValueSafe(numCriticalSpectralDimension, (decimal)PhysicsConstants.CriticalSpectralDimension);
            SetNumericValueSafe(numWarningSpectralDimension, (decimal)PhysicsConstants.WarningSpectralDimension);

            // === RQ Checklist controls ===
            SetNumericValueSafe(numEdgeWeightQuantum, (decimal)PhysicsConstants.EdgeWeightQuantum);
            SetNumericValueSafe(numRngStepCost, (decimal)PhysicsConstants.RngStepCost);
            SetNumericValueSafe(numEdgeCreationCost, (decimal)PhysicsConstants.EdgeCreationCost);
            SetNumericValueSafe(numInitialVacuumEnergy, (decimal)PhysicsConstants.InitialVacuumEnergy);

            // === Advanced Physics controls ===
            SetNumericValueSafe(numLapseFunctionAlpha, (decimal)PhysicsConstants.LapseFunctionAlpha);
            SetNumericValueSafe(numWilsonParameter, (decimal)PhysicsConstants.WilsonParameter);
            SetNumericValueSafe(numTopologyDecoherenceInterval, PhysicsConstants.TopologyDecoherenceInterval);
            SetNumericValueSafe(numTopologyDecoherenceTemperature, (decimal)PhysicsConstants.TopologyDecoherenceTemperature);
            SetNumericValueSafe(numGaugeTolerance, (decimal)PhysicsConstants.GaugeTolerance);
            SetNumericValueSafe(numMaxRemovableFlux, (decimal)PhysicsConstants.MaxRemovableFlux);
            SetNumericValueSafe(numGeometryInertiaMass, (decimal)PhysicsConstants.GeometryInertiaMass);
            SetNumericValueSafe(numGaugeFieldDamping, (decimal)PhysicsConstants.GaugeFieldDamping);
            SetNumericValueSafe(numPairCreationMassThreshold, (decimal)PhysicsConstants.PairCreationMassThreshold);
            SetNumericValueSafe(numPairCreationEnergy, (decimal)PhysicsConstants.PairCreationEnergy);

            // === Spectral Action controls ===
            SetNumericValueSafe(numSpectralLambdaCutoff, (decimal)PhysicsConstants.SpectralActionConstants.LambdaCutoff);
            SetNumericValueSafe(numSpectralTargetDimension, (decimal)PhysicsConstants.SpectralActionConstants.TargetSpectralDimension);
            SetNumericValueSafe(numSpectralDimensionPotentialStrength, (decimal)PhysicsConstants.SpectralActionConstants.DimensionPotentialStrength);

            // === Sync checkboxes with PhysicsConstants ===
            SyncCheckboxesWithConstants();
        }
        finally
        {
            ResumeControlEvents();
        }

        AppendSysConsole("[Settings] UI synchronized with PhysicsConstants\n");
    }

    private void SyncCheckboxesWithConstants()
    {
        // RQ Experimental flags
        if (chkEnableNaturalDimensionEmergence != null)
            chkEnableNaturalDimensionEmergence.Checked = PhysicsConstants.EnableNaturalDimensionEmergence;
        if (chkEnableTopologicalParity != null)
            chkEnableTopologicalParity.Checked = PhysicsConstants.EnableTopologicalParity;
        if (chkEnableLapseSynchronizedGeometry != null)
            chkEnableLapseSynchronizedGeometry.Checked = PhysicsConstants.EnableLapseSynchronizedGeometry;
        if (chkEnableTopologyEnergyCompensation != null)
            chkEnableTopologyEnergyCompensation.Checked = PhysicsConstants.EnableTopologyEnergyCompensation;
        if (chkEnablePlaquetteYangMills != null)
            chkEnablePlaquetteYangMills.Checked = PhysicsConstants.EnablePlaquetteYangMills;

        // Additional RQ flags
        if (chkEnableSymplecticGaugeEvolution != null)
            chkEnableSymplecticGaugeEvolution.Checked = PhysicsConstants.EnableSymplecticGaugeEvolution;
        if (chkEnableAdaptiveTopologyDecoherence != null)
            chkEnableAdaptiveTopologyDecoherence.Checked = PhysicsConstants.EnableAdaptiveTopologyDecoherence;
        if (chkEnableWilsonLoopProtection != null)
            chkEnableWilsonLoopProtection.Checked = PhysicsConstants.EnableWilsonLoopProtection;
        if (chkEnableSpectralActionMode != null)
            chkEnableSpectralActionMode.Checked = PhysicsConstants.SpectralActionConstants.EnableSpectralActionMode;
        if (chkEnableWheelerDeWittStrictMode != null)
            chkEnableWheelerDeWittStrictMode.Checked = PhysicsConstants.WheelerDeWittConstants.EnableStrictMode;
        if (chkUseHamiltonianGravity != null)
            chkUseHamiltonianGravity.Checked = PhysicsConstants.UseHamiltonianGravity;
        if (chkEnableVacuumEnergyReservoir != null)
            chkEnableVacuumEnergyReservoir.Checked = PhysicsConstants.EnableVacuumEnergyReservoir;
        if (chkPreferOllivierRicciCurvature != null)
            chkPreferOllivierRicciCurvature.Checked = PhysicsConstants.PreferOllivierRicciCurvature;
    }

    private bool _eventsSupressed = false;

    private void SuspendControlEvents()
    {
        _eventsSupressed = true;
    }

    private void ResumeControlEvents()
    {
        _eventsSupressed = false;
    }
}
