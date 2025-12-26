using RQSimulation.Core.Plugins;
using RqSimEngineApi.Contracts;

namespace RqSimForms.Forms.Interfaces;

/// <summary>
/// RqSimEngineApi extension for Dynamic Physics Parameters.
/// Provides methods to update physics parameters at runtime without restarting simulation.
/// 
/// Phase 4 of uni-pipeline implementation.
/// </summary>
public partial class RqSimEngineApi
{
    // === Dynamic Physics State ===
    private SimulationParameters _currentPhysicsParams = SimulationParameters.Default;
    private readonly object _physicsParamsLock = new();

    /// <summary>
    /// Gets the current physics parameters.
    /// Thread-safe.
    /// </summary>
    public SimulationParameters CurrentPhysicsParameters
    {
        get
        {
            lock (_physicsParamsLock)
            {
                return _currentPhysicsParams;
            }
        }
    }

    /// <summary>
    /// Updates physics parameters and applies them to the pipeline.
    /// Thread-safe: can be called from UI thread.
    /// </summary>
    /// <param name="parameters">New physics parameters from UI</param>
    public void UpdatePhysicsParameters(in SimulationParameters parameters)
    {
        lock (_physicsParamsLock)
        {
            _currentPhysicsParams = parameters;
        }

        // Convert to DTO for pipeline
        var dto = SimulationParametersConverter.ToDto(in parameters);
        
        // Convert DTO to DynamicPhysicsParams
        var dynamicParams = dto.ToDynamicPhysicsParams();

        // Apply to pipeline
        Pipeline.UpdateParameters(in dynamicParams);

        // Also update LiveConfig for backward compatibility
        SyncLiveConfigFromParameters(parameters);

        OnConsoleLog?.Invoke($"[Physics] Parameters updated: G={parameters.GravitationalCoupling:F4}, T={parameters.Temperature:F2}, ?={parameters.LazyWalkAlpha:F3}\n");
    }

    /// <summary>
    /// Applies a physics preset to the simulation.
    /// </summary>
    /// <param name="presetName">"Default", "FastPreview", or "Scientific"</param>
    public void ApplyPhysicsPreset(string presetName)
    {
        var preset = presetName.ToLowerInvariant() switch
        {
            "fastpreview" or "fast" => SimulationParameters.FastPreview,
            "scientific" or "science" => SimulationParameters.Scientific,
            _ => SimulationParameters.Default
        };

        UpdatePhysicsParameters(in preset);
        
        // Also apply to pipeline's preset system
        Pipeline.ApplyPreset(presetName);

        OnConsoleLog?.Invoke($"[Physics] Applied preset: {presetName}\n");
    }

    /// <summary>
    /// Synchronizes LiveConfig from SimulationParameters for backward compatibility.
    /// </summary>
    private void SyncLiveConfigFromParameters(in SimulationParameters p)
    {
        if (LiveConfig is null) return;

        LiveConfig.GravitationalCoupling = p.GravitationalCoupling;
        LiveConfig.Temperature = p.Temperature;
        LiveConfig.VacuumEnergyScale = p.VacuumEnergyScale;
        LiveConfig.AnnealingCoolingRate = p.AnnealingRate;
        LiveConfig.DecoherenceRate = p.DecoherenceRate;
        LiveConfig.WilsonParameter = p.WilsonParameter;
        LiveConfig.GaugeFieldDamping = p.GaugeFieldDamping;
        LiveConfig.EdgeTrialProb = p.EdgeTrialProbability;
        LiveConfig.MeasurementThreshold = p.MeasurementThreshold;
        LiveConfig.PairCreationEnergy = p.PairCreationEnergy;
        LiveConfig.SpectralLambdaCutoff = p.SpectralCutoff;
        LiveConfig.SpectralTargetDimension = p.TargetSpectralDimension;
        LiveConfig.SpectralDimensionPotentialStrength = p.SpectralDimensionStrength;
    }

    /// <summary>
    /// Creates SimulationParameters from current LiveConfig (for UI sync).
    /// </summary>
    public SimulationParameters GetParametersFromLiveConfig()
    {
        if (LiveConfig is null) return SimulationParameters.Default;

        return new SimulationParameters
        {
            DeltaTime = 0.01,
            CurrentTime = 0,
            TickId = 0,
            
            GravitationalCoupling = LiveConfig.GravitationalCoupling,
            RicciFlowAlpha = LiveConfig.LapseFunctionAlpha,
            LapseFunctionAlpha = LiveConfig.LapseFunctionAlpha,
            CosmologicalConstant = 0,
            VacuumEnergyScale = LiveConfig.VacuumEnergyScale,
            LazyWalkAlpha = 0.1,
            
            Temperature = LiveConfig.Temperature,
            InverseBeta = LiveConfig.Temperature > 0 ? 1.0 / LiveConfig.Temperature : 0.1,
            AnnealingRate = LiveConfig.AnnealingCoolingRate,
            DecoherenceRate = LiveConfig.DecoherenceRate,
            
            GaugeCoupling = LiveConfig.WilsonParameter,
            WilsonParameter = LiveConfig.WilsonParameter,
            GaugeFieldDamping = LiveConfig.GaugeFieldDamping,
            
            EdgeCreationProbability = LiveConfig.EdgeTrialProb,
            EdgeDeletionProbability = LiveConfig.EdgeTrialProb * 0.2,
            TopologyBreakThreshold = 0.001,
            EdgeTrialProbability = LiveConfig.EdgeTrialProb,
            
            MeasurementThreshold = LiveConfig.MeasurementThreshold,
            ScalarFieldMassSquared = LiveConfig.PairCreationMassThreshold * LiveConfig.PairCreationMassThreshold,
            FermionMass = LiveConfig.PairCreationMassThreshold,
            PairCreationEnergy = LiveConfig.PairCreationEnergy,
            
            SpectralCutoff = LiveConfig.SpectralLambdaCutoff,
            TargetSpectralDimension = LiveConfig.SpectralTargetDimension,
            SpectralDimensionStrength = LiveConfig.SpectralDimensionPotentialStrength,
            
            SinkhornIterations = 50,
            SinkhornEpsilon = 0.01,
            ConvergenceThreshold = 1e-6,
            
            Flags = SimulationParameters.Default.Flags
        };
    }
}
