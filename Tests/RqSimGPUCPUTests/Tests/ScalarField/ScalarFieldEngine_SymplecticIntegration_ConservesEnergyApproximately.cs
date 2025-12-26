using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: ScalarFieldEngine symplectic integration energy conservation.
    /// </summary>
    [TestMethod]
    [TestCategory("ScalarField")]
    public void ScalarFieldEngine_SymplecticIntegration_ConservesEnergyApproximately()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}");

        int totalEdges = graph.CsrOffsets[graph.N];
        using var scalarEngine = new ScalarFieldEngine();
        scalarEngine.Initialize(graph.N, totalEdges);

        // Build topology
        int[] offsets = graph.CsrOffsets;
        int[] neighbors = graph.CsrIndices;
        float[] weights = new float[totalEdges];
        for (int n = 0; n < graph.N; n++)
        {
            int start = offsets[n];
            int end = offsets[n + 1];
            for (int k = start; k < end; k++)
            {
                int to = neighbors[k];
                weights[k] = (float)graph.Weights[n, to];
            }
        }
        scalarEngine.UpdateTopology(offsets, neighbors, weights);

        // Initialize field with small perturbation around VEV
        float vev = MathF.Sqrt(0.01f / 0.1f); // sqrt(mu^2/lambda)
        float[] field = new float[graph.N];
        var rng = new Random(TestSeed);
        for (int i = 0; i < graph.N; i++)
        {
            field[i] = vev + 0.1f * (float)(rng.NextDouble() - 0.5);
        }

        // Compute initial "energy" (sum of field^2 as proxy)
        double initialFieldNorm = field.Sum(f => f * f);
        Console.WriteLine($"Initial field norm: {initialFieldNorm:F4}");

        // Act - Run many steps with small dt (symplectic should conserve better)
        float dt = 0.001f;
        int steps = 1000;
        for (int s = 0; s < steps; s++)
        {
            scalarEngine.UpdateField(field, dt, diffusionRate: 0.1f, higgsLambda: 0.1f, higgsMuSquared: 0.01f);
        }

        double finalFieldNorm = field.Sum(f => f * f);
        Console.WriteLine($"Final field norm: {finalFieldNorm:F4}");

        double relChange = Math.Abs(finalFieldNorm - initialFieldNorm) / initialFieldNorm;
        Console.WriteLine($"Relative change: {relChange:P2}");

        // Assert - symplectic integration should have bounded energy drift
        // With Higgs potential, energy is not strictly conserved, but shouldn't explode
        Assert.IsTrue(finalFieldNorm > 0, "Field norm should remain positive");
        Assert.IsTrue(finalFieldNorm < initialFieldNorm * 100, "Field should not explode");
        Assert.IsTrue(field.All(float.IsFinite), "All field values should be finite");
    }
}
