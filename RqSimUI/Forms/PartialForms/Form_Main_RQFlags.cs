using RQSimulation;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - RQ-Hypothesis Experimental Flags UI controls.
/// </summary>
public partial class Form_Main
{
    // === RQ Experimental Flags CheckBox Controls ===
    private Label lblRQExperimentalFlagsHeader = null!;
    private CheckBox chkEnableNaturalDimensionEmergence = null!;
    private CheckBox chkEnableTopologicalParity = null!;
    private CheckBox chkEnableLapseSynchronizedGeometry = null!;
    private CheckBox chkEnableTopologyEnergyCompensation = null!;
    private CheckBox chkEnablePlaquetteYangMills = null!;

    /// <summary>
    /// Initializes RQ-Hypothesis Experimental Flags UI controls on the Settings tab.
    /// These flags control various physics behaviors per the RQ-Hypothesis checklist.
    /// Call this method from Form_Main constructor after InitializeComponent().
    /// </summary>
    private void InitializeRQExperimentalFlagsControls()
    {
        // === Header Label for RQ Experimental Flags ===
        lblRQExperimentalFlagsHeader = new Label
        {
            Text = "─── RQ Experimental Flags ───",
            AutoSize = true,
            ForeColor = Color.DarkMagenta,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(3, 15, 3, 5)
        };
        flpPhysics.Controls.Add(lblRQExperimentalFlagsHeader);

        // === Enable Natural Dimension Emergence ===
        chkEnableNaturalDimensionEmergence = new CheckBox
        {
            AutoSize = true,
            Text = "Natural Dimension Emergence",
            Checked = PhysicsConstants.EnableNaturalDimensionEmergence,
            Name = "chkEnableNaturalDimensionEmergence",
            Margin = new Padding(3)
        };
        AddTooltip(chkEnableNaturalDimensionEmergence,
            "When TRUE: Disable DimensionPenalty - allow d_S to emerge naturally\n" +
            "from the interplay of Ricci curvature and matter coupling.");
        chkEnableNaturalDimensionEmergence.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableNaturalDimensionEmergence);

        // === Enable Topological Parity ===
        chkEnableTopologicalParity = new CheckBox
        {
            AutoSize = true,
            Text = "Topological Parity (Fermions)",
            Checked = PhysicsConstants.EnableTopologicalParity,
            Name = "chkEnableTopologicalParity",
            Margin = new Padding(3)
        };
        AddTooltip(chkEnableTopologicalParity,
            "When TRUE: Use dynamic graph 2-coloring for staggered fermion parity\n" +
            "instead of array index (i % 2). Background-independent.");
        chkEnableTopologicalParity.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableTopologicalParity);

        // === Enable Lapse-Synchronized Geometry ===
        chkEnableLapseSynchronizedGeometry = new CheckBox
        {
            AutoSize = true,
            Text = "Lapse-Synchronized Geometry",
            Checked = PhysicsConstants.EnableLapseSynchronizedGeometry,
            Name = "chkEnableLapseSynchronizedGeometry",
            Margin = new Padding(3)
        };
        AddTooltip(chkEnableLapseSynchronizedGeometry,
            "When TRUE: Geometry evolution dt is scaled by edge lapse function\n" +
            "dt_edge = dt_global × √(N_i × N_j). Near black holes, geometry evolves slowly.");
        chkEnableLapseSynchronizedGeometry.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableLapseSynchronizedGeometry);

        // === Enable Topology Energy Compensation ===
        chkEnableTopologyEnergyCompensation = new CheckBox
        {
            AutoSize = true,
            Text = "Topology Energy Compensation",
            Checked = PhysicsConstants.EnableTopologyEnergyCompensation,
            Name = "chkEnableTopologyEnergyCompensation",
            Margin = new Padding(3)
        };
        AddTooltip(chkEnableTopologyEnergyCompensation,
            "When TRUE: Energy stored in fields on an edge is captured\n" +
            "and transferred to vacuum/radiation pool when edge is removed.");
        chkEnableTopologyEnergyCompensation.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnableTopologyEnergyCompensation);

        // === Enable Plaquette-Based Yang-Mills ===
        chkEnablePlaquetteYangMills = new CheckBox
        {
            AutoSize = true,
            Text = "Plaquette Yang-Mills",
            Checked = PhysicsConstants.EnablePlaquetteYangMills,
            Name = "chkEnablePlaquetteYangMills",
            Margin = new Padding(3)
        };
        AddTooltip(chkEnablePlaquetteYangMills,
            "When TRUE: Use plaquette (triangle Wilson loop) definition for\n" +
            "Yang-Mills field strength. Gauge-invariant by construction.");
        chkEnablePlaquetteYangMills.CheckedChanged += OnRQExperimentalFlagChanged;
        flpPhysics.Controls.Add(chkEnablePlaquetteYangMills);

        // Call additional flags initialization
        InitializeAdditionalRQFlags();
    }

    /// <summary>
    /// Handler for RQ Experimental Flag checkbox changes.
    /// Updates RQFlags in _simApi for runtime physics behavior changes.
    /// </summary>
    private void OnRQExperimentalFlagChanged(object? sender, EventArgs e)
    {
        var flags = _simApi.RQFlags;

        if (chkEnableNaturalDimensionEmergence != null)
            flags.EnableNaturalDimensionEmergence = chkEnableNaturalDimensionEmergence.Checked;

        if (chkEnableTopologicalParity != null)
            flags.EnableTopologicalParity = chkEnableTopologicalParity.Checked;

        if (chkEnableLapseSynchronizedGeometry != null)
            flags.EnableLapseSynchronizedGeometry = chkEnableLapseSynchronizedGeometry.Checked;

        if (chkEnableTopologyEnergyCompensation != null)
            flags.EnableTopologyEnergyCompensation = chkEnableTopologyEnergyCompensation.Checked;

        if (chkEnablePlaquetteYangMills != null)
            flags.EnablePlaquetteYangMills = chkEnablePlaquetteYangMills.Checked;

        // Additional RQ flags
        if (chkEnableSymplecticGaugeEvolution != null)
            flags.EnableSymplecticGaugeEvolution = chkEnableSymplecticGaugeEvolution.Checked;

        if (chkEnableAdaptiveTopologyDecoherence != null)
            flags.EnableAdaptiveTopologyDecoherence = chkEnableAdaptiveTopologyDecoherence.Checked;

        if (chkEnableWilsonLoopProtection != null)
            flags.EnableWilsonLoopProtection = chkEnableWilsonLoopProtection.Checked;

        if (chkEnableSpectralActionMode != null)
            flags.EnableSpectralActionMode = chkEnableSpectralActionMode.Checked;

        if (chkEnableWheelerDeWittStrictMode != null)
            flags.EnableWheelerDeWittStrictMode = chkEnableWheelerDeWittStrictMode.Checked;

        if (chkUseHamiltonianGravity != null)
            flags.UseHamiltonianGravity = chkUseHamiltonianGravity.Checked;

        if (chkEnableVacuumEnergyReservoir != null)
            flags.EnableVacuumEnergyReservoir = chkEnableVacuumEnergyReservoir.Checked;

        if (chkPreferOllivierRicciCurvature != null)
            flags.PreferOllivierRicciCurvature = chkPreferOllivierRicciCurvature.Checked;

        flags.MarkUpdated();

        // Log if simulation is running
        if (_isModernRunning && sender is CheckBox chk)
        {
            AppendSimConsole($"[RQ Flag] {chk.Name}: {chk.Checked}\n");
        }
    }

    private void AddTooltip(Control control, string text)
    {
        var tooltip = new ToolTip
        {
            AutoPopDelay = 15000,
            InitialDelay = 500,
            ReshowDelay = 200,
            ShowAlways = true
        };
        tooltip.SetToolTip(control, text);
    }
}
