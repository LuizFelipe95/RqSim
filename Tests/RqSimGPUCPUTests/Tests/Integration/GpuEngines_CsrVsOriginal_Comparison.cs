using System;
using ComputeSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RQSimulation;
using RQSimulation.GPUCompressedSparseRow;
using RQSimulation.GPUOptimized.CayleyEvolution;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    [TestMethod]
    [TestCategory("Integration")]
    public void GpuEngines_CsrVsOriginal_Comparison()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // 1. Setup Graph
        int N = 50;
        var graph = CreateTestGraph(N, 0.2, TestSeed);
        graph.BuildSoAViews();

        // 2. Prepare Data
        bool[,] edges = new bool[N, N];
        double[,] weights = new double[N, N];
        int nnz = 0;

        for (int i = 0; i < N; i++)
        {
            foreach (var neighbor in graph.Neighbors(i))
            {
                edges[i, neighbor] = true;
                weights[i, neighbor] = graph.Weights[i, neighbor];
                nnz++;
            }
        }

        // 3. Initialize CSR Engine
        using var csrEngine = new GpuCayleyEvolutionEngineCsr();
        csrEngine.InitializeFromDense(edges, weights);

        // 4. Initialize Original Engine
        using var originalEngine = new GpuCayleyEvolutionEngineDouble();
        if (!originalEngine.IsDoublePrecisionSupported)
        {
            Assert.Inconclusive("Double precision not supported for Original Engine");
            return;
        }

        // Prepare CSR arrays for Original Engine
        int[] csrOffsets = new int[N + 1];
        int[] csrColumns = new int[nnz];
        double[] csrValues = new double[nnz];
        double[] potential = new double[N]; // Zero potential for now

        int currentIdx = 0;
        for (int i = 0; i < N; i++)
        {
            csrOffsets[i] = currentIdx;
            // Sort neighbors to ensure consistent order
            var neighbors = new System.Collections.Generic.List<int>();
            foreach (var n in graph.Neighbors(i)) neighbors.Add(n);
            neighbors.Sort();

            foreach (var neighbor in neighbors)
            {
                csrColumns[currentIdx] = neighbor;
                csrValues[currentIdx] = weights[i, neighbor];
                currentIdx++;
            }
        }
        csrOffsets[N] = currentIdx;

        originalEngine.Initialize(N, 1, nnz);
        originalEngine.UploadHamiltonian(csrOffsets, csrColumns, csrValues, potential);

        // 5. Set Initial Wavefunction
        double[] psiReal = new double[N];
        double[] psiImag = new double[N];
        double[] psiRealOrig = new double[N];
        double[] psiImagOrig = new double[N];

        // Normalize
        double norm = 0;
        var rng = new Random(TestSeed);
        for (int i = 0; i < N; i++)
        {
            psiReal[i] = rng.NextDouble(); // Random state
            norm += psiReal[i] * psiReal[i];
        }
        norm = Math.Sqrt(norm);
        for (int i = 0; i < N; i++)
        {
            psiReal[i] /= norm;
            psiRealOrig[i] = psiReal[i];
            psiImagOrig[i] = psiImag[i];
        }

        csrEngine.UploadWavefunction(psiReal, psiImag);

        // 6. Evolve
        double dt = 0.1;

        // CSR Evolve
        csrEngine.EvolveStep(dt);
        csrEngine.DownloadWavefunction(psiReal, psiImag);

        // Original Evolve
        originalEngine.EvolveUnitary(psiRealOrig, psiImagOrig, dt);

        // 7. Compare
        var resultReal = CompareArrays(psiRealOrig, psiReal, "PsiReal");
        var resultImag = CompareArrays(psiImagOrig, psiImag, "PsiImag");

        Console.WriteLine($"Real comparison: {resultReal}");
        Console.WriteLine($"Imag comparison: {resultImag}");

        Assert.IsTrue(resultReal.Passed, $"Real part mismatch: {resultReal.Message}");
        Assert.IsTrue(resultImag.Passed, $"Imag part mismatch: {resultImag.Message}");
    }
}
