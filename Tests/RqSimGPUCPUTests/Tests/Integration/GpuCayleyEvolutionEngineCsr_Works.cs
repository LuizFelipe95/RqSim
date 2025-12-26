using System;
using ComputeSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RQSimulation;
using RQSimulation.GPUCompressedSparseRow;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    [TestMethod]
    [TestCategory("Integration")]
    public void GpuCayleyEvolutionEngineCsr_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // 1. Setup
        int N = 50;
        var graph = CreateTestGraph(N, 0.2, TestSeed);
        graph.BuildSoAViews();

        // Extract dense matrices for initialization
        bool[,] edges = new bool[N, N];
        double[,] weights = new double[N, N];
        
        for (int i = 0; i < N; i++)
        {
            foreach (var neighbor in graph.Neighbors(i))
            {
                edges[i, neighbor] = true;
                weights[i, neighbor] = graph.Weights[i, neighbor];
            }
        }

        // 2. Initialize Engine
        using var engine = new GpuCayleyEvolutionEngineCsr();
        engine.InitializeFromDense(edges, weights);

        // 3. Set Initial Wavefunction (Gaussian packet or random)
        double[] psiReal = new double[N];
        double[] psiImag = new double[N];
        
        // Normalize
        double norm = 0;
        // Use a localized state (delta at node 0) to ensure evolution
        psiReal[0] = 1.0;
        norm = 1.0;
        
        // Or use random state
        /*
        var rng = new Random(TestSeed);
        for(int i=0; i<N; i++) {
            psiReal[i] = rng.NextDouble();
            norm += psiReal[i]*psiReal[i];
        }
        norm = Math.Sqrt(norm);
        for(int i=0; i<N; i++) psiReal[i] /= norm;
        */

        engine.UploadWavefunction(psiReal, psiImag);

        // 4. Evolve
        double dt = 0.1;
        int steps = 10;
        int iterations = engine.Evolve(dt, steps);

        Console.WriteLine($"Total iterations: {iterations}");

        // 5. Check Norm (Unitarity)
        double finalNorm = engine.ComputeNorm();
        Console.WriteLine($"Final Norm: {finalNorm}");
        Assert.AreEqual(1.0, finalNorm, 1e-10, "Unitarity violation");

        // 6. Check Evolution (State changed)
        double[] finalReal = new double[N];
        double[] finalImag = new double[N];
        engine.DownloadWavefunction(finalReal, finalImag);

        bool changed = false;
        for(int i=0; i<N; i++)
        {
            if (Math.Abs(finalReal[i] - psiReal[i]) > 1e-5 || Math.Abs(finalImag[i] - psiImag[i]) > 1e-5)
            {
                changed = true;
                break;
            }
        }
        Assert.IsTrue(changed, "Wavefunction did not evolve");
    }
}
