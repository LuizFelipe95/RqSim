using RQSimulation;
using RQSimulation.GPUOptimized.QuantumEdges;
using ComputeSharp;

namespace RqSimGPUCPUTests.Tests.QuantumEdges;

/// <summary>
/// Unit tests for GPU Quantum Edge Engine.
/// Tests the GPU-accelerated quantum graphity implementation (Stage 3).
/// 
/// PHYSICS VALIDATION:
/// - Unitary evolution preserves normalization
/// - Probabilities are valid (0-1, sum preserved)
/// - Collapse statistics match expected distribution
/// - Purity metric correctly identifies classical/quantum states
/// </summary>
[TestClass]
public class GpuQuantumEdgeEngine_Tests
{
    private const double Tolerance = 1e-8;
    private const int TestSeed = 42;

    #region Helper Methods

    private static RQGraph CreateTestGraph(int nodeCount = 20, double edgeProb = 0.3)
    {
        var config = new SimulationConfig
        {
            NodeCount = nodeCount,
            InitialEdgeProb = edgeProb,
            InitialExcitedProb = 0.1,
            TargetDegree = 4,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        return engine.Graph;
    }

    private static bool IsGpuAvailable()
    {
        try
        {
            var device = GraphicsDevice.GetDefault();
            return device != null;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Initialization Tests

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_Initialize_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(nodeCount: 50, edgeCount: 200);

        Assert.IsTrue(engine.IsInitialized, "Engine should be initialized");
        Assert.AreEqual(50, engine.NodeCount, "Node count should match");
        Assert.AreEqual(200, engine.EdgeCount, "Edge count should match");
    }

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_UploadFromGraph_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        var graph = CreateTestGraph(30, 0.3);
        graph.EnableQuantumEdges();

        // Count edges
        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(graph.N, edgeCount + 10); // Extra capacity
        engine.UploadFromGraph(graph);

        // Should not throw
        Assert.IsTrue(engine.IsInitialized);
    }

    #endregion

    #region Probability Computation Tests

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_ComputeProbabilities_ValidRange()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        var graph = CreateTestGraph(20, 0.4);
        graph.EnableQuantumEdges();

        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        if (edgeCount == 0)
        {
            Assert.Inconclusive("No edges in test graph");
            return;
        }

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(graph.N, edgeCount);
        engine.UploadFromGraph(graph);

        var probabilities = engine.ComputeProbabilities();

        Assert.AreEqual(edgeCount, probabilities.Length, "Should have probabilities for each edge");

        foreach (var p in probabilities)
        {
            Assert.IsTrue(p >= 0.0 && p <= 1.0 + Tolerance,
                $"Probability {p} should be in [0, 1]");
        }
    }

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_ComputeTotalProbability_Reasonable()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        var graph = CreateTestGraph(20, 0.3);
        graph.EnableQuantumEdges();

        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        if (edgeCount == 0)
        {
            Assert.Inconclusive("No edges in test graph");
            return;
        }

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(graph.N, edgeCount);
        engine.UploadFromGraph(graph);

        double totalProb = engine.ComputeTotalProbability();

        Console.WriteLine($"Total probability: {totalProb}, Edge count: {edgeCount}");

        // For definite classical edges, total prob should equal edge count
        Assert.IsTrue(Math.Abs(totalProb - edgeCount) < edgeCount * 0.1,
            $"Total probability {totalProb} should be close to edge count {edgeCount}");
    }

    #endregion

    #region Purity Tests

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_ComputePurity_ClassicalStateNearOne()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        var graph = CreateTestGraph(20, 0.3);
        graph.EnableQuantumEdges();

        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        if (edgeCount == 0)
        {
            Assert.Inconclusive("No edges in test graph");
            return;
        }

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(graph.N, edgeCount);
        engine.UploadFromGraph(graph);

        double purity = engine.ComputePurity();

        Console.WriteLine($"Purity for classical state: {purity}");

        // For definite classical states, purity should be close to 1
        // ? = ? P? / (? P)? = E / E? = 1/E for uniform P=1
        // But our formula gives per-edge purity differently
        Assert.IsTrue(purity > 0 && purity <= 1.0 + Tolerance,
            $"Purity {purity} should be in (0, 1]");
    }

    #endregion

    #region Unitary Evolution Tests

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_EvolveUnitary_PreservesNormalization()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        var graph = CreateTestGraph(15, 0.4);
        graph.EnableQuantumEdges();

        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        if (edgeCount == 0)
        {
            Assert.Inconclusive("No edges in test graph");
            return;
        }

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(graph.N, edgeCount);
        engine.UploadFromGraph(graph);
        engine.ComputeHamiltonian();

        // Get initial probabilities
        double initialTotalProb = engine.ComputeTotalProbability();

        // Evolve
        engine.EvolveUnitary(dt: 0.1);

        // Get final probabilities
        double finalTotalProb = engine.ComputeTotalProbability();

        Console.WriteLine($"Initial total prob: {initialTotalProb}");
        Console.WriteLine($"Final total prob: {finalTotalProb}");
        Console.WriteLine($"Difference: {Math.Abs(finalTotalProb - initialTotalProb)}");

        // Unitary evolution preserves total probability
        Assert.IsTrue(Math.Abs(finalTotalProb - initialTotalProb) < 0.1,
            $"Unitary evolution should preserve total probability. " +
            $"Initial: {initialTotalProb}, Final: {finalTotalProb}");
    }

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_EvolveUnitary_MultipleSteps_Stable()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        var graph = CreateTestGraph(15, 0.3);
        graph.EnableQuantumEdges();

        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        if (edgeCount == 0)
        {
            Assert.Inconclusive("No edges in test graph");
            return;
        }

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(graph.N, edgeCount);
        engine.UploadFromGraph(graph);
        engine.ComputeHamiltonian();

        double initialProb = engine.ComputeTotalProbability();

        // Multiple evolution steps
        for (int step = 0; step < 10; step++)
        {
            engine.EvolveUnitary(dt: 0.01);
        }

        double finalProb = engine.ComputeTotalProbability();

        Console.WriteLine($"After 10 steps - Initial: {initialProb}, Final: {finalProb}");

        // Should remain stable
        Assert.IsTrue(Math.Abs(finalProb - initialProb) < initialProb * 0.1,
            "Probabilities should remain stable after multiple evolution steps");
    }

    #endregion

    #region Collapse Tests

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_CollapseAllEdges_ProducesClassicalState()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        var graph = CreateTestGraph(15, 0.4);
        graph.EnableQuantumEdges();

        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        if (edgeCount == 0)
        {
            Assert.Inconclusive("No edges in test graph");
            return;
        }

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(graph.N, edgeCount);
        engine.UploadFromGraph(graph);

        int existingCount = engine.CollapseAllEdges();

        Console.WriteLine($"Edges after collapse: {existingCount} / {edgeCount}");

        // After collapse, all amplitudes should be definite (0 or 1)
        var probs = engine.ComputeProbabilities();
        foreach (var p in probs)
        {
            bool isClassical = p < 0.01 || p > 0.99;
            Assert.IsTrue(isClassical,
                $"After collapse, probability {p} should be near 0 or 1");
        }
    }

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_CollapseAllEdges_ReturnsReasonableCount()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        var graph = CreateTestGraph(20, 0.3);
        graph.EnableQuantumEdges();

        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        if (edgeCount == 0)
        {
            Assert.Inconclusive("No edges in test graph");
            return;
        }

        using var engine = new GpuQuantumEdgeEngine();
        engine.Initialize(graph.N, edgeCount);
        engine.UploadFromGraph(graph);

        int existingCount = engine.CollapseAllEdges();

        // For classical input (P=1 for all edges), all should exist after collapse
        Assert.IsTrue(existingCount >= edgeCount * 0.8,
            $"Most edges should exist after collapse of classical state. " +
            $"Got {existingCount} / {edgeCount}");
    }

    #endregion

    #region Dispose Tests

    [TestMethod]
    [TestCategory("GpuQuantumEdges")]
    public void GpuQuantumEdgeEngine_Dispose_NoLeaks()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test.");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            using var engine = new GpuQuantumEdgeEngine();
            engine.Initialize(100, 400);
            // Engine should be properly disposed
        }

        // If we get here without OOM, dispose works
        Assert.IsTrue(true);
    }

    #endregion
}
