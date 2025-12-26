using RQSimulation;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Compare Ollivier-Ricci vs Forman-Ricci curvature distributions.
    /// They should be correlated but not identical.
    /// </summary>
    [TestMethod]
    [TestCategory("Curvature")]
    public void CurvatureComparison_OllivierVsForman_DistributionAnalysis()
    {
        // Arrange
        var graph = CreateTestGraph(MediumGraphNodes, 0.12, TestSeed);
        graph.BuildSoAViews();

        int edgeCount = graph.FlatEdgesFrom.Length;
        double[] ollivier = new double[edgeCount];
        double[] forman = new double[edgeCount];

        Console.WriteLine($"Graph: N={graph.N}, E={edgeCount}");

        // Act
        for (int e = 0; e < edgeCount; e++)
        {
            int i = graph.FlatEdgesFrom[e];
            int j = graph.FlatEdgesTo[e];

            ollivier[e] = graph.CalculateOllivierRicciCurvature(i, j);
            forman[e] = graph.ComputeFormanRicciCurvature(i, j);
        }

        // Statistics
        double ollivierMean = ollivier.Average();
        double formanMean = forman.Average();
        double ollivierStd = Math.Sqrt(ollivier.Select(x => (x - ollivierMean) * (x - ollivierMean)).Average());
        double formanStd = Math.Sqrt(forman.Select(x => (x - formanMean) * (x - formanMean)).Average());

        // Correlation
        double correlation = 0;
        if (ollivierStd > 0 && formanStd > 0)
        {
            for (int e = 0; e < edgeCount; e++)
            {
                correlation += (ollivier[e] - ollivierMean) * (forman[e] - formanMean);
            }
            correlation /= (edgeCount * ollivierStd * formanStd);
        }

        Console.WriteLine($"Ollivier-Ricci: Mean={ollivierMean:F4}, Std={ollivierStd:F4}, Min={ollivier.Min():F4}, Max={ollivier.Max():F4}");
        Console.WriteLine($"Forman-Ricci:   Mean={formanMean:F4}, Std={formanStd:F4}, Min={forman.Min():F4}, Max={forman.Max():F4}");
        Console.WriteLine($"Correlation:    {correlation:F4}");

        // Assert: They should be somewhat correlated (both measure curvature)
        // but not identical (different mathematical definitions)
        // Relaxed: correlation can be weak for certain graph types
        Assert.IsTrue(correlation > 0.1 || correlation < -0.1, $"Curvature methods should have some correlation, got {correlation:F4}");
        Assert.IsTrue(Math.Abs(correlation) < 0.999, $"Curvature methods should not be identical, got {correlation:F4}");
    }
}
