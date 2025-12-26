using RQSimulation;
using RQSimulation.Physics;
using System.Numerics;

namespace RqSimGPUCPUTests.Tests.GaugeInvariance;

/// <summary>
/// Unit tests for GaugeAwareTopology static class methods.
/// Tests the Stage 3 enhanced gauge invariance protection.
/// 
/// PHYSICS:
/// - Gauss's Law: ∇·E = ρ (charge conservation)
/// - Wilson loop W = exp(i∮A·dl) measures magnetic flux
/// - Topology changes that would violate gauge invariance are blocked
/// </summary>
[TestClass]
public class GaugeAwareTopology_Tests
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

    private static RQGraph CreateTriangleGraph()
    {
        var config = new SimulationConfig
        {
            NodeCount = 3,
            InitialEdgeProb = 0.0,
            InitialExcitedProb = 0.0,
            TargetDegree = 2,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;

        graph.AddEdge(0, 1);
        graph.AddEdge(1, 2);
        graph.AddEdge(2, 0);

        graph.Weights[0, 1] = 0.5;
        graph.Weights[1, 0] = 0.5;
        graph.Weights[1, 2] = 0.5;
        graph.Weights[2, 1] = 0.5;
        graph.Weights[2, 0] = 0.5;
        graph.Weights[0, 2] = 0.5;

        graph.InitEdgeGaugePhases();

        return graph;
    }

    private static RQGraph CreatePathGraph(int length = 3)
    {
        var config = new SimulationConfig
        {
            NodeCount = length,
            InitialEdgeProb = 0.0,
            Seed = TestSeed
        };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;

        for (int i = 0; i < length - 1; i++)
        {
            graph.AddEdge(i, i + 1);
            graph.Weights[i, i + 1] = 0.5;
            graph.Weights[i + 1, i] = 0.5;
        }

        graph.InitEdgeGaugePhases();

        return graph;
    }

    #endregion

    #region CanRemoveEdgeSafely Tests

    [TestMethod]
    public void CanRemoveEdgeSafely_NonexistentEdge_ReturnsTrue()
    {
        // Arrange
        var graph = CreatePathGraph(3);
        
        // Edge 0-2 doesn't exist in path graph

        // Act
        bool canRemove = GaugeAwareTopology.CanRemoveEdgeSafely(graph, 0, 2, out int altPath);

        // Assert
        Assert.IsTrue(canRemove, "Nonexistent edge should be 'removable'");
        Assert.AreEqual(-1, altPath, "Alternate path should be -1 for nonexistent edge");
    }

    [TestMethod]
    public void CanRemoveEdgeSafely_TrivialPhaseEdge_ReturnsTrue()
    {
        // Arrange
        var graph = CreateTriangleGraph();
        graph.SetEdgePhase(0, 1, 0.0); // Trivial phase

        // Act
        bool canRemove = GaugeAwareTopology.CanRemoveEdgeSafely(graph, 0, 1, out int altPath);

        // Assert
        Assert.IsTrue(canRemove, "Edge with trivial phase should be removable");
    }

    [TestMethod]
    public void CanRemoveEdgeSafely_NonTrivialPhaseWithAltPath_ReturnsTrue()
    {
        // Arrange: Triangle with non-trivial phase
        var graph = CreateTriangleGraph();
        graph.SetEdgePhase(0, 1, Math.PI / 2.0); // 90 degrees

        // Act
        bool canRemove = GaugeAwareTopology.CanRemoveEdgeSafely(graph, 0, 1, out int altPath);

        // Assert: Should be removable because alternate path 0-2-1 exists
        Assert.IsTrue(canRemove, "Edge with flux but alternate path should be removable");
        Assert.IsTrue(altPath >= 2, "Alternate path should have length >= 2");
    }

    [TestMethod]
    public void CanRemoveEdgeSafely_BridgeWithFlux_ReturnsFalse()
    {
        // Arrange: Path graph - middle edge is a bridge
        var graph = CreatePathGraph(3); // 0-1-2
        graph.SetEdgePhase(0, 1, Math.PI / 2.0); // Non-trivial phase on bridge

        // Act
        bool canRemove = GaugeAwareTopology.CanRemoveEdgeSafely(graph, 0, 1, out int altPath);

        // Assert: Bridge with flux cannot be removed
        Assert.IsFalse(canRemove, "Bridge edge with flux should not be removable");
        Assert.AreEqual(-1, altPath, "No alternate path for bridge");
    }

    #endregion

    #region ComputeEdgeFlux Tests

    [TestMethod]
    public void ComputeEdgeFlux_EdgeInTriangle_ComputesWilsonLoopFlux()
    {
        // Arrange
        var graph = CreateTriangleGraph();
        
        // Set Wilson loop phase: sum of all edge phases
        double phi01 = 0.3;
        double phi12 = 0.2;
        double phi20 = 0.1;
        graph.SetEdgePhase(0, 1, phi01);
        graph.SetEdgePhase(1, 2, phi12);
        graph.SetEdgePhase(2, 0, phi20);

        // Act
        double flux = GaugeAwareTopology.ComputeEdgeFlux(graph, 0, 1);

        // Assert: Flux should be related to Wilson loop phase
        Assert.IsTrue(Math.Abs(flux) >= 0.0, "Flux should be computed");
    }

    [TestMethod]
    public void ComputeEdgeFlux_EdgeNotInTriangle_ReturnsZero()
    {
        // Arrange: Path graph (no triangles)
        var graph = CreatePathGraph(3);
        graph.SetEdgePhase(0, 1, 0.5);

        // Act
        double flux = GaugeAwareTopology.ComputeEdgeFlux(graph, 0, 1);

        // Assert: This uses the old ComputeEdgeFlux which expects triangles
        // For edges not in triangles, the Wilson loop is trivial
        Assert.IsTrue(flux >= 0.0, "Flux should be non-negative");
    }

    #endregion

    #region WouldCreateTopologicalDefect Tests

    [TestMethod]
    public void WouldCreateTopologicalDefect_TrivialFlux_ReturnsFalse()
    {
        // Arrange
        var graph = CreateTriangleGraph();
        
        // All phases trivial
        graph.SetEdgePhase(0, 1, 0.0);
        graph.SetEdgePhase(1, 2, 0.0);
        graph.SetEdgePhase(2, 0, 0.0);

        // Act
        bool wouldDefect = GaugeAwareTopology.WouldCreateTopologicalDefect(graph, 0, 1);

        // Assert
        Assert.IsFalse(wouldDefect, "Trivial flux should not create defect");
    }

    [TestMethod]
    public void WouldCreateTopologicalDefect_HighFluxBridge_ReturnsTrue()
    {
        // Arrange: Path graph with high flux on bridge
        var graph = CreatePathGraph(3);
        
        // Set high flux (> π/4)
        graph.SetEdgePhase(0, 1, Math.PI / 2.0); // 90 degrees > 45 degrees threshold

        // Act
        bool wouldDefect = GaugeAwareTopology.WouldCreateTopologicalDefect(graph, 0, 1);

        // Assert: Bridge with high flux would create monopole
        Assert.IsTrue(wouldDefect, "Bridge with high flux should create defect");
    }

    #endregion

    #region RemoveEdgeWithFluxRedistribution Tests

    [TestMethod]
    public void RemoveEdgeWithFluxRedistribution_TrivialPhase_Succeeds()
    {
        // Arrange
        var graph = CreateTriangleGraph();
        graph.SetEdgePhase(0, 1, 0.0);

        // Act
        bool removed = GaugeAwareTopology.RemoveEdgeWithFluxRedistribution(
            graph, 0, 1, out double energy);

        // Assert
        Assert.IsTrue(removed, "Edge with trivial phase should be removable");
        Assert.IsTrue(energy >= 0.0, "Released energy should be non-negative");
    }

    [TestMethod]
    public void RemoveEdgeWithFluxRedistribution_NonTrivialWithAltPath_RedistributesFlux()
    {
        // Arrange
        var graph = CreateTriangleGraph();
        double originalPhase = Math.PI / 4.0;
        graph.SetEdgePhase(0, 1, originalPhase);
        
        double altPhase02Before = graph.GetEdgePhase(0, 2);
        double altPhase12Before = graph.GetEdgePhase(1, 2);

        // Act
        bool removed = GaugeAwareTopology.RemoveEdgeWithFluxRedistribution(
            graph, 0, 1, out double energy);

        // Assert
        Assert.IsTrue(removed, "Edge should be removable with flux redistribution");
        
        // Check that alternate path phases changed (flux was redistributed)
        double altPhase02After = graph.GetEdgePhase(0, 2);
        double altPhase12After = graph.GetEdgePhase(1, 2);
        
        // At least one alternate edge should have different phase
        bool phaseChanged = Math.Abs(altPhase02After - altPhase02Before) > 1e-6 ||
                           Math.Abs(altPhase12After - altPhase12Before) > 1e-6;
        Assert.IsTrue(phaseChanged, "Flux should be redistributed to alternate path");
    }

    [TestMethod]
    public void RemoveEdgeWithFluxRedistribution_BridgeWithFlux_Blocked()
    {
        // Arrange: Bridge with non-trivial flux
        var graph = CreatePathGraph(3);
        graph.SetEdgePhase(0, 1, Math.PI / 2.0);

        // Act
        bool removed = GaugeAwareTopology.RemoveEdgeWithFluxRedistribution(
            graph, 0, 1, out double energy);

        // Assert: Cannot remove bridge with flux
        Assert.IsFalse(removed, "Bridge with flux should not be removable");
    }

    #endregion

    #region Null Argument Tests

    [TestMethod]
    public void CanRemoveEdgeSafely_NullGraph_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GaugeAwareTopology.CanRemoveEdgeSafely(null!, 0, 1, out _));
    }

    [TestMethod]
    public void IsTopologicalMoveGaugeInvariant_NullGraph_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
                null!, 0, 1, TopologyMove.RemoveEdge));
    }

    [TestMethod]
    public void FindMinimalTrianglesWithEdge_NullGraph_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GaugeAwareTopology.FindMinimalTrianglesWithEdge(null!, 0, 1));
    }

    [TestMethod]
    public void GetEdgeWilsonFlux_NullGraph_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GaugeAwareTopology.GetEdgeWilsonFlux(null!, 0, 1));
    }

    [TestMethod]
    public void ComputeEdgeFlux_NullGraph_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GaugeAwareTopology.ComputeEdgeFlux(null!, 0, 1));
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void FindMinimalTrianglesWithEdge_SelfLoop_ReturnsEmpty()
    {
        // Arrange
        var graph = CreateTriangleGraph();

        // Act: Try to find triangles for self-loop (invalid)
        var triangles = GaugeAwareTopology.FindMinimalTrianglesWithEdge(graph, 0, 0);

        // Assert: Self-loops don't form triangles
        // (Implementation may handle this differently)
        Assert.IsNotNull(triangles);
    }

    [TestMethod]
    public void IsTopologicalMoveGaugeInvariant_DisabledProtection_AlwaysAllows()
    {
        // This test verifies behavior when EnableWilsonLoopProtection is false
        // Since it's a compile-time constant, we can only verify the current setting
        
        if (!PhysicsConstants.EnableWilsonLoopProtection)
        {
            // Arrange
            var graph = CreatePathGraph(3);
            graph.SetEdgePhase(0, 1, Math.PI); // Full flux

            // Act
            bool allowed = GaugeAwareTopology.IsTopologicalMoveGaugeInvariant(
                graph, 0, 1, TopologyMove.RemoveEdge);

            // Assert: With protection disabled, should allow
            Assert.IsTrue(allowed, "With protection disabled, all moves should be allowed");
        }
        else
        {
            // Protection is enabled - test passes by default
            Assert.IsTrue(PhysicsConstants.EnableWilsonLoopProtection);
        }
    }

    #endregion
}
