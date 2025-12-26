using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

public partial class RqSimGPUCPUTest
{
    /// <summary>
    /// Test: SpectralDimensionValidator spinor compatibility check.
    /// </summary>
    [TestMethod]
    [TestCategory("SpectralDimension")]
    public void SpectralDimensionValidator_SpinorCompatibility_ClassifiesCorrectly()
    {
        // Test various spectral dimensions for spinor compatibility
        // Spinors require d_S ? 4 (within 10% tolerance: 3.6 to 4.4)

        Console.WriteLine("Testing spinor compatibility classification:");

        var testCases = new[]
        {
            (dim: 1.0, expected: false, desc: "1D (too low)"),
            (dim: 2.5, expected: false, desc: "2.5D fractal (too low)"),
            (dim: 3.5, expected: false, desc: "3.5D (just below threshold)"),
            (dim: 3.6, expected: true, desc: "3.6D (at lower threshold)"),
            (dim: 4.0, expected: true, desc: "4.0D (exact target)"),
            (dim: 4.4, expected: true, desc: "4.4D (at upper threshold)"),
            (dim: 4.5, expected: false, desc: "4.5D (just above threshold)"),
            (dim: 6.0, expected: false, desc: "6D (too high)"),
        };

        foreach (var (dim, expected, desc) in testCases)
        {
            bool actual = SpectralDimensionValidator.ShouldEnableSpinorFields(dim);
            string result = actual == expected ? "PASS" : "FAIL";
            Console.WriteLine($"  {desc}: d_S={dim}, expected={expected}, actual={actual} [{result}]");
            Assert.AreEqual(expected, actual, $"Spinor compatibility mismatch for {desc}");
        }
    }
}
