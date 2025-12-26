using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: GPU histogram computation.
    /// </summary>
    [TestMethod]
    [TestCategory("Statistics")]
    public void StatisticsEngine_Histogram_Works()
    {
        if (!IsGpuAvailable())
        {
            Assert.Inconclusive("GPU not available for this test");
            return;
        }

        // Arrange
        var rng = new Random(TestSeed);
        int size = 10000;
        float[] values = new float[size];

        // Create values with known distribution (uniform 0-1)
        for (int i = 0; i < size; i++)
        {
            values[i] = (float)rng.NextDouble();
        }

        Console.WriteLine($"Input: {size} values in [0, 1]");

        using var statsEngine = new StatisticsEngine();
        statsEngine.Initialize(size);

        // Act - Compute histogram (using correct method name)
        int[] histogram = statsEngine.ComputeHistogram(values);

        Console.WriteLine("Histogram (256 bins):");
        int totalCount = 0;
        int nonZeroBins = 0;
        for (int i = 0; i < histogram.Length; i++)
        {
            totalCount += histogram[i];
            if (histogram[i] > 0) nonZeroBins++;
        }
        Console.WriteLine($"  Total count: {totalCount}");
        Console.WriteLine($"  Non-zero bins: {nonZeroBins}");
        Console.WriteLine($"  Expected per bin (uniform): {size / 256.0:F1}");

        // Assert
        Assert.AreEqual(256, histogram.Length, "Histogram should have 256 bins");
        Assert.AreEqual(size, totalCount, $"Total histogram count should equal input size ({size})");
        // For uniform distribution, most bins should be non-empty
        Assert.IsTrue(nonZeroBins > 200, $"Most bins should be non-empty for uniform distribution, got {nonZeroBins}");
    }
}
