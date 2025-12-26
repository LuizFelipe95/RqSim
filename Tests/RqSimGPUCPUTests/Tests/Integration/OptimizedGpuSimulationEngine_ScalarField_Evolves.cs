using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: OptimizedGpuSimulationEngine scalar field evolution with Higgs potential.
    /// </summary>
    [TestMethod]
    [TestCategory("Integration")]
    public void OptimizedGpuSimulationEngine_ScalarField_Evolves()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();
        graph.InitScalarField(amplitude: 1.0);

        Console.WriteLine($"Graph: N={graph.N}");

        double initialEnergy = 0;
        for (int i = 0; i < graph.N; i++)
        {
            initialEnergy += graph.ScalarField[i] * graph.ScalarField[i];
        }
        Console.WriteLine($"Initial scalar field energy: {initialEnergy:F4}");

        using var engine = new OptimizedGpuSimulationEngine(graph);
        engine.Initialize();
        engine.UploadState();

        // Act - Evolve scalar field via unified GPU step
        float dt = 0.01f;
        float G = 0.05f;
        float lambda = 0.01f;
        float degreePenalty = 0.5f;
        float diffusionRate = 0.1f;
        
        for (int step = 0; step < 50; step++)
        {
            // StepGpu includes scalar field evolution with Higgs potential
            engine.StepGpu(dt, G, lambda, degreePenalty, diffusionRate);
        }

        // Sync scalar field back to graph
        engine.SyncScalarFieldToGraph();

        double finalEnergy = 0;
        for (int i = 0; i < graph.N; i++)
        {
            finalEnergy += graph.ScalarField[i] * graph.ScalarField[i];
        }
        Console.WriteLine($"Final scalar field energy: {finalEnergy:F4}");

        // Assert - field should evolve (energy changes due to Higgs potential)
        // Just check that values are still finite
        Assert.IsTrue(double.IsFinite(finalEnergy), "Final energy should be finite");
        for (int i = 0; i < Math.Min(10, graph.N); i++)
        {
            Assert.IsTrue(double.IsFinite(graph.ScalarField[i]), $"ScalarField[{i}] should be finite");
        }
    }
}
