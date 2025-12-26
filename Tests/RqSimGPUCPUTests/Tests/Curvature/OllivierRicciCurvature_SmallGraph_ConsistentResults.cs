using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: CPU Ollivier-Ricci (Jaccard approximation) vs GPU implementation
    /// 
    /// ?_OR(i,j) = |N(i) ? N(j)| / |N(i) ? N(j)| - 1
    /// </summary>
    [TestMethod]
    [TestCategory("Curvature")]
    public void OllivierRicciCurvature_SmallGraph_ConsistentResults()
    {
        // Arrange
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        int edgeCount = graph.FlatEdgesFrom.Length;
        double[] methodA = new double[edgeCount]; // Via RQGraph method
        double[] methodB = new double[edgeCount]; // Via static helper

        Console.WriteLine($"Graph: N={graph.N}, E={edgeCount}");

        // Act
        for (int e = 0; e < edgeCount; e++)
        {
            int i = graph.FlatEdgesFrom[e];
            int j = graph.FlatEdgesTo[e];

            methodA[e] = graph.CalculateOllivierRicciCurvature(i, j);
            methodB[e] = OllivierRicciCurvature.ComputeOllivierRicciJaccard(graph, i, j);
        }

        // Print first 10 values
        Console.WriteLine("First 10 Ollivier-Ricci curvatures (MethodA vs MethodB):");
        for (int e = 0; e < Math.Min(10, edgeCount); e++)
        {
            Console.WriteLine($"  [{e}] A={methodA[e]:F6}, B={methodB[e]:F6}");
        }

        // Assert
        var result = CompareArrays(methodA, methodB, "OllivierRicciCurvature");
        Console.WriteLine(result);
        Assert.IsTrue(result.Passed, result.Message);
    }
}
