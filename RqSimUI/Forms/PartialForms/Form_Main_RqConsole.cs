using RqSimForms.Forms.Interfaces;
using RqSimForms.ProcessesDispatcher;
using RqSimForms.ProcessesDispatcher.Contracts;
using RqSimForms.ProcessesDispatcher.IPC;
using RqSimForms.ProcessesDispatcher.Managers;
using RQSimulation;
using RQSimulation.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    // === Console Session Binding ===
    private MemoryMappedFile? _consoleSharedMemory;
    private MemoryMappedViewAccessor? _consoleSharedMemoryAccessor;
    private System.Windows.Forms.Timer? _consolePollingTimer;
    private bool _isConsoleBound;
    private Button? _boundConsoleButton;
    private DateTime _lastSharedMemoryAttachAttemptUtc = DateTime.MinValue;
    private bool _reportedSharedMemoryMissing;

    // === RQ Flag to Pipeline Module Mapping ===
    private static readonly Dictionary<string, string[]> FlagToModuleMapping = new()
    {
        ["UseHamiltonianGravity"] = ["Geometry Momenta", "Network Gravity"],
        ["EnableVacuumEnergyReservoir"] = ["Vacuum Fluctuations"],
        ["EnableNaturalDimensionEmergence"] = ["Spectral Geometry"],
        ["EnableTopologicalParity"] = ["Spinor Field"],
        ["EnableLapseSynchronizedGeometry"] = ["Relational Time", "Network Gravity"],
        ["EnableTopologyEnergyCompensation"] = ["Unified Physics Step"],
        ["EnablePlaquetteYangMills"] = ["Yang-Mills Gauge"],
        ["EnableSymplecticGaugeEvolution"] = ["Yang-Mills Gauge"],
        ["EnableAdaptiveTopologyDecoherence"] = ["Quantum Graphity"],
        ["EnableWilsonLoopProtection"] = ["Yang-Mills Gauge"],
        ["EnableSpectralActionMode"] = ["Spectral Geometry"],
        ["EnableWheelerDeWittStrictMode"] = ["Unified Physics Step"],
        ["PreferOllivierRicciCurvature"] = ["Network Gravity"]
    };


    public void ToTestConsole(string test)
    {
        this.textBox_HostSessionErrors.Text += $"[Test] {test}\n";
    }


    private async void button_TerminateSimSession_Click(object sender, EventArgs e)
    {
        bool anyAction = false;

        // 1. Stop Local Simulation
        if (_isModernRunning)
        {
            ToTestConsole("[Stop] Stopping local simulation...\n");
            _modernCts?.Cancel();
            _isModernRunning = false;
            button_RunModernSim.Text = "Run Modern Sim";
            anyAction = true;
        }

        // 2. Stop Remote Simulation
        if (_isConsoleBound || _lifeCycleManager.IsExternalProcessAttached)
        {
            ToTestConsole("[Stop] Stopping remote simulation...\n");

            try
            {
                // Send Stop command to console
                await _lifeCycleManager.Ipc.SendStopAsync();

                // Update UI to reflect stopped state, but keep connection alive
                if (_isConsoleBound)
                {
                    button_RunModernSim.Text = "Start Console Sim";
                }
            }
            catch (Exception ex)
            {
                ToTestConsole($"[Stop] Warning: Failed to send stop command: {ex.Message}\n");
            }

            // Do NOT unbind session - keep connection alive for next run
            // UnbindConsoleSession(); 
            anyAction = true;
        }

        if (anyAction)
        {
            ToTestConsole("[Stop] Session terminated.\n");
        }
        else
        {
            ToTestConsole("[Stop] No active session to terminate.\n");
        }
    }



    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_lifeCycleManager != null)
        {
            _lifeCycleManager.Logger = (msg) => AppendHostSessionError(msg);
        }
    }

    private void AppendHostSessionError(string message)
    {
        if (textBox_HostSessionErrors.InvokeRequired)
        {
            textBox_HostSessionErrors.BeginInvoke(new Action(() => AppendHostSessionError(message)));
            return;
        }
        textBox_HostSessionErrors.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    /// <summary>
    /// Applies current pipeline configuration to the running simulation.
    /// </summary>
    private void button_ApplyPipelineConfSet_Click(object sender, EventArgs e)
    {
        if (_settingsManager is null)
        {
            ToTestConsole("[Pipeline] Settings manager not initialized\n");
            return;
        }

        try
        {
            // 1. Capture current UI state into config
            _settingsManager.CaptureCurrentState();

            // 2. Apply physics parameters (non-module settings)
            var nonHotSwappable = _settingsManager.ApplyToSimulation(_isModernRunning);

            // 3. Apply RQ flags to pipeline modules
            ApplyRQFlagsToPipeline();

            // 4. Sync physics checkboxes to pipeline and update UniPipeline grid
            SyncPhysicsCheckboxesToPipelineAndGrid();

            // 5. Report status
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("  🔧 Pipeline Configuration Applied");
            sb.AppendLine("═══════════════════════════════════════════");

            // Show pipeline modules status
            if (_simApi.Pipeline is not null)
            {
                sb.AppendLine("\n📦 Pipeline Modules:");
                foreach (var module in _simApi.Pipeline.Modules)
                {
                    var status = module.IsEnabled ? "✓ ON " : "○ OFF";
                    sb.AppendLine($"  {status} {module.Name}");
                }
            }

            // Show RQ flags status  
            sb.AppendLine("\n⚛ RQ-Hypothesis Flags → LiveConfig:");
            var flags = _simApi.RQFlags;
            AppendFlagStatus(sb, "UseHamiltonianGravity", flags.UseHamiltonianGravity);
            AppendFlagStatus(sb, "EnableVacuumEnergyReservoir", flags.EnableVacuumEnergyReservoir);
            AppendFlagStatus(sb, "EnableNaturalDimensionEmergence", flags.EnableNaturalDimensionEmergence);
            AppendFlagStatus(sb, "EnableTopologicalParity", flags.EnableTopologicalParity);
            AppendFlagStatus(sb, "EnableLapseSynchronizedGeometry", flags.EnableLapseSynchronizedGeometry);
            AppendFlagStatus(sb, "EnableTopologyEnergyCompensation", flags.EnableTopologyEnergyCompensation);
            AppendFlagStatus(sb, "EnablePlaquetteYangMills", flags.EnablePlaquetteYangMills);
            AppendFlagStatus(sb, "EnableSymplecticGaugeEvolution", flags.EnableSymplecticGaugeEvolution);
            AppendFlagStatus(sb, "EnableAdaptiveTopologyDecoherence", flags.EnableAdaptiveTopologyDecoherence);
            AppendFlagStatus(sb, "EnableWilsonLoopProtection", flags.EnableWilsonLoopProtection);
            AppendFlagStatus(sb, "EnableSpectralActionMode", flags.EnableSpectralActionMode);
            AppendFlagStatus(sb, "EnableWheelerDeWittStrictMode", flags.EnableWheelerDeWittStrictMode);
            AppendFlagStatus(sb, "PreferOllivierRicciCurvature", flags.PreferOllivierRicciCurvature);

            // Non-hot-swappable warnings
            if (nonHotSwappable.Count > 0 && _isModernRunning)
            {
                sb.AppendLine("\n⏳ Require restart (not hot-swappable):");
                foreach (var param in nonHotSwappable)
                {
                    sb.AppendLine($"  ⏳ {param}");
                }
            }

            sb.AppendLine("\n═══════════════════════════════════════════");
            AppendSimConsole(sb.ToString());

            // Send to console if bound
            if (_isConsoleBound)
            {
                _ = SendSettingsToConsoleAsync();
            }

        }
        catch (Exception ex)
        {
            ToTestConsole($"[Pipeline] Error: {ex.Message}\n");
            MessageBox.Show($"Failed to apply: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Applies RQ-Hypothesis flags to corresponding pipeline modules.
    /// </summary>
    private void ApplyRQFlagsToPipeline()
    {
        var pipeline = _simApi.Pipeline;
        if (pipeline is null)
        {
            ToTestConsole("[Pipeline] No pipeline available\n");
            return;
        }

        var flags = _simApi.RQFlags;
        var changes = new List<string>();

        // Map each flag to its module(s) and sync enabled state
        void SyncFlagToModules(string flagName, bool flagValue)
        {
            if (!FlagToModuleMapping.TryGetValue(flagName, out var moduleNames))
                return;

            foreach (var moduleName in moduleNames)
            {
                var module = pipeline.GetModule(moduleName);
                if (module is null) continue;

                // Only update if state differs
                if (module.IsEnabled != flagValue)
                {
                    module.IsEnabled = flagValue;
                    var state = flagValue ? "ENABLED" : "DISABLED";
                    changes.Add($"{moduleName} → {state}");
                }
            }
        }

        // Sync all flags
        SyncFlagToModules("UseHamiltonianGravity", flags.UseHamiltonianGravity);
        SyncFlagToModules("EnableVacuumEnergyReservoir", flags.EnableVacuumEnergyReservoir);
        SyncFlagToModules("EnableNaturalDimensionEmergence", flags.EnableNaturalDimensionEmergence);
        SyncFlagToModules("EnableTopologicalParity", flags.EnableTopologicalParity);
        SyncFlagToModules("EnableLapseSynchronizedGeometry", flags.EnableLapseSynchronizedGeometry);
        SyncFlagToModules("EnableTopologyEnergyCompensation", flags.EnableTopologyEnergyCompensation);
        SyncFlagToModules("EnablePlaquetteYangMills", flags.EnablePlaquetteYangMills);
        SyncFlagToModules("EnableSymplecticGaugeEvolution", flags.EnableSymplecticGaugeEvolution);
        SyncFlagToModules("EnableAdaptiveTopologyDecoherence", flags.EnableAdaptiveTopologyDecoherence);
        SyncFlagToModules("EnableWilsonLoopProtection", flags.EnableWilsonLoopProtection);
        SyncFlagToModules("EnableSpectralActionMode", flags.EnableSpectralActionMode);
        SyncFlagToModules("EnableWheelerDeWittStrictMode", flags.EnableWheelerDeWittStrictMode);
        SyncFlagToModules("PreferOllivierRicciCurvature", flags.PreferOllivierRicciCurvature);

        if (changes.Count > 0)
        {
            AppendSimConsole($"[Pipeline] Module state changes:\n");
            foreach (var change in changes)
            {
                AppendSimConsole($"  • {change}\n");
            }
        }
    }

    private static void AppendFlagStatus(StringBuilder sb, string name, bool value)
    {
        var status = value ? "✓" : "○";
        sb.AppendLine($"  {status} {name}");
    }

    /// <summary>
    /// Binds to a running RqSimConsole session via shared memory and named pipes.
    /// Uses the existing LifeCycleManager's IPC controller.
    /// </summary>
    private async void button_BindConsoleSession_Click(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            _boundConsoleButton = btn;
        }

        if (_isConsoleBound)
        {
            UnbindConsoleSession();
            return;
        }

        try
        {
            ToTestConsole("[Console] Checking for RqSimConsole...\n");

            // Check if LifeCycleManager already has a connected process
            if (_lifeCycleManager.IsExternalProcessAttached)
            {
                ToTestConsole("[Console] ✓ Found attached console process via LifeCycleManager\n");
            }
            else
            {
                var consoleProcesses = Process.GetProcessesByName("RqSimConsole");
                if (consoleProcesses.Length == 0)
                {
                    ToTestConsole("[Console] ⚠️ RqSimConsole process not found.\n");

                    var result = MessageBox.Show(
                        "RqSimConsole is not running.\n\n" +
                        "Would you like to start it now in server mode?",
                        "Console Not Found",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        StartConsoleProcess();
                        ToTestConsole("[Console] Waiting for console to initialize...\n");
                        await Task.Delay(3000);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    ToTestConsole($"[Console] Found {consoleProcesses.Length} RqSimConsole process(es)\n");
                }
            }

            ToTestConsole("[Console] Attempting handshake...\n");
            bool handshakeOk = await _lifeCycleManager.Ipc.SendHandshakeWithRetryAsync(maxRetries: 5);

            if (!handshakeOk)
            {
                ToTestConsole("[Console] ⚠️ Handshake failed after 5 attempts.\n");
                ToTestConsole("[Console] Check console output for errors.\n");

                MessageBox.Show(
                    "Could not connect to RqSimConsole pipe.\n\n" +
                    "Possible reasons:\n" +
                    "• Console is not running in server mode (--server-mode)\n" +
                    "• Pipe server not yet initialized\n" +
                    "• Firewall or security software blocking pipes\n\n" +
                    "Check the console window for error messages.",
                    "Connection Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ToTestConsole("[Console] ✓ Handshake successful!\n");

            // Push current UI settings right after handshake so server uses them for initial graph init.
            await SendSettingsToConsoleAsync();

            // Start polling timer early; it will also auto-attach shared memory when it becomes available.
            _consolePollingTimer ??= new System.Windows.Forms.Timer { Interval = 200 };
            _consolePollingTimer.Tick -= ConsolePollingTimer_Tick;
            _consolePollingTimer.Tick += ConsolePollingTimer_Tick;
            _consolePollingTimer.Start();

            // Try to attach shared memory now (best effort)
            TryAttachConsoleSharedMemory(force: true);

            _isConsoleBound = true;
            _isExternalSimulation = true; // Sync external flag
            UpdateConsoleBindButton(true);

            // Update Run button to reflect console mode
            button_RunModernSim.Text = "Start Console Sim";

            ToTestConsole("[Console] ✓ Successfully bound to RqSimConsole!\n");

            MessageBox.Show(
                "Connected to RqSimConsole!\n\n" +
                "The UI will reattach to the running simulation automatically.",
                "Connected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ToTestConsole($"[Console] ✗ Error: {ex.Message}\n");
            Trace.WriteLine($"[Console] Exception: {ex}");
            MessageBox.Show($"Failed to connect: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryAttachConsoleSharedMemory(bool force)
    {
        if (!_isConsoleBound && !force)
            return false;

        // Throttle attach attempts to avoid hammering OpenExisting on every timer tick.
        var nowUtc = DateTime.UtcNow;
        if (!force && nowUtc - _lastSharedMemoryAttachAttemptUtc < TimeSpan.FromSeconds(1))
            return _consoleSharedMemoryAccessor is not null;

        _lastSharedMemoryAttachAttemptUtc = nowUtc;

        if (_consoleSharedMemoryAccessor is not null)
            return true;

        try
        {
            _consoleSharedMemory = MemoryMappedFile.OpenExisting(
                DispatcherConfig.SharedMemoryMapName,
                MemoryMappedFileRights.Read);

            _consoleSharedMemoryAccessor = _consoleSharedMemory.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            _reportedSharedMemoryMissing = false;

            ToTestConsole("[Console] ✓ Shared memory connected\n");
            return true;
        }
        catch (FileNotFoundException)
        {
            if (!_reportedSharedMemoryMissing)
            {
                _reportedSharedMemoryMissing = true;
                ToTestConsole("[Console] ⚠️ Shared memory not found yet (will retry)\n");
            }

            return false;
        }
        catch (IOException ex)
        {
            ToTestConsole($"[Console] ⚠️ Shared memory attach failed: {ex.Message}\n");
            return false;
        }
    }

    private void ReleaseConsoleSharedMemory()
    {
        _consoleSharedMemoryAccessor?.Dispose();
        _consoleSharedMemoryAccessor = null;

        _consoleSharedMemory?.Dispose();
        _consoleSharedMemory = null;
    }

    private void ConsolePollingTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isConsoleBound)
            return;

        if (_consoleSharedMemoryAccessor is null)
        {
            TryAttachConsoleSharedMemory(force: false);
            return;
        }

        try
        {
            _consoleSharedMemoryAccessor.Read(0, out SharedHeader header);

            if (header.LastUpdateTimestampUtcTicks <= 0)
                return;

            var age = DateTime.UtcNow - new DateTime(header.LastUpdateTimestampUtcTicks, DateTimeKind.Utc);
            if (age > DispatcherConfig.StaleDataThreshold)
            {
                // Data is stale. The server may have restarted or the mapping became invalid.
                ReleaseConsoleSharedMemory();

                // If pipe is also gone, treat as console shutdown and fully unbind.
                bool pipeAlive = false;
                try
                {
                    pipeAlive = IpcController.IsPipeServerAvailableAsync().GetAwaiter().GetResult();
                }
                catch
                {
                    pipeAlive = false;
                }

                if (!pipeAlive)
                {
                    ToTestConsole("[Console] ⚠️ Console session lost (process/pipe not available). Switching to local mode.\n");
                    UnbindConsoleSession();
                    _isExternalSimulation = false;
                    button_RunModernSim.Text = "Run Modern Sim";
                    return;
                }

                TryAttachConsoleSharedMemory(force: false);
                return;
            }

            // Update UI directly from console data
            UpdateDashboardFromConsole(header);
        }
        catch (ObjectDisposedException)
        {
            ReleaseConsoleSharedMemory();
        }
        catch (IOException)
        {
            ReleaseConsoleSharedMemory();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Console] Polling error: {ex.Message}");
        }
    }

    private void UpdateDashboardFromConsole(SharedHeader header)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateDashboardFromConsole(header)));
            return;
        }

        var status = (SimulationStatus)header.StateCode;

        // Use extended metrics from SharedHeader
        UpdateDashboard(
            (int)header.Iteration,
            0, // totalSteps not tracked in server mode
            header.ExcitedCount,
            header.HeavyMass,
            header.LargestCluster,
            header.StrongEdgeCount,
            status.ToString(),
            header.QNorm,
            header.Entanglement,
            header.Correlation);

        valDashNodes.Text = header.NodeCount.ToString();
        valDashStatus.Text = status.ToString();
        valDashSpectralDim.Text = header.LatestSpectralDimension.ToString("F3");
        valDashNetworkTemp.Text = header.NetworkTemperature.ToString("F3");
        valDashEffectiveG.Text = header.EffectiveG.ToString("F4");
        valDashStrongEdges.Text = header.StrongEdgeCount.ToString();

        // Show external simulation failure clearly
        switch (status)
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
        if (header.LatestSpectralDimension < 1.5)
            valDashSpectralDim.ForeColor = Color.Red;
        else if (header.LatestSpectralDimension > 4.0)
            valDashSpectralDim.ForeColor = Color.DarkOrange;
        else
            valDashSpectralDim.ForeColor = Color.Green;
    }

    private void UnbindConsoleSession()
    {
        _consolePollingTimer?.Stop();
        _consolePollingTimer?.Dispose();
        _consolePollingTimer = null;

        ReleaseConsoleSharedMemory();

        _isConsoleBound = false;
        _isExternalSimulation = false; // Sync external flag
        _reportedSharedMemoryMissing = false;
        _lastSharedMemoryAttachAttemptUtc = DateTime.MinValue;

        UpdateConsoleBindButton(false);
        ToTestConsole("[Console] Disconnected from RqSimConsole\n");

        // Reset UI state back to local mode defaults.
        button_RunModernSim.Text = "Run Modern Sim";
    }

    private void StartConsoleProcess()
    {
        try
        {
            var startInfo = DispatcherConfig.BuildStartInfo();
            // BuildStartInfo already uses "--server-mode", don't override

            var process = Process.Start(startInfo);
            if (process is not null)
            {
                ToTestConsole($"[Console] Started RqSimConsole (PID: {process.Id})\n");
                ToTestConsole($"[Console] Arguments: {startInfo.Arguments}\n");
            }
        }
        catch (Exception ex)
        {
            ToTestConsole($"[Console] Failed to start console: {ex.Message}\n");
            MessageBox.Show(
                $"Failed to start RqSimConsole:\n{ex.Message}\n\n" +
                $"Path: {DispatcherConfig.SimulationExecutablePath}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Handles simulation start/stop/pause when bound to a console process.
    /// Called from button_RunSimulation_Click when _isConsoleBound is true.
    /// </summary>
    internal async Task HandleConsoleBoundSimulationAsync()
    {
        if (!_isConsoleBound)
        {
            ToTestConsole("[Console] Not bound to console - cannot forward command\n");
            return;
        }

        // Check current simulation state from shared memory
        SimulationStatus currentStatus = SimulationStatus.Unknown;
        if (_consoleSharedMemoryAccessor is not null)
        {
            try
            {
                _consoleSharedMemoryAccessor.Read(0, out SharedHeader header);
                currentStatus = header.Status;
                ToTestConsole($"[Console] Read shared memory: Iteration={header.Iteration}, Nodes={header.NodeCount}, Status={currentStatus}\n");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Console] Error reading status: {ex.Message}");
                ToTestConsole($"[Console] ⚠️ Could not read shared memory: {ex.Message}\n");
            }
        }
        else
        {
            ToTestConsole("[Console] ⚠️ Shared memory not connected - sending command anyway\n");
        }

        ToTestConsole($"[Console] Current remote status: {currentStatus}\n");

        bool success;
        switch (currentStatus)
        {
            case SimulationStatus.Running:
                // Simulation is running - send pause
                ToTestConsole("[Console] Sending PAUSE command to console...\n");
                success = await _lifeCycleManager.Ipc.SendPauseAsync();
                if (success)
                {
                    ToTestConsole("[Console] ✓ PAUSE command sent successfully\n");
                    button_RunModernSim.Text = "Resume Console Sim";
                }
                else
                {
                    ToTestConsole("[Console] ✗ Failed to send PAUSE command\n");
                }
                break;

            case SimulationStatus.Paused:
                // Simulation is paused - resume it
                ToTestConsole("[Console] Sending START command to resume...\n");
                success = await _lifeCycleManager.Ipc.SendStartAsync();
                if (success)
                {
                    ToTestConsole("[Console] ✓ START command sent - resuming simulation\n");
                    button_RunModernSim.Text = "Pause Console Sim";
                }
                else
                {
                    ToTestConsole("[Console] ✗ Failed to send START command\n");
                }
                break;

            case SimulationStatus.Stopped:
            case SimulationStatus.Unknown:
            default:
                // Simulation not running - start it
                ToTestConsole("[Console] Sending START command to console...\n");

                // First, send current settings
                await SendSettingsToConsoleAsync();

                success = await _lifeCycleManager.Ipc.SendStartAsync();
                if (success)
                {
                    ToTestConsole("[Console] ✓ START command sent successfully\n");
                    button_RunModernSim.Text = "Pause Console Sim";
                }
                else
                {
                    ToTestConsole("[Console] ✗ Failed to send START command\n");
                    MessageBox.Show(
                        "Failed to send START command to console.\n\n" +
                        "Make sure RqSimConsole is running in server mode.",
                        "Command Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                break;
        }
    }

    private void UpdateConsoleBindButton(bool isBound)
    {
        if (_boundConsoleButton is not null)
        {
            _boundConsoleButton.Text = isBound ? "🔗 Disconnect" : "🔌 Bind Console";
            _boundConsoleButton.BackColor = isBound
                ? Color.FromArgb(100, 180, 100)
                : SystemColors.Control;
        }
    }

    private async Task SendSettingsToConsoleAsync()
    {
        try
        {
            ServerModeSettingsDto settings = null!;
            Invoke(new Action(() =>
            {
                settings = new ServerModeSettingsDto
                {
                    NodeCount = (int)numNodeCount.Value,
                    TargetDegree = (int)numTargetDegree.Value,
                    Seed = 42,
                    Temperature = (double)numTemperature.Value
                };
            }));

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = false });

            bool sent = await _lifeCycleManager.Ipc.SendUpdateSettingsAsync(json);
            if (sent)
            {
                AppendSimConsole($"[Console] Settings synchronized (Nodes={settings.NodeCount})\n");
            }
            else
            {
                AppendSimConsole("[Console] ⚠️ Failed to send settings\n");
            }
        }
        catch (Exception ex)
        {
            ToTestConsole($"[Console] Error: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Helper to read SimState directly from the bound console accessor.
    /// Used by Dashboard to ensure UI updates even if LifeCycleManager's reader fails.
    /// </summary>
    public SimState? GetConsoleStateFromAccessor()
    {
        if (!_isConsoleBound || _consoleSharedMemoryAccessor is null)
            return null;

        try
        {
            _consoleSharedMemoryAccessor.Read(0, out SharedHeader header);

            // Basic validation
            if (header.NodeCount < 0) return null;

            var status = (SimulationStatus)header.StateCode;
            var timestamp = new DateTimeOffset(header.LastUpdateTimestampUtcTicks, TimeSpan.Zero);

            return new SimState(
                header.Iteration,
                header.NodeCount,
                header.EdgeCount,
                header.SystemEnergy,
                status,
                timestamp,
                // Extended metrics
                header.ExcitedCount,
                header.HeavyMass,
                header.LargestCluster,
                header.StrongEdgeCount,
                header.QNorm,
                header.Entanglement,
                header.Correlation,
                header.LatestSpectralDimension,
                header.NetworkTemperature,
                header.EffectiveG);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Helper to read RenderNodes directly from the bound console accessor.
    /// Used by 3D Visualizer.
    /// </summary>
    public RenderNode[]? GetConsoleRenderNodesFromAccessor()
    {
        if (!_isConsoleBound || _consoleSharedMemoryAccessor is null)
            return null;

        try
        {
            _consoleSharedMemoryAccessor.Read(0, out SharedHeader header);
            int count = header.NodeCount;
            if (count <= 0) return null;

            RenderNode[] nodes = new RenderNode[count];
            _consoleSharedMemoryAccessor.ReadArray(SharedMemoryLayout.HeaderSize, nodes, 0, count);
            return nodes;
        }
        catch
        {
            return null;
        }
    }
}

































































































































































































































































