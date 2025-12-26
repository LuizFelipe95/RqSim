using RQSimulation;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: Verify CSR topology construction is correct
    /// </summary>
    [TestMethod]
    [TestCategory("Topology")]
    public void CsrTopology_Construction_Correct()
    {
        var graph = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}");

        // Verify CSR offsets
        Assert.AreEqual(graph.N + 1, graph.CsrOffsets.Length, "CSR offsets should have N+1 entries");
        Assert.AreEqual(0, graph.CsrOffsets[0], "First offset should be 0");

        int totalEdges = graph.CsrOffsets[graph.N];
        Console.WriteLine($"Total directed edges: {totalEdges}");

        // Verify each node's adjacency
        for (int i = 0; i < graph.N; i++)
        {
            int start = graph.CsrOffsets[i];
            int end = graph.CsrOffsets[i + 1];
            int csrDegree = end - start;

            // Count actual neighbors from adjacency
            int actualDegree = 0;
            foreach (int j in graph.Neighbors(i))
            {
                actualDegree++;
            }

            Assert.AreEqual(actualDegree, csrDegree, $"Node {i}: CSR degree mismatch");

            // Verify all CSR neighbors are valid
            for (int k = start; k < end; k++)
            {
                int neighbor = graph.CsrIndices[k];
                Assert.IsTrue(neighbor >= 0 && neighbor < graph.N, $"Invalid neighbor index {neighbor}");
                Assert.IsTrue(graph.Edges[i, neighbor], $"Edge ({i},{neighbor}) not in adjacency");
            }
        }

        Console.WriteLine("CSR topology verified successfully");
    }
}
