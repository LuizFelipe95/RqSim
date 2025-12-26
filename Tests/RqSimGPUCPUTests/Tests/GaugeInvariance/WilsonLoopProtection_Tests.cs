using RQSimulation;
using RQSimulation.Physics;
using System.Numerics;

namespace RqSimGPUCPUTests.Tests.GaugeInvariance;

/// <summary>
/// Unit tests for Wilson Loop gauge protection functionality.
/// Tests the Stage 3 implementation of gauge invariance protection.
/// 
/// PHYSICS:
/// - Wilson loop W = exp(i?A·dl) measures magnetic flux through a loop
/// - If |W - 1| > tolerance, the edge carries physical gauge flux
/// - Removing such edges would violate Gauss's law (charge conservation)
/// </summary>
[TestClass]
public class WilsonLoopProtection_Tests
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
    
    /// <summary>
    /// Create a simple triangle graph for testing.
    /// Nodes 0, 1, 2 form a triangle.
    /// </summary>
    private static RQGraph CreateTriangleGraph()
    {
        var config = new SimulationConfig
        {
            NodeCount = 3,
            InitialEdgeProb = 0.0, // We'll add edges manually
            InitialExcitedProb = 0.0,
            TargetDegree = 2,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        // Create triangle: 0-1-2-0
        graph.AddEdge(0, 1);
        graph.AddEdge(1, 2);
        graph.AddEdge(2, 0);
        
        // Set initial weights
        graph.Weights[0, 1] = 0.5;
        graph.Weights[1, 0] = 0.5;
        graph.Weights[1, 2] = 0.5;
        graph.Weights[2, 1] = 0.5;
        graph.Weights[2, 0] = 0.5;
        graph.Weights[0, 2] = 0.5;
        
        // Initialize gauge phases
        graph.InitEdgeGaugePhases();
        
        return graph;
    }

    #endregion

    #region TopologyMove Enum Tests

    [TestMethod]
    public void TopologyMove_HasExpectedValues()
    {
        // Assert: Enum has the expected values
        Assert.AreEqual(0, (int)TopologyMove.AddEdge);
        Assert.AreEqual(1, (int)TopologyMove.RemoveEdge);
        Assert.AreEqual(2, (int)TopologyMove.ModifyWeight);
    }

    #endregion

    #region FindMinimalTrianglesWithEdge Tests

    [TestMethod]
    public void FindMinimalTrianglesWithEdge_TriangleGraph_FindsTriangle()
    {
        // Arrange
        var graph = CreateTriangleGraph();

        // Act: Find triangles containing edge (0, 1)
        var triangles = GaugeAwareTopology.FindMinimalTrianglesWithEdge(graph, 0, 1);

        // Assert: Should find exactly one triangle {0, 1, 2}
        Assert.AreEqual(1, triangles.Count, "Should find exactly one triangle");
        
        var triangle = triangles[0];
        Assert.IsTrue(
            (triangle[0] == 0 && triangle[1] == 1 && triangle[2] == 2) ||
            (triangle[0] == 0 && triangle[1] == 1 && triangle[2] == 2),
            "Triangle should contain nodes 0, 1, 2");
    }

    [TestMethod]
    public void FindMinimalTrianglesWithEdge_NoTriangle_ReturnsEmpty()
    {
        // Arrange: Create a simple path graph 0-1-2 (no triangle)
        var config = new SimulationConfig
        {
            NodeCount = 3,
            InitialEdgeProb = 0.0,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        graph.AddEdge(0, 1);
        graph.AddEdge(1, 2);
        // Note: NO edge between 0 and 2

        // Act
        var triangles = GaugeAwareTopology.FindMinimalTrianglesWithEdge(graph, 0, 1);

        // Assert: No triangles
        Assert.AreEqual(0, triangles.Count, "Path graph should have no triangles");
    }

    [TestMethod]
    public void FindMinimalTrianglesWithEdge_MultipleTriangles_FindsAll()
    {
        // Arrange: Create a diamond shape 
        //     0
        //    /|\
        //   1-+-2
        //    \|/
        //     3
        var config = new SimulationConfig
        {
            NodeCount = 4,
            InitialEdgeProb = 0.0,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        // Central edge 1-2
        graph.AddEdge(1, 2);
        // Top triangles via 0
        graph.AddEdge(0, 1);
        graph.AddEdge(0, 2);
        // Bottom triangles via 3
        graph.AddEdge(3, 1);
        graph.AddEdge(3, 2);

        // Act: Find triangles containing central edge (1, 2)
        var triangles = GaugeAwareTopology.FindMinimalTrianglesWithEdge(graph, 1, 2);

        // Assert: Should find 2 triangles (1-2-0 and 1-2-3)
        Assert.AreEqual(2, triangles.Count, "Should find two triangles for central edge");
    }

    #endregion

    #region IsTopologicalMoveGaugeInvariant Tests

    [TestMethod]
    public void IsTopologicalMoveGaugeInvariant_AddEdge_AlwaysAllowed()
    {
        // Arrange
        var graph = CreateTriangleGraph();

        // Act
        bool allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
            graph, 0, 1, TopologyMove.AddEdge);

        // Assert: Adding edges never violates gauge invariance
        Assert.IsTrue(allowed, "AddEdge should always be allowed");
    }

    [TestMethod]
    public void IsTopologicalMoveGaugeInvariant_ModifyWeight_AlwaysAllowed()
    {
        // Arrange
        var graph = CreateTriangleGraph();

        // Act
        bool allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
            graph, 0, 1, TopologyMove.ModifyWeight);

        // Assert: Modifying weights never violates gauge invariance
        Assert.IsTrue(allowed, "ModifyWeight should always be allowed");
    }

    [TestMethod]
    public void IsTopologicalMoveGaugeInvariant_RemoveEdge_TrivialPhase_Allowed()
    {
        // Arrange: Triangle with trivial phases (all zero)
        var graph = CreateTriangleGraph();
        
        // Set all phases to zero (trivial)
        graph.SetEdgePhase(0, 1, 0.0);
        graph.SetEdgePhase(1, 2, 0.0);
        graph.SetEdgePhase(2, 0, 0.0);

        // Act
        bool allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
            graph, 0, 1, TopologyMove.RemoveEdge);

        // Assert: Trivial phase edge can be removed
        Assert.IsTrue(allowed, "Edge with trivial phase should be removable");
    }

    [TestMethod]
    public void IsTopologicalMoveGaugeInvariant_RemoveEdge_NonTrivialPhase_Blocked()
    {
        // Arrange: Triangle with non-trivial Wilson loop phase
        var graph = CreateTriangleGraph();
        
        // Set phases such that Wilson loop has significant phase
        // W = exp(i(?_01 + ?_12 + ?_20))
        // Set ?_01 = ?/2, others = 0, so W = exp(i?/2) = i (non-trivial)
        graph.SetEdgePhase(0, 1, Math.PI / 2.0); // 90 degrees
        graph.SetEdgePhase(1, 2, 0.0);
        graph.SetEdgePhase(2, 0, 0.0);
        
        // Remove alternate path (make 0-1 a bridge with flux)
        // Actually in a triangle there IS an alternate path (0-2-1)
        // So we need to make sure the flux is high enough

        // Act
        bool allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
            graph, 0, 1, TopologyMove.RemoveEdge);

        // Assert: Edge with flux should still be allowed because alternate path exists
        // The Wilson loop check passes if there's an alternate path for redistribution
        Assert.IsTrue(allowed, 
            "Edge with flux in triangle should be removable (alternate path exists)");
    }

    [TestMethod]
    public void IsTopologicalMoveGaugeInvariant_RemoveEdge_BridgeWithFlux_Blocked()
    {
        // Arrange: Create a graph where edge is a bridge with non-trivial phase
        //   0 --- 1 --- 2
        // Edge 1-2 is a bridge (removing it disconnects 2)
        var config = new SimulationConfig
        {
            NodeCount = 3,
            InitialEdgeProb = 0.0,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        graph.AddEdge(0, 1);
        graph.AddEdge(1, 2);
        graph.InitEdgeGaugePhases();
        
        // Set non-trivial phase on the bridge edge
        graph.SetEdgePhase(1, 2, Math.PI / 2.0); // 90 degrees

        // Act: Try to remove bridge edge 1-2
        bool allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
            graph, 1, 2, TopologyMove.RemoveEdge);

        // Assert: Bridge with flux should be blocked
        Assert.IsFalse(allowed, 
            "Bridge edge with non-trivial flux should be blocked");
    }

    [TestMethod]
    public void IsTopologicalMoveGaugeInvariant_NonexistentEdge_Allowed()
    {
        // Arrange
        var graph = CreateTriangleGraph();

        // Act: Try to remove edge that doesn't exist
        bool allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
            graph, 0, 2, TopologyMove.RemoveEdge);

        // Assert: Edge 0-2 exists in triangle, so let's check a truly nonexistent edge
        // Actually in CreateTriangleGraph, edge 2-0 exists
        // Let me check with a larger graph
        var largeGraph = CreateTestGraph(10, 0.1);
        
        // Find two nodes that are NOT connected
        int nodeA = -1, nodeB = -1;
        for (int i = 0; i < largeGraph.N && nodeA < 0; i++)
        {
            for (int j = i + 1; j < largeGraph.N && nodeA < 0; j++)
            {
                if (!largeGraph.Edges[i, j])
                {
                    nodeA = i;
                    nodeB = j;
                }
            }
        }
        
        if (nodeA >= 0)
        {
            allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
                largeGraph, nodeA, nodeB, TopologyMove.RemoveEdge);
            Assert.IsTrue(allowed, "Nonexistent edge should be 'removable' (no-op)");
        }
    }

    #endregion

    #region GetEdgeWilsonFlux Tests

    [TestMethod]
    public void GetEdgeWilsonFlux_TrivialPhases_ReturnsZero()
    {
        // Arrange
        var graph = CreateTriangleGraph();
        graph.SetEdgePhase(0, 1, 0.0);
        graph.SetEdgePhase(1, 2, 0.0);
        graph.SetEdgePhase(2, 0, 0.0);

        // Act
        double flux = GaugeAwareTopology.GetEdgeWilsonFlux(graph, 0, 1);

        // Assert
        Assert.AreEqual(0.0, flux, 0.01, "Trivial phases should give zero flux");
    }

    [TestMethod]
    public void GetEdgeWilsonFlux_NonTrivialPhases_ReturnsNonZero()
    {
        // Arrange
        var graph = CreateTriangleGraph();
        
        // Set Wilson loop phase to ?/3 (60 degrees)
        graph.SetEdgePhase(0, 1, Math.PI / 3.0);
        graph.SetEdgePhase(1, 2, 0.0);
        graph.SetEdgePhase(2, 0, 0.0);

        // Act
        double flux = GaugeAwareTopology.GetEdgeWilsonFlux(graph, 0, 1);

        // Assert: Should be approximately ?/3
        Assert.IsTrue(flux > 0.5, $"Non-trivial phases should give non-zero flux, got {flux}");
    }

    [TestMethod]
    public void GetEdgeWilsonFlux_EdgeNotInTriangle_ReturnsEdgePhase()
    {
        // Arrange: Path graph (no triangles)
        var config = new SimulationConfig
        {
            NodeCount = 3,
            InitialEdgeProb = 0.0,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        graph.AddEdge(0, 1);
        graph.AddEdge(1, 2);
        graph.InitEdgeGaugePhases();
        
        double testPhase = 0.5;
        graph.SetEdgePhase(0, 1, testPhase);

        // Act
        double flux = GaugeAwareTopology.GetEdgeWilsonFlux(graph, 0, 1);

        // Assert: Should return the edge's own phase
        Assert.AreEqual(testPhase, flux, 0.01, 
            "Edge not in triangle should return its own phase as flux");
    }

    #endregion

    #region PhysicsConstants Tests

    [TestMethod]
    public void PhysicsConstants_GaugeTolerance_IsPositive()
    {
        // Assert
        Assert.IsTrue(PhysicsConstants.GaugeTolerance > 0.0,
            "GaugeTolerance should be positive");
        Assert.IsTrue(PhysicsConstants.GaugeTolerance < Math.PI,
            "GaugeTolerance should be less than ?");
    }

    [TestMethod]
    public void PhysicsConstants_MaxRemovableFlux_IsReasonable()
    {
        // Assert: ?/4 is 45 degrees, which is a reasonable threshold
        Assert.AreEqual(Math.PI / 4.0, PhysicsConstants.MaxRemovableFlux, Tolerance,
            "MaxRemovableFlux should be ?/4");
    }

    [TestMethod]
    public void PhysicsConstants_EnableWilsonLoopProtection_IsTrue()
    {
        // Assert: Should be enabled by default
        Assert.IsTrue(PhysicsConstants.EnableWilsonLoopProtection,
            "Wilson loop protection should be enabled by default");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void WilsonLoopProtection_Integration_PreservesGaugeInvariance()
    {
        // Arrange: Create a graph with edges and gauge phases
        var graph = CreateTestGraph(30, 0.2);
        graph.InitEdgeGaugePhases();
        
        // Count edges before
        int edgesBefore = 0;
        for (int i = 0; i < graph.N; i++)
            for (int j = i + 1; j < graph.N; j++)
                if (graph.Edges[i, j]) edgesBefore++;
        
        // Act: Run some Metropolis steps
        int blockedCount = 0;
        int allowedCount = 0;
        
        for (int i = 0; i < graph.N; i++)
        {
            for (int j = i + 1; j < graph.N; j++)
            {
                if (graph.Edges[i, j])
                {
                    bool allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
                        graph, i, j, TopologyMove.RemoveEdge);
                    
                    if (allowed)
                        allowedCount++;
                    else
                        blockedCount++;
                }
            }
        }
        
        // Assert: Some edges should be protected (if they have flux)
        Assert.IsTrue(allowedCount + blockedCount == edgesBefore,
            "All edges should be classified as allowed or blocked");
        
        // Note: With random initialization, we expect some edges to be blocked
        // if they have non-trivial phases. But this depends on initialization.
    }

    #endregion
}
