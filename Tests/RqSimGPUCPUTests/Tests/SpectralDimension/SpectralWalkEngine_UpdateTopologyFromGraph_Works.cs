using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: SpectralWalkEngine topology update from graph.
    /// </summary>
    [TestMethod]
    [TestCategory("SpectralDimension")]
    public void SpectralWalkEngine_UpdateTopologyFromGraph_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var graph = CreateTestGraph(MediumGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}");

        using var walkEngine = new SpectralWalkEngine();

        // Act - Initialize from graph
        walkEngine.UpdateTopologyFromGraph(graph, walkerCount: 5000);

        Console.WriteLine($"Engine initialized: walkers={walkEngine.WalkerCount}, nodes={walkEngine.NodeCount}");
        Console.WriteLine($"TopologyVersion: engine={walkEngine.TopologyVersion}, graph={graph.TopologyVersion}");

        // Assert
        Assert.IsTrue(walkEngine.IsInitialized, "Engine should be initialized");
        Assert.AreEqual(graph.TopologyVersion, walkEngine.TopologyVersion, "Versions should match");
        Assert.AreEqual(5000, walkEngine.WalkerCount, "Walker count should match");
        Assert.AreEqual(graph.N, walkEngine.NodeCount, "Node count should match");
    }
}
