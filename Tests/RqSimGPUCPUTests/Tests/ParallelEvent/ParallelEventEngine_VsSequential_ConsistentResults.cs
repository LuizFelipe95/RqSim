using RQSimulation;
using RQSimulation.EventBasedModel;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: Parallel vs Sequential event processing produces similar results.
    /// </summary>
    [TestMethod]
    [TestCategory("ParallelEvent")]
    public void ParallelEventEngine_VsSequential_ConsistentResults()
    {
        // Arrange
        var graphSeq = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        var graphPar = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);

        Console.WriteLine($"Graph: N={graphSeq.N}");

        double dt = 0.01;
        int sweeps = 20;

        // Act - Sequential
        graphSeq.InitAsynchronousTime();
        for (int s = 0; s < sweeps; s++)
        {
            graphSeq.StepEventBasedBatch(graphSeq.N);
        }

        // Act - Parallel
        graphPar.InitAsynchronousTime();
        using var parallelEngine = new ParallelEventEngine(graphPar, workerCount: 2);
        parallelEngine.ComputeGraphColoring();

        for (int s = 0; s < sweeps; s++)
        {
            parallelEngine.ProcessParallelSweep(dt);
        }

        // Compare final states
        int excitedSeq = graphSeq.State.Count(s => s == NodeState.Excited);
        int excitedPar = graphPar.State.Count(s => s == NodeState.Excited);

        Console.WriteLine($"Sequential excited: {excitedSeq}");
        Console.WriteLine($"Parallel excited:   {excitedPar}");
        Console.WriteLine($"Parallel stats: {parallelEngine.GetStatsSummary()}");

        // Assert - results should be in similar range
        // Exact match not expected due to non-deterministic parallel execution
        int diff = Math.Abs(excitedSeq - excitedPar);
        Assert.IsTrue(diff < SmallGraphNodes / 2, $"Excited counts too different: seq={excitedSeq}, par={excitedPar}");
    }
}
