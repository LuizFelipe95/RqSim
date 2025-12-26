using RqSimForms.Helpers;
using RqSimForms.ProcessesDispatcher.Contracts;
using RQSimulation;
using RQSimulation.Analysis;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    /// <summary>
    /// Timer tick handler - updates UI from live metrics (runs on UI thread)
    /// Non-blocking: skips frame if dispatcher is busy with calculation thread
    /// Performance: Skips invisible tab updates, uses precomputed statistics
    /// Target: 5-10 FPS for smooth visualization
    /// 
    /// OPTIMIZATION: Graph drawing is throttled to every 5 seconds in Event-Based mode
    /// to reduce CPU overhead. Charts update more frequently (every tick).
    /// </summary>
    private void UiUpdateTimer_Tick(object? sender, EventArgs e)
    {
        // 1. Fetch and display logs from LogStatistics (Async batch update)
        var cpuLogs = LogStatistics.FetchCpuLogs();
        if (cpuLogs.Length > 0)
        {
            // Use consoleTextBox_SysConsole directly or via AppendSysConsole if it uses it
            // AppendSysConsole uses _consoleBuffer which wraps consoleTextBox_SysConsole
            AppendSysConsole(string.Join(Environment.NewLine, cpuLogs) + Environment.NewLine);
        }

        var gpuLogs = LogStatistics.FetchGpuLogs();
        if (gpuLogs.Length > 0)
        {
            AppendGPUConsole(string.Join(Environment.NewLine, gpuLogs) + Environment.NewLine);
        }

        if (_isExternalSimulation)
        {
            // 1) Состояние (для UI)
            var externalState = _lifeCycleManager.TryGetExternalSimulationState();
            if (externalState is null)
            {
                // SharedMemory не читается или stale
                RateLimitedLogExternal("externalState=null (shared memory not available or stale)");
                return;
            }

            var s = externalState.Value;

            // Use extended metrics from SharedHeader
            UpdateDashboard(
                (int)s.Iteration, 
                0, // totalSteps not tracked in server mode
                s.ExcitedCount, 
                s.HeavyMass, 
                s.LargestCluster, 
                s.StrongEdgeCount, 
                s.Status.ToString(), 
                s.QNorm, 
                s.Entanglement, 
                s.Correlation);
            
            valDashNodes.Text = s.NodeCount.ToString();
            valDashStatus.Text = s.Status.ToString();
            valDashSpectralDim.Text = s.SpectralDimension.ToString("F3");
            valDashNetworkTemp.Text = s.NetworkTemperature.ToString("F3");
            valDashEffectiveG.Text = s.EffectiveG.ToString("F4");
            valDashStrongEdges.Text = s.StrongEdgeCount.ToString();

            // Show external simulation failure clearly
            switch (s.Status)
            {
                case SimulationStatus.Faulted:
                    valDashStatus.ForeColor = Color.Red;
                    break;
                case SimulationStatus.Running:
                    valDashStatus.ForeColor = Color.Green;
                    break;
                case SimulationStatus.Paused:
                    valDashStatus.ForeColor = Color.DarkOrange;
                    break;
                default:
                    valDashStatus.ForeColor = SystemColors.ControlText;
                    break;
            }
            
            // Color-code spectral dimension
            if (s.SpectralDimension < 1.5)
                valDashSpectralDim.ForeColor = Color.Red;
            else if (s.SpectralDimension > 4.0)
                valDashSpectralDim.ForeColor = Color.DarkOrange;
            else
                valDashSpectralDim.ForeColor = Color.Green;

            // 2) IMPORTANT: do not auto-resume external simulation here.
            // The UI should only send Start/Pause/Stop based on explicit user action.
            // (Removed previous auto-start-on-paused behavior.)

            // 3) Чтение данных для рендера (RenderNode[])
            if (!_externalReader.IsConnected && !_externalReader.TryConnect())
            {
                RateLimitedLogExternal("DataReader.TryConnect() == false");
                return;
            }

            if (!_externalReader.TryReadHeader(out var header))
            {
                RateLimitedLogExternal("TryReadHeader() == false");
                return;
            }

            if (header.NodeCount <= 0)
            {
                RateLimitedLogExternal($"Header.NodeCount={header.NodeCount} (no render nodes published yet)");
                return;
            }

            if (_externalNodesBuffer.Length < header.NodeCount)
                _externalNodesBuffer = new RenderNode[header.NodeCount];

            if (!_externalReader.TryReadNodes(header.NodeCount, _externalNodesBuffer))
            {
                RateLimitedLogExternal("TryReadNodes() == false");
                return;
            }

            // Draw external simulation graph (throttled like local simulation)
            var currentTime = DateTime.UtcNow;
            if (currentTime - _lastGraphDrawTime >= _graphDrawInterval)
            {
                _lastGraphDrawTime = currentTime;
                DrawExternalGraph();
            }

            // Data is now in _externalNodesBuffer and will be picked up by Timer3D_Tick in PartialForm3D.cs
            return;
        }

        if (_simApi.SimulationComplete || !_isModernRunning)
        {
            // Keep timer running to flush logs even if sim is done, 
            // but maybe check if we should stop it eventually.
            // For now, let's stop it only if no logs and sim complete.
            if (cpuLogs.Length == 0 && gpuLogs.Length == 0)
            {
                _uiUpdateTimer?.Stop();
            }
            return;
        }

        // Force get display data with timeout - ensures we get data even under contention
        // This is critical for Event-Based mode where calculation thread holds lock frequently
        var displayData = _simApi.Dispatcher.ForceGetDisplayDataImmediate(timeoutMs: 50);

        // Read volatile fields (lock-free)
        int step = _simApi.LiveStep;
        int excited = _simApi.LiveExcited;
        double heavyMass = _simApi.LiveHeavyMass;
        int largestCluster = _simApi.LiveLargestCluster;
        int strongEdges = _simApi.LiveStrongEdges;
        double qNorm = _simApi.LiveQNorm;
        double entanglement = _simApi.LiveEntanglement;
        double correlation = _simApi.LiveCorrelation;
        int totalSteps = _simApi.LiveTotalSteps;

        // Calculate statistics without LINQ (precompute in loop for performance)
        double avgExcited = 0.0;
        int maxExcited = 0;
        if (displayData.DecimatedExcited.Length > 0)
        {
            int sum = 0;
            for (int i = 0; i < displayData.DecimatedExcited.Length; i++)
            {
                int val = displayData.DecimatedExcited[i];
                sum += val;
                if (val > maxExcited) maxExcited = val;
            }
            avgExcited = (double)sum / displayData.DecimatedExcited.Length;
        }

        // OPTIMIZATION: Draw graph less frequently (every 5 seconds)
        // This reduces CPU load significantly while keeping simulation fast
        var now = DateTime.UtcNow;
        if (now - _lastGraphDrawTime >= _graphDrawInterval)
        {
            _lastGraphDrawTime = now;
            DrawGraph();
        }

        // Invalidate chart panels (always - they're lightweight)
        panelOnChart?.Invalidate();
        panelHeavyChart?.Invalidate();
        panelClusterChart?.Invalidate();
        panelEnergyChart?.Invalidate();

        int graphN = _simulationEngine?.Graph?.N ?? 100;
        string phase = excited > graphN / 3 ? "Active" : (excited > graphN / 10 ? "Moderate" : "Quiet");

        // Update all dashboard components (always update, they're lightweight)
        UpdateDashboard(step, totalSteps, excited, heavyMass, largestCluster,
            strongEdges, phase, qNorm, entanglement, correlation);
        UpdateStatusBar(step, totalSteps, excited, avgExcited, heavyMass);
        UpdateRunSummary(totalSteps, step, avgExcited, maxExcited, 0, false, false);
        UpdateLiveMetrics(0.0, 0.0, strongEdges, largestCluster, heavyMass, _spectrumLoggingEnabled);

        // Update auto-tuning status display if enabled
        UpdateAutoTuningStatusDisplay();
    }
    void RateLimitedLogExternal(string msg)
    {
        var now = DateTime.UtcNow;
        if (now - _lastExternalNoDataLogUtc < TimeSpan.FromSeconds(1))
            return;

        _lastExternalNoDataLogUtc = now;
        AppendSysConsole($"[ExternalRender] no data: {msg}\n");
    }
    /// <summary>
    /// Updates dashboard metrics with current simulation state
    /// </summary>
    private void UpdateDashboard(int step, int totalSteps, int excited, double heavyMass,
        int largestCluster, int strongEdges, string phase, double qNorm,
        double entanglement, double correlation)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateDashboard(step, totalSteps, excited, heavyMass, largestCluster,
                strongEdges, phase, qNorm, entanglement, correlation));
            return;
        }

        valDashNodes.Text = _simulationEngine?.Graph?.N.ToString() ?? "0";
        valDashTotalSteps.Text = totalSteps.ToString();
        valDashCurrentStep.Text = step.ToString();
        valDashExcited.Text = excited.ToString();
        valDashHeavyMass.Text = heavyMass.ToString("F2");
        valDashLargestCluster.Text = largestCluster.ToString();
        valDashStrongEdges.Text = strongEdges.ToString();
        valDashPhase.Text = phase;
        valDashQNorm.Text = qNorm.ToString("F6");
        valDashEntanglement.Text = entanglement.ToString("F6");
        valDashCorrelation.Text = correlation.ToString("F6");
        valDashStatus.Text = _isModernRunning ? "Running..." : "Ready";

        // New spectral metrics
        double spectralDim = _simApi.LiveSpectralDim;
        double effectiveG = _simApi.LiveEffectiveG;
        double networkTemp = _simApi.LiveTemp;

        // Calculate gSuppression = effectiveG / targetG (after warmup+transition)
        double targetG = _simApi.LastConfig?.GravitationalCoupling ?? 0.2;
        double gSuppression = targetG > 0 ? effectiveG / targetG : 1.0;
        gSuppression = Math.Clamp(gSuppression, 0.0, 2.0);

        valDashSpectralDim.Text = spectralDim.ToString("F3");
        valDashEffectiveG.Text = effectiveG.ToString("F4");
        valDashGSuppression.Text = gSuppression.ToString("F3");
        valDashNetworkTemp.Text = networkTemp.ToString("F3");

        // Color-code spectral dimension for quick visual feedback
        if (spectralDim < 1.5)
            valDashSpectralDim.ForeColor = Color.Red;
        else if (spectralDim > 4.0)
            valDashSpectralDim.ForeColor = Color.DarkOrange;
        else
            valDashSpectralDim.ForeColor = Color.Green;

        // Color-code gSuppression
        if (gSuppression < 0.3)
            valDashGSuppression.ForeColor = Color.Red;
        else if (gSuppression < 0.7)
            valDashGSuppression.ForeColor = Color.DarkOrange;
        else
            valDashGSuppression.ForeColor = Color.Black;

        // === Extended Live Metrics from Dispatcher ===
        double clusterRatio = _simApi.Dispatcher.LiveClusterRatio;
        double avgDegree = _simApi.Dispatcher.LiveAvgDegree;
        int edgeCount = _simApi.Dispatcher.LiveEdgeCount;
        int componentCount = _simApi.Dispatcher.LiveComponentCount;

        // Color-code LargestCluster by ratio (giant cluster warning)
        if (clusterRatio >= 0.7)
            valDashLargestCluster.ForeColor = Color.Red;
        else if (clusterRatio >= 0.5)
            valDashLargestCluster.ForeColor = Color.DarkOrange;
        else
            valDashLargestCluster.ForeColor = Color.Green;

        // Update LargestCluster text to show ratio
        valDashLargestCluster.Text = $"{largestCluster} ({clusterRatio:P0})";
    }

    // Helpers to update newly added UI controls
    private void UpdateStatusBar(int currentStep, int totalSteps, int currentOn, double avgExcited, double? heavyMass)
    {
        if (statusLabelSteps is null) return;

        statusLabelSteps.Text = $"Step: {currentStep}/{totalSteps}";
        statusLabelExcited.Text = $"Excited: {currentOn} (avg {avgExcited:F2})";

        // Extended status bar with key topology metrics
        double clusterRatio = _simApi.Dispatcher.LiveClusterRatio;
        double avgDegree = _simApi.Dispatcher.LiveAvgDegree;
        int edgeCount = _simApi.Dispatcher.LiveEdgeCount;
        int componentCount = _simApi.Dispatcher.LiveComponentCount;

        statusLabelHeavyMass.Text = $"Giant:{clusterRatio:P0} | E:{edgeCount} | <k>:{avgDegree:F1} | Comp:{componentCount}";
    }

    private void UpdateRunSummary(int totalSteps, int currentStep, double avgExcited, int maxExcited, int avalanches, bool measurementConfigured, bool measurementTriggered)
    {
        if (valTotalSteps is null) return;
        valTotalSteps.Text = totalSteps.ToString();
        valCurrentStep.Text = currentStep.ToString();
        valExcitedAvg.Text = avgExcited.ToString("F2");
        valExcitedMax.Text = maxExcited.ToString();
        valAvalancheCount.Text = avalanches.ToString();
        valMeasurementStatus.Text = measurementConfigured
            ? (measurementTriggered ? "TRIGGERED" : "READY")
            : "NOT CONFIGURED";
    }

    private void UpdateLiveMetrics(double globalNbr, double globalSpont, int strongEdges, int largestCluster, double heavyMass, bool spectrumOn)
    {
        if (valGlobalNbr is null) return;
        valGlobalNbr.Text = globalNbr.ToString("F3");
        valGlobalSpont.Text = globalSpont.ToString("F3");
        valStrongEdges.Text = strongEdges.ToString();
        valLargestCluster.Text = largestCluster.ToString();
        valHeavyMass.Text = heavyMass.ToString("F2");
        valSpectrumInfo.Text = spectrumOn ? "on" : "off";
    }

    private void AddImportantEvent(int step, string type, string detail)
    {
        if (lvEvents is null) return;
        var item = new ListViewItem(new[] { step.ToString(), type, detail });
        lvEvents.Items.Add(item);
        if (lvEvents.Items.Count > 1000)
            lvEvents.Items.RemoveAt(0);
    }

    private void PanelOnChart_Paint(object? sender, PaintEventArgs e)
    {
        var data = _simApi.Dispatcher.ForceGetDisplayDataImmediate(timeoutMs: 20);
        if (data.DecimatedSteps.Length == 0)
        {
            e.Graphics.Clear(Color.White);
            e.Graphics.DrawString("Нет данных", new Font("Arial", 10), Brushes.Gray, 10, 10);
            return;
        }
        ChartRenderer.DrawSimpleLineChartFast(e.Graphics, panelOnChart.Width, panelOnChart.Height, data.DecimatedSteps, data.DecimatedExcited, "Excited nodes", Color.Red);
    }

    private void PanelHeavyChart_Paint(object? sender, PaintEventArgs e)
    {
        var data = _simApi.Dispatcher.ForceGetDisplayDataImmediate(timeoutMs: 20);
        if (data.DecimatedSteps.Length == 0)
        {
            e.Graphics.Clear(Color.White);
            e.Graphics.DrawString("Нет данных", new Font("Arial", 10), Brushes.Gray, 10, 10);
            return;
        }
        ChartRenderer.DrawSimpleLineChartFast(e.Graphics, panelHeavyChart.Width, panelHeavyChart.Height, data.DecimatedSteps, data.DecimatedHeavyMass, "Heavy mass", Color.DarkOrange);
    }

    private void PanelClusterChart_Paint(object? sender, PaintEventArgs e)
    {
        var data = _simApi.Dispatcher.ForceGetDisplayDataImmediate(timeoutMs: 20);
        if (data.DecimatedSteps.Length == 0)
        {
            e.Graphics.Clear(Color.White);
            e.Graphics.DrawString("Нет данных", new Font("Arial", 10), Brushes.Gray, 10, 10);
            return;
        }
        ChartRenderer.DrawSimpleLineChartFast(e.Graphics, panelClusterChart.Width, panelClusterChart.Height, data.DecimatedSteps, data.DecimatedLargestCluster, "Largest cluster size", Color.Blue);
    }

    private void PanelEnergyChart_Paint(object? sender, PaintEventArgs e)
    {
        var data = _simApi.Dispatcher.ForceGetDisplayDataImmediate(timeoutMs: 20);
        if (data.DecimatedSteps.Length == 0)
        {
            e.Graphics.Clear(Color.White);
            e.Graphics.DrawString("Нет данных", new Font("Arial", 10), Brushes.Gray, 10, 10);
            return;
        }

        if (data.DecimatedNetworkTemp.Length == 0)
        {
            ChartRenderer.DrawSimpleLineChartFast(e.Graphics, panelEnergyChart.Width, panelEnergyChart.Height, data.DecimatedSteps, data.DecimatedEnergy, "Total energy", Color.Green);
            return;
        }

        ChartRenderer.DrawDualLineChartFast(
            e.Graphics,
            panelEnergyChart.Width,
            panelEnergyChart.Height,
            data.DecimatedSteps,
            data.DecimatedEnergy,
            data.DecimatedNetworkTemp,
            "Energy vs Network Temp",
            Color.ForestGreen,
            Color.MediumSlateBlue);
    }
}
