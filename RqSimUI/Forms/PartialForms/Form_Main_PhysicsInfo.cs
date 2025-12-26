using RQSimulation;
using System.Drawing;
using System.Windows.Forms;

namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - All Physics Constants read-only display panel.
/// Shows a comprehensive reference of all physics constants from PhysicsConstants.
/// </summary>
public partial class Form_Main
{
    /// <summary>
    /// Initializes a scrollable panel displaying ALL physics constants from PhysicsConstants.cs.
    /// Read-only labels show const values; this provides a complete reference for users.
    /// Organized into logical groups matching PhysicsConstants file structure.
    /// </summary>
    private void InitializeAllPhysicsConstantsDisplay()
    {
        // Create scrollable panel for all physics constants
        panelAllPhysicsConstants = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };

        tlpAllPhysicsConstants = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Padding = new Padding(5)
        };
        tlpAllPhysicsConstants.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        tlpAllPhysicsConstants.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

        int row = 0;

        // ============================================================
        // FUNDAMENTAL CONSTANTS (Planck Units)
        // ============================================================
        AddSectionHeader("══ Fundamental Constants (Planck Units) ══", ref row, Color.DarkBlue);
        AddConstantRow("Speed of Light (c)", PhysicsConstants.C.ToString("G"), ref row);
        AddConstantRow("Reduced Planck Constant (ℏ)", PhysicsConstants.HBar.ToString("G"), ref row);
        AddConstantRow("Gravitational Constant (G)", PhysicsConstants.G.ToString("G"), ref row);
        AddConstantRow("Planck Length", PhysicsConstants.PlanckLength.ToString("G"), ref row);
        AddConstantRow("Planck Time", PhysicsConstants.PlanckTime.ToString("G"), ref row);
        AddConstantRow("Planck Mass", PhysicsConstants.PlanckMass.ToString("G"), ref row);
        AddConstantRow("Planck Energy", PhysicsConstants.PlanckEnergy.ToString("G"), ref row);

        // ============================================================
        // GAUGE COUPLING CONSTANTS
        // ============================================================
        AddSectionHeader("══ Gauge Coupling Constants ══", ref row, Color.DarkGreen);
        AddConstantRow("Fine Structure Constant (α)", $"{PhysicsConstants.FineStructureConstant:E6} (~1/137)", ref row);
        AddConstantRow("Strong Coupling Constant (α_s)", PhysicsConstants.StrongCouplingConstant.ToString("F4"), ref row);
        AddConstantRow("Weak Mixing Angle (sin²θ_W)", PhysicsConstants.WeakMixingAngle.ToString("F4"), ref row);
        AddConstantRow("Weak Coupling Constant (g_W)", PhysicsConstants.WeakCouplingConstant.ToString("F4"), ref row);
        AddConstantRow("Gauge Coupling Constant (e)", PhysicsConstants.GaugeCouplingConstant.ToString("F4"), ref row);

        // ============================================================
        // RQ-HYPOTHESIS v2.0 CONSTANTS
        // ============================================================
        AddSectionHeader("══ RQ-Hypothesis v2.0 ══", ref row, Color.DarkMagenta);
        AddConstantRow("Use Hamiltonian Gravity", PhysicsConstants.UseHamiltonianGravity.ToString(), ref row);
        AddConstantRow("Geometry Inertia Mass", PhysicsConstants.GeometryInertiaMass.ToString("F2"), ref row);
        AddConstantRow("Yukawa Coupling", PhysicsConstants.YukawaCoupling.ToString("F4"), ref row);
        AddConstantRow("Topological Mass Coupling", PhysicsConstants.TopoMassCoupling.ToString("F4"), ref row);
        AddConstantRow("Enable Vacuum Energy Reservoir", PhysicsConstants.EnableVacuumEnergyReservoir.ToString(), ref row);
        AddConstantRow("Initial Vacuum Energy", PhysicsConstants.InitialVacuumEnergy.ToString("F1"), ref row);
        AddConstantRow("Initial Vacuum Pool Fraction", PhysicsConstants.InitialVacuumPoolFraction.ToString("F4"), ref row);
        AddConstantRow("Vacuum Fluctuation Base Rate (α³)", PhysicsConstants.VacuumFluctuationBaseRate.ToString("E8"), ref row);
        AddConstantRow("Curvature Coupling Factor (4π)", PhysicsConstants.CurvatureCouplingFactor.ToString("F4"), ref row);
        AddConstantRow("Hawking Radiation Enhancement", PhysicsConstants.HawkingRadiationEnhancement.ToString("F1"), ref row);
        AddConstantRow("Pair Creation Energy Threshold", PhysicsConstants.PairCreationEnergyThreshold.ToString("F1"), ref row);

        // ============================================================
        // RQ-HYPOTHESIS EXPERIMENTAL FLAGS
        // ============================================================
        AddSectionHeader("══ RQ-Hypothesis Experimental Flags ══", ref row, Color.DarkOrchid);
        AddConstantRow("Enable Natural Dimension Emergence", PhysicsConstants.EnableNaturalDimensionEmergence.ToString(), ref row);
        AddConstantRow("Enable Topological Parity", PhysicsConstants.EnableTopologicalParity.ToString(), ref row);
        AddConstantRow("Enable Lapse-Synchronized Geometry", PhysicsConstants.EnableLapseSynchronizedGeometry.ToString(), ref row);
        AddConstantRow("Enable Topology Energy Compensation", PhysicsConstants.EnableTopologyEnergyCompensation.ToString(), ref row);
        AddConstantRow("Enable Plaquette Yang-Mills", PhysicsConstants.EnablePlaquetteYangMills.ToString(), ref row);
        AddConstantRow("Enable Symplectic Gauge Evolution", PhysicsConstants.EnableSymplecticGaugeEvolution.ToString(), ref row);
        AddConstantRow("Enable Adaptive Topology Decoherence", PhysicsConstants.EnableAdaptiveTopologyDecoherence.ToString(), ref row);
        AddConstantRow("Prefer Ollivier-Ricci Curvature", PhysicsConstants.PreferOllivierRicciCurvature.ToString(), ref row);

        // ============================================================
        // SYMPLECTIC YANG-MILLS DYNAMICS
        // ============================================================
        AddSectionHeader("══ Symplectic Yang-Mills Dynamics ══", ref row, Color.Indigo);
        AddConstantRow("Planck Constant² (ℏ²)", PhysicsConstants.PlanckConstantSqr.ToString("F1"), ref row);
        AddConstantRow("Landauer Limit (ln2)", PhysicsConstants.LandauerLimit.ToString("F4"), ref row);
        AddConstantRow("Gauge Momentum Mass U(1)", PhysicsConstants.GaugeMomentumMassU1.ToString("F2"), ref row);
        AddConstantRow("Gauge Momentum Mass SU(2)", PhysicsConstants.GaugeMomentumMassSU2.ToString("F2"), ref row);
        AddConstantRow("Gauge Momentum Mass SU(3)", PhysicsConstants.GaugeMomentumMassSU3.ToString("F2"), ref row);
        AddConstantRow("Gauge Field Damping", PhysicsConstants.GaugeFieldDamping.ToString("E4"), ref row);

        // ============================================================
        // TOPOLOGY DECOHERENCE (ZENO EFFECT)
        // ============================================================
        AddSectionHeader("══ Topology Decoherence (Zeno Effect) ══", ref row, Color.DarkSlateGray);
        AddConstantRow("Topology Decoherence Interval", PhysicsConstants.TopologyDecoherenceInterval.ToString(), ref row);
        AddConstantRow("Topology Decoherence Temperature", PhysicsConstants.TopologyDecoherenceTemperature.ToString("F4"), ref row);
        AddConstantRow("Topology Flip Amplitude Threshold", PhysicsConstants.TopologyFlipAmplitudeThreshold.ToString("F4"), ref row);

        // ============================================================
        // WILSON LOOPS GAUGE PROTECTION
        // ============================================================
        AddSectionHeader("══ Wilson Loops Gauge Protection ══", ref row, Color.DarkCyan);
        AddConstantRow("Gauge Tolerance (rad)", PhysicsConstants.GaugeTolerance.ToString("F4"), ref row);
        AddConstantRow("Enable Wilson Loop Protection", PhysicsConstants.EnableWilsonLoopProtection.ToString(), ref row);
        AddConstantRow("Max Removable Flux (rad)", PhysicsConstants.MaxRemovableFlux.ToString("F4"), ref row);

        // ============================================================
        // SPECTRAL ACTION (CHAMSEDDINE-CONNES)
        // ============================================================
        AddSectionHeader("══ Spectral Action (Chamseddine-Connes) ══", ref row, Color.Purple);
        AddConstantRow("Lambda Cutoff", PhysicsConstants.SpectralActionConstants.LambdaCutoff.ToString("F2"), ref row);
        AddConstantRow("F₀ Cosmological", PhysicsConstants.SpectralActionConstants.F0_Cosmological.ToString("F2"), ref row);
        AddConstantRow("F₂ Einstein-Hilbert", PhysicsConstants.SpectralActionConstants.F2_EinsteinHilbert.ToString("F2"), ref row);
        AddConstantRow("F₄ Weyl", PhysicsConstants.SpectralActionConstants.F4_Weyl.ToString("F2"), ref row);
        AddConstantRow("Target Spectral Dimension", PhysicsConstants.SpectralActionConstants.TargetSpectralDimension.ToString("F1"), ref row);
        AddConstantRow("Dimension Potential Strength", PhysicsConstants.SpectralActionConstants.DimensionPotentialStrength.ToString("F4"), ref row);
        AddConstantRow("Dimension Potential Width", PhysicsConstants.SpectralActionConstants.DimensionPotentialWidth.ToString("F4"), ref row);
        AddConstantRow("Enable Spectral Action Mode", PhysicsConstants.SpectralActionConstants.EnableSpectralActionMode.ToString(), ref row);

        // ============================================================
        // WHEELER-DEWITT CONSTRAINT
        // ============================================================
        AddSectionHeader("══ Wheeler-DeWitt Constraint ══", ref row, Color.Maroon);
        AddConstantRow("WdW Gravitational Coupling", PhysicsConstants.WheelerDeWittConstants.GravitationalCoupling.ToString("F4"), ref row);
        AddConstantRow("WdW Constraint Lagrange Multiplier", PhysicsConstants.WheelerDeWittConstants.ConstraintLagrangeMultiplier.ToString("F1"), ref row);
        AddConstantRow("WdW Constraint Tolerance", PhysicsConstants.WheelerDeWittConstants.ConstraintTolerance.ToString("E4"), ref row);
        AddConstantRow("WdW Enable Strict Mode", PhysicsConstants.WheelerDeWittConstants.EnableStrictMode.ToString(), ref row);
        AddConstantRow("WdW Enable Violation Logging", PhysicsConstants.WheelerDeWittConstants.EnableViolationLogging.ToString(), ref row);

        // ============================================================
        // GRAVITY AND CURVATURE
        // ============================================================
        AddSectionHeader("══ Gravity and Curvature ══", ref row, Color.SaddleBrown);
        AddConstantRow("Gravitational Coupling", PhysicsConstants.GravitationalCoupling.ToString("F4"), ref row);
        AddConstantRow("Warmup Gravitational Coupling", PhysicsConstants.WarmupGravitationalCoupling.ToString("F4"), ref row);
        AddConstantRow("Warmup Duration", PhysicsConstants.WarmupDuration.ToString(), ref row);
        AddConstantRow("Gravity Transition Duration (1/α)", PhysicsConstants.GravityTransitionDuration.ToString(), ref row);
        AddConstantRow("Cosmological Constant (Λ)", PhysicsConstants.CosmologicalConstant.ToString("E4"), ref row);
        AddConstantRow("Lapse Function Alpha", PhysicsConstants.LapseFunctionAlpha.ToString("F4"), ref row);
        AddConstantRow("Geometry Momentum Mass", PhysicsConstants.GeometryMomentumMass.ToString("F1"), ref row);
        AddConstantRow("Geometry Damping", PhysicsConstants.GeometryDamping.ToString("F4"), ref row);
        AddConstantRow("Curvature Term Scale", PhysicsConstants.CurvatureTermScale.ToString("F4"), ref row);

        // ============================================================
        // TIME DILATION
        // ============================================================
        AddSectionHeader("══ Time Dilation ══", ref row, Color.Teal);
        AddConstantRow("Time Dilation Alpha (entropy)", PhysicsConstants.TimeDilationAlpha.ToString("F4"), ref row);
        AddConstantRow("Time Dilation Mass Coupling", PhysicsConstants.TimeDilationMassCoupling.ToString("F4"), ref row);
        AddConstantRow("Time Dilation Curvature Coupling", PhysicsConstants.TimeDilationCurvatureCoupling.ToString("F4"), ref row);
        AddConstantRow("Min Time Dilation", PhysicsConstants.MinTimeDilation.ToString("F4"), ref row);
        AddConstantRow("Max Time Dilation", PhysicsConstants.MaxTimeDilation.ToString("F4"), ref row);
        AddConstantRow("Speed of Light (network)", PhysicsConstants.SpeedOfLight.ToString("F1"), ref row);
        AddConstantRow("Max Causal Distance", PhysicsConstants.MaxCausalDistance.ToString(), ref row);
        AddConstantRow("Base Timestep", PhysicsConstants.BaseTimestep.ToString("F4"), ref row);

        // ============================================================
        // WILSON LATTICE FERMION
        // ============================================================
        AddSectionHeader("══ Wilson Lattice Fermion ══", ref row, Color.SteelBlue);
        AddConstantRow("Causal Max Hops", PhysicsConstants.CausalMaxHops.ToString(), ref row);
        AddConstantRow("Wilson Mass Penalty", PhysicsConstants.WilsonMassPenalty.ToString("F2"), ref row);
        AddConstantRow("Wilson Parameter (r)", PhysicsConstants.WilsonParameter.ToString("F2"), ref row);

        // ============================================================
        // HAWKING RADIATION / PAIR CREATION
        // ============================================================
        AddSectionHeader("══ Hawking Radiation / Pair Creation ══", ref row, Color.Crimson);
        AddConstantRow("Pair Creation Mass Threshold", PhysicsConstants.PairCreationMassThreshold.ToString("F4"), ref row);
        AddConstantRow("Pair Creation Energy", PhysicsConstants.PairCreationEnergy.ToString("F4"), ref row);

        // ============================================================
        // FIELD THEORY
        // ============================================================
        AddSectionHeader("══ Field Theory ══", ref row, Color.DarkOliveGreen);
        AddConstantRow("Field Diffusion Rate", PhysicsConstants.FieldDiffusionRate.ToString("F4"), ref row);
        AddConstantRow("Field Decay Rate", PhysicsConstants.FieldDecayRate.ToString("E6"), ref row);
        AddConstantRow("Klein-Gordon Mass (μ²)", PhysicsConstants.KleinGordonMass.ToString("F4"), ref row);
        AddConstantRow("Dirac Coupling (λ_D)", PhysicsConstants.DiracCoupling.ToString("F4"), ref row);
        AddConstantRow("Spinor Normalization Threshold", PhysicsConstants.SpinorNormalizationThreshold.ToString("E4"), ref row);
        AddConstantRow("Spinor Norm Correction Factor", PhysicsConstants.SpinorNormalizationCorrectionFactor.ToString("F4"), ref row);

        // ============================================================
        // HIGGS POTENTIAL
        // ============================================================
        AddSectionHeader("══ Higgs Potential ══", ref row, Color.DarkOrange);
        AddConstantRow("Higgs μ²", PhysicsConstants.HiggsMuSquared.ToString("F4"), ref row);
        AddConstantRow("Higgs λ", PhysicsConstants.HiggsLambda.ToString("F4"), ref row);
        AddConstantRow("Higgs VEV", PhysicsConstants.HiggsVEV.ToString("F4"), ref row);

        // ============================================================
        // CLUSTER DYNAMICS
        // ============================================================
        AddSectionHeader("══ Cluster Dynamics ══", ref row, Color.Sienna);
        AddConstantRow("Default Heavy Cluster Threshold", PhysicsConstants.DefaultHeavyClusterThreshold.ToString("F4"), ref row);
        AddConstantRow("Adaptive Threshold Sigma", PhysicsConstants.AdaptiveThresholdSigma.ToString("F2"), ref row);
        AddConstantRow("Minimum Cluster Size", PhysicsConstants.MinimumClusterSize.ToString(), ref row);
        AddConstantRow("Cluster Stabilization Temperature", PhysicsConstants.ClusterStabilizationTemperature.ToString("F4"), ref row);
        AddConstantRow("Metropolis Trials per Cluster", PhysicsConstants.MetropolisTrialsPerCluster.ToString(), ref row);
        AddConstantRow("Overcorrelation Threshold", PhysicsConstants.OvercorrelationThreshold.ToString("F4"), ref row);

        // ============================================================
        // GRAPH HEALTH (RQ-HYPOTHESIS)
        // ============================================================
        AddSectionHeader("══ Graph Health (RQ-Hypothesis) ══", ref row, Color.DarkRed);
        AddConstantRow("Critical Spectral Dimension", PhysicsConstants.CriticalSpectralDimension.ToString("F2"), ref row);
        AddConstantRow("Warning Spectral Dimension", PhysicsConstants.WarningSpectralDimension.ToString("F2"), ref row);
        AddConstantRow("Giant Cluster Threshold (%N)", PhysicsConstants.GiantClusterThreshold.ToString("P0"), ref row);
        AddConstantRow("Emergency Giant Cluster Threshold", PhysicsConstants.EmergencyGiantClusterThreshold.ToString("P0"), ref row);
        AddConstantRow("Giant Cluster Decoherence Rate", PhysicsConstants.GiantClusterDecoherenceRate.ToString("F4"), ref row);
        AddConstantRow("Max Decoherence Edges Fraction", PhysicsConstants.MaxDecoherenceEdgesFraction.ToString("P0"), ref row);
        AddConstantRow("Fragmentation Recovery Edge Frac", PhysicsConstants.FragmentationRecoveryEdgeFraction.ToString("P0"), ref row);
        AddConstantRow("Fragmentation Grace Period Steps", PhysicsConstants.FragmentationGracePeriodSteps.ToString(), ref row);

        // ============================================================
        // EDGE QUANTIZATION / TOPOLOGY
        // ============================================================
        AddSectionHeader("══ Edge Quantization / Topology ══", ref row, Color.DarkSlateBlue);
        AddConstantRow("Edge Weight Quantum", PhysicsConstants.EdgeWeightQuantum.ToString("F4"), ref row);
        AddConstantRow("RNG Step Cost", PhysicsConstants.RngStepCost.ToString("E6"), ref row);
        AddConstantRow("Edge Creation Cost", PhysicsConstants.EdgeCreationCost.ToString("F4"), ref row);
        AddConstantRow("Planck Weight Threshold", PhysicsConstants.PlanckWeightThreshold.ToString("E6"), ref row);
        AddConstantRow("Edge Creation Barrier", PhysicsConstants.EdgeCreationBarrier.ToString("F4"), ref row);
        AddConstantRow("Edge Annihilation Barrier", PhysicsConstants.EdgeAnnihilationBarrier.ToString("F4"), ref row);
        AddConstantRow("Weight Lower Soft Wall", PhysicsConstants.WeightLowerSoftWall.ToString("F4"), ref row);
        AddConstantRow("Weight Upper Soft Wall", PhysicsConstants.WeightUpperSoftWall.ToString("F4"), ref row);
        AddConstantRow("Weight Absolute Minimum", PhysicsConstants.WeightAbsoluteMinimum.ToString("E4"), ref row);
        AddConstantRow("Weight Absolute Maximum", PhysicsConstants.WeightAbsoluteMaximum.ToString("F4"), ref row);

        // ============================================================
        // UPDATE INTERVALS
        // ============================================================
        AddSectionHeader("══ Update Intervals ══", ref row, Color.Gray);
        AddConstantRow("Topology Update Interval", PhysicsConstants.TopologyUpdateInterval.ToString(), ref row);
        AddConstantRow("Topology Flips Divisor", PhysicsConstants.TopologyFlipsDivisor.ToString(), ref row);
        AddConstantRow("Geometry Update Interval", PhysicsConstants.GeometryUpdateInterval.ToString(), ref row);
        AddConstantRow("Gauge Constraint Interval", PhysicsConstants.GaugeConstraintInterval.ToString(), ref row);
        AddConstantRow("Energy Validation Interval", PhysicsConstants.EnergyValidationInterval.ToString(), ref row);
        AddConstantRow("Topological Protection Interval", PhysicsConstants.TopologicalProtectionInterval.ToString(), ref row);

        // ============================================================
        // ANNEALING
        // ============================================================
        AddSectionHeader("══ Hot Start & Annealing ══", ref row, Color.OrangeRed);
        AddConstantRow("Initial Annealing Temperature", PhysicsConstants.InitialAnnealingTemperature.ToString("F1"), ref row);
        AddConstantRow("Final Annealing Temperature", PhysicsConstants.FinalAnnealingTemperature.ToString("E4"), ref row);
        AddConstantRow("Physical Annealing Time Const", PhysicsConstants.PhysicalAnnealingTimeConstant.ToString("F1"), ref row);

        // ============================================================
        // ENERGY WEIGHTS
        // ============================================================
        AddSectionHeader("══ Energy Weights (Unified Hamiltonian) ══", ref row, Color.DarkGoldenrod);
        AddConstantRow("Scalar Field Energy Weight", PhysicsConstants.ScalarFieldEnergyWeight.ToString("F1"), ref row);
        AddConstantRow("Fermion Field Energy Weight", PhysicsConstants.FermionFieldEnergyWeight.ToString("F1"), ref row);
        AddConstantRow("Gauge Field Energy Weight", PhysicsConstants.GaugeFieldEnergyWeight.ToString("F1"), ref row);
        AddConstantRow("Yang-Mills Field Energy Weight", PhysicsConstants.YangMillsFieldEnergyWeight.ToString("F1"), ref row);
        AddConstantRow("Graph Link Energy Weight", PhysicsConstants.GraphLinkEnergyWeight.ToString("F1"), ref row);
        AddConstantRow("Gravity Curvature Energy Weight", PhysicsConstants.GravityCurvatureEnergyWeight.ToString("F1"), ref row);
        AddConstantRow("Cluster Binding Energy Weight", PhysicsConstants.ClusterBindingEnergyWeight.ToString("F1"), ref row);

        panelAllPhysicsConstants.Controls.Add(tlpAllPhysicsConstants);

        // Create a GroupBox to contain the scrollable panel
        var grpAllConstants = new GroupBox
        {
            Text = "All Physics Constants (Read-Only Reference)",
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };
        grpAllConstants.Controls.Add(panelAllPhysicsConstants);

        // Add to Settings tab
        settingsMainLayout.RowCount = 2;
        settingsMainLayout.RowStyles.Clear();
        settingsMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        settingsMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        settingsMainLayout.Controls.Add(grpAllConstants, 0, 1);
        settingsMainLayout.SetColumnSpan(grpAllConstants, 3);
    }

    #region Helper Methods for Constants Display

    private void AddSectionHeader(string text, ref int row, Color color)
    {
        tlpAllPhysicsConstants.RowCount = row + 1;
        tlpAllPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = color,
            Font = new Font(Font, FontStyle.Bold),
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(0, 10, 0, 3)
        };
        tlpAllPhysicsConstants.Controls.Add(label, 0, row);
        tlpAllPhysicsConstants.SetColumnSpan(label, 2);
        row++;
    }

    private void AddConstantRow(string name, string value, ref int row)
    {
        tlpAllPhysicsConstants.RowCount = row + 1;
        tlpAllPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lblName = new Label
        {
            Text = name + ":",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = Color.DimGray
        };
        var lblValue = new Label
        {
            Text = value,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = Color.Black,
            Font = new Font("Consolas", 9F)
        };
        tlpAllPhysicsConstants.Controls.Add(lblName, 0, row);
        tlpAllPhysicsConstants.Controls.Add(lblValue, 1, row);
        row++;
    }

    #endregion
}
