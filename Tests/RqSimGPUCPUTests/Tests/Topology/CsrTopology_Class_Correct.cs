using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RQSimulation.GPUCompressedSparseRow.Data;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    [TestMethod]
    [TestCategory("Topology")]
    public void CsrTopology_Class_Correct()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // 1. Setup small graph
        int N = 4;
        bool[,] edges = new bool[N, N];
        double[,] weights = new double[N, N];
        
        // 0 -> 1, 2
        edges[0, 1] = true; weights[0, 1] = 0.5;
        edges[0, 2] = true; weights[0, 2] = 0.2;
        
        // 1 -> 2
        edges[1, 2] = true; weights[1, 2] = 0.8;
        
        // 2 -> 3
        edges[2, 3] = true; weights[2, 3] = 0.1;
        
        // 3 -> 0
        edges[3, 0] = true; weights[3, 0] = 0.9;

        // 2. Build CSR Topology
        using var topology = new CsrTopology();
        topology.BuildFromDenseMatrix(edges, weights);

        // 3. Verify CPU Data
        Assert.AreEqual(N, topology.NodeCount);
        Assert.AreEqual(5, topology.Nnz);

        // 4. Upload to GPU
        topology.UploadToGpu();
        Assert.IsTrue(topology.IsGpuReady);

        // 5. Verify GPU Buffers (indirectly via properties)
        Assert.IsNotNull(topology.RowOffsetsBuffer);
        Assert.IsNotNull(topology.ColIndicesBuffer);
        Assert.IsNotNull(topology.EdgeWeightsBuffer);
        Assert.IsNotNull(topology.NodePotentialBuffer);
        
        // Note: We can't easily read back from ReadOnlyBuffer without a kernel or copy method,
        // but successful upload and property access is a good basic test.
    }
}
