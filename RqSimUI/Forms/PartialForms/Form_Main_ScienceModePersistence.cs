using RqSimForms.Forms.Interfaces;

namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - Science Mode Persistence.
/// Handles saving/loading Science Mode state as part of settings persistence.
/// </summary>
public partial class Form_Main
{
    /// <summary>
    /// Captures Science Mode and related states to the settings config.
    /// Call this before saving settings.
    /// </summary>
    private void CaptureScienceModeState()
    {
        if (_settingsManager?.CurrentConfig is null) return;

        var config = _settingsManager.CurrentConfig;

        // Science Mode
        config.ScienceModeEnabled = _scienceModeEnabled;
        config.UseOllivierRicciCurvature = _useOllivierRicci;
        config.EnableConservationValidation = _enableConservation;
        config.UseGpuAnisotropy = _useGpuAnisotropy;

        // Auto-tuning (already captured by manager, but ensure checkbox sync)
        config.AutoTuningEnabled = checkBox_AutoTuning?.Checked ?? false;

        AppendSysConsole($"[Settings] Captured mode states: Science={config.ScienceModeEnabled}, AutoTune={config.AutoTuningEnabled}\n");
    }

    /// <summary>
    /// Restores Science Mode and related states from the settings config.
    /// Call this after loading settings.
    /// </summary>
    private void RestoreScienceModeState()
    {
        if (_settingsManager?.CurrentConfig is null) return;

        var config = _settingsManager.CurrentConfig;

        SuspendControlEvents();
        try
        {
            // Restore Science Mode checkbox
            if (checkBox_ScienceSimMode is not null)
            {
                checkBox_ScienceSimMode.Checked = config.ScienceModeEnabled;
            }
            _scienceModeEnabled = config.ScienceModeEnabled;

            // Restore Science Mode related settings
            _useOllivierRicci = config.UseOllivierRicciCurvature;
            _enableConservation = config.EnableConservationValidation;
            _useGpuAnisotropy = config.UseGpuAnisotropy;

            // Update Science Mode UI controls
            if (_checkBox_UseOllivierRicci is not null)
                _checkBox_UseOllivierRicci.Checked = _useOllivierRicci;
            if (_checkBox_EnableConservation is not null)
                _checkBox_EnableConservation.Checked = _enableConservation;
            if (_checkBox_UseGpuAnisotropy is not null)
                _checkBox_UseGpuAnisotropy.Checked = _useGpuAnisotropy;

            // Restore Auto-tuning checkbox
            if (checkBox_AutoTuning is not null)
            {
                checkBox_AutoTuning.Checked = config.AutoTuningEnabled;
            }

            // Apply Science Mode profile
            UpdateScienceModeProfile();
            UpdateScienceModeUI();

            AppendSysConsole($"[Settings] Restored mode states: Science={_scienceModeEnabled}, AutoTune={config.AutoTuningEnabled}\n");
        }
        finally
        {
            ResumeControlEvents();
        }
    }

    /// <summary>
    /// Extended save handler that includes mode states.
    /// Call this instead of just SavePhysicsSettingsOnExit().
    /// </summary>
    private void SaveAllSettingsOnExit()
    {
        if (_settingsManager is null) return;

        try
        {
            // Capture UI state
            _settingsManager.CaptureCurrentState();
            
            // Capture mode states
            CaptureScienceModeState();

            // Save to disk
            _settingsManager.SaveSettings();
            
            AppendSysConsole("[Settings] All settings (including modes) saved on exit\n");
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[Settings] Failed to save settings: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Extended load handler that includes mode states.
    /// Call this after InitializePhysicsSettingsManager().
    /// </summary>
    private void LoadAllSettingsOnStartup()
    {
        if (_settingsManager is null) return;

        try
        {
            // Settings are already loaded by InitializePhysicsSettingsManager
            // Just restore mode states
            RestoreScienceModeState();
            
            AppendSysConsole("[Settings] All settings (including modes) loaded on startup\n");
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[Settings] Failed to restore mode states: {ex.Message}\n");
        }
    }
}
