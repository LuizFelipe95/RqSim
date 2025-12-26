using RQSimulation;
using RQSimulation.EventBasedModel;
using RQSimulation.GPUOptimized;

namespace RqSimGPUCPUTests;

/// <summary>
/// GPU vs CPU comparison tests for RQ Simulation.
/// These tests verify that GPU and CPU implementations produce consistent results.
/// Discrepancies beyond tolerance indicate bugs in either implementation.
/// Test categories:
/// 1. Forman-Ricci Curvature
/// 2. Ollivier-Ricci Curvature  
/// 3. Gravity Evolution (weight updates)
/// 4. Scalar Field Diffusion
/// 5. Spectral Dimension Computation
/// 6. Statistics Engine (sum, histogram)
/// </summary>
[TestClass]
public partial class RqSimGPUCPUTest
{
    // Tolerance for floating-point comparisons (GPU uses float32, CPU uses float64)
    private const double RelativeTolerance = 0.05; // 5% relative difference allowed
    private const double AbsoluteTolerance = 1e-5; // For values near zero
    
    // Standard test graph sizes
    private const int SmallGraphNodes = 50;
    private const int MediumGraphNodes = 200;
    private const int LargeGraphNodes = 500;
    
    // Random seed for reproducibility
    private const int TestSeed = 42;

    #region Assembly and Class Initialization

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        // Log GPU availability at start
        bool gpuAvailable = IsGpuAvailable();
        Console.WriteLine($"GPU Available: {gpuAvailable}");
        if (gpuAvailable)
        {
            var device = ComputeSharp.GraphicsDevice.GetDefault();
            Console.WriteLine($"GPU Device: {device.Name}");
        }
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
    }

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
    }

    [TestInitialize]
    public void TestInit()
    {
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test graph with reproducible random topology.
    /// </summary>
    protected static RQGraph CreateTestGraph(int nodeCount, double edgeProb, int seed)
    {
        var config = new SimulationConfig
        {
            NodeCount = nodeCount,
            InitialEdgeProb = edgeProb,
            InitialExcitedProb = 0.1,
            TargetDegree = 6,
            Seed = seed
        };
        var engine = new SimulationEngine(config);
        return engine.Graph;
    }

    /// <summary>
    /// Compares two arrays and returns detailed comparison results.
    /// </summary>
    protected static ComparisonResult CompareArrays(double[] cpu, double[] gpu, string metricName)
    {
        if (cpu.Length != gpu.Length)
        {
            return new ComparisonResult
            {
                MetricName = metricName,
                Passed = false,
                Message = $"Array length mismatch: CPU={cpu.Length}, GPU={gpu.Length}"
            };
        }

        double maxAbsDiff = 0;
        double maxRelDiff = 0;
        int maxDiffIndex = -1;
        int mismatchCount = 0;
        double sumSquaredDiff = 0;

        for (int i = 0; i < cpu.Length; i++)
        {
            double absDiff = Math.Abs(cpu[i] - gpu[i]);
            double relDiff = Math.Abs(cpu[i]) > AbsoluteTolerance 
                ? absDiff / Math.Abs(cpu[i]) 
                : absDiff;

            sumSquaredDiff += absDiff * absDiff;

            if (absDiff > maxAbsDiff)
            {
                maxAbsDiff = absDiff;
                maxRelDiff = relDiff;
                maxDiffIndex = i;
            }

            if (relDiff > RelativeTolerance && absDiff > AbsoluteTolerance)
            {
                mismatchCount++;
            }
        }

        double rmse = Math.Sqrt(sumSquaredDiff / cpu.Length);
        bool passed = maxRelDiff <= RelativeTolerance || maxAbsDiff <= AbsoluteTolerance;

        return new ComparisonResult
        {
            MetricName = metricName,
            Passed = passed,
            MaxAbsoluteDifference = maxAbsDiff,
            MaxRelativeDifference = maxRelDiff,
            MaxDiffIndex = maxDiffIndex,
            MismatchCount = mismatchCount,
            RMSE = rmse,
            CpuValueAtMaxDiff = maxDiffIndex >= 0 ? cpu[maxDiffIndex] : 0,
            GpuValueAtMaxDiff = maxDiffIndex >= 0 ? gpu[maxDiffIndex] : 0,
            Message = passed 
                ? $"MATCH: MaxRelDiff={maxRelDiff:P2}, RMSE={rmse:E3}"
                : $"MISMATCH: {mismatchCount}/{cpu.Length} values differ. MaxRelDiff={maxRelDiff:P2} at [{maxDiffIndex}] (CPU={cpu[maxDiffIndex]:E4}, GPU={gpu[maxDiffIndex]:E4})"
        };
    }

    /// <summary>
    /// Compares single values.
    /// </summary>
    protected static ComparisonResult CompareValues(double cpu, double gpu, string metricName)
    {
        double absDiff = Math.Abs(cpu - gpu);
        double relDiff = Math.Abs(cpu) > AbsoluteTolerance ? absDiff / Math.Abs(cpu) : absDiff;
        bool passed = relDiff <= RelativeTolerance || absDiff <= AbsoluteTolerance;

        return new ComparisonResult
        {
            MetricName = metricName,
            Passed = passed,
            MaxAbsoluteDifference = absDiff,
            MaxRelativeDifference = relDiff,
            CpuValueAtMaxDiff = cpu,
            GpuValueAtMaxDiff = gpu,
            Message = passed
                ? $"MATCH: CPU={cpu:F6}, GPU={gpu:F6}, Diff={relDiff:P2}"
                : $"MISMATCH: CPU={cpu:F6}, GPU={gpu:F6}, Diff={relDiff:P2}"
        };
    }

    /// <summary>
    /// Checks if GPU is available for testing.
    /// </summary>
    protected static bool IsGpuAvailable()
    {
        try
        {
            var device = ComputeSharp.GraphicsDevice.GetDefault();
            return device != null;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Comparison Result Structure

    /// <summary>
    /// Detailed comparison result for test reporting.
    /// </summary>
    public record ComparisonResult
    {
        public required string MetricName { get; init; }
        public bool Passed { get; init; }
        public double MaxAbsoluteDifference { get; init; }
        public double MaxRelativeDifference { get; init; }
        public int MaxDiffIndex { get; init; }
        public int MismatchCount { get; init; }
        public double RMSE { get; init; }
        public double CpuValueAtMaxDiff { get; init; }
        public double GpuValueAtMaxDiff { get; init; }
        public required string Message { get; init; }

        public override string ToString() => $"[{MetricName}] {Message}";
    }

    #endregion
}
