using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: Gauss law divergence computation.
    /// </summary>
    [TestMethod]
    [TestCategory("GaugeTheory")]
    public void GaussLawProjection_DivergenceComputation_Reasonable()
    {
        // Arrange
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}, E={graph.FlatEdgesFrom.Length}");

        // Initialize gauge phases on edges
        graph.InitEdgeGaugePhases();

        // Act
        double[] divergence = GaussLawProjection.ComputeDivergenceOfElectricField(graph);
        double[] charge = GaussLawProjection.ComputeChargeDensity(graph);

        Console.WriteLine("First 10 divergences:");
        for (int i = 0; i < Math.Min(10, divergence.Length); i++)
        {
            Console.WriteLine($"  [{i}] div(E)={divergence[i]:F6}, ?={charge[i]:F6}");
        }

        double meanDiv = divergence.Average();
        double maxDiv = divergence.Max(Math.Abs);
        Console.WriteLine($"Divergence: mean={meanDiv:F6}, max|div|={maxDiv:F6}");

        // Assert - divergence should be computed for all nodes
        Assert.AreEqual(graph.N, divergence.Length, "Divergence array should have N elements");
        Assert.AreEqual(graph.N, charge.Length, "Charge array should have N elements");
        // Divergence values should be finite
        Assert.IsTrue(divergence.All(double.IsFinite), "All divergence values should be finite");
    }
}
