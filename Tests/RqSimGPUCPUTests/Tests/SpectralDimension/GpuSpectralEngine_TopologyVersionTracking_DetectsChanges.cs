using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: GpuSpectralEngine topology version tracking prevents stale data.
    /// </summary>
    [TestMethod]
    [TestCategory("SpectralDimension")]
    public void GpuSpectralEngine_TopologyVersionTracking_DetectsChanges()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}, E={graph.FlatEdgesFrom.Length}");

        // Act - Initial computation with first engine instance
        double dim1;
        int initialVersion;
        using (var gpuEngine1 = new GpuSpectralEngine())
        {
            gpuEngine1.UpdateTopology(graph);
            initialVersion = gpuEngine1.TopologyVersion;
            dim1 = gpuEngine1.ComputeSpectralDimension(graph, dt: 0.01, numSteps: 50);
            Console.WriteLine($"Initial: version={initialVersion}, d_S={dim1:F4}");
        }

        // Modify graph topology (add/remove edges) - AddEdge increments TopologyVersion
        int modifiedEdges = 0;
        int initialTopologyVersion = graph.TopologyVersion;
        for (int i = 0; i < 5 && i < graph.N - 1; i++)
        {
            int j = i + 1;
            if (!graph.Edges[i, j])
            {
                graph.AddEdge(i, j);
                graph.Weights[i, j] = 0.5;
                graph.Weights[j, i] = 0.5;
                modifiedEdges++;
            }
        }
        graph.BuildSoAViews();

        Console.WriteLine($"Modified {modifiedEdges} edges, TopologyVersion: {initialTopologyVersion} -> {graph.TopologyVersion}");

        // Act - Compute again with NEW engine instance (avoids ComputeSharp pipeline cache conflict)
        double dim2;
        int newVersion;
        using (var gpuEngine2 = new GpuSpectralEngine())
        {
            gpuEngine2.UpdateTopology(graph);
            dim2 = gpuEngine2.ComputeSpectralDimension(graph, dt: 0.01, numSteps: 50);
            newVersion = gpuEngine2.TopologyVersion;
            Console.WriteLine($"After update: version={newVersion}, d_S={dim2:F4}");
        }

        // Assert
        Assert.AreEqual(graph.TopologyVersion, newVersion, "GPU engine should track graph topology version");
        Assert.IsTrue(double.IsFinite(dim1) && double.IsFinite(dim2), "Both dimensions should be finite");
        
        // Verify version tracking: new engine should have updated version
        Assert.IsTrue(newVersion > initialVersion || modifiedEdges == 0, 
            $"Version should increase after topology change: initial={initialVersion}, new={newVersion}");
    }
}
