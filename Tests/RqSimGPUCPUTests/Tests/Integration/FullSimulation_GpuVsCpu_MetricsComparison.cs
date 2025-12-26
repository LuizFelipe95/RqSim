using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Integration test: Run full simulation on CPU and GPU, compare final metrics.
    /// </summary>
    [TestMethod]
    [TestCategory("Integration")]
    public void FullSimulation_GpuVsCpu_MetricsComparison()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var config = new SimulationConfig
        {
            NodeCount = 100,
            InitialEdgeProb = 0.1,
            InitialExcitedProb = 0.1,
            TargetDegree = 6,
            Seed = TestSeed,
            TotalSteps = 50,
            UseNetworkGravity = true,
            UseSpectralGeometry = true
        };

        var engineCpu = new SimulationEngine(config);
        var engineGpu = new SimulationEngine(config);

        var graphCpu = engineCpu.Graph;
        var graphGpu = engineGpu.Graph;

        Console.WriteLine($"Config: N={config.NodeCount}, Steps={config.TotalSteps}");

        // Run CPU simulation
        var cpuStopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int step = 0; step < config.TotalSteps; step++)
        {
            double dt = 0.01;
            ImprovedNetworkGravity.EvolveNetworkGeometryForman(graphCpu, dt, 0.05);
        }
        cpuStopwatch.Stop();

        // Run GPU simulation
        graphGpu.InitGpuGravity();
        var gpuStopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            for (int step = 0; step < config.TotalSteps; step++)
            {
                double dt = 0.01;
                ImprovedNetworkGravity.EvolveNetworkGeometryOllivierDynamic(graphGpu, dt, 0.05);
            }
        }
        finally
        {
            graphGpu.DisposeGpuGravity();
        }
        gpuStopwatch.Stop();

        // Compare metrics
        Console.WriteLine($"CPU Time: {cpuStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"GPU Time: {gpuStopwatch.ElapsedMilliseconds}ms");

        double cpuTotalWeight = 0, gpuTotalWeight = 0;
        graphCpu.BuildSoAViews();
        graphGpu.BuildSoAViews();

        for (int e = 0; e < graphCpu.FlatEdgesFrom.Length; e++)
        {
            int i = graphCpu.FlatEdgesFrom[e];
            int j = graphCpu.FlatEdgesTo[e];
            cpuTotalWeight += graphCpu.Weights[i, j];
            gpuTotalWeight += graphGpu.Weights[i, j];
        }

        Console.WriteLine($"CPU Total Weight: {cpuTotalWeight:F4}");
        Console.WriteLine($"GPU Total Weight: {gpuTotalWeight:F4}");

        double cpuDim = graphCpu.ComputeSpectralDimension(t_max: 50, num_walkers: 100);
        double gpuDim = graphGpu.ComputeSpectralDimension(t_max: 50, num_walkers: 100);

        Console.WriteLine($"CPU d_S: {cpuDim:F4}");
        Console.WriteLine($"GPU d_S: {gpuDim:F4}");

        // Assert - both simulations should produce physically reasonable results
        Assert.IsTrue(cpuTotalWeight > 0, "CPU total weight should be positive");
        Assert.IsTrue(gpuTotalWeight > 0, "GPU total weight should be positive");
        Assert.IsTrue(cpuDim > 0, "CPU spectral dimension should be positive");
        Assert.IsTrue(gpuDim > 0, "GPU spectral dimension should be positive");
    }
}
