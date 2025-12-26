using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: Verify GPU engine initialization and cleanup
    /// </summary>
    [TestMethod]
    [TestCategory("GPU")]
    public void GpuEngine_InitAndCleanup_NoLeaks()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);

        // Initialize and dispose multiple times
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"Iteration {i + 1}");

            bool init = graph.InitGpuGravity();
            Assert.IsTrue(init, $"GPU init should succeed on iteration {i + 1}");

            // Do some work
            ImprovedNetworkGravity.EvolveNetworkGeometryForman(graph, 0.01, 0.05);

            graph.DisposeGpuGravity();
        }

        Console.WriteLine("GPU init/cleanup cycles completed without issues");
    }
}
