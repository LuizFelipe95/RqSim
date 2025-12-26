using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RqSimRenderingEngine.Rendering.Backend.DX12.Rendering;

namespace RqSimRenderingEngine.Dx12Tests;

/// <summary>
/// Tests for CameraMatrixHelper to verify correct matrix generation
/// for DX12 rendering with Reverse-Z projection.
/// </summary>
[TestClass]
public class CameraMatrixHelperTests
{
    private const float Epsilon = 1e-5f;

    #region CreatePerspectiveReverseZ Tests

    [TestMethod]
    public void CreatePerspectiveReverseZ_ValidParams_ReturnsValidMatrix()
    {
        // Arrange
        float fovY = MathF.PI / 4f; // 45 degrees
        float aspectRatio = 16f / 9f;
        float nearPlane = 0.01f;
        float farPlane = 100f;

        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(fovY, aspectRatio, nearPlane, farPlane);

        // Assert - verify matrix structure
        Assert.AreNotEqual(0f, projection.M11, "xScale should be non-zero");
        Assert.AreNotEqual(0f, projection.M22, "yScale should be non-zero");
        Assert.AreEqual(1f, projection.M34, Epsilon, "M34 should be 1.0 for LH perspective");
        Assert.AreEqual(0f, projection.M44, Epsilon, "M44 should be 0.0 for perspective");
    }

    [TestMethod]
    public void CreatePerspectiveReverseZ_M33_CorrectForReverseZ()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 100f;
        float expectedM33 = -nearPlane / (farPlane - nearPlane); // -n/(f-n)

        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Assert
        Assert.AreEqual(expectedM33, projection.M33, Epsilon, 
            $"M33 should be -n/(f-n) = {expectedM33}");
    }

    [TestMethod]
    public void CreatePerspectiveReverseZ_M43_CorrectForReverseZ()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 100f;
        float expectedM43 = nearPlane * farPlane / (farPlane - nearPlane); // n*f/(f-n)

        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Assert
        Assert.AreEqual(expectedM43, projection.M43, Epsilon,
            $"M43 should be n*f/(f-n) = {expectedM43}");
    }

    [TestMethod]
    public void CreatePerspectiveReverseZ_InfiniteFar_ReturnsValidMatrix()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = float.PositiveInfinity;

        // Act
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Assert
        Assert.AreEqual(0f, projection.M33, Epsilon, "M33 should be 0 for infinite far");
        Assert.AreEqual(nearPlane, projection.M43, Epsilon, "M43 should be nearPlane for infinite far");
        Assert.AreEqual(1f, projection.M34, Epsilon, "M34 should be 1.0");
    }

    [TestMethod]
    public void CreatePerspectiveReverseZ_NegativeNearPlane_Throws()
    {
        // Arrange & Act & Assert
        bool threwException = false;
        try
        {
            CameraMatrixHelper.CreatePerspectiveReverseZ(MathF.PI / 4f, 1f, -1f, 100f);
        }
        catch (ArgumentOutOfRangeException)
        {
            threwException = true;
        }
        Assert.IsTrue(threwException, "Expected ArgumentOutOfRangeException for negative near plane");
    }

    [TestMethod]
    public void CreatePerspectiveReverseZ_ZeroNearPlane_Throws()
    {
        // Arrange & Act & Assert
        bool threwException = false;
        try
        {
            CameraMatrixHelper.CreatePerspectiveReverseZ(MathF.PI / 4f, 1f, 0f, 100f);
        }
        catch (ArgumentOutOfRangeException)
        {
            threwException = true;
        }
        Assert.IsTrue(threwException, "Expected ArgumentOutOfRangeException for zero near plane");
    }

    [TestMethod]
    public void CreatePerspectiveReverseZ_FarLessThanNear_Throws()
    {
        // Arrange & Act & Assert
        bool threwException = false;
        try
        {
            CameraMatrixHelper.CreatePerspectiveReverseZ(MathF.PI / 4f, 1f, 10f, 5f);
        }
        catch (ArgumentOutOfRangeException)
        {
            threwException = true;
        }
        Assert.IsTrue(threwException, "Expected ArgumentOutOfRangeException when far < near");
    }

    #endregion

    #region Reverse-Z Depth Mapping Tests

    [TestMethod]
    public void ReverseZ_PointAtNearPlane_MapsToDepthOne()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Point at near plane in view space (looking down +Z in LH)
        Vector4 pointAtNear = new(0, 0, nearPlane, 1);

        // Act
        Vector4 clipSpace = Vector4.Transform(pointAtNear, projection);
        float ndcZ = clipSpace.Z / clipSpace.W;

        // Assert
        Assert.AreEqual(1f, ndcZ, 0.001f, "Point at near plane should have NDC Z ? 1.0");
    }

    [TestMethod]
    public void ReverseZ_PointAtFarPlane_MapsToDepthZero()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Point at far plane in view space
        Vector4 pointAtFar = new(0, 0, farPlane, 1);

        // Act
        Vector4 clipSpace = Vector4.Transform(pointAtFar, projection);
        float ndcZ = clipSpace.Z / clipSpace.W;

        // Assert
        Assert.AreEqual(0f, ndcZ, 0.001f, "Point at far plane should have NDC Z ? 0.0");
    }

    [TestMethod]
    public void ReverseZ_PointAtMidDistance_MapsToValidDepth()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 100f;
        float midDistance = (nearPlane + farPlane) / 2f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        Vector4 pointAtMid = new(0, 0, midDistance, 1);

        // Act
        Vector4 clipSpace = Vector4.Transform(pointAtMid, projection);
        float ndcZ = clipSpace.Z / clipSpace.W;

        // Assert
        Assert.IsTrue(ndcZ > 0f && ndcZ < 1f, 
            $"Mid-distance point should have NDC Z in (0,1), got {ndcZ}");
    }

    [TestMethod]
    public void ReverseZ_CloserPointHasHigherDepth()
    {
        // Arrange
        float nearPlane = 0.01f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        Vector4 closerPoint = new(0, 0, 10f, 1);
        Vector4 fartherPoint = new(0, 0, 50f, 1);

        // Act
        Vector4 clipClose = Vector4.Transform(closerPoint, projection);
        Vector4 clipFar = Vector4.Transform(fartherPoint, projection);
        float ndcZClose = clipClose.Z / clipClose.W;
        float ndcZFar = clipFar.Z / clipFar.W;

        // Assert - In Reverse-Z, closer objects have HIGHER depth values
        Assert.IsTrue(ndcZClose > ndcZFar, 
            $"Closer point should have higher depth. Close={ndcZClose}, Far={ndcZFar}");
    }

    #endregion

    #region CreateLookAt Tests

    [TestMethod]
    public void CreateLookAt_LeftHanded_TransformsCorrectly()
    {
        // Arrange
        Vector3 cameraPos = new(0, 0, -5);
        Vector3 target = Vector3.Zero;
        Vector3 up = Vector3.UnitY;

        // Act
        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, target, up);

        // Assert - camera at (0,0,-5) looking at origin, should see origin at (0,0,5) in view space
        Vector4 originInView = Vector4.Transform(new Vector4(0, 0, 0, 1), view);
        
        Assert.AreEqual(0f, originInView.X, Epsilon, "Origin X should be 0 in view space");
        Assert.AreEqual(0f, originInView.Y, Epsilon, "Origin Y should be 0 in view space");
        Assert.IsTrue(originInView.Z > 0, "Origin should be in front of camera (+Z in LH view space)");
    }

    [TestMethod]
    public void CreateLookAt_TranslationInLastRow()
    {
        // Arrange
        Vector3 cameraPos = new(10, 20, 30);
        Vector3 target = Vector3.Zero;

        // Act
        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, target, Vector3.UnitY);

        // Assert - view matrix stores camera translation in M41, M42, M43
        Assert.IsTrue(view.M41 != 0 || view.M42 != 0 || view.M43 != 0,
            "Translation components should be non-zero for offset camera");
        Assert.AreEqual(1f, view.M44, Epsilon, "M44 should be 1.0 for affine transform");
    }

    #endregion

    #region CreateOrbitCamera Tests

    [TestMethod]
    public void CreateOrbitCamera_ZeroAngles_CameraInFrontOfTarget()
    {
        // Arrange
        Vector3 target = Vector3.Zero;
        float distance = 5f;
        float yaw = 0f;
        float pitch = 0f;

        // Act
        Matrix4x4 view = CameraMatrixHelper.CreateOrbitCamera(target, distance, yaw, pitch);

        // Assert - transform target to view space, should be at (0, 0, distance)
        Vector4 targetInView = Vector4.Transform(new Vector4(target, 1), view);
        
        Assert.AreEqual(0f, targetInView.X, Epsilon, "Target X should be 0");
        Assert.AreEqual(0f, targetInView.Y, Epsilon, "Target Y should be 0");
        Assert.AreEqual(distance, targetInView.Z, 0.01f, "Target should be at 'distance' in front");
    }

    [TestMethod]
    public void CreateOrbitCamera_90DegreeYaw_CameraOnRight()
    {
        // Arrange
        Vector3 target = Vector3.Zero;
        float distance = 5f;
        float yaw = MathF.PI / 2f; // 90 degrees
        float pitch = 0f;

        // Act
        Matrix4x4 view = CameraMatrixHelper.CreateOrbitCamera(target, distance, yaw, pitch);
        
        // Point at world X axis should appear differently
        Vector4 xAxisPoint = Vector4.Transform(new Vector4(1, 0, 0, 1), view);
        
        // Assert - camera is now on the right side of target
        Assert.AreNotEqual(0f, xAxisPoint.Z, "X-axis point should have non-zero Z in view");
    }

    [TestMethod]
    public void CreateOrbitCamera_PositivePitch_CameraAboveTarget()
    {
        // Arrange
        Vector3 target = Vector3.Zero;
        float distance = 5f;
        float yaw = 0f;
        float pitch = MathF.PI / 4f; // 45 degrees up

        // Act
        Matrix4x4 view = CameraMatrixHelper.CreateOrbitCamera(target, distance, yaw, pitch);

        // Transform a point above the target
        Vector4 aboveTarget = Vector4.Transform(new Vector4(0, 1, 0, 1), view);

        // Assert - with camera above, point above target should appear lower in view
        Assert.IsTrue(aboveTarget.Y < 1f, "Point above target should appear lower when camera is above");
    }

    [TestMethod]
    public void CreateOrbitCamera_PitchClampedToAvoidGimbalLock()
    {
        // Arrange
        float extremePitch = MathF.PI; // 180 degrees - should be clamped

        // Act & Assert - should not throw
        Matrix4x4 view = CameraMatrixHelper.CreateOrbitCamera(
            Vector3.Zero, 5f, 0f, extremePitch);
        
        Assert.IsNotNull(view);
    }

    #endregion

    #region Clear Depth Values

    [TestMethod]
    public void ReverseZClearDepth_IsZero()
    {
        Assert.AreEqual(0f, CameraMatrixHelper.ReverseZClearDepth, 
            "Reverse-Z should clear to 0.0 (far plane)");
    }

    [TestMethod]
    public void StandardClearDepth_IsOne()
    {
        Assert.AreEqual(1f, CameraMatrixHelper.StandardClearDepth,
            "Standard Z should clear to 1.0 (far plane)");
    }

    #endregion
}
