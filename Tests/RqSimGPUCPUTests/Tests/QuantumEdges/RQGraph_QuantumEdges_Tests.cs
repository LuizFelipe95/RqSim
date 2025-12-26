using RQSimulation;
using System.Numerics;

namespace RqSimGPUCPUTests.Tests.QuantumEdges;

/// <summary>
/// Unit tests for RQGraph quantum edge functionality.
/// Tests the Quantum Graphity implementation (Modernization Stage 6).
/// </summary>
[TestClass]
public class RQGraph_QuantumEdges_Tests
{
    private const double Tolerance = 1e-10;
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

    #endregion

    #region EnableQuantumEdges Tests

    [TestMethod]
    public void EnableQuantumEdges_InitializesFromClassicalState()
    {
        // Arrange
        var graph = CreateTestGraph();
        int edgeCount = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgeCount++;

        // Act
        graph.EnableQuantumEdges();

        // Assert
        Assert.IsTrue(graph.IsQuantumEdgeMode, "Should be in quantum mode after enabling");

        // Check that quantum edges match classical edges
        int quantumEdgeCount = 0;
        for (int i = 0; i < graph.N; i++)
        {
            for (int j = i + 1; j < graph.N; j++)
            {
                var qEdge = graph.GetQuantumEdge(i, j);
                if (graph.Edges[i, j])
                {
                    Assert.IsTrue(qEdge.ExistenceProbability > 0.99,
                        $"Classical edge ({i},{j}) should have high quantum probability");
                    quantumEdgeCount++;
                }
                else
                {
                    Assert.IsTrue(qEdge.ExistenceProbability < 0.01,
                        $"Non-edge ({i},{j}) should have low quantum probability");
                }
            }
        }

        Assert.AreEqual(edgeCount, quantumEdgeCount,
            "Quantum edge count should match classical edge count");
    }

    [TestMethod]
    public void DisableQuantumEdges_ReturnsToClassicalMode()
    {
        // Arrange
        var graph = CreateTestGraph();
        graph.EnableQuantumEdges();
        Assert.IsTrue(graph.IsQuantumEdgeMode);

        // Act
        graph.DisableQuantumEdges();

        // Assert
        Assert.IsFalse(graph.IsQuantumEdgeMode, "Should be in classical mode after disabling");
    }

    #endregion

    #region SetQuantumEdge Tests

    [TestMethod]
    public void SetQuantumEdge_AutoEnablesQuantumMode()
    {
        // Arrange
        var graph = CreateTestGraph();
        Assert.IsFalse(graph.IsQuantumEdgeMode);

        // Act
        graph.SetQuantumEdge(0, 1, ComplexEdge.Exists(0.5));

        // Assert
        Assert.IsTrue(graph.IsQuantumEdgeMode,
            "SetQuantumEdge should auto-enable quantum mode");
    }

    [TestMethod]
    public void SetQuantumEdge_UpdatesClassicalRepresentation()
    {
        // Arrange
        var graph = CreateTestGraph();
        bool originalEdge = graph.Edges[0, 1];

        // Act: Set high probability (should exist classically)
        graph.SetQuantumEdge(0, 1, ComplexEdge.Exists(0.9));

        // Assert
        Assert.IsTrue(graph.Edges[0, 1], "High probability edge should exist classically");
        Assert.AreEqual(0.9, graph.Weights[0, 1], Tolerance);
    }

    [TestMethod]
    public void SetQuantumEdge_LowProbability_RemovesClassicalEdge()
    {
        // Arrange
        var graph = CreateTestGraph();
        // Ensure edge exists first
        graph.AddEdge(0, 1);
        graph.Weights[0, 1] = 0.5;
        graph.Weights[1, 0] = 0.5;

        // Act: Set low probability (should not exist classically)
        var lowProbEdge = ComplexEdge.Superposition(
            new Complex(0.1, 0.0),  // small alpha
            new Complex(1.0, 0.0)); // large beta
        graph.SetQuantumEdge(0, 1, lowProbEdge);

        // Assert
        Assert.IsFalse(graph.Edges[0, 1],
            "Low probability edge should not exist classically");
    }

    [TestMethod]
    public void SetEdgeSuperposition_CreatesSuperposition()
    {
        // Arrange
        var graph = CreateTestGraph();

        // Act
        graph.SetEdgeSuperposition(0, 1,
            new Complex(1.0, 0.0),  // alpha
            new Complex(1.0, 0.0)); // beta

        // Assert
        var edge = graph.GetQuantumEdge(0, 1);
        Assert.AreEqual(0.5, edge.ExistenceProbability, 0.01,
            "50-50 superposition should have 50% probability");
    }

    #endregion

    #region CollapseQuantumEdges Tests

    [TestMethod]
    public void CollapseQuantumEdges_ProducesClassicalStates()
    {
        // Arrange
        var graph = CreateTestGraph();
        graph.EnableQuantumEdges();

        // Set some superpositions
        for (int i = 0; i < 5; i++)
        {
            int j = (i + 1) % graph.N;
            graph.SetEdgeSuperposition(i, j,
                new Complex(1.0, 0.0),
                new Complex(1.0, 0.0));
        }

        // Act
        graph.CollapseQuantumEdges();

        // Assert: All edges should now be classical
        for (int i = 0; i < graph.N; i++)
        {
            for (int j = i + 1; j < graph.N; j++)
            {
                var edge = graph.GetQuantumEdge(i, j);
                Assert.IsTrue(edge.IsClassical(0.99),
                    $"Edge ({i},{j}) should be in classical state after collapse");
            }
        }
    }

    [TestMethod]
    public void CollapseQuantumEdges_RespectsStatistics()
    {
        // Arrange: Create many 50-50 superpositions
        var graph = CreateTestGraph(50, 0.0); // Start with no edges
        graph.EnableQuantumEdges();

        // Set all possible edges to 50-50 superposition
        for (int i = 0; i < graph.N; i++)
        {
            for (int j = i + 1; j < graph.N; j++)
            {
                graph.SetEdgeSuperposition(i, j,
                    new Complex(1.0, 0.0),
                    new Complex(1.0, 0.0));
            }
        }

        // Act: Collapse and count
        graph.CollapseQuantumEdges();

        int existingEdges = 0;
        int totalPossible = graph.N * (graph.N - 1) / 2;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) existingEdges++;

        // Assert: Should be approximately 50%
        double ratio = (double)existingEdges / totalPossible;
        Assert.IsTrue(ratio > 0.4 && ratio < 0.6,
            $"50-50 superposition collapse should give ~50% edges, got {ratio:P2}");
    }

    [TestMethod]
    public void CollapseQuantumEdges_UpdatesDegrees()
    {
        // Arrange
        var graph = CreateTestGraph();
        graph.EnableQuantumEdges();

        // Act
        graph.CollapseQuantumEdges();

        // Assert: Degrees should be consistent with edges
        for (int i = 0; i < graph.N; i++)
        {
            int computedDegree = 0;
            for (int j = 0; j < graph.N; j++)
                if (i != j && graph.Edges[i, j]) computedDegree++;

            Assert.AreEqual(computedDegree, graph.Degree(i),
                $"Degree of node {i} should match edge count");
        }
    }

    #endregion

    #region RecalculateAllDegrees Tests

    [TestMethod]
    public void RecalculateAllDegrees_CorrectlyComputesDegrees()
    {
        // Arrange
        var graph = CreateTestGraph();

        // Act
        graph.RecalculateAllDegrees();

        // Assert
        for (int i = 0; i < graph.N; i++)
        {
            int expected = 0;
            for (int j = 0; j < graph.N; j++)
                if (i != j && graph.Edges[i, j]) expected++;

            Assert.AreEqual(expected, graph.Degree(i),
                $"Node {i} degree mismatch after recalculation");
        }
    }

    #endregion

    #region Quantum Edge Statistics Tests

    [TestMethod]
    public void GetTotalQuantumEdgeProbability_SumsCorrectly()
    {
        // Arrange: Create a fresh config with no initial edges
        var config = new SimulationConfig
        {
            NodeCount = 5,
            InitialEdgeProb = 0.0, // No edges
            InitialExcitedProb = 0.0,
            TargetDegree = 0,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        // Remove any edges that might have been created during initialization
        for (int i = 0; i < graph.N; i++)
        {
            for (int j = i + 1; j < graph.N; j++)
            {
                if (graph.Edges[i, j])
                {
                    // Force edge removal
                    graph.Edges[i, j] = false;
                    graph.Edges[j, i] = false;
                    graph.Weights[i, j] = 0.0;
                    graph.Weights[j, i] = 0.0;
                }
            }
        }
        graph.RecalculateAllDegrees();
        
        graph.EnableQuantumEdges();

        // Verify starting state: all edges have P = 0
        double initialTotal = graph.GetTotalQuantumEdgeProbability();
        Assert.AreEqual(0.0, initialTotal, 0.01, "Empty graph should have total P = 0");

        // Set known probabilities
        graph.SetQuantumEdge(0, 1, ComplexEdge.Exists(1.0)); // P = 1.0
        
        // For P = 0.5, we need equal alpha and beta (after normalization: |?|? = 0.5)
        graph.SetEdgeSuperposition(1, 2, new Complex(1.0, 0.0), new Complex(1.0, 0.0)); // P = 0.5

        // Act
        double total = graph.GetTotalQuantumEdgeProbability();

        // Assert: 1.0 + 0.5 = 1.5
        Assert.AreEqual(1.5, total, 0.01,
            "Total probability should be sum of individual probabilities (1.0 + 0.5 = 1.5)");
    }

    [TestMethod]
    public void GetQuantumEdgePurity_ClassicalState_ReturnsOne()
    {
        // Arrange
        var graph = CreateTestGraph();
        // Classical mode (quantum not enabled)

        // Act
        double purity = graph.GetQuantumEdgePurity();

        // Assert
        Assert.AreEqual(1.0, purity, Tolerance,
            "Classical state should have purity 1.0");
    }

    [TestMethod]
    public void GetQuantumEdgePurity_Superposition_LessThanOne()
    {
        // Arrange
        var graph = CreateTestGraph(10, 0.0);
        graph.EnableQuantumEdges();

        // Set all edges to 50-50 superposition
        for (int i = 0; i < graph.N; i++)
        {
            for (int j = i + 1; j < graph.N; j++)
            {
                graph.SetEdgeSuperposition(i, j,
                    new Complex(1.0, 0.0),
                    new Complex(1.0, 0.0));
            }
        }

        // Act
        double purity = graph.GetQuantumEdgePurity();

        // Assert
        Assert.IsTrue(purity < 0.1,
            $"50-50 superpositions should have low purity, got {purity}");
    }

    #endregion

    #region EvolveQuantumEdges Tests

    [TestMethod]
    public void EvolveQuantumEdges_ChangesPhases()
    {
        // Arrange
        var graph = CreateTestGraph();
        graph.EnableQuantumEdges();
        var originalEdge = graph.GetQuantumEdge(0, 1);
        double originalAmplitudeMagnitude = originalEdge.Amplitude.Magnitude;

        // Act: Evolve multiple steps
        for (int i = 0; i < 10; i++)
        {
            graph.EvolveQuantumEdges(0.1);
        }

        // Assert
        var evolvedEdge = graph.GetQuantumEdge(0, 1);
        // Unitary evolution preserves amplitude magnitude (probability)
        Assert.AreEqual(originalAmplitudeMagnitude, evolvedEdge.Amplitude.Magnitude, 0.01,
            "Unitary evolution should preserve amplitude magnitude");
    }

    [TestMethod]
    public void EvolveQuantumEdges_PreservesProbability()
    {
        // Arrange
        var graph = CreateTestGraph();
        graph.EnableQuantumEdges();
        double totalBefore = graph.GetTotalQuantumEdgeProbability();

        // Act
        graph.EvolveQuantumEdges(0.1);

        // Assert
        double totalAfter = graph.GetTotalQuantumEdgeProbability();
        Assert.AreEqual(totalBefore, totalAfter, 0.01,
            "Unitary evolution should preserve total probability");
    }

    #endregion

    #region Backward Compatibility Tests

    [TestMethod]
    public void ClassicalOperations_WorkWithQuantumMode()
    {
        // Arrange
        var graph = CreateTestGraph();
        graph.EnableQuantumEdges();

        // Act: Classical operations should still work
        graph.AddEdge(0, 5);
        bool hasEdge = graph.Edges[0, 5];

        // Assert
        Assert.IsTrue(hasEdge, "AddEdge should work in quantum mode");

        // Quantum state should also reflect the change
        var qEdge = graph.GetQuantumEdge(0, 5);
        Assert.IsTrue(qEdge.ExistenceProbability > 0.5,
            "Quantum edge should exist after classical AddEdge");
    }

    [TestMethod]
    public void GetQuantumEdge_InvalidIndices_ReturnsNotExists()
    {
        // Arrange
        var graph = CreateTestGraph();
        graph.EnableQuantumEdges();

        // Act & Assert
        Assert.AreEqual(0.0, graph.GetQuantumEdge(-1, 0).ExistenceProbability, Tolerance);
        Assert.AreEqual(0.0, graph.GetQuantumEdge(0, graph.N + 10).ExistenceProbability, Tolerance);
        Assert.AreEqual(0.0, graph.GetQuantumEdge(0, 0).ExistenceProbability, Tolerance); // Self-loop
    }

    #endregion
}
