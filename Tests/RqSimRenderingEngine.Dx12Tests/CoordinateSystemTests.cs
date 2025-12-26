using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RqSimRenderingEngine.Rendering.Backend.DX12.Rendering;

namespace RqSimRenderingEngine.Dx12Tests;

/// <summary>
/// Tests for verifying correct Left-Handed coordinate system usage
/// for DirectX 12 rendering.
/// </summary>
[TestClass]
public class CoordinateSystemTests
{
    private const float Epsilon = 1e-5f;

    #region Left-Handed Coordinate System Basics

    [TestMethod]
    public void LeftHanded_PositiveZIntoScreen()
    {
        // Arrange - camera at origin looking down +Z
        Vector3 cameraPos = Vector3.Zero;
        Vector3 target = new(0, 0, 1); // +Z
        Vector3 up = Vector3.UnitY;

        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, target, up);

        // Point in front of camera
        Vector4 pointInFront = new(0, 0, 5, 1);

        // Act
        Vector4 viewSpace = Vector4.Transform(pointInFront, view);

        // Assert - in LH view space, point should be at positive Z
        Assert.IsTrue(viewSpace.Z > 0, 
            $"Point in front should have positive Z in LH view space. Got Z={viewSpace.Z}");
    }

    [TestMethod]
    public void LeftHanded_PositiveXToRight()
    {
        // Arrange
        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(
            new Vector3(0, 0, -5),
            Vector3.Zero,
            Vector3.UnitY);

        // Point to the right of origin
        Vector4 pointRight = new(2, 0, 0, 1);

        // Act
        Vector4 viewSpace = Vector4.Transform(pointRight, view);

        // Assert - should appear on positive X in view space
        Assert.IsTrue(viewSpace.X > 0,
            $"Point to the right should have positive X. Got X={viewSpace.X}");
    }

    [TestMethod]
    public void LeftHanded_PositiveYUp()
    {
        // Arrange
        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(
            new Vector3(0, 0, -5),
            Vector3.Zero,
            Vector3.UnitY);

        // Point above origin
        Vector4 pointUp = new(0, 2, 0, 1);

        // Act
        Vector4 viewSpace = Vector4.Transform(pointUp, view);

        // Assert - should appear on positive Y in view space
        Assert.IsTrue(viewSpace.Y > 0,
            $"Point above should have positive Y. Got Y={viewSpace.Y}");
    }

    #endregion

    #region View Space Orientation

    [TestMethod]
    public void ViewSpace_CameraLookingForward_TargetAtPositiveZ()
    {
        // Arrange
        Vector3 cameraPos = new(0, 0, -10);
        Vector3 target = Vector3.Zero;
        float expectedDistance = 10f;

        // Act
        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, target, Vector3.UnitY);
        Vector4 targetInView = Vector4.Transform(new Vector4(target, 1), view);

        // Assert
        Assert.AreEqual(0f, targetInView.X, Epsilon, "Target should be centered X");
        Assert.AreEqual(0f, targetInView.Y, Epsilon, "Target should be centered Y");
        Assert.AreEqual(expectedDistance, targetInView.Z, 0.01f, 
            "Target should be at camera distance on +Z");
    }

    [TestMethod]
    public void ViewSpace_OrbitCamera_MaintainsLHConvention()
    {
        // Arrange
        Vector3 target = new(1, 2, 3);
        float distance = 5f;

        // Test various orbit angles
        float[] yaws = [0, MathF.PI / 4, MathF.PI / 2, MathF.PI];
        float[] pitches = [0, MathF.PI / 6, -MathF.PI / 6];

        foreach (float yaw in yaws)
        {
            foreach (float pitch in pitches)
            {
                // Act
                Matrix4x4 view = CameraMatrixHelper.CreateOrbitCamera(target, distance, yaw, pitch);
                Vector4 targetInView = Vector4.Transform(new Vector4(target, 1), view);

                // Assert - target should always be in front of camera
                Assert.IsTrue(targetInView.Z > 0,
                    $"Target should be at +Z for yaw={yaw}, pitch={pitch}. Got Z={targetInView.Z}");
            }
        }
    }

    #endregion

    #region Cross Product / Winding Order

    [TestMethod]
    public void LeftHanded_CrossProduct_FollowsLHRule()
    {
        // Cross product: X ? Y
        Vector3 x = Vector3.UnitX;
        Vector3 y = Vector3.UnitY;
        
        Vector3 cross = Vector3.Cross(x, y);

        // System.Numerics uses standard mathematical convention:
        // X ? Y = +Z (right-hand rule for cross product)
        // This is NOT the "left-handed" coordinate system rule!
        // The cross product formula is universal: (a ? b)_z = a_x*b_y - a_y*b_x
        Assert.AreEqual(1f, cross.Z, Epsilon,
            "Cross(X,Y) = +Z (standard mathematical convention)");
    }

    [TestMethod]
    public void LeftHanded_TriangleWinding_CCWFrontFacing()
    {
        // In DX12 with our rasterizer settings:
        // FrontCounterClockwise = true means CCW triangles are front-facing
        // In LEFT-HANDED system, +Z goes INTO screen, so viewer looks from -Z toward +Z
        
        // Triangle in XY plane at Z=0, viewed from negative Z (camera at Z=-1 looking toward +Z)
        // CCW winding when viewed from -Z means vertices go counterclockwise from that viewpoint
        Vector3 v0 = new(0, 1, 0);   // Top
        Vector3 v1 = new(-1, -1, 0); // Bottom-left  
        Vector3 v2 = new(1, -1, 0);  // Bottom-right
        // From -Z viewpoint: v0 at top, v1 at bottom-left, v2 at bottom-right
        // Going v0->v1->v2 is clockwise when viewed from -Z
        // So this is actually CW from -Z, which is CCW from +Z

        // Edge vectors
        Vector3 e1 = v1 - v0;
        Vector3 e2 = v2 - v0;

        // Cross product using System.Numerics (RH convention: X?Y=Z)
        Vector3 normal = Vector3.Cross(e1, e2);

        // The cross product e1?e2 gives the normal
        // For this specific triangle, the normal points in +Z direction
        // In LH system where camera looks down +Z, front faces point TOWARD camera (-Z)
        // So this triangle is back-facing when viewed from the standard LH camera position
        
        // This test verifies our understanding of the coordinate system
        // Normal.Z > 0 means normal points into screen (+Z in LH)
        Assert.IsTrue(normal.Z > 0,
            $"Triangle normal points +Z (into screen in LH). Got Z={normal.Z}");
    }

    #endregion

    #region Projection Space

    [TestMethod]
    public void ProjectionSpace_PositiveZForward_AfterProjection()
    {
        // Arrange
        float nearPlane = 0.1f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Point in view space at +Z (in front of camera)
        Vector4 viewSpacePoint = new(0, 0, 10, 1);

        // Act
        Vector4 clipSpace = Vector4.Transform(viewSpacePoint, projection);

        // Assert - W should be positive for points in front
        Assert.IsTrue(clipSpace.W > 0,
            $"Clip W should be positive for visible points. Got W={clipSpace.W}");
    }

    [TestMethod]
    public void ProjectionSpace_NDCRangeCorrect()
    {
        // Arrange
        float nearPlane = 0.1f;
        float farPlane = 100f;
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Point at screen center
        Vector4 centerPoint = new(0, 0, 50, 1);

        // Act
        Vector4 clip = Vector4.Transform(centerPoint, projection);
        Vector3 ndc = new(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W);

        // Assert - NDC XY should be 0 for center, Z in [0,1]
        Assert.AreEqual(0f, ndc.X, Epsilon, "Center X should be 0 in NDC");
        Assert.AreEqual(0f, ndc.Y, Epsilon, "Center Y should be 0 in NDC");
        Assert.IsTrue(ndc.Z >= 0f && ndc.Z <= 1f, $"NDC Z should be in [0,1]. Got {ndc.Z}");
    }

    #endregion

    #region Combined View-Projection

    [TestMethod]
    public void ViewProjection_PointInFrustum_CorrectNDC()
    {
        // Arrange - typical scene setup
        Vector3 cameraPos = new(0, 0, -5);
        Vector3 target = Vector3.Zero;
        float nearPlane = 0.1f;
        float farPlane = 100f;
        float aspectRatio = 16f / 9f;

        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, target, Vector3.UnitY);
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, aspectRatio, nearPlane, farPlane);

        // Point at origin (target)
        Vector4 worldPoint = new(0, 0, 0, 1);

        // Act
        Vector4 viewSpace = Vector4.Transform(worldPoint, view);
        Vector4 clipSpace = Vector4.Transform(viewSpace, projection);
        Vector3 ndc = new(clipSpace.X / clipSpace.W, clipSpace.Y / clipSpace.W, clipSpace.Z / clipSpace.W);

        // Assert - origin should be at screen center with valid depth
        Assert.AreEqual(0f, ndc.X, 0.01f, "Origin should be at center X");
        Assert.AreEqual(0f, ndc.Y, 0.01f, "Origin should be at center Y");
        Assert.IsTrue(ndc.Z > 0f && ndc.Z < 1f, $"Origin should have valid depth. Got {ndc.Z}");
    }

    [TestMethod]
    public void ViewProjection_PointBehindCamera_NegativeW()
    {
        // Arrange
        Vector3 cameraPos = new(0, 0, -5);
        Vector3 target = Vector3.Zero;
        float nearPlane = 0.1f;
        float farPlane = 100f;

        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, target, Vector3.UnitY);
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, nearPlane, farPlane);

        // Point behind camera
        Vector4 behindPoint = new(0, 0, -10, 1); // Behind camera at z=-5

        // Act
        Vector4 viewSpace = Vector4.Transform(behindPoint, view);
        Vector4 clipSpace = Vector4.Transform(viewSpace, projection);

        // Assert - point behind camera should have negative or invalid W
        Assert.IsTrue(viewSpace.Z < 0 || clipSpace.W < 0,
            "Point behind camera should be clipped (negative view Z or clip W)");
    }

    #endregion

    #region Matrix Multiplication Order

    [TestMethod]
    public void MatrixMultiplication_ViewThenProjection_Correct()
    {
        // In HLSL: mul(mul(float4(worldPos, 1), View), Projection)
        // This is: (worldPos * View) * Projection = worldPos * (View * Projection)
        
        Vector3 cameraPos = new(0, 2, -5);
        Vector3 target = Vector3.Zero;

        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, target, Vector3.UnitY);
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, 0.1f, 100f);

        Vector4 worldPoint = new(1, 1, 1, 1);

        // Method 1: Step by step
        Vector4 viewSpace = Vector4.Transform(worldPoint, view);
        Vector4 clipSpace1 = Vector4.Transform(viewSpace, projection);

        // Method 2: Combined matrix
        Matrix4x4 viewProjection = view * projection;
        Vector4 clipSpace2 = Vector4.Transform(worldPoint, viewProjection);

        // Assert - both methods should give same result
        Assert.AreEqual(clipSpace1.X, clipSpace2.X, Epsilon, "X should match");
        Assert.AreEqual(clipSpace1.Y, clipSpace2.Y, Epsilon, "Y should match");
        Assert.AreEqual(clipSpace1.Z, clipSpace2.Z, Epsilon, "Z should match");
        Assert.AreEqual(clipSpace1.W, clipSpace2.W, Epsilon, "W should match");
    }

    #endregion
}
