using RqSimUI.FormSimAPI.Interfaces;
using RQSimulation;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    // Shortcut accessors for _simApi state
    private bool _isModernRunning { get => _simApi.IsModernRunning; set => _simApi.IsModernRunning = value; }
    private bool _spectrumLoggingEnabled { get => _simApi.SpectrumLoggingEnabled; set => _simApi.SpectrumLoggingEnabled = value; }

    // Результат и метрики Modern для экспорта/GUI
    private RQSimulation.ExampleModernSimulation.ScenarioResult? _modernResult => _simApi.ModernResult;

    // Обработчик кнопки запуска/остановки Modern Sim
    private void button_RunSimulation_Click(object? sender, EventArgs e)
    {


        // Если мы подключены к внешнему консольному процессу, эта кнопка должна управлять этой сессией
        // (запуск/паузу/возобновление), а не завершать процесс.
        if (_isExternalSimulation)
        {
            Task.Run(async () =>
            {
                try
                {
                    // Убедитесь, что пользовательский интерфейс связан с IPC консоли + общей памятью, чтобы обновления состояния были видны.
                    if (!_isConsoleBound)
                    {
                        BeginInvoke(new Action(() => AppendSysConsole("[Консоль] Не привязано. Попытка привязки...\n")));
                        BeginInvoke(new Action(() => button_BindConsoleSession_Click(null!, EventArgs.Empty)));
                        await Task.Delay(300).ConfigureAwait(false);
                    }

                    // Если привязано, переключите запуск/паузу в зависимости от удаленного статуса.
                    if (_isConsoleBound)
                    {
                        BeginInvoke(new Action(() => AppendSysConsole("[Консоль] Перенаправление команды Run на консольную сессию...\n")));
                        await HandleConsoleBoundSimulationAsync().ConfigureAwait(false);
                        return;
                    }

                    // Резервный вариант: попытайтесь начать даже без привязки к общей памяти.
                    BeginInvoke(new Action(() => AppendSysConsole("[Консоль] Неизвестное состояние привязки; отправка START в любом случае...\n")));
                    bool started = await _lifeCycleManager.Ipc.SendStartAsync().ConfigureAwait(false);
                    if (!started)
                    {
                        BeginInvoke(new Action(() => AppendSysConsole("[Консоль] Не удалось отправить команду START.\n")));
                    }
                }
                catch (Exception ex)
                {
                    BeginInvoke(new Action(() => AppendSysConsole($"[Консоль] Ошибка управления консольной сессией: {ex.Message}\n")));
                }
            });
            return;
        }

        if (_isModernRunning)
        {
            // Ask for confirmation before stopping
            var result = MessageBox.Show(
                "Симуляция запущена. Остановить и сохранить текущую сессию?\n\n" +
                "Все данные будут сохранены в историю сессий и могут быть экспортированы в JSON.",
                "Остановка симуляции",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                AppendSysConsole("[Modern] Остановка симуляции по запросу пользователя...\n");
                _modernCts?.Cancel();
            }
            return;
        }

        // Start new simulation
        _isModernRunning = true;
        _simApi.SimulationComplete = false;
        _modernCts = new CancellationTokenSource();
        button_RunModernSim.Text = "Stop Modern Sim";

        // Initialize new session
        string? gpuDeviceName = comboBox_GPUIndex.SelectedItem as string;
        var filters = new DisplayFilters
        {
            WeightThreshold = _displayWeightThreshold,
            HeavyOnly = _displayShowHeavyOnly,
            DynamicLayout = _useDynamicCoords
        };

        bool useGpu = CanUseGpu();
        _simApi.CurrentSession = _simApi.CreateSession(useGpu, gpuDeviceName, filters);

        // Initialize GPU synchronization if GPU is used
        if (useGpu)
        {
            try
            {
                var deviceContext = new RqSimUI.Rendering.Interop.UnifiedDeviceContext();
                var initResult = deviceContext.Initialize();

                if (initResult.Success)
                {
                    var gpuSyncManager = new RqSimRenderingEngine.Rendering.Interop.GpuSyncManager(deviceContext);
                    _simApi.SetupGpuSynchronization(gpuSyncManager);
                    AppendSysConsole($"[GPU] Sync manager initialized: {initResult.DiagnosticMessage}\n");
                }
                else
                {
                    AppendSysConsole($"[GPU] Failed to initialize device context: {initResult.DiagnosticMessage}\n");
                }
            }
            catch (Exception ex)
            {
                AppendSysConsole($"[GPU] Error initializing sync manager: {ex.Message}\n");
            }
        }

        // Initialize Multi-GPU cluster if enabled
        if (checkBox_UseMultiGPU.Checked && useGpu)
        {
            TryInitializeMultiGpuCluster();
        }

        // Start UI update timer (runs on UI thread, FPS from control)
        _uiUpdateTimer = new System.Windows.Forms.Timer();
        int targetFps = Math.Max(1, (int)numericUpDown_MaxFPS.Value);
        _uiUpdateTimer.Interval = 1000 / targetFps;
        _uiUpdateTimer.Tick += UiUpdateTimer_Tick;
        _uiUpdateTimer.Start();

        // Run simulation entirely in background thread (Event-Based only)
        var ct = _modernCts.Token;
        Task.Run(() =>
        {
            try
            {
                // Get config from UI thread
                SimulationConfig config = null!;
                Invoke(new Action(() => config = GetConfigFromUI()));


                // Initialize simulation state for event-based loop
                _simApi.InitializeSimulation(config);
                _simApi.InitializeLiveConfig(config);



                PhysicsConstants.ScientificMode = checkBox_ScienceSimMode.Checked;

                if (!PhysicsConstants.ScientificMode)
                {
                    // Initialize auto-tuning system if enabled
                    if (_simApi.AutoTuningEnabled)
                    {
                        _simApi.InitializeAutoTuning();
                        Invoke(new Action(() => AppendSimConsole("[AUTO-TUNE] Auto-tuning initialized for simulation\n")));
                    }
                }



                // Register background plugins to pipeline
                Invoke(new Action(() => RegisterBackgroundPluginsToPipeline()));

                // RQ-HYPOTHESIS: totalEvents = sweeps * N
                // Each sweep updates all N nodes once (in proper time order)
                // TotalSteps in UI maps to number of sweeps for event-based mode
                int totalSweeps = Math.Max(1, config.TotalSteps);
                int totalEvents = totalSweeps * Math.Max(1, config.NodeCount);

                Invoke(new Action(() => AppendSimConsole(
                    $"[EventBased] Mode: {totalSweeps} sweeps ? {config.NodeCount} nodes = {totalEvents} events\n")));

                // Pass GPU flag to enable GPU acceleration when checkbox is checked
                bool useGpuAcceleration = false;
                Invoke(new Action(() => useGpuAcceleration = CanUseGpu()));

                _simApi.RunParallelEventBasedLoop(ct, totalEvents, useParallel: true, useGpu: useGpuAcceleration);

                // Signal completion
                _simApi.SimulationComplete = true;
                BeginInvoke(new Action(() => OnSimulationCompleted(SessionEndReason.Completed)));
            }
            catch (OperationCanceledException)
            {
                _simApi.SimulationComplete = true;
                BeginInvoke(new Action(() => OnSimulationCompleted(SessionEndReason.CancelledByUser)));
            }
            catch (EnergyConservationException ex)
            {
                _simApi.SimulationComplete = true;
                BeginInvoke(new Action(() =>
                {
                    AppendSimConsole($"[ENERGY FATAL] {ex.Message}\n");
                    OnSimulationCompleted(SessionEndReason.EnergyTerminated, ex.Message);
                }));
            }
            catch (GraphFragmentationException ex)
            {
                _simApi.SimulationComplete = true;
                BeginInvoke(new Action(() =>
                {
                    AppendSimConsole($"[FRAGMENTATION FATAL] {ex.Message}\n");
                    OnSimulationCompleted(SessionEndReason.FragmentationTerminated, ex.Message);
                }));
            }
            catch (Exception ex)
            {
                _simApi.SimulationComplete = true;
                BeginInvoke(new Action(() =>
                {
                    AppendSysConsole($"[Error] {ex.Message}\n");
                    OnSimulationCompleted(SessionEndReason.Error);
                }));
            }
        });
    }

    /// <summary>
    /// Called when simulation completes (on UI thread)
    /// </summary>
    private void OnSimulationCompleted(SessionEndReason reason, string? terminationMessage = null)
    {
        // Stop timer
        _uiUpdateTimer?.Stop();
        _uiUpdateTimer?.Dispose();
        _uiUpdateTimer = null;

        // Archive session
        var events = lvEvents?.Items.Cast<ListViewItem>()
            .Select(i => new ImportantEvent
            {
                Step = int.TryParse(i.SubItems.Count > 0 ? i.SubItems[0].Text : "0", out int s) ? s : 0,
                Type = i.SubItems.Count > 1 ? i.SubItems[1].Text : string.Empty,
                Detail = i.SubItems.Count > 2 ? i.SubItems[2].Text : string.Empty
            }).ToList() ?? [];

        _simApi.ArchiveSession(reason, textBox_SysConsole?.Text ?? "", summaryTextBox?.Text ?? "", events);

        string reasonText = reason switch
        {
            SessionEndReason.Completed => "завершена успешно",
            SessionEndReason.CancelledByUser => "отменена",
            SessionEndReason.EnergyTerminated => "остановлена из-за нарушения сохранения энергии",
            SessionEndReason.FragmentationTerminated => "остановлена из-за фрагментации графа",
            _ => "завершилась с ошибкой"
        };
        AppendSimConsole($"[Session] Сессия {_simApi.CurrentSession?.SessionId} {reasonText} и добавлена в историю.\n");

        // Show user notification for physics-based termination
        if (reason == SessionEndReason.EnergyTerminated)
        {
            MessageBox.Show(
                $"Симуляция остановлена: нарушение закона сохранения энергии.\n\n" +
                $"Это физически обоснованное завершение - система достигла состояния,\n" +
                $"в котором энергетический баланс нарушен сверх допустимой погрешности.\n\n" +
                $"Детали:\n{terminationMessage ?? "См. консоль для подробностей."}",
                "Энергетическое завершение",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        else if (reason == SessionEndReason.FragmentationTerminated)
        {
            MessageBox.Show(
                $"Симуляция остановлена: критическая фрагментация графа.\n\n" +
                $"Это физически обоснованное завершение - пространственно-временной граф\n" +
                $"распался на несвязные компоненты, что нарушает каузальную структуру.\n\n" +
                $"Детали:\n{terminationMessage ?? "См. консоль для подробностей."}",
                "Фрагментация пространства-времени",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        // Cleanup background plugins
        CleanupBackgroundPlugins();
        // Cleanup
        _simApi.Cleanup();
        _cachedNodePositions = null;

        // Dispose Multi-GPU cluster
        DisposeMultiGpuCluster();

        _modernCts?.Dispose();
        _modernCts = null;
        _isModernRunning = false;
        _simApi.CurrentSession = null;
        button_RunModernSim.Text = "Run Modern Sim";

        // Final UI update
        DrawGraph();
        panelOnChart?.Invalidate();
        panelHeavyChart?.Invalidate();
        panelClusterChart?.Invalidate();
        panelEnergyChart?.Invalidate();

        AppendSimConsole($"[Session] Всего сессий в истории: {_simApi.SessionHistory.Count}\n");
    }


    /// <summary>
    /// Reads simulation configuration from UI controls with clamped ranges
    /// </summary>
    private SimulationConfig GetConfigFromUI()
    {
        var config = new SimulationConfig
        {
            NodeCount = (int)numNodeCount.Value,
            InitialEdgeProb = (double)numInitialEdgeProb.Value, // From UI: connectivity at Big Bang
            InitialExcitedProb = (double)numInitialExcitedProb.Value,
            TargetDegree = (int)numTargetDegree.Value,
            LambdaState = (double)numLambdaState.Value,
            Temperature = (double)numTemperature.Value,
            EdgeTrialProbability = (double)numEdgeTrialProb.Value,
            MeasurementThreshold = (double)numMeasurementThreshold.Value,
            Seed = 42,
            TotalSteps = Math.Max(1, (int)numTotalSteps.Value),
            LogEvery = 1,
            BaselineWindow = 50,
            FirstImpulse = -1, // Disabled - use hot start annealing
            ImpulsePeriod = -1, // Disabled
            CalibrationStep = -1, // Disabled
            VisualizationInterval = 1,
            MeasurementLogInterval = 50,
            FractalLevels = (int)numFractalLevels.Value,
            FractalBranchFactor = (int)numFractalBranchFactor.Value,

            UseQuantumDrivenStates = chkQuantumDriven.Checked,
            UseSpacetimePhysics = chkSpacetimePhysics.Checked,
            UseSpinorField = chkSpinorField.Checked,
            UseVacuumFluctuations = chkVacuumFluctuations.Checked,
            UseBlackHolePhysics = chkBlackHolePhysics.Checked,
            UseYangMillsGauge = chkYangMillsGauge.Checked,
            UseEnhancedKleinGordon = chkEnhancedKleinGordon.Checked,
            UseInternalTime = chkInternalTime.Checked,
            UseSpectralGeometry = chkSpectralGeometry.Checked,
            UseQuantumGraphity = chkQuantumGraphity.Checked,

            // === Physics Constants (from new panel) ===
            GravitationalCoupling = (double)numGravitationalCoupling.Value,
            VacuumEnergyScale = (double)numVacuumEnergyScale.Value,

            DecoherenceRate = (double)numDecoherenceRate.Value,
            HotStartTemperature = (double)numHotStartTemperature.Value,
            InitialNetworkTemperature = (double)numHotStartTemperature.Value,

            // === Extended Physics Flags ===
            UseRelationalTime = chkRelationalTime.Checked,
            UseRelationalYangMills = chkRelationalYangMills.Checked,
            UseNetworkGravity = chkNetworkGravity.Checked,
            UseUnifiedPhysicsStep = chkUnifiedPhysicsStep.Checked,
            EnforceGaugeConstraints = chkEnforceGaugeConstraints.Checked,
            UseCausalRewiring = chkCausalRewiring.Checked,
            UseTopologicalProtection = chkTopologicalProtection.Checked,
            ValidateEnergyConservation = chkValidateEnergyConservation.Checked,
            UseMexicanHatPotential = chkMexicanHatPotential.Checked,
            UseHotStartAnnealing = false, // Hot start annealing removed from UI; controlled internally
            UseGeometryMomenta = chkGeometryMomenta.Checked,
            UseTopologicalCensorship = chkTopologicalCensorship.Checked,

            UseEventBasedSimulation = true
        };

        // Clamp ranges for robustness
        if (config.InitialExcitedProb < 0) config.InitialExcitedProb = 0;
        if (config.InitialExcitedProb > 1) config.InitialExcitedProb = 1;
        if (config.EdgeTrialProbability < 0) config.EdgeTrialProbability = 0;
        if (config.EdgeTrialProbability > 1) config.EdgeTrialProbability = 1;
        if (config.MeasurementThreshold < 0) config.MeasurementThreshold = 0;
        if (config.VisualizationInterval < 1) config.VisualizationInterval = 1;
        if (config.MeasurementLogInterval < 1) config.MeasurementLogInterval = 1;
        if (config.InitialEdgeProb < 0) config.InitialEdgeProb = 0;
        if (config.InitialEdgeProb > 1) config.InitialEdgeProb = 1;
        if (config.GravitationalCoupling < 0) config.GravitationalCoupling = 0;
        if (config.DecoherenceRate < 0) config.DecoherenceRate = 0;

        return config;
    }
}
