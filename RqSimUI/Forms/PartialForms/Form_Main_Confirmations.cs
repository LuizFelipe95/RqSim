using RqSimForms.Forms.Interfaces;

namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - Mode and Parameter Change Confirmations.
/// Provides confirmation dialogs when changing modes/parameters during running simulation.
/// </summary>
public partial class Form_Main
{
    /// <summary>
    /// Shows confirmation dialog for applying physics parameters during running simulation.
    /// </summary>
    /// <param name="parameterName">Name of the parameter being changed.</param>
    /// <param name="oldValue">Current value.</param>
    /// <param name="newValue">Proposed new value.</param>
    /// <returns>True if user confirms the change.</returns>
    private bool ConfirmPhysicsParameterChange(string parameterName, object? oldValue, object? newValue)
    {
        if (!_isModernRunning)
        {
            return true; // No confirmation needed when simulation is not running
        }

        var result = MessageBox.Show(
            $"Change {parameterName}?\n\n" +
            $"Current: {oldValue}\n" +
            $"New: {newValue}\n\n" +
            "This change will be applied immediately to the running simulation.",
            "Confirm Parameter Change",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        return result == DialogResult.Yes;
    }

    /// <summary>
    /// Shows confirmation dialog for applying a preset during running simulation.
    /// </summary>
    /// <param name="presetName">Name of the preset being applied.</param>
    /// <param name="description">Description of what the preset changes.</param>
    /// <returns>True if user confirms the change.</returns>
    private bool ConfirmPresetChange(string presetName, string description)
    {
        if (!_isModernRunning)
        {
            return true;
        }

        var result = MessageBox.Show(
            $"Apply '{presetName}' preset?\n\n{description}\n\n" +
            "Multiple parameters will be changed immediately.",
            "Confirm Preset Application",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        return result == DialogResult.Yes;
    }

    /// <summary>
    /// Shows confirmation dialog for a mode toggle (Science Mode, Auto-tuning, etc.)
    /// </summary>
    /// <param name="modeName">Name of the mode.</param>
    /// <param name="enabling">True if enabling, false if disabling.</param>
    /// <param name="warning">Warning message about implications.</param>
    /// <returns>True if user confirms the change.</returns>
    private bool ConfirmModeChange(string modeName, bool enabling, string warning)
    {
        if (!_isModernRunning)
        {
            return true;
        }

        string action = enabling ? "enable" : "disable";
        var result = MessageBox.Show(
            $"Do you want to {action} {modeName}?\n\n{warning}\n\n" +
            "This change will be applied immediately to the running simulation.",
            $"Confirm {modeName} Change",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        return result == DialogResult.Yes;
    }

    /// <summary>
    /// Shows warning for non-hot-swappable parameters.
    /// </summary>
    /// <param name="parameterNames">List of parameters that cannot be changed at runtime.</param>
    private void ShowNonHotSwappableWarning(IReadOnlyList<string> parameterNames)
    {
        if (parameterNames.Count == 0) return;

        var message = "The following parameters cannot be changed while simulation is running:\n\n";
        foreach (var name in parameterNames)
        {
            message += $"  • {name}\n";
        }
        message += "\nStop the simulation to change these parameters.";

        MessageBox.Show(
            message,
            "Parameters Require Restart",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>
    /// Enhanced Apply Physics button handler with confirmation.
    /// </summary>
    private void ApplyPhysicsWithConfirmation()
    {
        if (_settingsManager is null) return;

        // Count pending changes
        _settingsManager.CaptureCurrentState();
        
        if (_isModernRunning)
        {
            var result = MessageBox.Show(
                "Apply current physics settings to the running simulation?\n\n" +
                "Hot-swappable parameters will be applied immediately.\n" +
                "Non-hot-swappable parameters will require a restart.",
                "Confirm Apply Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }
        }

        // Apply settings
        var nonHotSwappable = _settingsManager.ApplyToSimulation(_isModernRunning);

        // Show warning for non-hot-swappable parameters
        if (nonHotSwappable.Count > 0 && _isModernRunning)
        {
            ShowNonHotSwappableWarning(nonHotSwappable);
        }

        AppendSimConsole("[Settings] Physics settings applied\n");
    }
}
