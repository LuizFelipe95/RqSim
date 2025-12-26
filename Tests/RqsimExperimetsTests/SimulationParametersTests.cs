using Microsoft.VisualStudio.TestTools.UnitTesting;
using RqSimEngineApi.Contracts;
using RqSimForms.Forms.Interfaces;

namespace RqsimExperimetsTests;

/// <summary>
/// Tests for SimulationParameters and PhysicsSettingsConfig conversion.
/// Verifies the uni-pipeline infrastructure works correctly.
/// </summary>
[TestClass]
public class SimulationParametersTests
{
    [TestMethod]
    public void Default_HasReasonableValues()
    {
        var p = SimulationParameters.Default;
        
        Assert.AreEqual(0.01, p.DeltaTime, 1e-10);
        Assert.AreEqual(0.05, p.GravitationalCoupling, 1e-10);
        Assert.AreEqual(0.5, p.RicciFlowAlpha, 1e-10);
        Assert.AreEqual(10.0, p.Temperature, 1e-10);
        Assert.AreEqual(50, p.SinkhornIterations);
        Assert.IsTrue(p.UseDoublePrecision);
    }

    [TestMethod]
    public void FastPreview_HasLowerPrecision()
    {
        var p = SimulationParameters.FastPreview;
        
        // FastPreview should have larger timestep
        Assert.IsTrue(p.DeltaTime > SimulationParameters.Default.DeltaTime);
        
        // Fewer Sinkhorn iterations
        Assert.IsTrue(p.SinkhornIterations < SimulationParameters.Default.SinkhornIterations);
    }

    [TestMethod]
    public void Scientific_HasHighPrecision()
    {
        var p = SimulationParameters.Scientific;
        
        // Scientific should have smaller timestep
        Assert.IsTrue(p.DeltaTime < SimulationParameters.Default.DeltaTime);
        
        // More Sinkhorn iterations
        Assert.IsTrue(p.SinkhornIterations > SimulationParameters.Default.SinkhornIterations);
        
        // Scientific mode flag should be set
        Assert.IsTrue(p.ScientificMode);
    }

    [TestMethod]
    public void Validate_ReturnsNoErrors_ForDefaultParams()
    {
        var p = SimulationParameters.Default;
        var errors = p.Validate();
        
        Assert.AreEqual(0, errors.Count, $"Validation errors: {string.Join(", ", errors)}");
    }

    [TestMethod]
    public void Validate_ReturnsErrors_ForInvalidParams()
    {
        var p = SimulationParameters.Default;
        p.DeltaTime = -1;  // Invalid
        p.Temperature = 0;  // Invalid
        
        var errors = p.Validate();
        
        Assert.IsTrue(errors.Count >= 2);
        Assert.IsTrue(errors.Any(e => e.Contains("DeltaTime")));
        Assert.IsTrue(errors.Any(e => e.Contains("Temperature")));
    }

    [TestMethod]
    public void With_CreatesModifiedCopy()
    {
        var original = SimulationParameters.Default;
        var modified = original.With(gravitationalCoupling: 0.1, temperature: 20.0);
        
        // Original unchanged
        Assert.AreEqual(0.05, original.GravitationalCoupling, 1e-10);
        Assert.AreEqual(10.0, original.Temperature, 1e-10);
        
        // Modified has new values
        Assert.AreEqual(0.1, modified.GravitationalCoupling, 1e-10);
        Assert.AreEqual(20.0, modified.Temperature, 1e-10);
    }

    [TestMethod]
    public void Flags_SetAndGet_WorkCorrectly()
    {
        var p = new SimulationParameters();
        
        // Initially all false
        Assert.IsFalse(p.UseDoublePrecision);
        Assert.IsFalse(p.ScientificMode);
        Assert.IsFalse(p.EnableOllivierRicci);
        
        // Set flags
        p.UseDoublePrecision = true;
        p.EnableOllivierRicci = true;
        
        // Verify
        Assert.IsTrue(p.UseDoublePrecision);
        Assert.IsFalse(p.ScientificMode);
        Assert.IsTrue(p.EnableOllivierRicci);
        
        // Clear one flag
        p.UseDoublePrecision = false;
        Assert.IsFalse(p.UseDoublePrecision);
        Assert.IsTrue(p.EnableOllivierRicci);  // Other flag unchanged
    }

    [TestMethod]
    public void StructSize_IsReasonable()
    {
        // Ensure struct isn't accidentally huge
        int size = System.Runtime.InteropServices.Marshal.SizeOf<SimulationParameters>();
        
        // Should be around 240 bytes (adjust if fields change)
        Assert.IsTrue(size < 500, $"SimulationParameters is too large: {size} bytes");
        Assert.IsTrue(size > 100, $"SimulationParameters is suspiciously small: {size} bytes");
    }
}

/// <summary>
/// Tests for PhysicsSettingsConfig GPU conversion.
/// </summary>
[TestClass]
public class PhysicsSettingsConfigConversionTests
{
    [TestMethod]
    public void ToGpuParameters_PreservesGravitationalCoupling()
    {
        var config = new PhysicsSettingsConfig
        {
            GravitationalCoupling = 0.123
        };
        
        var gpu = config.ToGpuParameters();
        
        Assert.AreEqual(0.123, gpu.GravitationalCoupling, 1e-10);
    }

    [TestMethod]
    public void ToGpuParameters_PreservesTemperature()
    {
        var config = new PhysicsSettingsConfig
        {
            Temperature = 42.0
        };
        
        var gpu = config.ToGpuParameters();
        
        Assert.AreEqual(42.0, gpu.Temperature, 1e-10);
        Assert.AreEqual(1.0 / 42.0, gpu.InverseBeta, 1e-10);
    }

    [TestMethod]
    public void ToGpuParameters_PreservesFlags()
    {
        var config = new PhysicsSettingsConfig
        {
            EnableVacuumEnergyReservoir = true,
            PreferOllivierRicciCurvature = true,
            UseHamiltonianGravity = true,
            EnableWilsonLoopProtection = false
        };
        
        var gpu = config.ToGpuParameters();
        
        Assert.IsTrue(gpu.EnableVacuumReservoir);
        Assert.IsTrue(gpu.EnableOllivierRicci);
        Assert.IsTrue(gpu.EnableHamiltonianGravity);
        Assert.IsFalse(gpu.EnableWilsonProtection);
    }

    [TestMethod]
    public void RoundTrip_PreservesValues()
    {
        var original = new PhysicsSettingsConfig
        {
            GravitationalCoupling = 0.07,
            Temperature = 15.0,
            LapseFunctionAlpha = 0.8,
            VacuumEnergyScale = 0.0001,
            DecoherenceRate = 0.005,
            WilsonParameter = 1.5,
            EnableVacuumEnergyReservoir = true,
            PreferOllivierRicciCurvature = false
        };
        
        // Convert to GPU and back
        var gpu = original.ToGpuParameters();
        var restored = new PhysicsSettingsConfig();
        restored.FromGpuParameters(gpu);
        
        // Check key values preserved
        Assert.AreEqual(original.GravitationalCoupling, restored.GravitationalCoupling, 1e-10);
        Assert.AreEqual(original.Temperature, restored.Temperature, 1e-10);
        Assert.AreEqual(original.LapseFunctionAlpha, restored.LapseFunctionAlpha, 1e-10);
        Assert.AreEqual(original.EnableVacuumEnergyReservoir, restored.EnableVacuumEnergyReservoir);
        Assert.AreEqual(original.PreferOllivierRicciCurvature, restored.PreferOllivierRicciCurvature);
    }
}

/// <summary>
/// Tests for SimulationContext with parameters.
/// </summary>
[TestClass]
public class SimulationContextWithParamsTests
{
    [TestMethod]
    public void Default_IncludesDefaultParams()
    {
        var ctx = SimulationContext.Default;
        
        // Params should be populated with defaults
        Assert.AreEqual(0.05, ctx.Params.GravitationalCoupling, 1e-10);
    }

    [TestMethod]
    public void SyncFromParams_UpdatesLegacyFields()
    {
        var ctx = new SimulationContext
        {
            Params = SimulationParameters.Default.With(gravitationalCoupling: 0.2)
        };
        
        ctx.SyncFromParams();
        
        Assert.AreEqual(0.2, ctx.GravityStrength, 1e-10);
    }

    [TestMethod]
    public void SyncToParams_UpdatesParamsFields()
    {
        var ctx = new SimulationContext
        {
            Time = 100.0,
            DeltaTime = 0.05,
            TickId = 5000,
            GravityStrength = 0.15
        };
        
        ctx.SyncToParams();
        
        Assert.AreEqual(100.0, ctx.Params.CurrentTime, 1e-10);
        Assert.AreEqual(0.05, ctx.Params.DeltaTime, 1e-10);
        Assert.AreEqual(5000, ctx.Params.TickId);
        Assert.AreEqual(0.15, ctx.Params.GravitationalCoupling, 1e-10);
    }

    [TestMethod]
    public void Snapshot_PreservesParams()
    {
        var ctx = SimulationContext.Default;
        ctx.Params = ctx.Params.With(temperature: 99.0);
        
        var snapshot = SimulationContextSnapshot.FromContext(ctx);
        var restored = snapshot.ToContext();
        
        Assert.AreEqual(99.0, restored.Params.Temperature, 1e-10);
    }
}
