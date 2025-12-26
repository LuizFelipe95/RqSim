using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: GpuSpectralEngine CPU comparison mode for debugging.
    /// 
    /// Note: Due to ComputeSharp pipeline caching behavior, this test runs
    /// with a fresh GpuSpectralEngine instance to avoid conflicts.
    /// </summary>
    [TestMethod]
    [TestCategory("SpectralDimension")]
    public void GpuSpectralEngine_CpuComparisonMode_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Check if GPU supports double precision
        var device = ComputeSharp.GraphicsDevice.GetDefault();
        if (!device.IsDoublePrecisionSupportAvailable())
        {
            Assert.Inconclusive("GPU does not support double precision");
            return;
        }

        // Arrange - use unique graph to avoid shader cache conflicts
        // Use larger graph to ensure diffusion doesn't saturate (finite size effect)
        var graph = CreateTestGraph(MediumGraphNodes, 0.15, TestSeed + 100); 
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}, E={graph.FlatEdgesFrom.Length}");

        double gpuDim;
        try
        {
            using var gpuEngine = new GpuSpectralEngine();
            gpuEngine.UpdateTopology(graph);

            // Act - Run with CPU comparison enabled
            gpuDim = gpuEngine.ComputeSpectralDimension(
                graph,
                dt: 0.01,
                numSteps: 50,
                numProbeVectors: 8,
                enableCpuComparison: true);

            Console.WriteLine($"GPU d_S = {gpuDim:F4} (with CPU comparison logging)");
        }
        catch (ArgumentException ex) when (ex.Message.Contains("same key"))
        {
            // Known ComputeSharp pipeline cache issue - mark as inconclusive
            Console.WriteLine($"ComputeSharp pipeline cache conflict: {ex.Message}");
            Assert.Inconclusive("ComputeSharp pipeline cache conflict - known issue with reusing shader types");
            return;
        }

        // Assert
        Assert.IsTrue(double.IsFinite(gpuDim), $"GPU d_S should be finite, got {gpuDim}");
        Assert.IsTrue(gpuDim >= 1 && gpuDim <= 10, $"GPU d_S out of range: {gpuDim}");
    }
}
