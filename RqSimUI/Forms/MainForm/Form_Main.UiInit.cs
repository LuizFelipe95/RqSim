using RqSimUI.Forms.PartialForms;
using RqSimUI.FormSimAPI.Interfaces;
using RQSimulation.Analysis;

namespace RqSimForms;

partial class Form_Main
{
    private void InitializeUiAfterDesigner()
    {
        InitializeMainFormStatusBar();
        HookBackendStatusUpdates();

        drawingPanel = new DoubleBufferedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        drawingPanel.Paint += DrawingPanel_Paint;
        drawingPanel.MouseClick += DrawingPanel_MouseClick;

        tabPage_GUI.Controls.Add(drawingPanel);

        EnsureChartsLayout();

        // Use safe paint handlers to prevent crashes and show errors
        panelOnChart.Paint += (s, e) => SafePaint(s, e, PanelOnChart_Paint);
        panelHeavyChart.Paint += (s, e) => SafePaint(s, e, PanelHeavyChart_Paint);
        panelClusterChart.Paint += (s, e) => SafePaint(s, e, PanelClusterChart_Paint);
        panelEnergyChart.Paint += (s, e) => SafePaint(s, e, PanelEnergyChart_Paint);

        InitializeSynthesisTab();
        InitializeGpuControls();

        InitializeMultiGpuControls();
        InitializeAllSettingsControls();
        InitializePresets();
        InitializeExperiments();
        InitializeExperimentsTab();
        
        // Initialize UniPipeline tab for physics module management
        InitializeUniPipelineTab();

        _simApi.OnConsoleLog = msg => AppendSimLog(msg);

        numericUpDown_MaxFPS.Value = 10;

        WireLiveParameterHandlers();
        WireModuleCheckboxHandlers(); // Wire physics module checkboxes to pipeline

        _sysConsoleBuffer = new ConsoleBuffer(textBox_SysConsole, checkBox_AutoScrollSysConsole);
        _simConsoleBuffer = new ConsoleBuffer(textBox_SimConsole, checkBox_AutoScrollSimConsole);

        Initialize3DVisual();
        Initialize3DVisualCSR();

        // Initialize auto-tuning UI controls in splitMain.Panel2
        InitializeAutoTuningUI();

        // Wire existing checkBox_AutoTuning to handler
        checkBox_AutoTuning.CheckedChanged += CheckBox_AutoTuning_CheckedChanged;

        // Initialize physics events UI and TabPage
        InitializeTopEvents();
    }
    private void EnsureChartsLayout()
    {
        // Ensure panels exist
        panelOnChart ??= new Panel { Name = nameof(panelOnChart) };
        panelHeavyChart ??= new Panel { Name = nameof(panelHeavyChart) };
        panelClusterChart ??= new Panel { Name = nameof(panelClusterChart) };
        panelEnergyChart ??= new Panel { Name = nameof(panelEnergyChart) };

        // Enable double buffering for smooth rendering
        EnableDoubleBuffering(panelOnChart);
        EnableDoubleBuffering(panelHeavyChart);
        EnableDoubleBuffering(panelClusterChart);
        EnableDoubleBuffering(panelEnergyChart);

        // Force layout properties on panels (in case they came from Designer with different settings)
        panelOnChart.BackColor = Color.White;
        panelOnChart.Dock = DockStyle.Fill;
        panelOnChart.Margin = new Padding(4);

        panelHeavyChart.BackColor = Color.White;
        panelHeavyChart.Dock = DockStyle.Fill;
        panelHeavyChart.Margin = new Padding(4);

        panelClusterChart.BackColor = Color.White;
        panelClusterChart.Dock = DockStyle.Fill;
        panelClusterChart.Margin = new Padding(4);

        panelEnergyChart.BackColor = Color.White;
        panelEnergyChart.Dock = DockStyle.Fill;
        panelEnergyChart.Margin = new Padding(4);

        // Ensure TableLayoutPanel exists
        tlpCharts ??= new TableLayoutPanel { Name = nameof(tlpCharts) };

        // Force TLP properties
        tlpCharts.Dock = DockStyle.Fill;
        tlpCharts.ColumnCount = 2;
        tlpCharts.RowCount = 2;
        tlpCharts.Margin = new Padding(0);
        tlpCharts.Padding = new Padding(0);

        tlpCharts.SuspendLayout();

        tlpCharts.ColumnStyles.Clear();
        tlpCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tlpCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        tlpCharts.RowStyles.Clear();
        tlpCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        tlpCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        if (!tabPage_Charts.Controls.Contains(tlpCharts))
            tabPage_Charts.Controls.Add(tlpCharts);

        tlpCharts.BringToFront();

        // IMPORTANT: clear existing child controls first.
        // TableLayoutPanel allows multiple controls per cell; subsequent Add(...) does NOT replace.
        // Without clearing, controls can stack and only the top-most one is visible.
        tlpCharts.Controls.Clear();

        // Add controls explicitly into the intended cells
        tlpCharts.Controls.Add(panelOnChart, 0, 0);
        tlpCharts.Controls.Add(panelHeavyChart, 1, 0);
        tlpCharts.Controls.Add(panelClusterChart, 0, 1);
        tlpCharts.Controls.Add(panelEnergyChart, 1, 1);

        tlpCharts.ResumeLayout(performLayout: true);
    }

    private void SafePaint(object? sender, PaintEventArgs e, Action<object?, PaintEventArgs> paintAction)
    {
        try
        {
            paintAction(sender, e);
        }
        catch (Exception ex)
        {
            e.Graphics.DrawString($"Chart Error: {ex.Message}", SystemFonts.DefaultFont, Brushes.Red, 10, 40);
        }
    }

    private void EnableDoubleBuffering(Control control)
    {
        typeof(Control).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, control, new object[] { true });
    }
}

