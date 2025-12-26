using System;
using System.Diagnostics;
using System.Windows.Forms;
using RqSimUI.Rendering.Plugins;
using RqSimRenderingEngine.Abstractions;

namespace RqSimForms;

partial class Form_Main
{
    private void HookBackendStatusUpdates()
    {
        // Keep small: called from constructor if needed; safe to call multiple times.
        UpdateMainFormStatusBar();
    }

    private void LogActiveRendererState(string source)
    {
        string api = _activeBackend switch
        {
            RenderBackendKind.Dx12 => "DirectX 12",
            _ => "-"
        };

        string msg = $"[RenderBackend] ({source}) Selected={_selectedBackend}, Active={_activeBackend}, API={api}, DX12Available={SafeIsDx12Available()}";
        Debug.WriteLine(msg);
        _consoleBuffer?.Append(msg + "\n");

        UpdateMainFormStatusBar();
    }

    private static bool SafeIsDx12Available()
    {
        try
        {
            return RenderHostFactory.IsDx12Available();
        }
        catch
        {
            return false;
        }
    }

    // Optional helper: can be called from a menu/button to force DX12 and restart.
    private void ForceDx12AndRestart()
    {
        _selectedBackend = RenderBackendKind.Dx12;
        RenderBackendPreferenceStore.Save(_selectedBackend);

        LogActiveRendererState("ForceDx12:before");

        RestartRenderHost();

        LogActiveRendererState("ForceDx12:after");
    }
}
