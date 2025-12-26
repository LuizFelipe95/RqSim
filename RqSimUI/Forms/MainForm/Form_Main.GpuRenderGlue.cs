using RqSimForms.Forms.Interfaces;
using RqSimForms.ProcessesDispatcher.Contracts;
using RqSimForms.ProcessesDispatcher.Managers;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace RqSimForms;

partial class Form_Main
{

    // === Simulation API (contains all non-UI logic) ===
    private readonly Forms.Interfaces.RqSimEngineApi _simApi = new();
    private readonly LifeCycleManager _lifeCycleManager = new();
    private bool _isExternalSimulation = false;

    /// <summary>
    /// Synchronizes UI to external simulation that was already running when UI attached.
    /// Updates button states and starts render timer to display external simulation data.
    /// </summary>
    private void SyncToExternalSimulation(SimState state)
    {
        _isExternalSimulation = true;

        // ¬ажно: статус внешней симул€ции != всегда Running.
        // UI должен отобразить корректное состо€ние, иначе кнопки/логика ввод€т в заблуждение.
        switch (state.Status)
        {
            case SimulationStatus.Running:
                _isModernRunning = true;
                button_RunModernSim.Text = "Stop Modern Sim";
                AppendSysConsole($"[Dispatcher] Attached to RUNNING simulation: Iteration={state.Iteration}, Nodes={state.NodeCount}\n");
                break;

            case SimulationStatus.Paused:
                _isModernRunning = false;
                button_RunModernSim.Text = "Run Modern Sim";
                AppendSysConsole($"[Dispatcher] Attached to PAUSED simulation: Iteration={state.Iteration}, Nodes={state.NodeCount}. Press Start to resume.\n");
                break;

            default:
                _isModernRunning = false;
                button_RunModernSim.Text = "Run Modern Sim";
                AppendSysConsole($"[Dispatcher] Attached to external simulation (Status={state.Status}): Iteration={state.Iteration}, Nodes={state.NodeCount}\n");
                break;
        }

        // Tailor UI timer for external simulation display
        _uiUpdateTimer?.Stop();
        _uiUpdateTimer?.Dispose();

        _uiUpdateTimer = new System.Windows.Forms.Timer();
        int targetFps = Math.Max(1, (int)numericUpDown_MaxFPS.Value);
        _uiUpdateTimer.Interval = 1000 / targetFps;
        _uiUpdateTimer.Tick += UiUpdateTimer_Tick;
        _uiUpdateTimer.Start();
    }

    // Ensures panel has focus to receive keyboard events (DX12 input adapter relies on WinForms events)
    private void EnsureRenderPanelFocus()
    {
        // Prefer DX12 render panel.
        var panel = _dx12Panel;
        if (panel is null)
            return;

        panel.TabStop = true;
        panel.Focus();
    }

    // Call after backend restart.
    private void AfterBackendRestartUi()
    {
        EnsureRenderPanelFocus();
        UpdateMainFormStatusBar();

        Debug.WriteLine($"[RenderBackend] AfterBackendRestartUi (Active={_activeBackend})");
    }
}
