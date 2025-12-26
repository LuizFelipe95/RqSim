using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test Forman-Ricci on medium-sized graph for performance comparison.
    /// </summary>
    [TestMethod]
    [TestCategory("Curvature")]
    public void FormanRicciCurvature_MediumGraph_GpuMatchesCpu()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var graph = CreateTestGraph(MediumGraphNodes, 0.10, TestSeed);
        graph.BuildSoAViews();

        int edgeCount = graph.FlatEdgesFrom.Length;
        double[] cpuCurvatures = new double[edgeCount];

        Console.WriteLine($"Graph: N={graph.N}, E={edgeCount}");

        var cpuStopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - CPU computation
        for (int e = 0; e < edgeCount; e++)
        {
            int i = graph.FlatEdgesFrom[e];
            int j = graph.FlatEdgesTo[e];
            cpuCurvatures[e] = graph.ComputeFormanRicciCurvature(i, j);
        }
        cpuStopwatch.Stop();

        var gpuStopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - GPU computation
        graph.InitGpuGravity();
        double[] gpuCurvatures = new double[edgeCount];

        try
        {
            // Run 10 iterations to warm up and measure
            for (int iter = 0; iter < 10; iter++)
            {
                ImprovedNetworkGravity.EvolveNetworkGeometryForman(graph, dt: 0.0001, effectiveG: 0.01);
            }

            for (int e = 0; e < edgeCount; e++)
            {
                int i = graph.FlatEdgesFrom[e];
                int j = graph.FlatEdgesTo[e];
                gpuCurvatures[e] = graph.ComputeFormanRicciCurvature(i, j);
            }
        }
        finally
        {
            graph.DisposeGpuGravity();
        }
        gpuStopwatch.Stop();

        // Report timing
        Console.WriteLine($"CPU Time: {cpuStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"GPU Time: {gpuStopwatch.ElapsedMilliseconds}ms");
        double speedup = cpuStopwatch.ElapsedMilliseconds > 0
            ? (double)cpuStopwatch.ElapsedMilliseconds / Math.Max(1, gpuStopwatch.ElapsedMilliseconds)
            : 1.0;
        Console.WriteLine($"Speedup: {speedup:F2}x");

        // Assert
        var result = CompareArrays(cpuCurvatures, gpuCurvatures, "FormanRicciCurvature_Medium");
        Console.WriteLine(result);
        Assert.IsTrue(result.Passed, result.Message);
    }
}
