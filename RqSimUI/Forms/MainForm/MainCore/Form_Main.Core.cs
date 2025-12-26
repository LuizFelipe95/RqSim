using RqSimForms.ProcessesDispatcher.Contracts;
using RqSimForms.ProcessesDispatcher.Managers;
using RQSimulation;
using System.Diagnostics;

namespace RqSimForms;

partial class Form_Main
{

    private void valAnnealingTimeConstant_Click(object sender, EventArgs e)
    {

    }

    // Shortcut accessors for commonly used properties
    private SimulationEngine? _simulationEngine => _simApi.SimulationEngine;
    private List<int> _seriesSteps => _simApi.SeriesSteps;
    private List<int> _seriesExcited => _simApi.SeriesExcited;
    private List<double> _seriesHeavyMass => _simApi.SeriesHeavyMass;
    private List<int> _seriesLargestCluster => _simApi.SeriesLargestCluster;
    private List<double> _seriesEnergy => _simApi.SeriesEnergy;
    private List<int> _seriesHeavyCount => _simApi.SeriesHeavyCount;
    private List<int> _seriesStrongEdges => _simApi.SeriesStrongEdges;
    private List<double> _seriesCorr => _simApi.SeriesCorr;
    private List<double> _seriesQNorm => _simApi.SeriesQNorm;
    private List<double> _seriesEntanglement => _simApi.SeriesEntanglement;
    private List<double> _seriesSpectralDimension => _simApi.SeriesSpectralDimension;
    private List<double> _seriesNetworkTemperature => _simApi.SeriesNetworkTemperature;
    private List<double> _seriesEffectiveG => _simApi.SeriesEffectiveG;
    private List<double> _seriesAdaptiveThreshold => _simApi.SeriesAdaptiveThreshold;


    // Существующий конструктор по умолчанию для Designer
    public Form_Main() : this(new LifeCycleManager())
    {
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _lifeCycleManager?.Dispose();
        }
        base.Dispose(disposing);
    }

    // Новый конструктор с инжекцией
    public Form_Main(LifeCycleManager lifeCycleManager)
    {
        ArgumentNullException.ThrowIfNull(lifeCycleManager);
        _lifeCycleManager = lifeCycleManager;

        InitializeComponent();
        InitializeUiAfterDesigner();

        FormClosing += Form_Main_FormClosing;
        Shown += Form_Main_Shown;
    }


    private async void Form_Main_Shown(object? sender, EventArgs e)
    {
        try
        {
            await _lifeCycleManager.OnFormLoadAsync();

            // Ключевое изменение:
            // если shared memory даёт валидный state (не stale) — считаем, что внешняя симуляция есть,
            // не требуем обязательно Status==Running.
            for (int i = 0; i < 10; i++)
            {
                var externalState = _lifeCycleManager.TryGetExternalSimulationState();
                if (externalState is not null)
                {
                    // Подтверждаем, что backend реально публикует данные (не только header/state)
                    if (!_externalReader.IsConnected && !_externalReader.TryConnect())
                    {
                        AppendSysConsole("[Dispatcher] State detected but shared memory not connectable yet.\n");
                    }
                    else if (_externalReader.TryReadHeader(out var header))
                    {
                        // Sync if either:
                        // 1. NodeCount > 0 (simulation has nodes to render)
                        // 2. Status is Running/Paused (simulation is active even if no render data yet)
                        bool hasNodes = header.NodeCount > 0;
                        bool isActiveStatus = header.StateCode == (int)SimulationStatus.Running ||
                                               header.StateCode == (int)SimulationStatus.Paused;

                        if (hasNodes || isActiveStatus)
                        {
                            SyncToExternalSimulation(externalState.Value);
                            return;
                        }
                        else
                        {
                            AppendSysConsole($"[Dispatcher] State detected but Status={header.StateCode}, NodeCount={header.NodeCount}.\n");
                        }
                    }
                    else
                    {
                        AppendSysConsole("[Dispatcher] State detected but header read failed.\n");
                    }
                }

                await Task.Delay(200);
            }

            // Если процесс прикреплён, но shared memory ещё не «пошло» — просто сообщаем.
            if (_lifeCycleManager.IsExternalProcessAttached)
                AppendSysConsole("[Dispatcher] External process attached but no shared memory data yet.\n");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Simulation Process Dispatcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }




    private void Form_Main_Load(object sender, EventArgs e)
    {



        try
        {
            _lifeCycleManager.OnFormLoadAsync().GetAwaiter().GetResult();

            if (comboBox_GPUComputeEngine.Items.Count > 2)
                comboBox_GPUComputeEngine.SelectedIndex = 2;
            else if (comboBox_GPUComputeEngine.Items.Count > 0)
                comboBox_GPUComputeEngine.SelectedIndex = 0;

            // Load saved settings and apply to UI
            LoadAndApplySettings();
        }
        catch (Exception ex)
        {
            // Логгирование и показ ошибки
            MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool _isAsyncCloseCompleted = false;

    private async void Form_Main_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isAsyncCloseCompleted) return;

        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            try
            {
                bool shouldClose = await _lifeCycleManager.HandleClosingAsync(e.CloseReason);
                if (shouldClose)
                {
                    SaveCurrentSettings();
                    _modernCts?.Cancel();
                    _simApi.Cleanup();
                    DisposeGdiResources();
                    _isAsyncCloseCompleted = true;
                    Close();
                }
            }
            catch
            {
                _isAsyncCloseCompleted = true;
                Close();
            }
        }
        else
        {
            try
            {
                _lifeCycleManager.HandleClosingAsync(e.CloseReason).GetAwaiter().GetResult();
                SaveCurrentSettings();
                _simApi.Cleanup();
            }
            catch { }
        }
    }
}
