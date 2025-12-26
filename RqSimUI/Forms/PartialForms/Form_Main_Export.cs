using RQSimulation;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    private void BtnSnapshotImage_Click(object? sender, EventArgs e)
    {
        try
        {
            if (canvasBitmap is null)
            {
                AppendSysConsole("Нет изображения для сохранения.\n");
                return;
            }

            using SaveFileDialog dlg = new()
            {
                Filter = "PNG Image (*.png)|*.png",
                FileName = "rq-simulation-snapshot.png"
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                canvasBitmap.Save(dlg.FileName);
                AppendSysConsole($"Снимок сохранён: {dlg.FileName}\n");
            }
        }
        catch (Exception ex)
        {
            AppendSysConsole($"Ошибка сохранения снимка: {ex.Message}\n");
        }
    }

    private void button_SaveSimReult_Click(object sender, EventArgs e)
    {
        if (_simApi.IsModernRunning)
        {
            AppendSysConsole("Экспорт невозможен: симуляция ещё выполняется.\n");
            return;
        }

        // Check if there's any data to export
        bool hasSessionHistory = _simApi.SessionHistory.Count > 0;
        bool hasCurrentData = _simApi.LastResult != null || _seriesSteps.Count > 0;

        if (!hasSessionHistory && !hasCurrentData)
        {
            AppendSysConsole("Нет данных для сохранения. Запустите симуляцию.\n");
            return;
        }

        try
        {
            using SaveFileDialog dlg = new()
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = $"rq-simulation-export-{DateTime.Now:yyyyMMdd-HHmmss}.json"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // Build complete export including all session history
            var export = new
            {
                meta = new
                {
                    exportedAt = DateTime.UtcNow,
                    application = "RqSimForms",
                    version = Application.ProductVersion,
                    sessionCount = _simApi.SessionHistory.Count,
                    hasUnsavedCurrentSession = hasCurrentData && _simApi.CurrentSession == null,
                    currentFilters = new { weightThreshold = _displayWeightThreshold, heavyOnly = _displayShowHeavyOnly, dynamicLayout = _useDynamicCoords },
                    synthesis = new { synthesisCount = _simApi.SynthesisCount, fissionCount = _simApi.FissionCount }
                },
                // Export PhysicsConstants snapshot for reproducibility
                physicsConstants = new
                {
                    // Fundamental
                    fineStructureConstant = PhysicsConstants.FineStructureConstant,
                    strongCouplingConstant = PhysicsConstants.StrongCouplingConstant,
                    weakMixingAngle = PhysicsConstants.WeakMixingAngle,
                    // Gravity
                    gravitationalCoupling = PhysicsConstants.GravitationalCoupling,
                    warmupGravitationalCoupling = PhysicsConstants.WarmupGravitationalCoupling,
                    warmupDuration = PhysicsConstants.WarmupDuration,
                    gravityTransitionDuration = PhysicsConstants.GravityTransitionDuration,
                    cosmologicalConstant = PhysicsConstants.CosmologicalConstant,
                    // Annealing
                    initialAnnealingTemperature = PhysicsConstants.InitialAnnealingTemperature,
                    finalAnnealingTemperature = PhysicsConstants.FinalAnnealingTemperature,
                    annealingTimeConstant = PhysicsConstants.AnnealingTimeConstant,
                    // Fields
                    fieldDiffusionRate = PhysicsConstants.FieldDiffusionRate,
                    kleinGordonMass = PhysicsConstants.KleinGordonMass,
                    diracCoupling = PhysicsConstants.DiracCoupling,
                    gaugeCouplingConstant = PhysicsConstants.GaugeCouplingConstant,
                    // Quantum
                    vacuumFluctuationBaseRate = PhysicsConstants.VacuumFluctuationBaseRate,
                    pairCreationEnergyThreshold = PhysicsConstants.PairCreationEnergyThreshold,
                    spinorNormalizationThreshold = PhysicsConstants.SpinorNormalizationThreshold,
                    // Topology
                    edgeCreationBarrier = PhysicsConstants.EdgeCreationBarrier,
                    edgeAnnihilationBarrier = PhysicsConstants.EdgeAnnihilationBarrier,
                    defaultHeavyClusterThreshold = PhysicsConstants.DefaultHeavyClusterThreshold,
                    adaptiveThresholdSigma = PhysicsConstants.AdaptiveThresholdSigma,
                    minimumClusterSize = PhysicsConstants.MinimumClusterSize
                },
                // Export all archived sessions
                sessions = _simApi.SessionHistory.Select(s => new
                {
                    s.SessionId,
                    s.StartedAt,
                    s.EndedAt,
                    endReason = s.EndReason.ToString(),
                    s.GpuEnabled,
                    s.GpuDeviceName,
                    s.LastStep,
                    s.TotalStepsPlanned,
                    config = s.Config == null ? null : new
                    {
                        s.Config.NodeCount,
                        s.Config.InitialEdgeProb,
                        s.Config.InitialExcitedProb,
                        s.Config.TargetDegree,
                        s.Config.LambdaState,
                        s.Config.Temperature,
                        s.Config.EdgeTrialProbability,
                        s.Config.MeasurementThreshold,
                        s.Config.DynamicMeasurementThreshold,
                        s.Config.Seed,
                        s.Config.TotalSteps,
                        s.Config.LogEvery,
                        s.Config.BaselineWindow,
                        s.Config.FirstImpulse,
                        s.Config.ImpulsePeriod,
                        s.Config.CalibrationStep,
                        s.Config.VisualizationInterval,
                        s.Config.MeasurementLogInterval,
                        s.Config.UseQuantumDrivenStates,
                        s.Config.UseSpacetimePhysics,
                        s.Config.UseSpinorField,
                        s.Config.UseVacuumFluctuations,
                        s.Config.UseBlackHolePhysics,
                        s.Config.UseYangMillsGauge,
                        s.Config.UseEnhancedKleinGordon,
                        s.Config.UseInternalTime,
                        s.Config.UseSpectralGeometry,
                        s.Config.UseQuantumGraphity,
                        s.Config.FractalLevels,
                        s.Config.FractalBranchFactor,
                        // Extended physics parameters
                        s.Config.GravitationalCoupling,
                        s.Config.VacuumEnergyScale,
                        s.Config.InitialNetworkTemperature,
                        s.Config.AnnealingCoolingRate,
                        s.Config.UseRelationalTime,
                        s.Config.UseRelationalYangMills,
                        s.Config.UseNetworkGravity,
                        s.Config.DecoherenceRate,
                        s.Config.UseUnifiedPhysicsStep,
                        s.Config.EnforceGaugeConstraints,
                        s.Config.UseCausalRewiring,
                        s.Config.UseAsynchronousTime,
                        s.Config.UseTopologicalProtection,
                        s.Config.ValidateEnergyConservation,
                        s.Config.UseMexicanHatPotential,
                        s.Config.UseHotStartAnnealing,
                        s.Config.UseGeometryMomenta,
                        s.Config.UseTopologicalCensorship,
                        s.Config.UseSpectralMass,
                        s.Config.HotStartTemperature
                    },
                    filters = new { s.Filters.WeightThreshold, s.Filters.HeavyOnly, s.Filters.DynamicLayout },
                    // Session metadata
                    s.FinalSpectralDimension,
                    s.FinalNetworkTemperature,
                    s.WallClockDurationSeconds,
                    timeSeries = new
                    {
                        steps = s.SeriesSteps.ToArray(),
                        excited = s.SeriesExcited.ToArray(),
                        heavyMass = s.SeriesHeavyMass.ToArray(),
                        heavyCount = s.SeriesHeavyCount.ToArray(),
                        largestCluster = s.SeriesLargestCluster.ToArray(),
                        avgLinkDistance = s.SeriesAvgDist.ToArray(),
                        density = s.SeriesDensity.ToArray(),
                        energy = s.SeriesEnergy.ToArray(),
                        corr = s.SeriesCorr.ToArray(),
                        strongEdges = s.SeriesStrongEdges.ToArray(),
                        qNorm = s.SeriesQNorm.ToArray(),
                        qEnergy = s.SeriesQEnergy.ToArray(),
                        entanglement = s.SeriesEntanglement.ToArray(),
                        spectralDimension = s.SeriesSpectralDimension.ToArray(),
                        networkTemperature = s.SeriesNetworkTemperature.ToArray(),
                        effectiveG = s.SeriesEffectiveG.ToArray(),
                        adaptiveThreshold = s.SeriesAdaptiveThreshold.ToArray()
                    },
                    synthesisScatter = s.SynthesisData?.Select(x => new { x.volume, x.deltaMass }).ToArray(),
                    synthesisCount = s.SynthesisCount,
                    fissionCount = s.FissionCount,
                    result = s.Result == null ? null : new
                    {
                        s.Result.AverageExcited,
                        s.Result.MaxExcited,
                        s.Result.MeasurementConfigured,
                        s.Result.MeasurementTriggered
                    },
                    modernResult = s.ModernResult == null ? null : new
                    {
                        s.ModernResult.FinalTime,
                        s.ModernResult.ExcitedCount,
                        s.ModernResult.HeavyClusterCount,
                        s.ModernResult.HeavyClusterTotalMass,
                        s.ModernResult.HeavyClusterMaxMass,
                        s.ModernResult.HeavyClusterMeanMass,
                        s.ModernResult.ScalarFieldEnergy,
                        s.ModernResult.HiggsFieldEnergy
                    },
                    csv = s.Result == null ? null : new
                    {
                        timeSeriesCsv = s.Result.TimeSeries?.ToArray(),
                        avalancheStatsCsv = s.Result.AvalancheStats?.ToArray(),
                        measurementEventsCsv = s.Result.MeasurementEvents?.ToArray(),
                        heavyClustersCsv = s.Result.HeavyClusters?.ToArray()
                    },
                    consoleLog = s.ConsoleLog,
                    summaryText = s.SummaryText,
                    importantEvents = s.ImportantEvents.Select(ev => new { ev.Step, ev.Type, ev.Detail }).ToArray()
                }).ToArray(),
                // Also include current UI state (for continuity)
                currentUiState = new
                {
                    summary = summaryTextBox?.Text,
                    console = textBox_SysConsole?.Text,
                    importantEvents = lvEvents?.Items.Cast<ListViewItem>().Select(i => new
                    {
                        step = i.SubItems.Count > 0 ? i.SubItems[0].Text : null,
                        type = i.SubItems.Count > 1 ? i.SubItems[1].Text : null,
                        detail = i.SubItems.Count > 2 ? i.SubItems[2].Text : null
                    }).ToArray()
                }
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(export, options);
            File.WriteAllText(dlg.FileName, json, Encoding.UTF8);

            AppendSysConsole($"Данные сохранены: {dlg.FileName}\n");
            AppendSysConsole($"Экспортировано сессий: {_simApi.SessionHistory.Count}\n");

            // Ask if user wants to clear session history after export
            if (_simApi.SessionHistory.Count > 0)
            {
                var clearResult = MessageBox.Show(
                    $"Экспортировано {_simApi.SessionHistory.Count} сессий.\n\nОчистить историю сессий?",
                    "Очистка истории",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (clearResult == DialogResult.Yes)
                {
                    _simApi.SessionHistory.Clear();
                    AppendSysConsole("История сессий очищена.\n");
                }
            }
        }
        catch (Exception ex)
        {
            AppendSysConsole($"Ошибка сохранения JSON: {ex.Message}\n");
        }
    }

    private void btnExpornShortJson_Click(object sender, EventArgs e)
    {
        // Quick snapshot export - current state + startup parameters + decimated dynamics
        try
        {
            using SaveFileDialog dlg = new()
            {
                Filter = "JSON file (*.json)|*.json",
                FileName = $"rq-snapshot-{DateTime.Now:yyyyMMdd-HHmmss}.json",
                Title = "Сохранить снимок текущего состояния"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // Get last available step data
            int lastStep = _seriesSteps.Count > 0 ? _seriesSteps[^1] : _simApi.LiveStep;
            int lastExcited = _seriesExcited.Count > 0 ? _seriesExcited[^1] : _simApi.LiveExcited;
            double lastHeavyMass = _seriesHeavyMass.Count > 0 ? _seriesHeavyMass[^1] : _simApi.LiveHeavyMass;
            double lastSpectralDim = _seriesSpectralDimension.Count > 0 ? _seriesSpectralDimension[^1] : _simApi.LiveSpectralDim;
            double lastTemp = _seriesNetworkTemperature.Count > 0 ? _seriesNetworkTemperature[^1] : _simApi.LiveTemp;
            double lastEffectiveG = _seriesEffectiveG.Count > 0 ? _seriesEffectiveG[^1] : _simApi.LiveEffectiveG;
            double lastThreshold = _seriesAdaptiveThreshold.Count > 0 ? _seriesAdaptiveThreshold[^1] : _simApi.LiveAdaptiveThreshold;
            int lastLargestCluster = _seriesLargestCluster.Count > 0 ? _seriesLargestCluster[^1] : _simApi.LiveLargestCluster;
            int lastStrongEdges = _seriesStrongEdges.Count > 0 ? _seriesStrongEdges[^1] : _simApi.LiveStrongEdges;

            // Get decimated dynamics (max 1000 points for ~50KB JSON)
            var dynamics = _simApi.GetDecimatedDynamics(1000);

            var snapshot = new
            {
                meta = new
                {
                    exportedAt = DateTime.UtcNow,
                    type = "snapshot",
                    isRunning = _isModernRunning,
                    totalDataPoints = _seriesSteps.Count,
                    decimatedPoints = dynamics.Steps.Length,
                    decimationStride = dynamics.DecimationStride
                },
                // GPU acceleration info
                gpuInfo = new
                {
                    enabled = CanUseGpu(),
                    available = _simApi.GpuAvailable,
                    deviceName = comboBox_GPUIndex.SelectedItem as string,
                    stats = new
                    {
                        kernelLaunches = _simApi.GpuStats.KernelLaunches,
                        topologyRebuilds = _simApi.GpuStats.TopologyRebuilds,
                        weightSyncs = _simApi.GpuStats.WeightSyncs
                    },
                    acceleratedOps = _simApi.GpuStats.AcceleratedOperations,
                    cpuBoundOps = _simApi.GpuStats.CpuBoundOperations
                },
                // Startup parameters
                startupConfig = _simApi.LastConfig == null ? null : new
                {
                    NodeCount = _simApi.LastConfig.NodeCount,
                    TotalSteps = _simApi.LastConfig.TotalSteps,
                    InitialEdgeProb = _simApi.LastConfig.InitialEdgeProb,
                    InitialExcitedProb = _simApi.LastConfig.InitialExcitedProb,
                    TargetDegree = _simApi.LastConfig.TargetDegree,
                    Seed = _simApi.LastConfig.Seed,
                    GravitationalCoupling = _simApi.LastConfig.GravitationalCoupling,
                    HotStartTemperature = _simApi.LastConfig.HotStartTemperature,
                    AnnealingCoolingRate = _simApi.LastConfig.AnnealingCoolingRate,
                    DecoherenceRate = _simApi.LastConfig.DecoherenceRate,
                    VacuumEnergyScale = _simApi.LastConfig.VacuumEnergyScale,
                    UseSpectralGeometry = _simApi.LastConfig.UseSpectralGeometry,
                    UseNetworkGravity = _simApi.LastConfig.UseNetworkGravity,
                    UseQuantumDrivenStates = _simApi.LastConfig.UseQuantumDrivenStates,
                    UseSpinorField = _simApi.LastConfig.UseSpinorField,
                    UseVacuumFluctuations = _simApi.LastConfig.UseVacuumFluctuations,
                    UseYangMillsGauge = _simApi.LastConfig.UseYangMillsGauge,
                    UseRelationalTime = _simApi.LastConfig.UseRelationalTime,
                    UseTopologicalProtection = _simApi.LastConfig.UseTopologicalProtection,
                    UseCausalRewiring = _simApi.LastConfig.UseCausalRewiring,
                    UseMexicanHatPotential = _simApi.LastConfig.UseMexicanHatPotential,
                    UseGeometryMomenta = _simApi.LastConfig.UseGeometryMomenta
                },
                // Physics constants
                physicsConstants = new
                {
                    FineStructureConstant = PhysicsConstants.FineStructureConstant,
                    StrongCouplingConstant = PhysicsConstants.StrongCouplingConstant,
                    GravitationalCoupling = PhysicsConstants.GravitationalCoupling,
                    WarmupGravitationalCoupling = PhysicsConstants.WarmupGravitationalCoupling,
                    WarmupDuration = PhysicsConstants.WarmupDuration,
                    GravityTransitionDuration = PhysicsConstants.GravityTransitionDuration,
                    InitialAnnealingTemperature = PhysicsConstants.InitialAnnealingTemperature,
                    FinalAnnealingTemperature = PhysicsConstants.FinalAnnealingTemperature,
                    AnnealingTimeConstant = PhysicsConstants.AnnealingTimeConstant,
                    AdaptiveThresholdSigma = PhysicsConstants.AdaptiveThresholdSigma,
                    DefaultHeavyClusterThreshold = PhysicsConstants.DefaultHeavyClusterThreshold,
                    MinimumClusterSize = PhysicsConstants.MinimumClusterSize,
                    CosmologicalConstant = PhysicsConstants.CosmologicalConstant
                },
                // Current state
                currentState = new
                {
                    step = lastStep,
                    totalStepsPlanned = _simApi.LastConfig?.TotalSteps ?? 0,
                    excited = lastExcited,
                    heavyMass = lastHeavyMass,
                    spectralDimension = lastSpectralDim,
                    networkTemperature = lastTemp,
                    effectiveG = lastEffectiveG,
                    adaptiveThreshold = lastThreshold,
                    largestCluster = lastLargestCluster,
                    strongEdges = lastStrongEdges
                },
                // Final metrics
                finalMetrics = new
                {
                    spectralDimension = _simApi.FinalSpectralDimension,
                    networkTemperature = _simApi.FinalNetworkTemperature,
                    wallClockSeconds = (DateTime.UtcNow - _simApi.SimulationWallClockStart).TotalSeconds
                },
                // Decimated dynamics (compact, ~1000 points max)
                dynamics = new
                {
                    steps = dynamics.Steps,
                    excited = dynamics.Excited,
                    energy = dynamics.Energy,
                    heavyMass = dynamics.HeavyMass,
                    largestCluster = dynamics.LargestCluster,
                    strongEdges = dynamics.StrongEdges,
                    spectralDimension = dynamics.SpectralDimension,
                    networkTemperature = dynamics.NetworkTemperature
                }
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(snapshot, options);
            File.WriteAllText(dlg.FileName, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
