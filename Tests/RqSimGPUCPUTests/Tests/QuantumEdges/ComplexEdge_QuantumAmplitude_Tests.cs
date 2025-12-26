using RQSimulation;
using System.Numerics;

namespace RqSimGPUCPUTests.Tests.QuantumEdges;

/// <summary>
/// Unit tests for ComplexEdge quantum amplitude functionality.
/// Tests the Quantum Graphity implementation (Modernization Stage 6).
/// </summary>
[TestClass]
public class ComplexEdge_QuantumAmplitude_Tests
{
    private const double Tolerance = 1e-10;

    #region ComplexEdge.Superposition Tests

    [TestMethod]
    public void Superposition_EqualAmplitudes_NormalizesCorrectly()
    {
        // Arrange: 50-50 superposition
        Complex alpha = new Complex(1.0, 0.0);
        Complex beta = new Complex(1.0, 0.0);

        // Act
        var edge = ComplexEdge.Superposition(alpha, beta);

        // Assert: Normalized amplitude magnitude should be 1/sqrt(2)
        double expectedAmplitudeMag = 1.0 / Math.Sqrt(2.0);
        Assert.AreEqual(expectedAmplitudeMag, edge.Amplitude.Magnitude, Tolerance,
            "Superposition should normalize amplitude to 1/sqrt(2) for equal coefficients");

        // Existence probability should be 0.5
        Assert.AreEqual(0.5, edge.ExistenceProbability, Tolerance,
            "Equal superposition should have 50% existence probability");
        
        // Classical weight for superposition defaults to 0.5
        Assert.AreEqual(0.5, edge.GetMagnitude(), Tolerance,
            "Classical weight for superposition should be 0.5");
    }

    [TestMethod]
    public void Superposition_PureExistence_HasFullProbability()
    {
        // Arrange: Pure |exists> state
        Complex alpha = new Complex(1.0, 0.0);
        Complex beta = Complex.Zero;

        // Act
        var edge = ComplexEdge.Superposition(alpha, beta);

        // Assert: Probability should be 1.0
        Assert.AreEqual(1.0, edge.ExistenceProbability, Tolerance,
            "Pure exists state should have 100% probability");
    }

    [TestMethod]
    public void Superposition_PureNonExistence_HasZeroProbability()
    {
        // Arrange: Pure |not-exists> state
        Complex alpha = Complex.Zero;
        Complex beta = new Complex(1.0, 0.0);

        // Act
        var edge = ComplexEdge.Superposition(alpha, beta);

        // Assert: Probability should be 0.0
        Assert.AreEqual(0.0, edge.ExistenceProbability, Tolerance,
            "Pure not-exists state should have 0% probability");
    }

    [TestMethod]
    public void Superposition_ComplexAmplitudes_PreservesPhase()
    {
        // Arrange: Complex amplitude with phase
        double magnitude = 1.0;
        double phase = Math.PI / 4; // 45 degrees
        Complex alpha = Complex.FromPolarCoordinates(magnitude, phase);
        Complex beta = Complex.Zero;

        // Act
        var edge = ComplexEdge.Superposition(alpha, beta);

        // Assert: Phase should be preserved
        Assert.AreEqual(phase, edge.Phase, Tolerance,
            "Superposition should preserve phase of alpha amplitude");
    }

    [TestMethod]
    public void Superposition_ZeroAmplitudes_ReturnsZeroEdge()
    {
        // Arrange: Both zero
        Complex alpha = Complex.Zero;
        Complex beta = Complex.Zero;

        // Act
        var edge = ComplexEdge.Superposition(alpha, beta);

        // Assert
        Assert.AreEqual(0.0, edge.GetMagnitude(), Tolerance,
            "Zero amplitudes should produce zero edge");
        Assert.AreEqual(0.0, edge.ExistenceProbability, Tolerance,
            "Zero edge should have zero probability");
    }

    [TestMethod]
    public void Superposition_UnequalAmplitudes_CorrectProbability()
    {
        // Arrange: 3:1 amplitude ratio (9:1 probability ratio)
        Complex alpha = new Complex(3.0, 0.0);
        Complex beta = new Complex(1.0, 0.0);

        // Act
        var edge = ComplexEdge.Superposition(alpha, beta);

        // Assert: |?|? / (|?|? + |?|?) = 9/10 = 0.9
        Assert.AreEqual(0.9, edge.ExistenceProbability, Tolerance,
            "3:1 amplitude ratio should give 90% probability");
    }

    #endregion

    #region ComplexEdge.Measure Tests

    [TestMethod]
    public void Measure_HighProbability_MostlyReturnsTrue()
    {
        // Arrange: Edge with 95% existence probability
        // Use the constructor that takes a Complex directly for full control
        // For P = 0.95, amplitude magnitude = sqrt(0.95) ? 0.9746794
        var amplitude = Complex.FromPolarCoordinates(Math.Sqrt(0.95), 0.0);
        var edge = new ComplexEdge(amplitude);
        var rng = new Random(42);
        int trueCount = 0;
        int trials = 10000;

        // Act
        for (int i = 0; i < trials; i++)
        {
            if (edge.Measure(rng))
                trueCount++;
        }

        // Assert: Should be approximately 95% true
        double observedProbability = (double)trueCount / trials;
        Assert.AreEqual(0.95, observedProbability, 0.02,
            $"Expected ~95% true, got {observedProbability:P2}");
    }

    [TestMethod]
    public void Measure_ZeroProbability_AlwaysReturnsFalse()
    {
        // Arrange
        var edge = ComplexEdge.NotExists();
        var rng = new Random(42);

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            Assert.IsFalse(edge.Measure(rng),
                "Zero probability edge should never exist after measurement");
        }
    }

    [TestMethod]
    public void Measure_FullProbability_AlwaysReturnsTrue()
    {
        // Arrange
        var edge = ComplexEdge.Exists(1.0);
        var rng = new Random(42);

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            Assert.IsTrue(edge.Measure(rng),
                "Full probability edge should always exist after measurement");
        }
    }

    [TestMethod]
    public void Measure_NullRng_ThrowsArgumentNullException()
    {
        // Arrange
        var edge = new ComplexEdge(0.5, 0.0);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => edge.Measure(null!));
    }

    #endregion

    #region ComplexEdge Factory Methods

    [TestMethod]
    public void Exists_CreatesDefiniteState()
    {
        // Act
        var edge = ComplexEdge.Exists(0.75, Math.PI / 6);

        // Assert
        Assert.AreEqual(0.75, edge.GetMagnitude(), Tolerance,
            "Classical weight should be preserved");
        Assert.AreEqual(Math.PI / 6, edge.Phase, Tolerance,
            "Phase should be preserved");
        // Exists() creates a DEFINITE exists state, so probability = 1.0
        Assert.AreEqual(1.0, edge.ExistenceProbability, Tolerance,
            "Definite exists state should have P = 1.0");
    }

    [TestMethod]
    public void NotExists_CreatesZeroState()
    {
        // Act
        var edge = ComplexEdge.NotExists();

        // Assert
        Assert.AreEqual(0.0, edge.GetMagnitude(), Tolerance);
        Assert.AreEqual(0.0, edge.ExistenceProbability, Tolerance);
    }

    [TestMethod]
    public void IsClassical_HighProbability_ReturnsTrue()
    {
        // Arrange
        var classicalEdge = ComplexEdge.Exists(1.0);

        // Assert
        Assert.IsTrue(classicalEdge.IsClassical(0.99),
            "Edge with P=1 should be classical");
    }

    [TestMethod]
    public void IsClassical_Superposition_ReturnsFalse()
    {
        // Arrange
        var superpositionEdge = ComplexEdge.Superposition(
            new Complex(1.0, 0.0),
            new Complex(1.0, 0.0));

        // Assert
        Assert.IsFalse(superpositionEdge.IsClassical(0.99),
            "50-50 superposition should not be classical");
    }

    #endregion

    #region ComplexEdge With* Methods

    [TestMethod]
    public void WithMagnitude_PreservesPhase()
    {
        // Arrange
        var original = new ComplexEdge(0.5, Math.PI / 3);

        // Act
        var modified = original.WithMagnitude(0.8);

        // Assert
        Assert.AreEqual(0.8, modified.GetMagnitude(), Tolerance);
        Assert.AreEqual(Math.PI / 3, modified.Phase, Tolerance,
            "WithMagnitude should preserve phase");
    }

    [TestMethod]
    public void WithPhase_PreservesMagnitude()
    {
        // Arrange
        var original = new ComplexEdge(0.5, Math.PI / 3);

        // Act
        var modified = original.WithPhase(Math.PI / 6);

        // Assert
        Assert.AreEqual(0.5, modified.GetMagnitude(), Tolerance,
            "WithPhase should preserve magnitude");
        Assert.AreEqual(Math.PI / 6, modified.Phase, Tolerance);
    }

    #endregion
}
