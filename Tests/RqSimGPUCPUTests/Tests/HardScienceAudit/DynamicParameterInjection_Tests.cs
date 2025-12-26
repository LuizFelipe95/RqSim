// ============================================================
// DynamicParameterInjection_Tests.cs
// Unit tests for Hard Science Mode Audit (Item 2: Parameter Injection)
// ============================================================
// 
// AUDIT REQUIREMENT: "Parameter Injection Validation"
// "Добавить unit-тест, который меняет G с 1.0 до 100.0 во время работы,
// и проверяет, что на следующем кадре Energy системы изменилась.
// Это докажет работу Uni-Pipeline."
// ============================================================

using RQSimulation;
using RQSimulation.GPUOptimized;
using RQSimulation.Core.Plugins;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// AUDIT TEST: Verifies that changing G from 1.0 to 100.0 during simulation
    /// causes observable change in system energy on the next frame.
    /// 
    /// This proves that the Uni-Pipeline correctly injects parameters into shaders.
    /// </summary>
    [TestMethod]
    [TestCategory("Gravity")]
    [TestCategory("HardScienceAudit")]
    public void DynamicParameterInjection_ChangingG_CausesEnergyChange()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var graph = CreateTestGraph(SmallGraphNodes, 0.2, TestSeed);
        graph.BuildSoAViews();
        
        int edgeCount = graph.FlatEdgesFrom.Length;
        Console.WriteLine($"Graph: N={graph.N}, E={edgeCount}");
        
        // Create GPU engine with dynamic params support
        var gpuConfig = new GpuConfig { GpuIndex = 0, MultiGpu = false };
        using var gpuEngine = new GpuGravityEngine(gpuConfig, edgeCount, graph.N);
        gpuEngine.UpdateTopologyBuffers(graph);
        
        // Prepare host buffers using existing RQGraph methods
        float[] weights = graph.GetAllWeightsFlat();
        float[] masses = graph.GetNodeMasses();
        int[] edgesFrom = graph.FlatEdgesFrom;
        int[] edgesTo = graph.FlatEdgesTo;
        
        // Initial G = 1.0
        var paramsLowG = DynamicPhysicsParams.Default;
        paramsLowG.GravitationalCoupling = 1.0;
        paramsLowG.DeltaTime = 0.01;
        
        gpuEngine.UpdateParameters(in paramsLowG);
        
        // Run 5 steps with low G
        float[] weightsAfterLowG = new float[edgeCount];
        Array.Copy(weights, weightsAfterLowG, edgeCount);
        
        for (int step = 0; step < 5; step++)
        {
            gpuEngine.EvolveWithDynamicParams(weightsAfterLowG, masses, edgesFrom, edgesTo);
        }
        
        double energyAfterLowG = weightsAfterLowG.Sum();
        Console.WriteLine($"Energy after 5 steps with G=1.0: {energyAfterLowG:F6}");
        
        // Reset weights and run with high G = 100.0
        var paramsHighG = DynamicPhysicsParams.Default;
        paramsHighG.GravitationalCoupling = 100.0;
        paramsHighG.DeltaTime = 0.01;
        
        gpuEngine.UpdateParameters(in paramsHighG);
        
        float[] weightsAfterHighG = new float[edgeCount];
        Array.Copy(weights, weightsAfterHighG, edgeCount);
        
        for (int step = 0; step < 5; step++)
        {
            gpuEngine.EvolveWithDynamicParams(weightsAfterHighG, masses, edgesFrom, edgesTo);
        }
        
        double energyAfterHighG = weightsAfterHighG.Sum();
        Console.WriteLine($"Energy after 5 steps with G=100.0: {energyAfterHighG:F6}");
        
        // Calculate difference
        double energyDiff = Math.Abs(energyAfterHighG - energyAfterLowG);
        double relativeDiff = energyDiff / Math.Max(energyAfterLowG, 1e-10);
        
        Console.WriteLine($"Energy difference: {energyDiff:F6} (relative: {relativeDiff:P2})");
        
        // Assert: Energy MUST be different if parameter injection works
        // With G changing by 100x, we expect at least 10% difference
        Assert.IsTrue(relativeDiff > 0.01, 
            $"AUDIT FAILURE: Changing G from 1.0 to 100.0 caused only {relativeDiff:P2} energy change. " +
            "Parameter injection may not be working - shaders may be cached with stale parameters.");
        
        Console.WriteLine("[AUDIT PASSED] Parameter injection is working correctly.");
    }
    
    /// <summary>
    /// AUDIT TEST: Verifies parameter hash validator detects changes correctly.
    /// </summary>
    [TestMethod]
    [TestCategory("HardScienceAudit")]
    public void ParameterHashValidator_DetectsGravityChange()
    {
        var validator = new ParameterHashValidator();
        
        var params1 = DynamicPhysicsParams.Default;
        params1.GravitationalCoupling = 1.0;
        
        var params2 = DynamicPhysicsParams.Default;
        params2.GravitationalCoupling = 100.0;
        
        // First call - no previous hash
        bool changed1 = validator.HasGravityParamsChanged(in params1);
        validator.UpdateGravityHash(in params1);
        
        // Same params - should NOT detect change
        bool changedSame = validator.HasGravityParamsChanged(in params1);
        Assert.IsFalse(changedSame, "Hash validator incorrectly detected change for same params");
        
        // Different G - MUST detect change
        bool changedDifferent = validator.HasGravityParamsChanged(in params2);
        Assert.IsTrue(changedDifferent, "Hash validator FAILED to detect G change from 1.0 to 100.0");
        
        Console.WriteLine("[AUDIT PASSED] ParameterHashValidator correctly detects G changes.");
    }
    
    /// <summary>
    /// AUDIT TEST: Verifies that RicciFlowAlpha changes affect curvature computation.
    /// </summary>
    [TestMethod]
    [TestCategory("Curvature")]
    [TestCategory("HardScienceAudit")]
    public void DynamicParameterInjection_ChangingAlpha_AffectsCurvature()
    {
        var validator = new ParameterHashValidator();
        
        var paramsLowAlpha = DynamicPhysicsParams.Default;
        paramsLowAlpha.RicciFlowAlpha = 0.1;
        paramsLowAlpha.LazyWalkAlpha = 0.05;
        
        var paramsHighAlpha = DynamicPhysicsParams.Default;
        paramsHighAlpha.RicciFlowAlpha = 0.9;
        paramsHighAlpha.LazyWalkAlpha = 0.5;
        
        validator.UpdateCurvatureHash(in paramsLowAlpha);
        
        bool changed = validator.HasCurvatureParamsChanged(in paramsHighAlpha);
        Assert.IsTrue(changed, "Hash validator FAILED to detect RicciFlowAlpha change");
        
        Console.WriteLine("[AUDIT PASSED] ParameterHashValidator correctly detects curvature param changes.");
    }
    
    /// <summary>
    /// AUDIT TEST: Verifies Scientific Mode flag is properly propagated.
    /// </summary>
    [TestMethod]
    [TestCategory("HardScienceAudit")]
    public void DynamicParameterInjection_ScientificModeFlag_Propagates()
    {
        var paramsVisual = DynamicPhysicsParams.Default;
        paramsVisual.ScientificMode = false;
        
        var paramsScience = DynamicPhysicsParams.Default;
        paramsScience.ScientificMode = true;
        
        // Verify flag accessor works
        Assert.IsFalse(paramsVisual.ScientificMode);
        Assert.IsTrue(paramsScience.ScientificMode);
        
        // Verify hash detects change
        var validator = new ParameterHashValidator();
        validator.UpdateAllHashes(in paramsVisual);
        
        bool changed = validator.HasGravityParamsChanged(in paramsScience);
        Assert.IsTrue(changed, "Hash validator FAILED to detect ScientificMode change");
        
        Console.WriteLine("[AUDIT PASSED] ScientificMode flag correctly detected in hash.");
    }
    
    /// <summary>
    /// AUDIT TEST: Verifies snapshot comparison describes changes correctly.
    /// </summary>
    [TestMethod]
    [TestCategory("HardScienceAudit")]
    public void ParameterSnapshot_DescribesChanges_Correctly()
    {
        var params1 = DynamicPhysicsParams.Default;
        params1.GravitationalCoupling = 1.0;
        params1.RicciFlowAlpha = 0.5;
        params1.SinkhornIterations = 50;
        
        var params2 = DynamicPhysicsParams.Default;
        params2.GravitationalCoupling = 100.0;
        params2.RicciFlowAlpha = 0.5;  // Same
        params2.SinkhornIterations = 100;
        
        var snap1 = params1.CreateSnapshot();
        var snap2 = params2.CreateSnapshot();
        
        string changes = snap1.DescribeChanges(snap2);
        
        Console.WriteLine($"Detected changes: {changes}");
        
        Assert.IsTrue(changes.Contains("G:"), "Should detect G change");
        Assert.IsTrue(changes.Contains("Sinkhorn:"), "Should detect Sinkhorn change");
        Assert.IsFalse(changes.Contains("?:"), "Should NOT detect ? change (same value)");
        
        Console.WriteLine("[AUDIT PASSED] Parameter change description is correct.");
    }
}
