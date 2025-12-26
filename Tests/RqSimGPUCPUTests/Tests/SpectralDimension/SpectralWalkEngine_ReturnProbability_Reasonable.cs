using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: SpectralWalkEngine return probability computation.
    /// </summary>
    [TestMethod]
    [TestCategory("SpectralDimension")]
    public void SpectralWalkEngine_ReturnProbability_Reasonable()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange - dense graph for higher return probability
        var graph = CreateTestGraph(SmallGraphNodes, 0.30, TestSeed);
        graph.BuildSoAViews();

        int totalEdges = graph.CsrOffsets[graph.N];
        Console.WriteLine($"Graph: N={graph.N}, TotalDirectedEdges={totalEdges}");

        using var walkEngine = new SpectralWalkEngine();
        walkEngine.UpdateTopologyFromGraph(graph, walkerCount: 10000);
        walkEngine.InitializeWalkersRandom(new Random(TestSeed));

        // Act - Run walks
        int[] returns = walkEngine.RunSteps(50);

        Console.WriteLine("Return counts:");
        int totalReturns = 0;
        for (int t = 0; t < returns.Length; t++)
        {
            totalReturns += returns[t];
            if (t < 10 || returns[t] > 0)
            {
                Console.WriteLine($"  t={t}: {returns[t]}");
            }
        }
        Console.WriteLine($"Total returns: {totalReturns}");

        // Compute spectral dimension from returns
        double dim = walkEngine.ComputeSpectralDimension(returns, skipInitial: 3);
        Console.WriteLine($"Spectral dimension: {dim:F4}");

        // Assert
        Assert.IsTrue(totalReturns > 0, "Should have some returns in dense graph");
        // If we got returns, dimension should be computable
        if (totalReturns > 10)
        {
            Assert.IsTrue(double.IsFinite(dim), $"Dimension should be finite with {totalReturns} returns");
        }
    }
}
