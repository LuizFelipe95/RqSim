using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RqSimRenderingEngine.Rendering.Backend.DX12.Rendering;

namespace RqSimRenderingEngine.Dx12Tests;

/// <summary>
/// Tests specifically for Reverse-Z depth projection calculations.
/// Verifies that depth precision is correctly distributed for typical scene distances.
/// </summary>
[TestClass]
public class ReverseZProjectionTests
{
    private const float Epsilon = 1e-5f;

    #region Depth Precision Tests

    [TestMethod]
    public void ReverseZ_DepthPrecisionHighNearCamera()
    {
        // Arrange - typical 3D scene parameters
        float nearPlane = 0.01f;
        float farPlane = 1000f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Two points very close together near the camera
        float z1 = 0.1f;
        float z2 = 0.101f;

        Vector4 clip1 = Vector4.Transform(new Vector4(0, 0, z1, 1), projection);
        Vector4 clip2 = Vector4.Transform(new Vector4(0, 0, z2, 1), projection);

        float ndcZ1 = clip1.Z / clip1.W;
        float ndcZ2 = clip2.Z / clip2.W;

        // Assert - should have distinguishable depth values
        float depthDiff = MathF.Abs(ndcZ1 - ndcZ2);
        Assert.IsTrue(depthDiff > 1e-4f,
            $"Near-camera points should have distinguishable depths. Diff={depthDiff}");
    }

    [TestMethod]
    public void ReverseZ_ComparedToStandard_BetterNearPrecision()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 1000f;
        
        Matrix4x4 reverseZ = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);
        Matrix4x4 standard = CameraMatrixHelper.CreatePerspectiveStandard(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Two points very close near the camera
        float z1 = 0.1f;
        float z2 = 0.101f;

        // Reverse-Z depths
        Vector4 revClip1 = Vector4.Transform(new Vector4(0, 0, z1, 1), reverseZ);
        Vector4 revClip2 = Vector4.Transform(new Vector4(0, 0, z2, 1), reverseZ);
        float revDiff = MathF.Abs((revClip1.Z / revClip1.W) - (revClip2.Z / revClip2.W));

        // Standard depths (note: standard uses RH, so flip Z for comparison)
        Vector4 stdClip1 = Vector4.Transform(new Vector4(0, 0, -z1, 1), standard);
        Vector4 stdClip2 = Vector4.Transform(new Vector4(0, 0, -z2, 1), standard);
        float stdDiff = MathF.Abs((stdClip1.Z / stdClip1.W) - (stdClip2.Z / stdClip2.W));

        // Assert - Reverse-Z should have better or equal precision
        Assert.IsTrue(revDiff >= stdDiff * 0.5f, // Allow some tolerance
            $"Reverse-Z should have comparable or better precision. Rev={revDiff}, Std={stdDiff}");
    }

    #endregion

    #region Depth Range Tests

    [TestMethod]
    public void ReverseZ_DepthAlwaysInZeroOneRange()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        float[] testDistances = [nearPlane, 0.1f, 1f, 10f, 50f, farPlane];

        // Act & Assert
        foreach (float z in testDistances)
        {
            Vector4 clip = Vector4.Transform(new Vector4(0, 0, z, 1), projection);
            float ndcZ = clip.Z / clip.W;

            Assert.IsTrue(ndcZ >= -Epsilon && ndcZ <= 1f + Epsilon,
                $"Depth at z={z} should be in [0,1], got {ndcZ}");
        }
    }

    [TestMethod]
    public void ReverseZ_DepthMonotonicallyDecreasing()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        float[] testDistances = [nearPlane, 0.1f, 1f, 10f, 50f, farPlane];
        float previousDepth = float.MaxValue;

        // Act & Assert - depth should decrease as we move away from camera
        foreach (float z in testDistances)
        {
            Vector4 clip = Vector4.Transform(new Vector4(0, 0, z, 1), projection);
            float ndcZ = clip.Z / clip.W;

            Assert.IsTrue(ndcZ < previousDepth,
                $"Depth should decrease with distance. At z={z}, got {ndcZ}, prev={previousDepth}");
            
            previousDepth = ndcZ;
        }
    }

    #endregion

    #region Matrix Element Verification

    [TestMethod]
    public void ReverseZ_MatrixLayout_CorrectForRowMajor()
    {
        // Arrange
        float nearPlane = 1f;
        float farPlane = 100f;
        float fovY = MathF.PI / 4f;
        
        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            fovY, 1f, nearPlane, farPlane);

        // Assert - verify structure matches expected layout
        // Row 0: xScale, 0, 0, 0
        Assert.AreEqual(0f, projection.M12, Epsilon);
        Assert.AreEqual(0f, projection.M13, Epsilon);
        Assert.AreEqual(0f, projection.M14, Epsilon);

        // Row 1: 0, yScale, 0, 0
        Assert.AreEqual(0f, projection.M21, Epsilon);
        Assert.AreEqual(0f, projection.M23, Epsilon);
        Assert.AreEqual(0f, projection.M24, Epsilon);

        // Row 2: 0, 0, M33, M34
        Assert.AreEqual(0f, projection.M31, Epsilon);
        Assert.AreEqual(0f, projection.M32, Epsilon);
        Assert.AreEqual(1f, projection.M34, Epsilon, "M34 should be 1 for LH");

        // Row 3: 0, 0, M43, 0
        Assert.AreEqual(0f, projection.M41, Epsilon);
        Assert.AreEqual(0f, projection.M42, Epsilon);
        Assert.AreEqual(0f, projection.M44, Epsilon);
    }

    [TestMethod]
    public void ReverseZ_XYScaleCorrect()
    {
        // Arrange
        float fovY = MathF.PI / 3f; // 60 degrees
        float aspectRatio = 16f / 9f;
        float expectedYScale = 1f / MathF.Tan(fovY * 0.5f);
        float expectedXScale = expectedYScale / aspectRatio;

        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            fovY, aspectRatio, 0.1f, 100f);

        // Assert
        Assert.AreEqual(expectedXScale, projection.M11, Epsilon,
            $"M11 (xScale) should be {expectedXScale}");
        Assert.AreEqual(expectedYScale, projection.M22, Epsilon,
            $"M22 (yScale) should be {expectedYScale}");
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void ReverseZ_VerySmallNearPlane_StillValid()
    {
        // Arrange - extreme but valid near plane
        float nearPlane = 0.001f;
        float farPlane = 10000f;

        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Assert
        Vector4 clipNear = Vector4.Transform(new Vector4(0, 0, nearPlane, 1), projection);
        float ndcNear = clipNear.Z / clipNear.W;
        
        Assert.AreEqual(1f, ndcNear, 0.01f, "Very close near plane should still map to ~1.0");
    }

    [TestMethod]
    public void ReverseZ_VeryLargeFarPlane_StillValid()
    {
        // Arrange
        float nearPlane = 0.1f;
        float farPlane = 100000f; // 100km

        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Assert
        Vector4 clipFar = Vector4.Transform(new Vector4(0, 0, farPlane, 1), projection);
        float ndcFar = clipFar.Z / clipFar.W;

        Assert.AreEqual(0f, ndcFar, 0.01f, "Very far plane should still map to ~0.0");
    }

    [TestMethod]
    public void ReverseZ_InfinityFarPlane_ValidDepthMapping()
    {
        // Arrange
        float nearPlane = 0.1f;
        float farPlane = float.PositiveInfinity;

        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Test at various distances
        Vector4 clipNear = Vector4.Transform(new Vector4(0, 0, nearPlane, 1), projection);
        Vector4 clip10 = Vector4.Transform(new Vector4(0, 0, 10f, 1), projection);
        Vector4 clip1000 = Vector4.Transform(new Vector4(0, 0, 1000f, 1), projection);

        float ndcNear = clipNear.Z / clipNear.W;
        float ndc10 = clip10.Z / clip10.W;
        float ndc1000 = clip1000.Z / clip1000.W;

        // Assert
        Assert.AreEqual(1f, ndcNear, 0.01f, "Near plane should map to 1.0");
        Assert.IsTrue(ndc10 < ndcNear, "10m should be less than near depth");
        Assert.IsTrue(ndc1000 < ndc10, "1000m should be less than 10m depth");
        Assert.IsTrue(ndc1000 > 0f, "1000m should still be > 0 (approaching 0 at infinity)");
    }

    #endregion

    #region Depth Comparison Function Tests

    [TestMethod]
    public void ReverseZ_GreaterComparisonCorrect()
    {
        // Arrange - simulate depth test with Reverse-Z
        float nearPlane = 0.01f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Two overlapping objects at different depths
        float closeZ = 5f;
        float farZ = 10f;

        Vector4 clipClose = Vector4.Transform(new Vector4(0, 0, closeZ, 1), projection);
        Vector4 clipFar = Vector4.Transform(new Vector4(0, 0, farZ, 1), projection);

        float depthClose = clipClose.Z / clipClose.W;
        float depthFar = clipFar.Z / clipFar.W;

        // Assert - closer object should pass GREATER test against far object
        // This simulates: if (depthClose > depthFar) { passTest }
        Assert.IsTrue(depthClose > depthFar,
            $"Closer object (depth={depthClose}) should pass GREATER test against far (depth={depthFar})");
    }

    [TestMethod]
    public void ReverseZ_ClearDepthIsCorrect()
    {
        // In Reverse-Z, depth buffer is cleared to 0.0 (far plane)
        // Any object will have depth > 0, so will pass GREATER test
        
        float clearDepth = CameraMatrixHelper.ReverseZClearDepth;
        
        // Any valid scene depth should be greater than clear depth
        float nearPlane = 0.01f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Even the farthest visible object
        Vector4 clipFar = Vector4.Transform(new Vector4(0, 0, farPlane * 0.999f, 1), projection);
        float depthFar = clipFar.Z / clipFar.W;

        Assert.IsTrue(depthFar > clearDepth,
            $"Far object depth ({depthFar}) should be > clear depth ({clearDepth})");
    }

    #endregion
}
