using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: Poisson solver on graph.
    /// </summary>
    [TestMethod]
    [TestCategory("GaugeTheory")]
    public void GaussLawProjection_PoissonSolver_Converges()
    {
        // Arrange
        var graph = CreateTestGraph(SmallGraphNodes, 0.20, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}");

        // Create a simple RHS (right-hand side) for Poisson equation
        double[] rhs = new double[graph.N];
        var rng = new Random(TestSeed);
        for (int i = 0; i < graph.N; i++)
        {
            rhs[i] = rng.NextDouble() - 0.5;
        }

        // Make RHS sum to zero (necessary for Poisson on finite graph)
        double sum = rhs.Sum();
        for (int i = 0; i < graph.N; i++)
        {
            rhs[i] -= sum / graph.N;
        }

        // Act
        double[] solution = GaussLawProjection.SolvePoissonOnGraph(graph, rhs);

        Console.WriteLine("First 10 Poisson solution values:");
        for (int i = 0; i < Math.Min(10, solution.Length); i++)
        {
            Console.WriteLine($"  [{i}] ?={solution[i]:F6}");
        }

        // Assert
        Assert.AreEqual(graph.N, solution.Length, "Solution array should have N elements");
        Assert.IsTrue(solution.All(double.IsFinite), "All solution values should be finite");
    }
}
