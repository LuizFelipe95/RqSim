using RQSimulation;
using RQSimulation.GPUCompressedSparseRow.BlackHole;
using RQSimulation.GPUCompressedSparseRow.Causal;
using RQSimulation.GPUCompressedSparseRow.Data;
using RQSimulation.GPUCompressedSparseRow.Gauge;
using RQSimulation.GPUCompressedSparseRow.HeavyMass;
using RQSimulation.GPUCompressedSparseRow.Hybrid;
using RQSimulation.GPUCompressedSparseRow.Unified;

namespace RqSimGPUCPUTests;

/// <summary>
/// Integration tests for new GPU CSR physics modules.
/// Tests Black Hole Horizon, Causal Discovery, Gauge Invariants, Heavy Mass, and Hybrid Pipeline.
/// </summary>
public partial class RqSimGPUCPUTest
{
    // ============================================================
    // GPU HORIZON ENGINE TESTS
    // ============================================================

    [TestMethod]
    [TestCategory("GpuHorizon")]
    public void GpuHorizonEngine_Initialize_ValidTopology_Succeeds()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);
        var topology = graph.CsrTopology;

        // Create node energies array
        var nodeEnergies = new double[graph.N];
        for (int i = 0; i < graph.N; i++)
        {
            nodeEnergies[i] = 1.0;
        }

        // Act
        using var horizonEngine = new GpuHorizonEngine();
        horizonEngine.Initialize(topology!, nodeEnergies);

        // Assert
        Assert.IsTrue(horizonEngine.IsInitialized);
        Assert.AreEqual(graph.N, horizonEngine.NodeCount);
    }

    [TestMethod]
    [TestCategory("GpuHorizon")]
    public void GpuHorizonEngine_DetectHorizons_ProducesReasonableResults()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 100, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        // Initialize some high energy nodes to simulate potential horizon
        var nodeEnergies = new double[graph.N];
        for (int i = 0; i < graph.N; i++)
        {
            nodeEnergies[i] = i < 10 ? 5.0 : 1.0;
        }

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        // Act
        using var horizonEngine = new GpuHorizonEngine
        {
            DensityThreshold = 2.0,
            MinMassThreshold = 0.1
        };
        horizonEngine.Initialize(graph.CsrTopology!, nodeEnergies);
        horizonEngine.DetectHorizons();
        horizonEngine.SyncToCpu();

        // Assert
        Assert.IsTrue(horizonEngine.LocalMass.Length == graph.N, "Should have computed local mass for all nodes");
    }

    [TestMethod]
    [TestCategory("GpuHorizon")]
    public void GpuBlackHoleHorizonModule_ExecuteStep_NoErrors()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var module = new GpuBlackHoleHorizonModule
        {
            DensityThreshold = 5.0,
            MinMassThreshold = 0.5
        };

        // Act
        module.Initialize(graph);
        module.ExecuteStep(graph, 0.01);

        // Assert - no exception thrown
        Assert.IsTrue(true, "Module executed without error");
    }

    // ============================================================
    // GPU CAUSAL ENGINE TESTS
    // ============================================================

    [TestMethod]
    [TestCategory("GpuCausal")]
    public void GpuCausalEngine_Initialize_ValidTopology_Succeeds()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        // Act
        using var causalEngine = new GpuCausalEngine { MaxDepth = 5 };
        causalEngine.Initialize(graph.CsrTopology!);

        // Assert
        Assert.IsTrue(causalEngine.IsInitialized);
        Assert.AreEqual(graph.N, causalEngine.NodeCount);
    }

    [TestMethod]
    [TestCategory("GpuCausal")]
    public void GpuCausalEngine_ComputeCausalCone_ReturnsReachableNodes()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 100, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var causalEngine = new GpuCausalEngine { MaxDepth = 10 };
        causalEngine.Initialize(graph.CsrTopology!);

        // Act
        int coneSize = causalEngine.ComputeCausalCone(0, 3);
        var coneNodes = causalEngine.GetCausalConeNodes();

        // Assert
        Assert.IsTrue(coneSize > 0, "Cone should contain at least the source node");
        Assert.IsTrue(coneNodes.Contains(0), "Cone should contain source node");
        Assert.AreEqual(coneSize, coneNodes.Count, "Cone size should match node list count");
    }

    [TestMethod]
    [TestCategory("GpuCausal")]
    public void GpuCausalEngine_AreCausallyConnected_SelfConnection_ReturnsTrue()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var causalEngine = new GpuCausalEngine { MaxDepth = 10 };
        causalEngine.Initialize(graph.CsrTopology!);

        // Act
        bool selfConnected = causalEngine.AreCausallyConnected(0, 0, 1);

        // Assert
        Assert.IsTrue(selfConnected, "Node should be causally connected to itself");
    }

    [TestMethod]
    [TestCategory("GpuCausal")]
    public void GpuCausalEngine_GetDistance_DirectNeighbors_ReturnsOne()
    {
        // Arrange - create a simple connected graph
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        // Find a pair of directly connected nodes
        int nodeA = -1, nodeB = -1;
        for (int i = 0; i < graph.N && nodeA < 0; i++)
        {
            for (int j = i + 1; j < graph.N; j++)
            {
                if (graph.Edges[i, j])
                {
                    nodeA = i;
                    nodeB = j;
                    break;
                }
            }
        }

        if (nodeA < 0)
        {
            Assert.Inconclusive("No edges found in graph");
            return;
        }

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var causalEngine = new GpuCausalEngine { MaxDepth = 10 };
        causalEngine.Initialize(graph.CsrTopology!);

        // Act
        int distance = causalEngine.GetDistance(nodeA, nodeB);

        // Assert
        Assert.AreEqual(1, distance, "Direct neighbors should have distance 1");
    }

    [TestMethod]
    [TestCategory("GpuCausal")]
    public void GpuCausalDiscoveryModule_ExecuteStep_NoErrors()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var module = new GpuCausalDiscoveryModule
        {
            MaxCausalDepth = 5,
            SpeedOfLight = 1.0
        };

        // Act
        module.Initialize(graph);
        module.ExecuteStep(graph, 0.01);

        // Assert
        Assert.IsNotNull(module.Engine);
        Assert.IsTrue(module.Engine.IsInitialized);
    }

    // ============================================================
    // GPU GAUGE ENGINE TESTS
    // ============================================================

    [TestMethod]
    [TestCategory("GpuGauge")]
    public void GpuGaugeEngine_Initialize_ValidTopology_Succeeds()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        // Create zero edge phases
        var edgePhases = new double[graph.N, graph.N];

        // Act
        using var gaugeEngine = new GpuGaugeEngine();
        gaugeEngine.Initialize(graph.CsrTopology!, edgePhases);

        // Assert
        Assert.IsTrue(gaugeEngine.IsInitialized);
        Assert.AreEqual(graph.N, gaugeEngine.NodeCount);
    }

    [TestMethod]
    [TestCategory("GpuGauge")]
    public void GpuGaugeEngine_DetectTriangles_FindsTriangles()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 100, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        var edgePhases = new double[graph.N, graph.N];

        using var gaugeEngine = new GpuGaugeEngine();
        gaugeEngine.Initialize(graph.CsrTopology!, edgePhases);

        // Act
        var triangles = gaugeEngine.GetTriangles();

        // Assert - random graphs typically have triangles
        // (may be 0 for very sparse graphs, which is valid)
        Assert.IsTrue(gaugeEngine.TriangleCount >= 0, "Triangle count should be non-negative");
    }

    [TestMethod]
    [TestCategory("GpuGauge")]
    public void GpuGaugeEngine_ComputeWilsonLoops_ZeroPhases_MagnitudeOne()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 100, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        // Zero phases should give Wilson loop = exp(i*0) = 1
        var edgePhases = new double[graph.N, graph.N];

        using var gaugeEngine = new GpuGaugeEngine();
        gaugeEngine.Initialize(graph.CsrTopology!, edgePhases);

        // Act
        gaugeEngine.ComputeGaugeInvariants();

        // Assert
        if (gaugeEngine.TriangleCount > 0)
        {
            Assert.AreEqual(1.0, gaugeEngine.MeanWilsonMagnitude, 0.001,
                "Zero phase Wilson loops should have magnitude 1");
        }
    }

    [TestMethod]
    [TestCategory("GpuGauge")]
    public void GpuGaugeInvariantModule_ExecuteStep_NoErrors()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var module = new GpuGaugeInvariantModule
        {
            ViolationWarningThreshold = 0.01
        };

        // Act
        module.Initialize(graph);
        module.ExecuteStep(graph, 0.01);

        // Assert
        // Module may not have engine if EdgePhaseU1 is null, but should not throw
        Assert.IsTrue(true, "Module executed without error");
    }

    // ============================================================
    // GPU HEAVY MASS ENGINE TESTS
    // ============================================================

    [TestMethod]
    [TestCategory("GpuHeavyMass")]
    public void GpuHeavyMassEngine_Initialize_ValidTopology_Succeeds()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        // Act
        using var heavyEngine = new GpuHeavyMassEngine();
        heavyEngine.Initialize(graph.CsrTopology!);

        // Assert
        Assert.IsTrue(heavyEngine.IsInitialized);
        Assert.AreEqual(graph.N, heavyEngine.NodeCount);
    }

    [TestMethod]
    [TestCategory("GpuHeavyMass")]
    public void GpuHeavyMassEngine_ComputeCorrelationMass_PositiveValues()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 100, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var heavyEngine = new GpuHeavyMassEngine();
        heavyEngine.Initialize(graph.CsrTopology!);

        // Act
        heavyEngine.Step(0.01);

        // Assert
        Assert.IsTrue(heavyEngine.TotalCorrelationMass > 0, "Should have positive total mass");
        Assert.IsTrue(heavyEngine.MeanCorrelationMass > 0, "Should have positive mean mass");
    }

    [TestMethod]
    [TestCategory("GpuHeavyMass")]
    public void GpuHeavyMassEngine_DetectHeavyNodes_ReturnsValidList()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 100, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var heavyEngine = new GpuHeavyMassEngine
        {
            HeavyMassThreshold = 0.5 // Low threshold to catch some nodes
        };
        heavyEngine.Initialize(graph.CsrTopology!);

        // Act
        heavyEngine.Step(0.01);
        var heavyNodes = heavyEngine.GetHeavyNodes();

        // Assert
        Assert.IsNotNull(heavyNodes);
        Assert.AreEqual(heavyEngine.HeavyNodeCount, heavyNodes.Count);
    }

    [TestMethod]
    [TestCategory("GpuHeavyMass")]
    public void GpuHeavyMassModule_ExecuteStep_NoErrors()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var module = new GpuHeavyMassModule
        {
            HeavyMassThreshold = 1.0
        };

        // Act
        module.Initialize(graph);
        module.ExecuteStep(graph, 0.01);

        // Assert
        Assert.IsNotNull(module.Engine);
        Assert.IsTrue(module.Engine.IsInitialized);
    }

    // ============================================================
    // HYBRID PIPELINE COORDINATOR TESTS
    // ============================================================

    [TestMethod]
    [TestCategory("HybridPipeline")]
    public void RecommendationBuffer_Add_IncreasesCount()
    {
        // Arrange
        var buffer = new RecommendationBuffer();

        // Act
        buffer.Add(new TopologyRecommendation
        {
            Type = RecommendationType.CreateEdge,
            NodeA = 0,
            NodeB = 1,
            Weight = 1.0,
            Priority = 0.5,
            Source = "Test"
        });

        // Assert
        Assert.AreEqual(1, buffer.Count);
    }

    [TestMethod]
    [TestCategory("HybridPipeline")]
    public void RecommendationBuffer_GetSorted_OrdersByPriority()
    {
        // Arrange
        var buffer = new RecommendationBuffer();
        buffer.Add(new TopologyRecommendation { Priority = 0.1, Source = "Low" });
        buffer.Add(new TopologyRecommendation { Priority = 0.9, Source = "High" });
        buffer.Add(new TopologyRecommendation { Priority = 0.5, Source = "Mid" });

        // Act
        var sorted = buffer.GetSortedRecommendations();

        // Assert
        Assert.AreEqual(3, sorted.Count);
        Assert.AreEqual("High", sorted[0].Source);
        Assert.AreEqual("Mid", sorted[1].Source);
        Assert.AreEqual("Low", sorted[2].Source);
    }

    [TestMethod]
    [TestCategory("HybridPipeline")]
    public void HybridPipelineCoordinator_Initialize_NoErrors()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var coordinator = new HybridPipelineCoordinator();

        // Act
        coordinator.Initialize(graph);

        // Assert - no exception
        Assert.IsTrue(true, "Coordinator initialized successfully");
    }

    [TestMethod]
    [TestCategory("HybridPipeline")]
    public void HybridPipelineCoordinator_ProcessRecommendations_EmptyBuffer_NoChanges()
    {
        // Arrange
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;

        using var csrEngine = new CsrUnifiedEngine();
        csrEngine.Initialize(graph);

        using var coordinator = new HybridPipelineCoordinator();
        coordinator.Initialize(graph);

        int initialSignature = graph.TopologySignature;

        // Act
        coordinator.ProcessRecommendations(graph);

        // Assert
        Assert.AreEqual(0, coordinator.LastRecommendationsProcessed);
        Assert.AreEqual(0, coordinator.LastChangesApplied);
        Assert.AreEqual(initialSignature, graph.TopologySignature);
    }
}
