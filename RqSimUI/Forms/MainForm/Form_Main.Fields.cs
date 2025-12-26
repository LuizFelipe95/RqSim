using RqSimForms.ProcessesDispatcher.Contracts;
using RqSimForms.ProcessesDispatcher.IPC;
using RqSimUI.Forms.PartialForms;
using RqSimUI.FormSimAPI.Interfaces;

namespace RqSimForms;





public partial class Form_Main
{
    private readonly DataReader _externalReader = new();

    // Буфер под ноды, чтобы не аллоцировать массив каждый тик
    private RenderNode[] _externalNodesBuffer = Array.Empty<RenderNode>();

    private DateTime _lastExternalNoDataLogUtc = DateTime.MinValue;

    private DoubleBufferedPanel drawingPanel;
    private Bitmap? canvasBitmap;

    // Throttling mechanism for UI updates
    private DateTime _lastUiUpdate = DateTime.MinValue;
    private readonly TimeSpan _minUiUpdateInterval = TimeSpan.FromMilliseconds(250);

    // Separate throttle for chart updates (less frequent)
    private DateTime _lastChartUpdate = DateTime.MinValue;
    private readonly TimeSpan _minChartUpdateInterval = TimeSpan.FromMilliseconds(500);

    // Drawing lock to prevent multiple concurrent drawing tasks
    private int _isDrawing = 0;

    // UI Timer for periodic updates (runs on UI thread)
    private System.Windows.Forms.Timer? _uiUpdateTimer;
    private CancellationTokenSource? _modernCts;

    private PointF[]? _cachedNodePositions; // cached for hit test
    private bool _useDynamicCoords = false; // toggle dynamic layout; circle if false
    private double _displayWeightThreshold = 0.0; // текущий порог веса для отображения рёбер
    private bool _displayShowHeavyOnly = false; // режим показа только тяжёлых кластеров

    // === Graph drawing throttling (for Event-Based performance) ===
    private DateTime _lastGraphDrawTime = DateTime.MinValue;
    private readonly TimeSpan _graphDrawInterval = TimeSpan.FromSeconds(5); // Draw graph every 5 sec

    private ConsoleBuffer? _consoleBuffer;

    // CSR visualization state (used by `Forms\3DVisual\CSRMode\*.cs` partials)
    private CsrVisualizationMode _csrVisMode = CsrVisualizationMode.QuantumPhase;

    private enum CsrVisualizationMode
    {
        QuantumPhase = 0,
        ProbabilityDensity = 1,
        Curvature = 2,
        GravityHeatmap = 3,
        NetworkTopology = 4,
        Clusters = 5
    }
}
