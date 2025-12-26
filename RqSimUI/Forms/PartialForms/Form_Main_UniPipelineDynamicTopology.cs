using System;
using RQSimulation.GPUCompressedSparseRow;

namespace RqSimForms;

/// <summary>
/// Extension of UniPipeline Form_Main for Dynamic Hard Rewiring support.
/// </summary>
partial class Form_Main
{
    /// <summary>
    /// Extended ApplyTopologyModeToSimApi that handles Dynamic Hard Rewiring.
    /// This method is called from ComboBox_TopologyMode_SelectedIndexChanged.
    /// </summary>
    private void ApplyTopologyModeToSimApiExtended()
    {
        if (_simApi is null) return;

        try
        {
            var csrEngine = _simApi.CsrCayleyEngine;
            if (csrEngine is null || !csrEngine.IsInitialized)
            {
                AppendSysConsole($"[UI] CSR engine not available - topology mode will be applied on next init\n");
                return;
            }

            var mode = _topologyModeSelection switch
            {
                "CSR (Static)" => GpuCayleyEvolutionEngineCsr.TopologyMode.CsrStatic,
                "StreamCompaction (Hybrid)" => GpuCayleyEvolutionEngineCsr.TopologyMode.StreamCompaction,
                "StreamCompaction (Full GPU)" => GpuCayleyEvolutionEngineCsr.TopologyMode.StreamCompactionFullGpu,
                "Dynamic Hard Rewiring" => GpuCayleyEvolutionEngineCsr.TopologyMode.DynamicHardRewiring,
                _ => GpuCayleyEvolutionEngineCsr.TopologyMode.CsrStatic
            };

            csrEngine.CurrentTopologyMode = mode;
            
            // Update visibility of dynamic topology settings panel
            UpdateDynamicTopologyPanelVisibility();
            
            // If switching to dynamic hard rewiring, apply configuration
            if (mode == GpuCayleyEvolutionEngineCsr.TopologyMode.DynamicHardRewiring)
            {
                ApplyDynamicTopologySettingsInternal();
            }
            
            AppendSysConsole($"[UI] Applied topology mode to CSR engine: {mode}\n");
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[UI] Failed to apply topology mode: {ex.Message}\n");
        }
    }
}
