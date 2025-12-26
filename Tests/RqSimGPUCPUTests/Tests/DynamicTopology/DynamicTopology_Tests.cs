using ComputeSharp;
using RQSimulation;
using RQSimulation.GPUCompressedSparseRow;
using RQSimulation.GPUCompressedSparseRow.Data;
using RQSimulation.GPUCompressedSparseRow.DynamicTopology;

namespace RqSimGPUCPUTests;

/// <summary>
/// Tests for Dynamic CSR Topology operations:
/// - Edge proposal collection
/// - Prefix scan correctness
/// - Full rebuild cycle
/// - Integration with GpuCayleyEvolutionEngineCsr
/// </summary>
public partial class RqSimGPUCPUTest
{
    #region EdgeProposalBuffer Tests

    [TestMethod]
    [TestCategory("DynamicTopology")]
    public void EdgeProposalBuffer_Allocate_CreatesBuffers()
    {
        if (!CheckGpuAvailable()) 
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();
        using var buffer = new EdgeProposalBuffer(device);

        buffer.Allocate(100, 50);

        Assert.IsTrue(buffer.IsAllocated);
        Assert.AreEqual(100, buffer.AdditionCapacity);
        Assert.AreEqual(50, buffer.DeletionCapacity);

        Console.WriteLine("EdgeProposalBuffer allocation verified");
    }

    [TestMethod]
    [TestCategory("DynamicTopology")]
    public void EdgeProposalBuffer_Reset_ZerosCounters()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();
        using var buffer = new EdgeProposalBuffer(device);

        buffer.Allocate(100, 50);
        buffer.Reset();

        int addCount = buffer.GetAdditionCount();
        int delCount = buffer.GetDeletionCount();

        Assert.AreEqual(0, addCount);
        Assert.AreEqual(0, delCount);

        Console.WriteLine("EdgeProposalBuffer reset verified");
    }

    #endregion

    #region DynamicCsrTopology Tests

    [TestMethod]
    [TestCategory("DynamicTopology")]
    public void DynamicCsrTopology_BuildFromCsrArrays_CorrectStructure()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();

        // Create simple triangle graph: 0-1, 1-2, 0-2
        int nodeCount = 3;
        int nnz = 6; // Symmetric: each edge appears twice
        int[] rowOffsets = [0, 2, 4, 6];
        int[] colIndices = [1, 2, 0, 2, 0, 1];
        double[] weights = [0.5, 0.5, 0.5, 0.5, 0.5, 0.5];

        using var topology = new DynamicCsrTopology(device);
        topology.BuildFromCsrArrays(nodeCount, nnz, rowOffsets, colIndices, weights);
        topology.UploadToGpu();

        Assert.AreEqual(nodeCount, topology.NodeCount);
        Assert.AreEqual(nnz, topology.Nnz);
        Assert.IsTrue(topology.IsGpuReady);
        Assert.AreEqual(1, topology.Version);

        // Verify degrees
        Assert.AreEqual(2, topology.GetDegree(0));
        Assert.AreEqual(2, topology.GetDegree(1));
        Assert.AreEqual(2, topology.GetDegree(2));

        // Verify edge lookup
        Assert.IsTrue(topology.FindEdge(0, 1) >= 0);
        Assert.IsTrue(topology.FindEdge(1, 2) >= 0);
        Assert.AreEqual(-1, topology.FindEdge(0, 0)); // No self-loop

        Console.WriteLine("DynamicCsrTopology construction verified");
    }

    [TestMethod]
    [TestCategory("DynamicTopology")]
    public void DynamicCsrTopology_ToStandardTopology_PreservesStructure()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();

        // Create small graph
        int nodeCount = 4;
        int nnz = 8;
        int[] rowOffsets = [0, 2, 4, 6, 8];
        int[] colIndices = [1, 2, 0, 3, 0, 3, 1, 2];
        double[] weights = [0.5, 0.6, 0.5, 0.7, 0.6, 0.8, 0.7, 0.8];

        using var dynamicTopo = new DynamicCsrTopology(device);
        dynamicTopo.BuildFromCsrArrays(nodeCount, nnz, rowOffsets, colIndices, weights);

        using var standardTopo = dynamicTopo.ToStandardTopology();

        Assert.AreEqual(nodeCount, standardTopo.NodeCount);
        Assert.AreEqual(nnz, standardTopo.Nnz);

        // Verify row offsets match
        for (int i = 0; i <= nodeCount; i++)
        {
            Assert.AreEqual(rowOffsets[i], standardTopo.RowOffsets[i]);
        }

        Console.WriteLine("DynamicCsrTopology to CsrTopology conversion verified");
    }

    #endregion

    #region GpuDynamicTopologyEngine Tests

    [TestMethod]
    [TestCategory("DynamicTopology")]
    public void GpuDynamicTopologyEngine_Initialize_Works()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();
        using var engine = new GpuDynamicTopologyEngine(device);

        engine.Initialize(100, 500);

        Assert.IsNotNull(engine.ProposalBuffer);
        Assert.IsTrue(engine.ProposalBuffer.IsAllocated);

        Console.WriteLine("GpuDynamicTopologyEngine initialization verified");
    }

    [TestMethod]
    [TestCategory("DynamicTopology")]
    public void GpuDynamicTopologyEngine_EvolveTopology_ProducesValidOutput()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();

        // Create test graph
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        // Build CSR topology
        using var topology = new CsrTopology(device);
        topology.BuildFromDenseMatrix(graph.Edges, graph.Weights);
        topology.UploadToGpu();

        int initialNnz = topology.Nnz;
        Console.WriteLine($"Initial topology: {topology.NodeCount} nodes, {initialNnz} edges");

        // Create masses buffer using correlation mass
        var correlationMass = graph.ComputePerNodeCorrelationMass();
        double[] masses = new double[topology.NodeCount];
        for (int i = 0; i < masses.Length; i++)
        {
            masses[i] = correlationMass[i];
        }
        using var massesBuffer = device.AllocateReadWriteBuffer<double>(masses.Length);
        massesBuffer.CopyFrom(masses);

        // Run dynamic topology evolution
        using var engine = new GpuDynamicTopologyEngine(device);
        engine.Initialize(topology.NodeCount, topology.Nnz);

        // Configure for testing
        engine.Config.Beta = 0.5;
        engine.Config.DeletionThreshold = 0.1;
        engine.Config.InitialWeight = 0.5;

        var newTopology = engine.EvolveTopology(topology, massesBuffer);

        // Stats should be available
        Assert.IsNotNull(engine.LastStats);
        Console.WriteLine($"Proposed additions: {engine.LastStats.ProposedAdditions}");
        Console.WriteLine($"Proposed deletions: {engine.LastStats.ProposedDeletions}");
        Console.WriteLine($"Time: {engine.LastStats.TotalTimeMs:F2}ms");

        if (newTopology is not null)
        {
            Console.WriteLine($"New topology: {newTopology.NodeCount} nodes, {newTopology.Nnz} edges");
            Assert.AreEqual(topology.NodeCount, newTopology.NodeCount);
            Assert.IsTrue(newTopology.IsGpuReady);
            newTopology.Dispose();
        }
        else
        {
            Console.WriteLine("No topology changes proposed");
        }

        Console.WriteLine("GpuDynamicTopologyEngine evolution verified");
    }

    #endregion

    #region Prefix Scan Tests

    [TestMethod]
    [TestCategory("DynamicTopology")]
    public void BlellochScan_SmallArray_CorrectResult()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();

        // Test array: [1, 2, 3, 4, 5, 6, 7, 8]
        // Expected exclusive scan: [0, 1, 3, 6, 10, 15, 21, 28]
        int[] input = [1, 2, 3, 4, 5, 6, 7, 8];
        int[] expected = [0, 1, 3, 6, 10, 15, 21, 28];
        int n = input.Length;

        using var buffer = device.AllocateReadWriteBuffer<int>(n);
        buffer.CopyFrom(input);

        // Run Blelloch scan using the existing engine
        using var compactionEngine = new GpuStreamCompactionEngine(device);
        compactionEngine.BlellochScan(buffer, n);

        int[] result = new int[n];
        buffer.CopyTo(result);

        Console.WriteLine($"Input:    [{string.Join(", ", input)}]");
        Console.WriteLine($"Result:   [{string.Join(", ", result)}]");
        Console.WriteLine($"Expected: [{string.Join(", ", expected)}]");

        for (int i = 0; i < n; i++)
        {
            Assert.AreEqual(expected[i], result[i], $"Mismatch at index {i}");
        }

        Console.WriteLine("Blelloch scan verified");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    [TestCategory("DynamicTopology")]
    [TestCategory("Integration")]
    public void GpuCayleyEvolutionEngineCsr_DynamicHardRewiring_Works()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        // Create test graph
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        using var engine = new GpuCayleyEvolutionEngineCsr();
        engine.InitializeFromDense(graph.Edges, graph.Weights, gaugeDim: 1);
        engine.CurrentTopologyMode = GpuCayleyEvolutionEngineCsr.TopologyMode.DynamicHardRewiring;

        // Configure dynamic topology
        engine.ConfigureDynamicTopology(config =>
        {
            config.Beta = 0.5;
            config.RebuildInterval = 5;
            config.DeletionThreshold = 0.05;
            config.InitialWeight = 0.5;
        });

        // Upload masses using correlation mass
        var correlationMass = graph.ComputePerNodeCorrelationMass();
        double[] masses = new double[graph.N];
        for (int i = 0; i < masses.Length; i++)
        {
            masses[i] = correlationMass[i];
        }
        engine.UpdateMasses(masses);

        // Upload initial wavefunction
        double[] psiReal = new double[graph.N];
        double[] psiImag = new double[graph.N];
        psiReal[0] = 1.0; // Localized initial state

        engine.UploadWavefunction(psiReal, psiImag);

        Console.WriteLine($"Initial topology: {engine.Topology?.Nnz} edges");

        // Run several evolution steps
        double dt = 0.01;
        int totalIterations = 0;
        for (int step = 0; step < 10; step++)
        {
            totalIterations += engine.EvolveStep(dt);
        }

        Console.WriteLine($"After 10 steps: {engine.Topology?.Nnz} edges");
        Console.WriteLine($"Total BiCGStab iterations: {totalIterations}");

        if (engine.LastDynamicStats is not null)
        {
            Console.WriteLine($"Last dynamic stats: +{engine.LastDynamicStats.AcceptedAdditions}, -{engine.LastDynamicStats.AcceptedDeletions}");
        }

        // Verify norm is approximately preserved
        double norm = engine.ComputeNorm();
        Console.WriteLine($"Final norm: {norm:F6}");
        Assert.IsTrue(norm > 0.9 && norm < 1.1, $"Norm {norm} outside expected range");

        Console.WriteLine("Dynamic hard rewiring integration test passed");
    }

    [TestMethod]
    [TestCategory("DynamicTopology")]
    [TestCategory("Integration")]
    public void GpuCayleyEvolutionEngineCsr_ForceTopologyRebuild_Works()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        using var engine = new GpuCayleyEvolutionEngineCsr();
        engine.InitializeFromDense(graph.Edges, graph.Weights, gaugeDim: 1);
        engine.CurrentTopologyMode = GpuCayleyEvolutionEngineCsr.TopologyMode.DynamicHardRewiring;

        // Upload masses using correlation mass
        var correlationMass = graph.ComputePerNodeCorrelationMass();
        double[] masses = new double[graph.N];
        for (int i = 0; i < masses.Length; i++)
        {
            masses[i] = correlationMass[i];
        }
        engine.UpdateMasses(masses);

        int initialNnz = engine.Topology?.Nnz ?? 0;
        Console.WriteLine($"Initial edges: {initialNnz}");

        // Force rebuild
        engine.ForceTopologyRebuild();

        int afterNnz = engine.Topology?.Nnz ?? 0;
        Console.WriteLine($"After rebuild: {afterNnz}");

        // Just verify no exception was thrown
        Assert.IsNotNull(engine.Topology);

        Console.WriteLine("Force topology rebuild test passed");
    }

    #endregion

    #region Top-K Selection Tests

    [TestMethod]
    [TestCategory("DynamicTopology")]
    [TestCategory("TopK")]
    public void TopKSelection_CpuVsBlockTopM_ProducesSameResults()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();

        // Create graph with varied weights for meaningful top-K
        var graph = CreateTestGraph(SmallGraphNodes, 0.2, TestSeed);
        graph.BuildSoAViews();

        using var topology = new CsrTopology(device);
        topology.BuildFromDenseMatrix(graph.Edges, graph.Weights);
        topology.UploadToGpu();

        int k = System.Math.Min(10, topology.Nnz / 4);
        Console.WriteLine($"Testing top-{k} selection on {topology.Nnz} edges");

        using var engine = new GpuStreamCompactionEngine(device);

        // Get CPU result
        int[] cpuResult = engine.SelectTopK(topology, k, TopKSelectionStrategy.CpuOnly);
        var cpuMetrics = engine.LastTopKMetrics;
        Console.WriteLine($"CPU: {cpuResult.Length} results, time={cpuMetrics?.TotalTimeMs:F2}ms");

        // Get BlockTopM result
        int[] blockTopMResult = engine.SelectTopK(topology, k, TopKSelectionStrategy.BlockTopM, M: 4);
        var blockTopMMetrics = engine.LastTopKMetrics;
        Console.WriteLine($"BlockTopM: {blockTopMResult.Length} results, gpu={blockTopMMetrics?.GpuTimeMs:F2}ms, cpu={blockTopMMetrics?.CpuRefineTimeMs:F2}ms, candidates={blockTopMMetrics?.GpuCandidateCount}");

        // Both should return k elements
        Assert.AreEqual(k, cpuResult.Length, "CPU result count mismatch");
        Assert.AreEqual(k, blockTopMResult.Length, "BlockTopM result count mismatch");

        // Compare sets - the actual top-K elements should be the same (though order may differ)
        var cpuSet = new HashSet<int>(cpuResult);
        var blockTopMSet = new HashSet<int>(blockTopMResult);

        // Verify same top-K by weight sum
        double[] weights = topology.EdgeWeights.ToArray();
        double cpuWeightSum = cpuResult.Sum(i => weights[i]);
        double blockTopMWeightSum = blockTopMResult.Sum(i => weights[i]);

        Console.WriteLine($"CPU weight sum: {cpuWeightSum:F4}");
        Console.WriteLine($"BlockTopM weight sum: {blockTopMWeightSum:F4}");

        // Weight sums should be approximately equal (may differ slightly if ties exist)
        Assert.IsTrue(System.Math.Abs(cpuWeightSum - blockTopMWeightSum) < 0.001 * cpuWeightSum,
            $"Weight sum mismatch: CPU={cpuWeightSum:F4}, BlockTopM={blockTopMWeightSum:F4}");

        Console.WriteLine("CPU vs BlockTopM parity verified");
    }

    [TestMethod]
    [TestCategory("DynamicTopology")]
    [TestCategory("TopK")]
    public void TopKSelection_CpuVsParallelBlockTopM_ProducesSameResults()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();

        // Create larger graph for parallel test
        var graph = CreateTestGraph(SmallGraphNodes * 2, 0.15, TestSeed);
        graph.BuildSoAViews();

        using var topology = new CsrTopology(device);
        topology.BuildFromDenseMatrix(graph.Edges, graph.Weights);
        topology.UploadToGpu();

        int k = System.Math.Min(20, topology.Nnz / 5);
        Console.WriteLine($"Testing parallel top-{k} selection on {topology.Nnz} edges");

        using var engine = new GpuStreamCompactionEngine(device);

        // Get CPU result
        int[] cpuResult = engine.SelectTopK(topology, k, TopKSelectionStrategy.CpuOnly);
        var cpuMetrics = engine.LastTopKMetrics;
        Console.WriteLine($"CPU: {cpuResult.Length} results, time={cpuMetrics?.TotalTimeMs:F2}ms");

        // Get ParallelBlockTopM result
        int[] parallelResult = engine.SelectTopK(topology, k, TopKSelectionStrategy.ParallelBlockTopM, M: 4);
        var parallelMetrics = engine.LastTopKMetrics;
        Console.WriteLine($"ParallelBlockTopM: {parallelResult.Length} results, gpu={parallelMetrics?.GpuTimeMs:F2}ms, cpu={parallelMetrics?.CpuRefineTimeMs:F2}ms, candidates={parallelMetrics?.GpuCandidateCount}");

        // Both should return k elements
        Assert.AreEqual(k, cpuResult.Length, "CPU result count mismatch");
        Assert.AreEqual(k, parallelResult.Length, "ParallelBlockTopM result count mismatch");

        // Verify same top-K by weight sum
        double[] weights = topology.EdgeWeights.ToArray();
        double cpuWeightSum = cpuResult.Sum(i => weights[i]);
        double parallelWeightSum = parallelResult.Sum(i => weights[i]);

        Console.WriteLine($"CPU weight sum: {cpuWeightSum:F4}");
        Console.WriteLine($"ParallelBlockTopM weight sum: {parallelWeightSum:F4}");

        // Weight sums should be approximately equal
        Assert.IsTrue(System.Math.Abs(cpuWeightSum - parallelWeightSum) < 0.001 * cpuWeightSum,
            $"Weight sum mismatch: CPU={cpuWeightSum:F4}, Parallel={parallelWeightSum:F4}");

        Console.WriteLine("CPU vs ParallelBlockTopM parity verified");
    }

    [TestMethod]
    [TestCategory("DynamicTopology")]
    [TestCategory("TopK")]
    public void TopKSelection_AutoStrategy_SelectsAppropriateMethod()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();

        // Test with small graph - should use CPU
        var smallGraph = CreateTestGraph(20, 0.3, TestSeed);
        smallGraph.BuildSoAViews();

        using var smallTopology = new CsrTopology(device);
        smallTopology.BuildFromDenseMatrix(smallGraph.Edges, smallGraph.Weights);
        smallTopology.UploadToGpu();

        using var engine = new GpuStreamCompactionEngine(device);

        int k = 5;
        _ = engine.SelectTopK(smallTopology, k, TopKSelectionStrategy.Auto);
        var smallMetrics = engine.LastTopKMetrics;

        Console.WriteLine($"Small graph ({smallTopology.Nnz} edges): used {smallMetrics?.UsedStrategy}");
        
        // For small graphs (<10k edges), Auto should select CpuOnly
        if (smallTopology.Nnz < 10_000)
        {
            Assert.AreEqual(TopKSelectionStrategy.CpuOnly, smallMetrics?.UsedStrategy,
                "Auto should select CpuOnly for small graphs");
        }

        Console.WriteLine("Auto strategy selection verified");
    }

    [TestMethod]
    [TestCategory("DynamicTopology")]
    [TestCategory("TopK")]
    public void TopKSelection_MetricsArePopulated()
    {
        if (!CheckGpuAvailable())
        {
            Assert.Inconclusive("GPU not available");
            return;
        }

        var device = GraphicsDevice.GetDefault();

        var graph = CreateTestGraph(SmallGraphNodes, 0.2, TestSeed);
        graph.BuildSoAViews();

        using var topology = new CsrTopology(device);
        topology.BuildFromDenseMatrix(graph.Edges, graph.Weights);
        topology.UploadToGpu();

        using var engine = new GpuStreamCompactionEngine(device);

        int k = 10;
        _ = engine.SelectTopK(topology, k, TopKSelectionStrategy.BlockTopM, M: 4);

        var metrics = engine.LastTopKMetrics;
        Assert.IsNotNull(metrics, "LastTopKMetrics should not be null");
        Assert.AreEqual(k, metrics.RequestedK, "RequestedK mismatch");
        Assert.AreEqual(topology.Nnz, metrics.SourceNnz, "SourceNnz mismatch");
        Assert.IsTrue(metrics.TotalTimeMs > 0, "TotalTimeMs should be positive");
        Assert.IsTrue(metrics.GpuTimeMs >= 0, "GpuTimeMs should be non-negative");
        Assert.IsTrue(metrics.ResultCount > 0, "ResultCount should be positive");

        Console.WriteLine($"Metrics populated: k={metrics.RequestedK}, nnz={metrics.SourceNnz}, " +
                         $"total={metrics.TotalTimeMs:F2}ms, gpu={metrics.GpuTimeMs:F2}ms, " +
                         $"cpu={metrics.CpuRefineTimeMs:F2}ms, candidates={metrics.GpuCandidateCount}");

        Console.WriteLine("Metrics population verified");
    }

    #endregion

    #region Helper Methods for DynamicTopology Tests

    /// <summary>
    /// Check if GPU is available for tests (local helper to avoid conflicts).
    /// </summary>
    private static bool CheckGpuAvailable()
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
}
