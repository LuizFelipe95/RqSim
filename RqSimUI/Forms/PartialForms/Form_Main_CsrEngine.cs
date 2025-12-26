using RqSimForms.Forms.Interfaces;

namespace RqSimForms;

/// <summary>
/// Partial class for CSR Engine UI integration.
/// Contains handler for GPU compute engine type selection.
/// </summary>
public partial class Form_Main
{
    /// <summary>
    /// Handler for GPU compute engine type selection.
    /// Maps UI selection to GpuEngineType enum and updates RqSimEngineApi.
    /// </summary>
    private void comboBox_GPUComputeEngine_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selection = comboBox_GPUComputeEngine.SelectedItem?.ToString() ?? "Auto";

        var engineType = selection switch
        {
            "Auto" or "Auto (Recommended)" => GpuEngineType.Auto,
            "Original (Dense GPU)" => GpuEngineType.Original,
            "CSR (Sparse GPU)" => GpuEngineType.Csr,
            "CPU Only" => GpuEngineType.CpuOnly,
            _ => GpuEngineType.Auto
        };

        _simApi.SetGpuEngineType(engineType);
        AppendSysConsole($"[GPU] Engine type: {selection} (Active: {_simApi.ActiveEngineType})\n");

    }

    /// <summary>
    /// Initialize GPU engine type combobox with default selection.
    /// Called from InitializeGpuControls.
    /// </summary>
    private void InitializeGpuEngineTypeComboBox()
    {
        if (comboBox_GPUComputeEngine.Items.Count > 0 && comboBox_GPUComputeEngine.SelectedIndex < 0)
        {
            comboBox_GPUComputeEngine.SelectedIndex = 0; // Auto
        }
    }
}
