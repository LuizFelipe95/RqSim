using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: OptimizedGpuSimulationEngine initialization and basic operation.
    /// </summary>
    [TestMethod]
    [TestCategory("Integration")]
    public void OptimizedGpuSimulationEngine_InitAndBasicOps_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var graph = CreateTestGraph(MediumGraphNodes, 0.12, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}, E={graph.FlatEdgesFrom.Length}");

        using var engine = new OptimizedGpuSimulationEngine(graph);

        // Act - Initialize
        engine.Initialize();
        Console.WriteLine("Engine initialized");

        // Upload initial state
        engine.UploadState();

        // Perform a few simulation steps using StepGpu
        float dt = 0.01f;
        float G = 0.05f;
        float lambda = 0.01f;
        float degreePenalty = 0.5f;
        float diffusionRate = 0.1f;
        
        for (int step = 0; step < 10; step++)
        {
            engine.StepGpu(dt, G, lambda, degreePenalty, diffusionRate);
        }
        Console.WriteLine("Ran 10 GPU physics steps");

        // Sync weights back to graph
        engine.SyncWeightsToGraph();

        // Get performance stats
        var (gpuTimeMs, copyTimeMs, kernelLaunches) = engine.GetPerformanceStats();
        Console.WriteLine($"Performance: GPU={gpuTimeMs:F2}ms, Copy={copyTimeMs:F2}ms, Kernels={kernelLaunches}");

        // Assert - basic sanity checks
        Assert.IsTrue(graph.FlatEdgesFrom.Length > 0, "Graph should have edges");
        Assert.IsTrue(kernelLaunches > 0, "Should have launched GPU kernels");
    }
}
