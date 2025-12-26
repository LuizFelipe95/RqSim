using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: SpectralDimensionValidator suspicious stability detection.
    /// </summary>
    [TestMethod]
    [TestCategory("SpectralDimension")]
    public void SpectralDimensionValidator_SuspiciousStability_DetectedCorrectly()
    {
        // Arrange - reset history
        SpectralDimensionValidator.ResetHistory();

        Console.WriteLine("Testing suspicious stability detection (d_S = 1.0 pattern)");

        // Act - Feed exactly 1.0 values repeatedly
        for (int i = 0; i < 15; i++)
        {
            var status = SpectralDimensionValidator.CheckForSuspiciousStability(1.0);
            if (status.IsSuspicious)
            {
                Console.WriteLine($"Suspicious detected at iteration {i}:");
                Console.WriteLine($"  ConsecutiveExactOne: {status.ConsecutiveExactOneCount}");
                Console.WriteLine($"  Diagnosis: {status.Diagnosis}");
            }
        }

        var finalStatus = SpectralDimensionValidator.CheckForSuspiciousStability(1.0);

        // Assert - should detect as suspicious after many 1.0 values
        Assert.IsTrue(finalStatus.IsSuspicious, "Should detect suspicious stability after many d_S = 1.0 values");
        Assert.IsTrue(finalStatus.ConsecutiveExactOneCount >= 10, 
            $"Should have >= 10 consecutive 1.0 values, got {finalStatus.ConsecutiveExactOneCount}");

        // Reset and verify normal values don't trigger
        SpectralDimensionValidator.ResetHistory();
        var normalStatus = SpectralDimensionValidator.CheckForSuspiciousStability(3.5);
        Assert.IsFalse(normalStatus.IsSuspicious, "Normal d_S values should not be suspicious");
    }
}
