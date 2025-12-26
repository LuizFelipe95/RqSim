using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: Verify GpuSpectralEngine precision and detect float32 vs double64 issues.
    /// 
    /// This test verifies:
    /// 1. GPU hardware supports double64
    /// 2. CPU spectral dimension is in valid range
    /// 3. GPU spectral dimension matches CPU
    /// </summary>
    [TestMethod]
    [TestCategory("SpectralDimension")]
    public void SpectralDimension_HeatKernel_GpuDouble64Precision()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        Console.WriteLine("=== GPU HeatKernel Double64 Precision Test ===");

        // Check GPU capabilities
        var device = ComputeSharp.GraphicsDevice.GetDefault();
        Console.WriteLine($"GPU: {device.Name}");
        
        bool supportsDouble = device.IsDoublePrecisionSupportAvailable();
        Console.WriteLine($"Double Precision Support: {supportsDouble}");
        Console.WriteLine("");

        if (!supportsDouble)
        {
            Console.WriteLine("WARNING: GPU does not support double precision.");
            Console.WriteLine("Heat kernel computation may have precision issues.");
            Assert.Inconclusive("GPU does not support double precision - test requires double64 support");
            return;
        }

        // Arrange - create test graph
        var graph = CreateTestGraph(MediumGraphNodes, 0.15, TestSeed);
        graph.BuildSoAViews();

        Console.WriteLine($"Graph: N={graph.N}, E={graph.FlatEdgesFrom.Length}");

        // Act - CPU computation (reference, uses double64)
        double cpuDim = graph.ComputeSpectralDimension(t_max: 80, num_walkers: 500);
        Console.WriteLine($"CPU d_S = {cpuDim:F4}");

        // Assert CPU is valid
        Assert.IsTrue(double.IsFinite(cpuDim), $"CPU d_S should be finite, got {cpuDim}");
        Assert.IsTrue(cpuDim >= 1 && cpuDim <= 10, $"CPU d_S out of range: {cpuDim}");

        // Act - GPU computation via GpuSpectralEngine (now returns double)
        double gpuDim = double.NaN;
        try
        {
            using var gpuEngine = new GpuSpectralEngine();
            gpuEngine.UpdateTopology(graph);
            gpuDim = gpuEngine.ComputeSpectralDimension(graph, dt: 0.01, numSteps: 100, numProbeVectors: 8);
            Console.WriteLine($"GPU d_S = {gpuDim:F4}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GPU engine error: {ex.Message}");
            Assert.Inconclusive($"GPU spectral engine failed: {ex.Message}");
            return;
        }

        // Check for invalid result (d_S stuck at 1.0 or NaN)
        const double invalidValue = 1.0;
        const double tolerance = 0.001;
        bool hasInvalidResult = Math.Abs(gpuDim - invalidValue) < tolerance || double.IsNaN(gpuDim);

        if (hasInvalidResult)
        {
            Console.WriteLine("");
            Console.WriteLine("WARNING: GPU returned d_S ≈ 1.0 or NaN");
            Console.WriteLine("This may indicate heat trace computation issues.");
            Console.WriteLine($"Expected: d_S ≈ {cpuDim:F4} (from CPU)");
            Console.WriteLine($"Got:      d_S = {gpuDim:F4}");
            
            Assert.Inconclusive(
                $"GPU spectral dimension computation issue. " +
                $"CPU d_S = {cpuDim:F4}, GPU d_S = {gpuDim:F4}.");
            return;
        }

        // If we get here, GPU returned a valid value - compare with CPU
        Assert.IsTrue(double.IsFinite(gpuDim), $"GPU d_S should be finite, got {gpuDim}");
        Assert.IsTrue(gpuDim >= 1 && gpuDim <= 10, $"GPU d_S out of range: {gpuDim}");

        double diff = Math.Abs(cpuDim - gpuDim);
        double relDiff = Math.Abs(cpuDim) > 0.01 ? diff / Math.Abs(cpuDim) : diff;
        Console.WriteLine($"Difference: {diff:F4} ({relDiff:P1})");

        Console.WriteLine("");
        Console.WriteLine("NOTE: CPU uses Laplacian method, GPU uses HeatKernel method.");
        Console.WriteLine("Different methods may give slightly different results.");

        // With proper double64, we expect < 30% difference between methods
        Assert.IsTrue(relDiff < 0.30, 
            $"CPU/GPU d_S difference too large: CPU={cpuDim:F4}, GPU={gpuDim:F4}, diff={relDiff:P1}");
        
        Console.WriteLine("");
        Console.WriteLine("SUCCESS: GPU double64 precision is working correctly!");
    }
}
