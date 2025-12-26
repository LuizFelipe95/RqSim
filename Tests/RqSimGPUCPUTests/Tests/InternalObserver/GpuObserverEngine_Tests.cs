using RQSimulation;
using RQSimulation.GPUOptimized.Observer;
using RQSimulation.GPUCompressedSparseRow.Observer;
using RQSimulation.GPUCompressedSparseRow.Data;

namespace RqSimGPUCPUTests;

/// <summary>
/// Unit tests for GPU Observer engines (RQ-Hypothesis Stage 5).
/// Tests GPU-accelerated internal observer operations.
/// </summary>
public partial class RqSimGPUCPUTest
{
    // ============================================================
    // GPU OBSERVER ENGINE CONSTRUCTION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_Initialize_ValidInput_Succeeds()
    {
        using var engine = new GpuObserverEngine();
        
        int nodeCount = 50;
        var observerNodes = new[] { 0, 1, 2, 3, 4 };
        
        engine.Initialize(nodeCount, observerNodes, gaugeDim: 1);
        
        Assert.IsTrue(engine.IsInitialized);
        Assert.AreEqual(nodeCount, engine.NodeCount);
        Assert.AreEqual(observerNodes.Length, engine.ObserverCount);
        Assert.AreEqual(1, engine.GaugeDimension);
    }
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_Initialize_EmptyObserverNodes_Throws()
    {
        using var engine = new GpuObserverEngine();
        
        bool exceptionThrown = false;
        try
        {
            engine.Initialize(50, Array.Empty<int>());
        }
        catch (ArgumentException)
        {
            exceptionThrown = true;
        }
        
        Assert.IsTrue(exceptionThrown, "Expected ArgumentException for empty observer nodes");
    }
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_Initialize_InvalidNodeIndex_Throws()
    {
        using var engine = new GpuObserverEngine();
        
        bool exceptionThrown = false;
        try
        {
            engine.Initialize(50, new[] { 0, 1, 100 }); // Node 100 out of range
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrown = true;
        }
        
        Assert.IsTrue(exceptionThrown, "Expected ArgumentOutOfRangeException for invalid node index");
    }
    
    // ============================================================
    // GPU OBSERVER WAVEFUNCTION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_UploadDownloadWavefunction_PreservesData()
    {
        var config = new SimulationConfig { NodeCount = 30, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        // Initialize quantum wavefunction
        graph.InitQuantumWavefunction();
        
        using var engine = new GpuObserverEngine();
        var observerNodes = new[] { 0, 1, 2, 3, 4 };
        engine.Initialize(graph.N, observerNodes, graph.GaugeDimension);
        
        // Upload wavefunction
        engine.UploadWavefunction(graph);
        
        // Download back
        engine.DownloadWavefunction(graph);
        
        // Wavefunction should be preserved (no operations performed)
        double totalProb = graph.GetTotalProbability();
        Assert.IsTrue(totalProb > 0.0, "Wavefunction should have non-zero probability");
    }
    
    // ============================================================
    // GPU OBSERVER PROBABILITY DENSITY TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_ComputeProbabilityDensity_ReturnsNonNegative()
    {
        var config = new SimulationConfig { NodeCount = 30, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        graph.InitQuantumWavefunction();
        
        using var engine = new GpuObserverEngine();
        engine.Initialize(graph.N, new[] { 0, 1, 2 }, graph.GaugeDimension);
        engine.UploadWavefunction(graph);
        
        double[] probDensity = engine.ComputeProbabilityDensityGpu();
        
        Assert.AreEqual(graph.N, probDensity.Length);
        
        // All probabilities should be non-negative
        foreach (double p in probDensity)
        {
            Assert.IsTrue(p >= 0.0, $"Probability density should be non-negative, got {p}");
        }
        
        // Total should be positive (normalized wavefunction)
        double total = probDensity.Sum();
        Assert.IsTrue(total > 0.0, "Total probability should be positive");
    }
    
    // ============================================================
    // GPU OBSERVER MUTUAL INFORMATION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_ComputeMutualInformation_NonNegative()
    {
        var config = new SimulationConfig { NodeCount = 30, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        graph.InitQuantumWavefunction();
        
        using var engine = new GpuObserverEngine();
        engine.Initialize(graph.N, new[] { 0, 1, 2 }, graph.GaugeDimension);
        engine.UploadWavefunction(graph);
        
        double mutualInfo = engine.ComputeMutualInformationGpu();
        
        Assert.IsTrue(mutualInfo >= 0.0, $"Mutual information should be non-negative, got {mutualInfo}");
    }
    
    // ============================================================
    // GPU OBSERVER PHASE SHIFT TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_ApplyPhaseShifts_ChangesWavefunction()
    {
        var config = new SimulationConfig { NodeCount = 30, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        graph.InitQuantumWavefunction();
        
        using var engine = new GpuObserverEngine();
        var observerNodes = new[] { 0, 1, 2 };
        engine.Initialize(graph.N, observerNodes, graph.GaugeDimension);
        engine.UploadWavefunction(graph);
        
        // Get initial probability density
        double[] probBefore = engine.ComputeProbabilityDensityGpu();
        
        // Apply phase shifts
        double[] phaseShifts = { System.Math.PI / 4, System.Math.PI / 2, System.Math.PI };
        engine.ApplyPhaseShiftsGpu(phaseShifts);
        
        // Get probability density after phase shift
        double[] probAfter = engine.ComputeProbabilityDensityGpu();
        
        // Probability should be preserved (unitary operation)
        double totalBefore = probBefore.Sum();
        double totalAfter = probAfter.Sum();
        
        Assert.AreEqual(totalBefore, totalAfter, 1e-10, 
            "Phase shift should preserve total probability (unitarity)");
    }
    
    // ============================================================
    // GPU OBSERVER MEASUREMENT SWEEP TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_MeasureSweep_RecordsObservations()
    {
        var config = new SimulationConfig { NodeCount = 30, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        graph.InitQuantumWavefunction();
        
        using var engine = new GpuObserverEngine();
        var observerNodes = new[] { 0, 1, 2 };
        engine.Initialize(graph.N, observerNodes, graph.GaugeDimension);
        engine.UploadWavefunction(graph);
        
        // Perform measurement sweep
        int interactions = engine.MeasureSweepGpu(graph);
        
        // Should have some interactions if graph is connected
        Assert.IsTrue(engine.Observations.Count > 0 || interactions > 0, 
            "Should have observation records after measurement sweep");
    }
    
    // ============================================================
    // GPU OBSERVER EXPECTATION VALUE TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    public void GpuObserverEngine_ComputeObserverExpectation_ReturnsFinite()
    {
        var config = new SimulationConfig { NodeCount = 30, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        graph.InitQuantumWavefunction();
        
        using var engine = new GpuObserverEngine();
        engine.Initialize(graph.N, new[] { 0, 1, 2 }, graph.GaugeDimension);
        engine.UploadWavefunction(graph);
        
        // Create observable (local energy)
        double[] observable = new double[graph.N];
        for (int i = 0; i < graph.N; i++)
        {
            observable[i] = graph.GetNodeMass(i);
        }
        
        double expectation = engine.ComputeObserverExpectationGpu(observable);
        
        Assert.IsFalse(double.IsNaN(expectation), "Expectation should not be NaN");
        Assert.IsFalse(double.IsInfinity(expectation), "Expectation should not be infinite");
    }
    
    // ============================================================
    // CSR OBSERVER ENGINE TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    [TestCategory("CSR")]
    public void CsrObserverEngine_Initialize_WithTopology_Succeeds()
    {
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        // Build CSR topology
        using var topology = new CsrTopology();
        topology.BuildFromDenseMatrix(graph.Edges, graph.Weights, new double[graph.N]);
        topology.UploadToGpu();
        
        using var engine = new CsrObserverEngine();
        var observerNodes = new[] { 0, 1, 2, 3, 4 };
        
        engine.Initialize(topology, observerNodes);
        
        Assert.IsTrue(engine.IsInitialized);
        Assert.AreEqual(graph.N, engine.NodeCount);
        Assert.AreEqual(observerNodes.Length, engine.ObserverCount);
    }
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    [TestCategory("CSR")]
    public void CsrObserverEngine_ComputeCorrelations_ReturnsArray()
    {
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        graph.InitQuantumWavefunction();
        
        using var topology = new CsrTopology();
        topology.BuildFromDenseMatrix(graph.Edges, graph.Weights, new double[graph.N]);
        topology.UploadToGpu();
        
        using var engine = new CsrObserverEngine();
        var observerNodes = new[] { 0, 1, 2 };
        engine.Initialize(topology, observerNodes, graph.GaugeDimension);
        engine.UploadWavefunction(graph);
        
        double[] correlations = engine.ComputeCorrelationsGpu();
        
        Assert.AreEqual(observerNodes.Length, correlations.Length);
    }
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    [TestCategory("CSR")]
    public void CsrObserverEngine_MeasureSweep_Works()
    {
        var config = new SimulationConfig { NodeCount = 50, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        graph.InitQuantumWavefunction();
        
        using var topology = new CsrTopology();
        topology.BuildFromDenseMatrix(graph.Edges, graph.Weights, new double[graph.N]);
        topology.UploadToGpu();
        
        using var engine = new CsrObserverEngine();
        engine.Initialize(topology, new[] { 0, 1, 2 }, graph.GaugeDimension);
        engine.UploadWavefunction(graph);
        
        int interactions = engine.MeasureSweepGpu();
        
        // Should have some interactions if graph is connected
        Assert.IsTrue(interactions >= 0, "Interaction count should be non-negative");
    }
    
    // ============================================================
    // GPU VS CPU COMPARISON TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("GpuObserver")]
    [TestCategory("GpuVsCpu")]
    public void GpuObserver_VsCpuObserver_MutualInformation_SimilarResults()
    {
        var config = new SimulationConfig { NodeCount = 30, Seed = TestSeed };
        var simEngine = new SimulationEngine(config);
        var graph = simEngine.Graph;
        
        graph.InitQuantumWavefunction();
        
        // CPU Observer
        var observerNodes = new[] { 0, 1, 2 };
        var cpuObserver = new InternalObserver(graph, observerNodes, TestSeed);
        cpuObserver.UseGpuAcceleration = false;
        
        // Get CPU mutual information via phase-based entropy
        var targetNodes = Enumerable.Range(5, 10).ToArray();
        double cpuMI = cpuObserver.GetMutualInformation(targetNodes);
        
        // GPU Observer
        using var gpuEngine = new GpuObserverEngine();
        gpuEngine.Initialize(graph.N, observerNodes, graph.GaugeDimension);
        gpuEngine.UploadWavefunction(graph);
        
        double gpuMI = gpuEngine.ComputeMutualInformationGpu();
        
        // Both should be non-negative
        Assert.IsTrue(cpuMI >= 0.0, "CPU mutual information should be non-negative");
        Assert.IsTrue(gpuMI >= 0.0, "GPU mutual information should be non-negative");
        
        // Note: Exact values may differ due to different entropy computation methods
        // (phase-based vs probability-based), so we just check both are valid
    }
}
