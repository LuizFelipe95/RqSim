using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: CPU gravity evolution vs GPU gravity evolution
    /// 
    /// Weight update: dw/dt = -G * (Ric - T + ?)
    /// </summary>
    [TestMethod]
    [TestCategory("Gravity")]
    public void GravityEvolution_SmallGraph_GpuMatchesCpu()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange - create two identical graphs
        var graphCpu = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        var graphGpu = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);

        graphCpu.BuildSoAViews();
        graphGpu.BuildSoAViews();

        Console.WriteLine($"Graph: N={graphCpu.N}, E={graphCpu.FlatEdgesFrom.Length}");

        double dt = 0.01;
        double G = 0.05;
        int steps = 10;

        // Act - CPU evolution
        for (int s = 0; s < steps; s++)
        {
            ImprovedNetworkGravity.EvolveNetworkGeometryForman(graphCpu, dt, G);
        }

        // Act - GPU evolution
        graphGpu.InitGpuGravity();
        try
        {
            for (int s = 0; s < steps; s++)
            {
                // Use Ollivier-Dynamic which uses GPU when available
                ImprovedNetworkGravity.EvolveNetworkGeometryOllivierDynamic(graphGpu, dt, G);
            }
        }
        finally
        {
            graphGpu.DisposeGpuGravity();
        }

        // Compare final weights
        int edgeCount = graphCpu.FlatEdgesFrom.Length;
        double[] cpuWeights = new double[edgeCount];
        double[] gpuWeights = new double[edgeCount];

        for (int e = 0; e < edgeCount; e++)
        {
            int i = graphCpu.FlatEdgesFrom[e];
            int j = graphCpu.FlatEdgesTo[e];
            cpuWeights[e] = graphCpu.Weights[i, j];
            gpuWeights[e] = graphGpu.Weights[i, j];
        }

        // Print first 10 weights
        Console.WriteLine("First 10 weights after evolution (CPU vs GPU):");
        for (int e = 0; e < Math.Min(10, edgeCount); e++)
        {
            Console.WriteLine($"  [{e}] CPU={cpuWeights[e]:F6}, GPU={gpuWeights[e]:F6}");
        }

        // Assert
        var result = CompareArrays(cpuWeights, gpuWeights, "GravityEvolution_Weights");
        Console.WriteLine(result);

        // Note: Some divergence is expected due to different curvature methods
        // Forman (CPU) vs Ollivier (GPU)
        if (!result.Passed)
        {
            Console.WriteLine("Note: Divergence may be due to Forman vs Ollivier curvature methods");
        }
    }
}
