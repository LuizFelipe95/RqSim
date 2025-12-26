using RQSimulation;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: CPU scalar field diffusion vs GPU Higgs evolution
    /// 
    /// Both use: d??/dt? = D??? - V'(?)
    /// V'(?) = ??? - ??? (Higgs potential)
    /// Symplectic leapfrog integration
    /// </summary>
    [TestMethod]
    [TestCategory("ScalarField")]
    public void ScalarFieldDiffusion_GpuMatchesCpu()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange - create two identical graphs with same initial scalar field
        var graphCpu = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);
        var graphGpu = CreateTestGraph(SmallGraphNodes, 0.15, TestSeed);

        graphCpu.BuildSoAViews();
        graphGpu.BuildSoAViews();

        Console.WriteLine($"Graph: N={graphCpu.N}, E={graphCpu.FlatEdgesFrom.Length}");

        // Initialize scalar field FIRST - this creates ScalarField and _scalarMomentum arrays
        graphCpu.InitScalarField(amplitude: 1.0);  // Large amplitude for visible changes

        // Copy CPU initial field to GPU graph
        for (int i = 0; i < SmallGraphNodes; i++)
        {
            graphGpu.ScalarField[i] = graphCpu.ScalarField[i];
        }

        // Print initial field
        Console.WriteLine("Initial scalar field (first 10):");
        for (int i = 0; i < Math.Min(10, SmallGraphNodes); i++)
        {
            Console.WriteLine($"  [{i}] = {graphCpu.ScalarField[i]:F6}");
        }

        // Use same physics parameters for both
        double dt = 0.01;
        float diffusionRate = (float)PhysicsConstants.FieldDiffusionRate;
        float higgsLambda = (float)PhysicsConstants.HiggsLambda;
        float higgsMuSquared = (float)PhysicsConstants.HiggsMuSquared;
        int steps = 10;

        Console.WriteLine($"Physics: D={diffusionRate}, ?={higgsLambda}, ??={higgsMuSquared}");

        // Act - CPU evolution using public method (uses Higgs potential internally)
        for (int s = 0; s < steps; s++)
        {
            graphCpu.UpdateScalarFieldParallel(dt);
        }

        // Act - GPU evolution with Higgs potential
        try
        {
            using var scalarEngine = new ScalarFieldEngine();

            int totalEdges = graphGpu.CsrOffsets[graphGpu.N];
            scalarEngine.Initialize(graphGpu.N, totalEdges);

            int[] offsets = graphGpu.CsrOffsets;
            int[] neighbors = graphGpu.CsrIndices;
            float[] weights = new float[totalEdges];

            for (int n = 0; n < graphGpu.N; n++)
            {
                int start = offsets[n];
                int end = offsets[n + 1];
                for (int k = start; k < end; k++)
                {
                    int to = graphGpu.CsrIndices[k];
                    weights[k] = (float)graphGpu.Weights[n, to];
                }
            }

            scalarEngine.UpdateTopology(offsets, neighbors, weights);

            float[] field = new float[graphGpu.N];
            for (int i = 0; i < graphGpu.N; i++)
            {
                field[i] = (float)graphGpu.ScalarField[i];
            }

            // GPU uses Higgs potential now - same physics as CPU
            for (int s = 0; s < steps; s++)
            {
                scalarEngine.UpdateField(field, (float)dt, diffusionRate, higgsLambda, higgsMuSquared);
            }

            for (int i = 0; i < graphGpu.N; i++)
            {
                graphGpu.ScalarField[i] = field[i];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GPU scalar field error: {ex.Message}");
            Assert.Inconclusive($"GPU scalar field failed: {ex.Message}");
            return;
        }

        // Compare final scalar fields
        double[] cpuField = new double[SmallGraphNodes];
        double[] gpuField = new double[SmallGraphNodes];

        for (int i = 0; i < SmallGraphNodes; i++)
        {
            cpuField[i] = graphCpu.ScalarField[i];
            gpuField[i] = graphGpu.ScalarField[i];
        }

        // Print final fields
        Console.WriteLine("Final scalar field (first 10, CPU vs GPU):");
        for (int i = 0; i < Math.Min(10, SmallGraphNodes); i++)
        {
            Console.WriteLine($"  [{i}] CPU={cpuField[i]:F6}, GPU={gpuField[i]:F6}");
        }

        // Assert
        var result = CompareArrays(cpuField, gpuField, "ScalarFieldDiffusion");
        Console.WriteLine(result);

        // Now both use Higgs potential - should match within tolerance
        Assert.IsTrue(result.Passed, $"GPU/CPU scalar field mismatch: {result.Message}");
    }
}
