using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test gravity evolution preserves weight bounds.
    /// </summary>
    [TestMethod]
    [TestCategory("Gravity")]
    public void GravityEvolution_WeightBounds_Preserved()
    {
        // Arrange
        var graph = CreateTestGraph(MediumGraphNodes, 0.12, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}, E={graph.FlatEdgesFrom.Length}");

        double dt = 0.01;
        double G = 0.1; // Stronger gravity
        int steps = 100;

        // Act
        for (int s = 0; s < steps; s++)
        {
            ImprovedNetworkGravity.EvolveNetworkGeometryForman(graph, dt, G);
        }

        // Assert - check all weights are in [0, 1]
        int edgeCount = graph.FlatEdgesFrom.Length;
        int outOfBoundsCount = 0;
        double minWeight = double.MaxValue;
        double maxWeight = double.MinValue;

        for (int e = 0; e < edgeCount; e++)
        {
            int i = graph.FlatEdgesFrom[e];
            int j = graph.FlatEdgesTo[e];
            double w = graph.Weights[i, j];

            minWeight = Math.Min(minWeight, w);
            maxWeight = Math.Max(maxWeight, w);

            if (w < 0 || w > 1)
            {
                outOfBoundsCount++;
            }
        }

        Console.WriteLine($"Weight range: [{minWeight:F6}, {maxWeight:F6}]");
        Console.WriteLine($"Out of bounds: {outOfBoundsCount}/{edgeCount}");

        Assert.AreEqual(0, outOfBoundsCount, "All weights should be in [0, 1]");
    }
}
