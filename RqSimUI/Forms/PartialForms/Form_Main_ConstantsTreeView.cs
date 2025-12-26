using RQSimulation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - Constants TreeView Display Panel.
/// Provides a hierarchical TreeView for viewing all PhysicsConstants with descriptions.
/// This is an alternative display to the table-based view in Form_Main_PhysicsInfo.cs.
/// </summary>
public partial class Form_Main
{
    private TreeView? _constantsTreeView;
    private RichTextBox? _constantDescriptionBox;
    private Panel? _constantsTreeDisplayPanel;

    /// <summary>
    /// Initializes the hierarchical TreeView-based constants display panel.
    /// This provides a navigable tree structure for all PhysicsConstants.
    /// Call this AFTER InitializeAllPhysicsConstantsDisplay() from Form_Main_PhysicsInfo.cs.
    /// </summary>
    private void InitializeConstantsTreeViewDisplay()
    {
        // Create a container panel for the constants display
        _constantsTreeDisplayPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 200,
            BorderStyle = BorderStyle.FixedSingle,
            Visible = false // Hidden by default, toggle with button
        };

        // Create TreeView for hierarchical constants display
        _constantsTreeView = new TreeView
        {
            Dock = DockStyle.Left,
            Width = 350,
            ShowNodeToolTips = true,
            Font = new Font("Consolas", 9F)
        };
        _constantsTreeView.AfterSelect += ConstantsTreeView_AfterSelect;

        // Create description box for selected constant
        _constantDescriptionBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(250, 250, 255)
        };

        // Create toggle button
        var btnToggleConstants = new Button
        {
            Text = "?? Show Constants Tree",
            Dock = DockStyle.Top,
            Height = 25,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(230, 230, 240)
        };
        btnToggleConstants.Click += (s, e) =>
        {
            if (_constantsTreeDisplayPanel == null) return;
            _constantsTreeDisplayPanel.Visible = !_constantsTreeDisplayPanel.Visible;
            btnToggleConstants.Text = _constantsTreeDisplayPanel.Visible
                ? "?? Hide Constants Tree"
                : "?? Show Constants Tree";
        };

        // Populate the TreeView with constants
        PopulateConstantsTree();

        // Add controls to panel
        _constantsTreeDisplayPanel.Controls.Add(_constantDescriptionBox);
        _constantsTreeDisplayPanel.Controls.Add(_constantsTreeView);

        // Add panel to Settings tab
        var settingsButtonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 25
        };
        settingsButtonPanel.Controls.Add(btnToggleConstants);

        tabPage_Settings.Controls.Add(_constantsTreeDisplayPanel);
        tabPage_Settings.Controls.Add(settingsButtonPanel);
    }

    /// <summary>
    /// Populates the TreeView with all PhysicsConstants organized by category.
    /// </summary>
    private void PopulateConstantsTree()
    {
        if (_constantsTreeView == null) return;
        
        _constantsTreeView.BeginUpdate();
        _constantsTreeView.Nodes.Clear();

        // === Fundamental Constants ===
        var fundamentalNode = new TreeNode("?? Fundamental Constants");
        AddTreeConstantNode(fundamentalNode, "Speed of Light (c)", nameof(PhysicsConstants.C), $"{PhysicsConstants.C}");
        AddTreeConstantNode(fundamentalNode, "Planck Constant (?)", nameof(PhysicsConstants.HBar), $"{PhysicsConstants.HBar}");
        AddTreeConstantNode(fundamentalNode, "Gravitational Constant (G)", nameof(PhysicsConstants.G), $"{PhysicsConstants.G}");
        AddTreeConstantNode(fundamentalNode, "Boltzmann Constant (k_B)", nameof(PhysicsConstants.KBoltzmann), $"{PhysicsConstants.KBoltzmann}");
        AddTreeConstantNode(fundamentalNode, "Planck Length", nameof(PhysicsConstants.PlanckLength), $"{PhysicsConstants.PlanckLength}");
        AddTreeConstantNode(fundamentalNode, "Planck Time", nameof(PhysicsConstants.PlanckTime), $"{PhysicsConstants.PlanckTime}");
        AddTreeConstantNode(fundamentalNode, "Planck Mass", nameof(PhysicsConstants.PlanckMass), $"{PhysicsConstants.PlanckMass}");
        AddTreeConstantNode(fundamentalNode, "Planck Energy", nameof(PhysicsConstants.PlanckEnergy), $"{PhysicsConstants.PlanckEnergy}");
        _constantsTreeView.Nodes.Add(fundamentalNode);

        // === Gauge Coupling Constants ===
        var gaugeNode = new TreeNode("? Gauge Coupling Constants");
        AddTreeConstantNode(gaugeNode, "Fine Structure Constant (?)", nameof(PhysicsConstants.FineStructureConstant), $"{PhysicsConstants.FineStructureConstant:E4}");
        AddTreeConstantNode(gaugeNode, "Strong Coupling (?_s)", nameof(PhysicsConstants.StrongCouplingConstant), $"{PhysicsConstants.StrongCouplingConstant}");
        AddTreeConstantNode(gaugeNode, "Weak Mixing Angle (sin??_W)", nameof(PhysicsConstants.WeakMixingAngle), $"{PhysicsConstants.WeakMixingAngle}");
        AddTreeConstantNode(gaugeNode, "Weak Coupling (g_W)", nameof(PhysicsConstants.WeakCouplingConstant), $"{PhysicsConstants.WeakCouplingConstant:F4}");
        AddTreeConstantNode(gaugeNode, "Hypercharge Coupling (g')", nameof(PhysicsConstants.HyperchargeCoupling), $"{PhysicsConstants.HyperchargeCoupling:F4}");
        AddTreeConstantNode(gaugeNode, "Strong Coupling (g_s)", nameof(PhysicsConstants.StrongCoupling), $"{PhysicsConstants.StrongCoupling:F4}");
        _constantsTreeView.Nodes.Add(gaugeNode);

        // === Simulation Parameters ===
        var simNode = new TreeNode("?? Simulation Parameters");
        AddTreeConstantNode(simNode, "Base Timestep", nameof(PhysicsConstants.BaseTimestep), $"{PhysicsConstants.BaseTimestep}");
        AddTreeConstantNode(simNode, "Gravitational Coupling", nameof(PhysicsConstants.GravitationalCoupling), $"{PhysicsConstants.GravitationalCoupling}");
        AddTreeConstantNode(simNode, "Warmup Duration", nameof(PhysicsConstants.WarmupDuration), $"{PhysicsConstants.WarmupDuration}");
        AddTreeConstantNode(simNode, "Annealing Time Constant", nameof(PhysicsConstants.AnnealingTimeConstant), $"{PhysicsConstants.AnnealingTimeConstant}");
        AddTreeConstantNode(simNode, "Topology Update Interval", nameof(PhysicsConstants.TopologyUpdateInterval), $"{PhysicsConstants.TopologyUpdateInterval}");
        AddTreeConstantNode(simNode, "Edge Weight Quantum", nameof(PhysicsConstants.EdgeWeightQuantum), $"{PhysicsConstants.EdgeWeightQuantum}");
        AddTreeConstantNode(simNode, "Weight Lower Bound", nameof(PhysicsConstants.WeightLowerSoftWall), $"{PhysicsConstants.WeightLowerSoftWall}");
        AddTreeConstantNode(simNode, "Weight Upper Bound", nameof(PhysicsConstants.WeightUpperSoftWall), $"{PhysicsConstants.WeightUpperSoftWall}");
        _constantsTreeView.Nodes.Add(simNode);

        // === RQ-Hypothesis Flags ===
        var rqFlagsNode = new TreeNode("?? RQ-Hypothesis Flags");
        AddTreeConstantNode(rqFlagsNode, "Hamiltonian Gravity", nameof(PhysicsConstants.UseHamiltonianGravity), $"{PhysicsConstants.UseHamiltonianGravity}");
        AddTreeConstantNode(rqFlagsNode, "Natural Dimension Emergence", nameof(PhysicsConstants.EnableNaturalDimensionEmergence), $"{PhysicsConstants.EnableNaturalDimensionEmergence}");
        AddTreeConstantNode(rqFlagsNode, "Topological Parity", nameof(PhysicsConstants.EnableTopologicalParity), $"{PhysicsConstants.EnableTopologicalParity}");
        AddTreeConstantNode(rqFlagsNode, "Lapse-Synchronized Geometry", nameof(PhysicsConstants.EnableLapseSynchronizedGeometry), $"{PhysicsConstants.EnableLapseSynchronizedGeometry}");
        AddTreeConstantNode(rqFlagsNode, "Topology Energy Compensation", nameof(PhysicsConstants.EnableTopologyEnergyCompensation), $"{PhysicsConstants.EnableTopologyEnergyCompensation}");
        AddTreeConstantNode(rqFlagsNode, "Plaquette Yang-Mills", nameof(PhysicsConstants.EnablePlaquetteYangMills), $"{PhysicsConstants.EnablePlaquetteYangMills}");
        AddTreeConstantNode(rqFlagsNode, "Symplectic Gauge Evolution", nameof(PhysicsConstants.EnableSymplecticGaugeEvolution), $"{PhysicsConstants.EnableSymplecticGaugeEvolution}");
        AddTreeConstantNode(rqFlagsNode, "Wilson Loop Protection", nameof(PhysicsConstants.EnableWilsonLoopProtection), $"{PhysicsConstants.EnableWilsonLoopProtection}");
        AddTreeConstantNode(rqFlagsNode, "Vacuum Energy Reservoir", nameof(PhysicsConstants.EnableVacuumEnergyReservoir), $"{PhysicsConstants.EnableVacuumEnergyReservoir}");
        _constantsTreeView.Nodes.Add(rqFlagsNode);

        // === RQ-Hypothesis Parameters ===
        var rqParamsNode = new TreeNode("?? RQ-Hypothesis Parameters");
        AddTreeConstantNode(rqParamsNode, "Geometry Inertia Mass", nameof(PhysicsConstants.GeometryInertiaMass), $"{PhysicsConstants.GeometryInertiaMass}");
        AddTreeConstantNode(rqParamsNode, "Yukawa Coupling", nameof(PhysicsConstants.YukawaCoupling), $"{PhysicsConstants.YukawaCoupling}");
        AddTreeConstantNode(rqParamsNode, "Gauge Field Damping", nameof(PhysicsConstants.GaugeFieldDamping), $"{PhysicsConstants.GaugeFieldDamping}");
        AddTreeConstantNode(rqParamsNode, "Gauge Tolerance", nameof(PhysicsConstants.GaugeTolerance), $"{PhysicsConstants.GaugeTolerance}");
        AddTreeConstantNode(rqParamsNode, "Max Removable Flux", nameof(PhysicsConstants.MaxRemovableFlux), $"{PhysicsConstants.MaxRemovableFlux:F4}");
        AddTreeConstantNode(rqParamsNode, "Lapse Function Alpha", nameof(PhysicsConstants.LapseFunctionAlpha), $"{PhysicsConstants.LapseFunctionAlpha}");
        AddTreeConstantNode(rqParamsNode, "Time Dilation Alpha", nameof(PhysicsConstants.TimeDilationAlpha), $"{PhysicsConstants.TimeDilationAlpha}");
        AddTreeConstantNode(rqParamsNode, "Wilson Parameter", nameof(PhysicsConstants.WilsonParameter), $"{PhysicsConstants.WilsonParameter}");
        _constantsTreeView.Nodes.Add(rqParamsNode);

        // === Spectral Action ===
        var spectralNode = new TreeNode("?? Spectral Action (Chamseddine-Connes)");
        AddTreeConstantNode(spectralNode, "Lambda Cutoff", "SpectralActionConstants.LambdaCutoff", $"{PhysicsConstants.SpectralActionConstants.LambdaCutoff}");
        AddTreeConstantNode(spectralNode, "Target Spectral Dimension", "SpectralActionConstants.TargetSpectralDimension", $"{PhysicsConstants.SpectralActionConstants.TargetSpectralDimension}");
        AddTreeConstantNode(spectralNode, "f? (Cosmological)", "SpectralActionConstants.F0_Cosmological", $"{PhysicsConstants.SpectralActionConstants.F0_Cosmological}");
        AddTreeConstantNode(spectralNode, "f? (Einstein-Hilbert)", "SpectralActionConstants.F2_EinsteinHilbert", $"{PhysicsConstants.SpectralActionConstants.F2_EinsteinHilbert}");
        AddTreeConstantNode(spectralNode, "f? (Weyl)", "SpectralActionConstants.F4_Weyl", $"{PhysicsConstants.SpectralActionConstants.F4_Weyl}");
        AddTreeConstantNode(spectralNode, "Dimension Potential Strength", "SpectralActionConstants.DimensionPotentialStrength", $"{PhysicsConstants.SpectralActionConstants.DimensionPotentialStrength}");
        AddTreeConstantNode(spectralNode, "Spectral Action Mode", "SpectralActionConstants.EnableSpectralActionMode", $"{PhysicsConstants.SpectralActionConstants.EnableSpectralActionMode}");
        _constantsTreeView.Nodes.Add(spectralNode);

        // === Wheeler-DeWitt ===
        var wdwNode = new TreeNode("??? Wheeler-DeWitt Constraint");
        AddTreeConstantNode(wdwNode, "Gravitational Coupling (?)", "WheelerDeWittConstants.GravitationalCoupling", $"{PhysicsConstants.WheelerDeWittConstants.GravitationalCoupling}");
        AddTreeConstantNode(wdwNode, "Lagrange Multiplier", "WheelerDeWittConstants.ConstraintLagrangeMultiplier", $"{PhysicsConstants.WheelerDeWittConstants.ConstraintLagrangeMultiplier}");
        AddTreeConstantNode(wdwNode, "Constraint Tolerance", "WheelerDeWittConstants.ConstraintTolerance", $"{PhysicsConstants.WheelerDeWittConstants.ConstraintTolerance}");
        AddTreeConstantNode(wdwNode, "Strict Mode", "WheelerDeWittConstants.EnableStrictMode", $"{PhysicsConstants.WheelerDeWittConstants.EnableStrictMode}");
        _constantsTreeView.Nodes.Add(wdwNode);

        // === Graph Health ===
        var healthNode = new TreeNode("?? Graph Health Parameters");
        AddTreeConstantNode(healthNode, "Critical Spectral Dimension", nameof(PhysicsConstants.CriticalSpectralDimension), $"{PhysicsConstants.CriticalSpectralDimension}");
        AddTreeConstantNode(healthNode, "Warning Spectral Dimension", nameof(PhysicsConstants.WarningSpectralDimension), $"{PhysicsConstants.WarningSpectralDimension}");
        AddTreeConstantNode(healthNode, "Giant Cluster Threshold", nameof(PhysicsConstants.GiantClusterThreshold), $"{PhysicsConstants.GiantClusterThreshold}");
        AddTreeConstantNode(healthNode, "Emergency Giant Cluster", nameof(PhysicsConstants.EmergencyGiantClusterThreshold), $"{PhysicsConstants.EmergencyGiantClusterThreshold}");
        AddTreeConstantNode(healthNode, "Decoherence Rate", nameof(PhysicsConstants.GiantClusterDecoherenceRate), $"{PhysicsConstants.GiantClusterDecoherenceRate}");
        _constantsTreeView.Nodes.Add(healthNode);

        // === Energy Weights ===
        var energyNode = new TreeNode("?? Energy Weights");
        AddTreeConstantNode(energyNode, "Scalar Field", nameof(PhysicsConstants.ScalarFieldEnergyWeight), $"{PhysicsConstants.ScalarFieldEnergyWeight}");
        AddTreeConstantNode(energyNode, "Fermion Field", nameof(PhysicsConstants.FermionFieldEnergyWeight), $"{PhysicsConstants.FermionFieldEnergyWeight}");
        AddTreeConstantNode(energyNode, "Gauge Field", nameof(PhysicsConstants.GaugeFieldEnergyWeight), $"{PhysicsConstants.GaugeFieldEnergyWeight}");
        AddTreeConstantNode(energyNode, "Yang-Mills Field", nameof(PhysicsConstants.YangMillsFieldEnergyWeight), $"{PhysicsConstants.YangMillsFieldEnergyWeight}");
        AddTreeConstantNode(energyNode, "Gravity Curvature", nameof(PhysicsConstants.GravityCurvatureEnergyWeight), $"{PhysicsConstants.GravityCurvatureEnergyWeight}");
        _constantsTreeView.Nodes.Add(energyNode);

        // === Higgs Parameters ===
        var higgsNode = new TreeNode("? Higgs Parameters");
        AddTreeConstantNode(higgsNode, "?? (negative for SSB)", nameof(PhysicsConstants.HiggsMuSquared), $"{PhysicsConstants.HiggsMuSquared}");
        AddTreeConstantNode(higgsNode, "? (quartic)", nameof(PhysicsConstants.HiggsLambda), $"{PhysicsConstants.HiggsLambda}");
        AddTreeConstantNode(higgsNode, "VEV (v)", nameof(PhysicsConstants.HiggsVEV), $"{PhysicsConstants.HiggsVEV:F4}");
        _constantsTreeView.Nodes.Add(higgsNode);

        _constantsTreeView.EndUpdate();
        _constantsTreeView.ExpandAll();
    }

    private void AddTreeConstantNode(TreeNode parent, string displayName, string memberName, string value)
    {
        var node = new TreeNode($"{displayName}: {value}")
        {
            Tag = memberName,
            ToolTipText = $"{memberName} = {value}"
        };
        parent.Nodes.Add(node);
    }

    private void ConstantsTreeView_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is not string memberName || _constantDescriptionBox == null)
            return;

        var type = typeof(PhysicsConstants);
        
        // Try to get member info for documentation
        var member = type.GetField(memberName, BindingFlags.Public | BindingFlags.Static);
        if (member == null)
        {
            // Try nested type
            if (memberName.Contains('.'))
            {
                var parts = memberName.Split('.');
                var nestedType = type.GetNestedType(parts[0]);
                if (nestedType != null)
                {
                    member = nestedType.GetField(parts[1], BindingFlags.Public | BindingFlags.Static);
                }
            }
        }

        var description = new System.Text.StringBuilder();
        description.AppendLine($"??? {e.Node.Text} ???");
        description.AppendLine();
        description.AppendLine($"Member: PhysicsConstants.{memberName}");
        description.AppendLine();

        if (member != null)
        {
            description.AppendLine($"Type: {member.FieldType.Name}");
            description.AppendLine($"Value: {member.GetValue(null)}");
            description.AppendLine();

            // Get documentation if available
            var xmlDoc = GetConstantDescription(member);
            if (!string.IsNullOrEmpty(xmlDoc))
            {
                description.AppendLine("Description:");
                description.AppendLine(xmlDoc);
            }
        }

        _constantDescriptionBox.Text = description.ToString();
    }

    private static string GetConstantDescription(FieldInfo field)
    {
        var summaries = new Dictionary<string, string>
        {
            { "FineStructureConstant", "Electromagnetic coupling ? = e?/(4????c) ? 1/137\nDetermines strength of electromagnetic interactions." },
            { "StrongCouplingConstant", "QCD coupling ?_s(M_Z) ? 0.118\nRuns with energy scale (asymptotic freedom)." },
            { "WeakMixingAngle", "sin??_W ? 0.231\nDetermines W/Z boson mass ratio." },
            { "GravitationalCoupling", "Effective gravitational coupling for simulation.\nScaled from physical G = 1 in Planck units." },
            { "UseHamiltonianGravity", "Enable 2nd order dynamics for geometry evolution.\nGeometry has inertia and propagates like waves." },
            { "EnableNaturalDimensionEmergence", "Allow spectral dimension to emerge from dynamics.\n4D should be energetically preferred without forcing." },
            { "EnableWilsonLoopProtection", "Protect gauge flux during topology changes.\nEnsures Gauss's law conservation." },
            { "GeometryInertiaMass", "Inertial mass of spacetime geometry.\nControls how fast geometry responds to matter." },
            { "KBoltzmann", "Boltzmann constant k_B = 1 in Planck units.\nRelates temperature to energy." },
            { "LapseFunctionAlpha", "Gravitational time dilation: N = 1/(1 + ?|R|)\nControls how curvature slows time." },
            { "WilsonParameter", "Wilson fermion parameter r ? 1.0\nSuppresses fermion doubling on lattice." }
        };

        return summaries.TryGetValue(field.Name, out var summary) ? summary : "";
    }
}
