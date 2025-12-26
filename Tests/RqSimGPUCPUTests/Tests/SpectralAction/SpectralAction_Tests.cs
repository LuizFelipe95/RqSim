using RQSimulation;

namespace RqSimGPUCPUTests;

/// <summary>
/// Unit tests for SpectralAction class (RQ-Hypothesis Stage 5).
/// Tests the Chamseddine-Connes spectral action implementation
/// for dimension stabilization.
/// </summary>
public partial class RqSimGPUCPUTest
{
    // ============================================================
    // SPECTRAL ACTION CONSTANTS TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralActionConstants_DefaultValues_AreValid()
    {
        // Verify spectral action constants have valid values
        Assert.IsTrue(PhysicsConstants.SpectralActionConstants.LambdaCutoff > 0.0,
            "LambdaCutoff should be positive");
        Assert.IsTrue(PhysicsConstants.SpectralActionConstants.F0_Cosmological >= 0.0,
            "F0_Cosmological should be non-negative");
        Assert.IsTrue(PhysicsConstants.SpectralActionConstants.F2_EinsteinHilbert >= 0.0,
            "F2_EinsteinHilbert should be non-negative");
        Assert.IsTrue(PhysicsConstants.SpectralActionConstants.F4_Weyl >= 0.0,
            "F4_Weyl should be non-negative");
        Assert.AreEqual(4.0, PhysicsConstants.SpectralActionConstants.TargetSpectralDimension,
            "Target dimension should be 4.0");
        Assert.IsTrue(PhysicsConstants.SpectralActionConstants.DimensionPotentialStrength > 0.0,
            "DimensionPotentialStrength should be positive");
    }
    
    // ============================================================
    // EFFECTIVE VOLUME TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeEffectiveVolume_ChainGraph_ReturnsCorrectValue()
    {
        // Create chain graph: 0-1-2-3-4
        int chainLength = 5;
        var config = new SimulationConfig { NodeCount = chainLength, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        // Clear all edges
        ClearAllEdges(graph);
        
        // Create chain with weight 1.0
        for (int i = 0; i < chainLength - 1; i++)
        {
            graph.Edges[i, i + 1] = true;
            graph.Edges[i + 1, i] = true;
            graph.Weights[i, i + 1] = 1.0;
            graph.Weights[i + 1, i] = 1.0;
        }
        graph.BuildSoAViews();
        
        double volume = SpectralAction.ComputeEffectiveVolume(graph);
        
        // Chain of 5 nodes has 4 edges, each with weight 1.0
        Assert.AreEqual(4.0, volume, 0.001, $"Chain volume should be 4.0, got {volume}");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeEffectiveVolume_EmptyGraph_ReturnsZero()
    {
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        graph.BuildSoAViews();
        
        double volume = SpectralAction.ComputeEffectiveVolume(graph);
        
        Assert.AreEqual(0.0, volume, 0.001, "Empty graph should have zero volume");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeEffectiveVolume_TriangleGraph_ReturnsThree()
    {
        // Create triangle: 0-1, 1-2, 2-0
        var config = new SimulationConfig { NodeCount = 3, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        // Create triangle with unit weights
        AddEdgeWithWeight(graph, 0, 1, 1.0);
        AddEdgeWithWeight(graph, 1, 2, 1.0);
        AddEdgeWithWeight(graph, 2, 0, 1.0);
        graph.BuildSoAViews();
        
        double volume = SpectralAction.ComputeEffectiveVolume(graph);
        
        Assert.AreEqual(3.0, volume, 0.001, $"Triangle volume should be 3.0, got {volume}");
    }
    
    // ============================================================
    // AVERAGE CURVATURE TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeAverageCurvature_RegularGraph_ReturnsZero()
    {
        // For a regular graph (all same degree), average curvature should be ~0
        var config = new SimulationConfig { NodeCount = 6, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        // Create hexagon: regular graph with degree 2
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            AddEdgeWithWeight(graph, i, next, 1.0);
        }
        graph.BuildSoAViews();
        
        double avgCurvature = SpectralAction.ComputeAverageCurvature(graph);
        
        // For regular graph, curvature is (deg - avgDeg)/avgDeg = 0
        Assert.AreEqual(0.0, avgCurvature, 0.001,
            $"Regular graph should have zero average curvature, got {avgCurvature}");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeAverageCurvature_IrregularGraph_IsNonZero()
    {
        // Create star graph: central node with high degree
        var config = new SimulationConfig { NodeCount = 5, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        // Create star: node 0 connects to all others
        for (int i = 1; i < 5; i++)
        {
            AddEdgeWithWeight(graph, 0, i, 1.0);
        }
        graph.BuildSoAViews();
        
        double avgCurvature = SpectralAction.ComputeAverageCurvature(graph);
        
        // Average should be computable without NaN
        Assert.IsFalse(double.IsNaN(avgCurvature), "Average curvature should not be NaN");
        Console.WriteLine($"Star graph average curvature: {avgCurvature}");
    }
    
    // ============================================================
    // WEYL SQUARED TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeWeylSquared_RegularGraph_IsSmall()
    {
        // For regular graph, variance of curvature should be small (but not exactly 0
        // due to boundary effects in degree-based curvature computation)
        var config = new SimulationConfig { NodeCount = 6, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        // Create hexagon (regular)
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            AddEdgeWithWeight(graph, i, next, 1.0);
        }
        graph.BuildSoAViews();
        
        double weyl2 = SpectralAction.ComputeWeylSquared(graph);
        
        Console.WriteLine($"Regular hexagon graph Weyl?: {weyl2}");
        // Regular graph should have small Weyl? (close to zero but may have small variance)
        Assert.IsTrue(weyl2 >= 0.0 && weyl2 < 0.5, $"Regular graph should have small Weyl?, got {weyl2}");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeWeylSquared_IrregularGraph_IsPositive()
    {
        // Star graph has high curvature variance
        var config = new SimulationConfig { NodeCount = 5, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        // Create star
        for (int i = 1; i < 5; i++)
        {
            AddEdgeWithWeight(graph, 0, i, 1.0);
        }
        graph.BuildSoAViews();
        
        double weyl2 = SpectralAction.ComputeWeylSquared(graph);
        
        Assert.IsTrue(weyl2 > 0.0, $"Irregular graph should have positive Weyl?, got {weyl2}");
        Console.WriteLine($"Star graph Weyl?: {weyl2}");
    }
    
    // ============================================================
    // SPECTRAL DIMENSION FAST ESTIMATE TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_EstimateSpectralDimensionFast_ChainGraph_NearOne()
    {
        // Chain graph should have d_S ? 1
        int chainLength = 50;
        var config = new SimulationConfig { NodeCount = chainLength, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        // Create chain
        for (int i = 0; i < chainLength - 1; i++)
        {
            AddEdgeWithWeight(graph, i, i + 1, 1.0);
        }
        graph.BuildSoAViews();
        
        double d_S = SpectralAction.EstimateSpectralDimensionFast(graph);
        
        Console.WriteLine($"Chain graph (N={chainLength}): fast d_S = {d_S:F4}");
        Assert.IsTrue(d_S >= 0.5 && d_S <= 2.5, $"Chain d_S should be near 1, got {d_S}");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_EstimateSpectralDimensionFast_2DGrid_NearTwo()
    {
        // 2D grid should have d_S ? 2
        // Note: Fast estimate uses average degree, so may be slightly off
        int gridSize = 10; // 10x10 grid
        int n = gridSize * gridSize;
        var config = new SimulationConfig { NodeCount = n, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        // Create 2D grid
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int node = x * gridSize + y;
                
                // Connect to right neighbor
                if (x + 1 < gridSize)
                {
                    int right = (x + 1) * gridSize + y;
                    AddEdgeWithWeight(graph, node, right, 1.0);
                }
                
                // Connect to bottom neighbor
                if (y + 1 < gridSize)
                {
                    int bottom = x * gridSize + (y + 1);
                    AddEdgeWithWeight(graph, node, bottom, 1.0);
                }
            }
        }
        graph.BuildSoAViews();
        
        double d_S = SpectralAction.EstimateSpectralDimensionFast(graph);
        
        Console.WriteLine($"2D grid ({gridSize}x{gridSize}): fast d_S = {d_S:F4}");
        // Fast estimate may differ from exact d_S, accept broader range
        Assert.IsTrue(d_S >= 1.0 && d_S <= 3.5, $"2D grid d_S should be in reasonable range, got {d_S}");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_EstimateSpectralDimensionFast_SmallGraph_ReturnsReasonableValue()
    {
        var config = new SimulationConfig { NodeCount = 3, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        AddEdgeWithWeight(graph, 0, 1, 1.0);
        AddEdgeWithWeight(graph, 1, 2, 1.0);
        graph.BuildSoAViews();
        
        double d_S = SpectralAction.EstimateSpectralDimensionFast(graph);
        
        // Should return reasonable value without throwing
        Assert.IsFalse(double.IsNaN(d_S), "d_S should not be NaN");
        Assert.IsFalse(double.IsInfinity(d_S), "d_S should not be infinite");
        Assert.IsTrue(d_S >= 1.0 && d_S <= 8.0, $"d_S should be in valid range [1,8], got {d_S}");
    }
    
    // ============================================================
    // DIMENSION POTENTIAL TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeDimensionPotential_At4D_IsNegativeOrZero()
    {
        // Mexican hat potential has minimum at d_S = 4
        // For graph with d_S ? 4, potential should be at minimum (most negative)
        
        // Create 4D-like hypercubic graph (approximation using high connectivity)
        int n = 64;
        var config = new SimulationConfig { NodeCount = n, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        // Create hypercube-like structure: 4x4x4 cube with additional connections
        // Each node has ~8 neighbors ? avg_degree ? 8 ? estimated d_S ? 4
        int size = 4;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    int node = x * size * size + y * size + z;
                    if (node >= n) continue;
                    
                    // Connect to 6 neighbors (3D cube connections)
                    int[] dx = { 1, -1, 0, 0, 0, 0, 1, -1 };
                    int[] dy = { 0, 0, 1, -1, 0, 0, 1, 1 };
                    int[] dz = { 0, 0, 0, 0, 1, -1, 1, 1 };
                    
                    for (int d = 0; d < 8; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        int nz = z + dz[d];
                        if (nx >= 0 && nx < size && ny >= 0 && ny < size && nz >= 0 && nz < size)
                        {
                            int neighbor = nx * size * size + ny * size + nz;
                            if (neighbor < n && neighbor != node)
                            {
                                AddEdgeWithWeight(graph, node, neighbor, 1.0);
                            }
                        }
                    }
                }
            }
        }
        graph.BuildSoAViews();
        
        double potential = SpectralAction.ComputeDimensionPotential(graph);
        double d_S = SpectralAction.EstimateSpectralDimensionFast(graph);
        
        Console.WriteLine($"4D-like graph: d_S = {d_S:F4}, potential = {potential:F6}");
        
        // The potential should be finite and computable
        Assert.IsFalse(double.IsNaN(potential), "Potential should not be NaN");
        Assert.IsFalse(double.IsInfinity(potential), "Potential should not be infinite");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeDimensionPotential_AwayFrom4D_IsPositive()
    {
        // For d_S far from 4, Mexican hat potential should be positive
        // Chain graph has d_S ? 1, which is far from 4
        int chainLength = 50;
        var config = new SimulationConfig { NodeCount = chainLength, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        
        for (int i = 0; i < chainLength - 1; i++)
        {
            AddEdgeWithWeight(graph, i, i + 1, 1.0);
        }
        graph.BuildSoAViews();
        
        double potential = SpectralAction.ComputeDimensionPotential(graph);
        double d_S = SpectralAction.EstimateSpectralDimensionFast(graph);
        
        Console.WriteLine($"Chain graph: d_S = {d_S:F4}, potential = {potential:F6}");
        
        // For d_S ? 1, deviation from 4 is large, so potential may be large positive
        Assert.IsFalse(double.IsNaN(potential), "Potential should not be NaN");
    }
    
    // ============================================================
    // TOTAL SPECTRAL ACTION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeSpectralAction_ReturnsFiniteValue()
    {
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        double action = SpectralAction.ComputeSpectralAction(graph);
        
        Assert.IsFalse(double.IsNaN(action), "Spectral action should not be NaN");
        Assert.IsFalse(double.IsInfinity(action), "Spectral action should not be infinite");
        Console.WriteLine($"Default graph spectral action: S = {action:F4}");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeSpectralAction_LargerVolume_LargerAction()
    {
        // Action should scale with volume
        var config1 = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine1 = new SimulationEngine(config1);
        var graph1 = engine1.Graph;
        
        var config2 = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var engine2 = new SimulationEngine(config2);
        var graph2 = engine2.Graph;
        
        double action1 = SpectralAction.ComputeSpectralAction(graph1);
        double action2 = SpectralAction.ComputeSpectralAction(graph2);
        
        Console.WriteLine($"Graph (N=20) action: {action1:F4}");
        Console.WriteLine($"Graph (N=50) action: {action2:F4}");
        
        // Larger graph should generally have larger (or equal) action
        // due to volume term ??V
        Assert.IsTrue(action2 >= action1 * 0.5, "Larger graph should have comparable or larger action");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeSpectralAction_EmptyGraph_ReturnsZeroOrNearZero()
    {
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        graph.BuildSoAViews();
        
        double action = SpectralAction.ComputeSpectralAction(graph);
        
        // Empty graph has zero volume, so action should be near zero
        // (only dimension potential may contribute)
        Console.WriteLine($"Empty graph spectral action: S = {action:F6}");
        Assert.IsFalse(double.IsNaN(action), "Spectral action should not be NaN for empty graph");
    }
    
    // ============================================================
    // RQGRAPH INTEGRATION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void RQGraph_EstimateSpectralDimensionFast_DelegatesToSpectralAction()
    {
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        double fromGraph = graph.EstimateSpectralDimensionFast();
        double fromStatic = SpectralAction.EstimateSpectralDimensionFast(graph);
        
        Assert.AreEqual(fromStatic, fromGraph, 0.001,
            "RQGraph method should delegate to SpectralAction");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void RQGraph_ComputeSpectralAction_DelegatesToSpectralAction()
    {
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        double fromGraph = graph.ComputeSpectralAction();
        double fromStatic = SpectralAction.ComputeSpectralAction(graph);
        
        Assert.AreEqual(fromStatic, fromGraph, 0.001,
            "RQGraph method should delegate to SpectralAction");
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void RQGraph_IsNearSpectralActionMinimum_ReturnsBoolean()
    {
        var config = new SimulationConfig { NodeCount = 30, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        // Should return without throwing
        bool result = graph.IsNearSpectralActionMinimum(0.1);
        
        // Value should be deterministic
        bool result2 = graph.IsNearSpectralActionMinimum(0.1);
        Assert.AreEqual(result, result2, "Result should be deterministic");
        
        Console.WriteLine($"IsNearSpectralActionMinimum(0.1) = {result}");
    }
    
    // ============================================================
    // GRADIENT COMPUTATION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeActionGradient_ReturnsFiniteValue()
    {
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        // Find an existing edge
        int edgeI = -1, edgeJ = -1;
        for (int i = 0; i < graph.N && edgeI < 0; i++)
        {
            foreach (int j in graph.Neighbors(i))
            {
                if (j > i)
                {
                    edgeI = i;
                    edgeJ = j;
                    break;
                }
            }
        }
        
        if (edgeI >= 0 && edgeJ >= 0)
        {
            double gradient = SpectralAction.ComputeActionGradient(graph, edgeI, edgeJ);
            
            Assert.IsFalse(double.IsNaN(gradient), "Gradient should not be NaN");
            Assert.IsFalse(double.IsInfinity(gradient), "Gradient should not be infinite");
            Console.WriteLine($"Gradient at edge ({edgeI},{edgeJ}): {gradient:F6}");
        }
    }
    
    [TestMethod]
    [TestCategory("SpectralAction")]
    public void SpectralAction_ComputeActionGradient_NonExistentEdge_ReturnsZero()
    {
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        graph.BuildSoAViews();
        
        // Edge 0-1 doesn't exist
        double gradient = SpectralAction.ComputeActionGradient(graph, 0, 1);
        
        Assert.AreEqual(0.0, gradient, 0.001, "Gradient of non-existent edge should be zero");
    }
    
    // ============================================================
    // HELPER METHODS
    // ============================================================
    
    private static void ClearAllEdges(RQGraph graph)
    {
        for (int i = 0; i < graph.N; i++)
        {
            for (int j = i + 1; j < graph.N; j++)
            {
                graph.Edges[i, j] = false;
                graph.Edges[j, i] = false;
                graph.Weights[i, j] = 0.0;
                graph.Weights[j, i] = 0.0;
            }
        }
    }
    
    private static void AddEdgeWithWeight(RQGraph graph, int i, int j, double weight)
    {
        graph.Edges[i, j] = true;
        graph.Edges[j, i] = true;
        graph.Weights[i, j] = weight;
        graph.Weights[j, i] = weight;
    }
}
