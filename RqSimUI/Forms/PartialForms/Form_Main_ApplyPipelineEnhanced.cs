using System.Text;
using RqSimEngineApi.Contracts;

namespace RqSimForms;

/// <summary>
/// Partial class for enhanced Apply Pipeline button functionality.
/// Part of Phase 4 of uni-pipeline implementation.
/// </summary>
partial class Form_Main
{
    /// <summary>
    /// Enhanced version that also applies dynamic physics parameters.
    /// This method is called by button_ApplyPipelineConfSet_Click after the existing logic.
    /// </summary>
    private void ApplyDynamicPhysicsParametersToReport(StringBuilder sb)
    {
        try
        {
            // Apply dynamic physics parameters to pipeline
            ApplyPhysicsParametersToPipeline();
            
            // Add to report
            sb.AppendLine("\n? Dynamic Physics Parameters (Phase 4):");
            if (_currentPhysicsConfig is not null)
            {
                sb.AppendLine($"  G (coupling): {_currentPhysicsConfig.GravitationalCoupling:F4}");
                sb.AppendLine($"  T (temp): {_currentPhysicsConfig.Temperature:F2}");
                sb.AppendLine($"  Vacuum energy: {_currentPhysicsConfig.VacuumEnergyScale:E2}");
                sb.AppendLine($"  Decoherence: {_currentPhysicsConfig.DecoherenceRate:F4}");
            }
            
            // Get lazy walk alpha from slider
            double lazyAlpha = _trkLazyWalkAlpha?.Value / 100.0 ?? 0.1;
            sb.AppendLine($"  Lazy Walk ?: {lazyAlpha:F2}");
            sb.AppendLine($"  Edge threshold: {_edgeThresholdValue:F2}");
            
            // Science mode status
            bool scienceMode = checkBox_ScienceSimMode?.Checked ?? false;
            sb.AppendLine($"  Science mode: {(scienceMode ? "ENABLED" : "disabled")}");
            
            if (scienceMode)
            {
                sb.AppendLine("  ? Edge threshold locked (Science mode)");
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"\n? Error applying physics parameters: {ex.Message}");
        }
    }

    /// <summary>
    /// Synchronizes edge threshold value to embedded CSR visualization.
    /// </summary>
    private void SyncEdgeThresholdToCsrVisualization()
    {
        _csrEdgeWeightThreshold = _edgeThresholdValue;
    }

    /// <summary>
    /// Gets physics parameters from current UI state for pipeline update.
    /// </summary>
    private SimulationParameters GetCurrentPhysicsParametersFromUI()
    {
        if (_currentPhysicsConfig is null)
        {
            return SimulationParameters.Default;
        }
        
        var gpuParams = _currentPhysicsConfig.ToGpuParameters();
        
        // Update lazy walk alpha from slider
        if (_trkLazyWalkAlpha is not null)
        {
            gpuParams = gpuParams.With(lazyWalkAlpha: _trkLazyWalkAlpha.Value / 100.0);
        }
        
        return gpuParams;
    }
}
