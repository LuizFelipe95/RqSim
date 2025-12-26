using RqSimGraphEngine.RQSimulation.GPUOptimized.Compute;
using RQSimulation;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: RQHypothesisIntegration full physics step.
    /// </summary>
    [TestMethod]
    [TestCategory("Integration")]
    public void RQHypothesisIntegration_StepPhysics_RunsWithoutError()
    {
        // Arrange
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}");

        // Initialize
        RQHypothesisIntegration.UseImprovedGravity = true;
        RQHypothesisIntegration.ValidateSpectralDimension = true;
        RQHypothesisIntegration.UseEventDrivenTime = false;

        RQHypothesisIntegration.Initialize(graph);

        // Act - Run several physics steps
        var diagnostics = new List<string>();
        double dt = 0.01;

        for (int step = 0; step < 200; step++)
        {
            RQHypothesisIntegration.StepPhysics(graph, step, dt, diagnostics);
        }

        Console.WriteLine($"Completed 200 physics steps");
        Console.WriteLine($"Diagnostics entries: {diagnostics.Count}");

        if (diagnostics.Count > 0)
        {
            Console.WriteLine("Sample diagnostics:");
            for (int i = 0; i < Math.Min(5, diagnostics.Count); i++)
            {
                Console.WriteLine($"  {diagnostics[i]}");
            }
        }

        // Assert - should complete without throwing
        Assert.IsTrue(true, "Physics steps completed without error");
    }
}
