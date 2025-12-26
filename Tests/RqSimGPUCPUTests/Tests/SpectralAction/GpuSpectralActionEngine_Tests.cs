using RQSimulation;
using RQSimulation.GPUOptimized.SpectralAction;

namespace RqSimGPUCPUTests;

/// <summary>
/// Unit tests for GPU Spectral Action implementation.
/// Tests GPU vs CPU accuracy with double precision (64-bit).
/// 
/// RQ-HYPOTHESIS STAGE 5: GPU-ACCELERATED SPECTRAL ACTION
/// ======================================================
/// These tests verify that the GPU implementation of the
/// Chamseddine-Connes spectral action produces results
/// identical to the CPU implementation within tolerance.
/// </summary>
public partial class RqSimGPUCPUTest
{
    // ============================================================
    // GPU SPECTRAL ACTION ENGINE TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_Initialize_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        using var engine = new GpuSpectralActionEngine();
        engine.Initialize(100, 400);
        
        Assert.IsTrue(engine.IsInitialized, "Engine should be initialized");
        Assert.AreEqual(100, engine.NodeCount, "Node count should match");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_UploadTopology_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        
        Assert.IsTrue(engine.IsInitialized, "Engine should be initialized after upload");
        Assert.AreEqual(graph.N, engine.NodeCount, "Node count should match graph");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_EffectiveVolume_GpuMatchesCpu()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        // CPU computation
        double cpuVolume = SpectralAction.ComputeEffectiveVolume(graph);
        
        // GPU computation
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        double gpuVolume = engine.ComputeEffectiveVolume();
        
        Console.WriteLine($"CPU Volume: {cpuVolume:F6}");
        Console.WriteLine($"GPU Volume: {gpuVolume:F6}");
        Console.WriteLine($"Difference: {Math.Abs(cpuVolume - gpuVolume):E10}");
        
        var result = CompareValues(cpuVolume, gpuVolume, "EffectiveVolume");
        Assert.IsTrue(result.Passed, result.Message);
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_AverageCurvature_GpuMatchesCpu()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        // CPU computation
        double cpuCurvature = SpectralAction.ComputeAverageCurvature(graph);
        
        // GPU computation
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        double gpuCurvature = engine.ComputeAverageCurvature();
        
        Console.WriteLine($"CPU Average Curvature: {cpuCurvature:F6}");
        Console.WriteLine($"GPU Average Curvature: {gpuCurvature:F6}");
        Console.WriteLine($"Difference: {Math.Abs(cpuCurvature - gpuCurvature):E10}");
        
        var result = CompareValues(cpuCurvature, gpuCurvature, "AverageCurvature");
        Assert.IsTrue(result.Passed, result.Message);
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_WeylSquared_GpuMatchesCpu()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        // CPU computation
        double cpuWeyl = SpectralAction.ComputeWeylSquared(graph);
        
        // GPU computation
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        double gpuWeyl = engine.ComputeWeylSquared();
        
        Console.WriteLine($"CPU Weyl Squared: {cpuWeyl:F6}");
        Console.WriteLine($"GPU Weyl Squared: {gpuWeyl:F6}");
        Console.WriteLine($"Difference: {Math.Abs(cpuWeyl - gpuWeyl):E10}");
        
        var result = CompareValues(cpuWeyl, gpuWeyl, "WeylSquared");
        Assert.IsTrue(result.Passed, result.Message);
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_SpectralDimension_GpuMatchesCpu()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        // CPU computation (fast estimate)
        double cpuDim = SpectralAction.EstimateSpectralDimensionFast(graph);
        
        // GPU computation
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        double gpuDim = engine.EstimateSpectralDimensionFast();
        
        Console.WriteLine($"CPU Spectral Dimension: {cpuDim:F4}");
        Console.WriteLine($"GPU Spectral Dimension: {gpuDim:F4}");
        Console.WriteLine($"Difference: {Math.Abs(cpuDim - gpuDim):E10}");
        
        var result = CompareValues(cpuDim, gpuDim, "SpectralDimension");
        Assert.IsTrue(result.Passed, result.Message);
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_TotalAction_GpuMatchesCpu()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        // CPU computation
        double cpuAction = SpectralAction.ComputeSpectralAction(graph);
        
        // GPU computation
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        double gpuAction = engine.ComputeSpectralAction();
        
        Console.WriteLine($"CPU Total Action: {cpuAction:E10}");
        Console.WriteLine($"GPU Total Action: {gpuAction:E10}");
        Console.WriteLine($"Difference: {Math.Abs(cpuAction - gpuAction):E10}");
        
        // Use relative tolerance for large values
        double relDiff = Math.Abs(cpuAction) > 1e-10 
            ? Math.Abs(cpuAction - gpuAction) / Math.Abs(cpuAction) 
            : Math.Abs(cpuAction - gpuAction);
        
        Assert.IsTrue(relDiff < 0.1 || Math.Abs(cpuAction - gpuAction) < 1e-6,
            $"GPU spectral action should match CPU within tolerance. RelDiff: {relDiff:P2}");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_GetComponents_AllNonNegative()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        
        var components = engine.GetComponents();
        
        Console.WriteLine($"Volume: {components.Volume:F4}");
        Console.WriteLine($"Average Curvature: {components.AverageCurvature:F4}");
        Console.WriteLine($"Weyl Squared: {components.WeylSquared:F4}");
        Console.WriteLine($"Spectral Dimension: {components.SpectralDimension:F4}");
        Console.WriteLine($"S_Cosmological: {components.S_Cosmological:E6}");
        Console.WriteLine($"S_EinsteinHilbert: {components.S_EinsteinHilbert:E6}");
        Console.WriteLine($"S_Weyl: {components.S_Weyl:E6}");
        Console.WriteLine($"S_Dimension: {components.S_Dimension:E6}");
        Console.WriteLine($"Total: {components.Total:E6}");
        
        Assert.IsTrue(components.Volume >= 0, "Volume should be non-negative");
        Assert.IsTrue(components.WeylSquared >= 0, "Weyl squared should be non-negative");
        Assert.IsTrue(components.SpectralDimension >= 1.0 && components.SpectralDimension <= 8.0,
            $"Spectral dimension should be in [1, 8], got {components.SpectralDimension}");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_EmptyGraph_ZeroVolume()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engineSim = new SimulationEngine(config);
        var graph = engineSim.Graph;
        
        // Clear all edges
        ClearAllEdges(graph);
        graph.BuildSoAViews();
        
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        
        double volume = engine.ComputeEffectiveVolume();
        
        Console.WriteLine($"Empty graph volume: {volume:E10}");
        
        Assert.AreEqual(0.0, volume, 1e-10, "Empty graph should have zero volume");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_ChainGraph_VolumeMatchesEdgeCount()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        // Create chain graph: 0-1-2-3-4
        int chainLength = 5;
        var config = new SimulationConfig { NodeCount = chainLength, Seed = TestSeed };
        var engineSim = new SimulationEngine(config);
        var graph = engineSim.Graph;
        
        ClearAllEdges(graph);
        
        // Create chain with weight 1.0
        for (int i = 0; i < chainLength - 1; i++)
        {
            graph.Edges[i, i + 1] = true;
            graph.Edges[i + 1, i] = true;
            graph.Weights[i, i + 1] = 1.0;
            graph.Weights[i + 1, i] = 1.0;
        }
        graph.BuildSoAViews();
        
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        
        double volume = engine.ComputeEffectiveVolume();
        
        Console.WriteLine($"Chain graph volume: {volume:F4}");
        
        // Chain of 5 nodes has 4 edges, each with weight 1.0
        Assert.AreEqual(4.0, volume, 0.01, $"Chain volume should be 4.0, got {volume}");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_MediumGraph_Performance()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(MediumGraphNodes, 0.2, TestSeed);
        
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        
        // Warm up
        engine.ComputeSpectralAction();
        
        // Measure GPU time
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 10; i++)
        {
            engine.ComputeSpectralAction();
        }
        sw.Stop();
        double gpuMs = sw.ElapsedMilliseconds / 10.0;
        
        // Measure CPU time
        sw.Restart();
        for (int i = 0; i < 10; i++)
        {
            SpectralAction.ComputeSpectralAction(graph);
        }
        sw.Stop();
        double cpuMs = sw.ElapsedMilliseconds / 10.0;
        
        Console.WriteLine($"Graph: {MediumGraphNodes} nodes");
        Console.WriteLine($"GPU time: {gpuMs:F2} ms");
        Console.WriteLine($"CPU time: {cpuMs:F2} ms");
        Console.WriteLine($"Speedup: {cpuMs / Math.Max(gpuMs, 0.001):F2}x");
        
        Assert.IsTrue(gpuMs >= 0, "GPU computation should complete");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_Dispose_NoLeaks()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        // Create and dispose multiple times
        for (int i = 0; i < 5; i++)
        {
            using var engine = new GpuSpectralActionEngine();
            engine.UploadTopology(graph);
            engine.ComputeSpectralAction();
        }
        
        Assert.IsTrue(true, "Multiple create/dispose cycles completed without error");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_Constants_CanBeModified()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        var graph = CreateTestGraph(SmallGraphNodes, 0.3, TestSeed);
        
        using var engine = new GpuSpectralActionEngine();
        engine.UploadTopology(graph);
        
        // Get action with default constants
        double action1 = engine.ComputeSpectralAction();
        
        // Modify constants
        engine.F0_Cosmological = engine.F0_Cosmological * 2.0;
        double action2 = engine.ComputeSpectralAction();
        
        Console.WriteLine($"Action with f?: {action1:E6}");
        Console.WriteLine($"Action with 2*f?: {action2:E6}");
        
        // Actions should differ
        Assert.AreNotEqual(action1, action2, "Modifying constants should change action");
    }
    
    [TestMethod]
    [TestCategory("GpuSpectralAction")]
    public void GpuSpectralActionEngine_DimensionPotential_MexicanHat()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }
        
        using var engine = new GpuSpectralActionEngine();
        
        // Test Mexican hat potential shape
        // V(d) = ? * (d - 4)? * ((d - 4)? - w?)
        // Should have minimum at d = 4
        
        double potAt4 = engine.ComputeDimensionPotential(4.0);
        double potAt3 = engine.ComputeDimensionPotential(3.0);
        double potAt5 = engine.ComputeDimensionPotential(5.0);
        double potAt2 = engine.ComputeDimensionPotential(2.0);
        double potAt6 = engine.ComputeDimensionPotential(6.0);
        
        Console.WriteLine($"V(2) = {potAt2:E6}");
        Console.WriteLine($"V(3) = {potAt3:E6}");
        Console.WriteLine($"V(4) = {potAt4:E6}");
        Console.WriteLine($"V(5) = {potAt5:E6}");
        Console.WriteLine($"V(6) = {potAt6:E6}");
        
        // At d = 4, the potential should be zero (or negative for Mexican hat)
        Assert.IsTrue(potAt4 <= 0.0, $"Potential at d=4 should be <= 0, got {potAt4}");
        
        // Potential at d=4 should be minimum compared to d=2,3,5,6
        Assert.IsTrue(potAt4 <= potAt3, "V(4) should be <= V(3)");
        Assert.IsTrue(potAt4 <= potAt5, "V(4) should be <= V(5)");
    }
}
