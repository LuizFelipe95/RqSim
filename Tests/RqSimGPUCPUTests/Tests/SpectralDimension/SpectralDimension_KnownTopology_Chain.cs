using RQSimulation;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Additional test: Verify spectral dimension computation on simple known topologies
    /// </summary>
    [TestMethod]
    [TestCategory("SpectralDimension")]
    public void SpectralDimension_KnownTopology_Chain()
    {
        // A chain/line graph should have d_S ? 1 (1D structure)
        int chainLength = 100;
        var config = new SimulationConfig { NodeCount = chainLength, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;

        // Clear all edges and create chain
        for (int i = 0; i < chainLength; i++)
        {
            for (int j = i + 1; j < chainLength; j++)
            {
                if (graph.Edges[i, j])
                {
                    graph.Edges[i, j] = false;
                    graph.Edges[j, i] = false;
                    graph.Weights[i, j] = 0;
                    graph.Weights[j, i] = 0;
                }
            }
        }

        // Create chain: 0-1-2-...-N
        for (int i = 0; i < chainLength - 1; i++)
        {
            graph.Edges[i, i + 1] = true;
            graph.Edges[i + 1, i] = true;
            graph.Weights[i, i + 1] = 0.5;
            graph.Weights[i + 1, i] = 0.5;
        }

        graph.BuildSoAViews();

        double cpuDim = graph.ComputeSpectralDimension(t_max: 100, num_walkers: 500);
        Console.WriteLine($"Chain graph (N={chainLength}): d_S = {cpuDim:F4} (expected ? 1.0)");

        // Chain should have d_S close to 1
        Assert.IsTrue(cpuDim >= 0.5 && cpuDim <= 2.0, $"Chain d_S should be ?1, got {cpuDim}");
    }
}
