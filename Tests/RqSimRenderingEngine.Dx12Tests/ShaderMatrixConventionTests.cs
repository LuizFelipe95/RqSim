using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RqSimRenderingEngine.Rendering.Backend.DX12.Rendering;

namespace RqSimRenderingEngine.Dx12Tests;

/// <summary>
/// Tests for verifying correct matrix convention between C# and HLSL.
/// Ensures proper row-major/column-major handling and constant buffer layout.
/// </summary>
[TestClass]
public class ShaderMatrixConventionTests
{
    private const float Epsilon = 1e-5f;

    #region Matrix Memory Layout

    [TestMethod]
    public void Matrix4x4_RowMajorLayout_ElementOrder()
    {
        // System.Numerics.Matrix4x4 uses row-major layout:
        // M11 M12 M13 M14  (row 1)
        // M21 M22 M23 M24  (row 2)
        // M31 M32 M33 M34  (row 3)
        // M41 M42 M43 M44  (row 4)
        
        // In memory: M11, M12, M13, M14, M21, M22, ...
        
        Matrix4x4 m = new(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16);

        Assert.AreEqual(1f, m.M11, Epsilon);
        Assert.AreEqual(2f, m.M12, Epsilon);
        Assert.AreEqual(3f, m.M13, Epsilon);
        Assert.AreEqual(4f, m.M14, Epsilon);
        Assert.AreEqual(5f, m.M21, Epsilon);
    }

    [TestMethod]
    public unsafe void Matrix4x4_MemoryLayout_Contiguous()
    {
        // Verify that Matrix4x4 is laid out as expected in memory
        Matrix4x4 m = new(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16);

        float* ptr = (float*)&m;

        // Row-major means row 1 comes first: M11, M12, M13, M14
        Assert.AreEqual(1f, ptr[0], Epsilon, "ptr[0] should be M11");
        Assert.AreEqual(2f, ptr[1], Epsilon, "ptr[1] should be M12");
        Assert.AreEqual(3f, ptr[2], Epsilon, "ptr[2] should be M13");
        Assert.AreEqual(4f, ptr[3], Epsilon, "ptr[3] should be M14");
        Assert.AreEqual(5f, ptr[4], Epsilon, "ptr[4] should be M21");
    }

    [TestMethod]
    public void Matrix4x4_Size_Is64Bytes()
    {
        // 16 floats * 4 bytes = 64 bytes
        int size = Marshal.SizeOf<Matrix4x4>();
        Assert.AreEqual(64, size, "Matrix4x4 should be 64 bytes");
    }

    #endregion

    #region HLSL Convention Simulation

    [TestMethod]
    public void HLSL_MulVectorMatrix_SimulatedCorrectly()
    {
        // HLSL: mul(vector, matrix) - row vector * matrix
        // This is what System.Numerics.Vector4.Transform does
        
        Matrix4x4 m = Matrix4x4.CreateTranslation(10, 20, 30);
        Vector4 v = new(0, 0, 0, 1);

        // C# Transform = row-vector multiplication (v * M)
        Vector4 result = Vector4.Transform(v, m);

        Assert.AreEqual(10f, result.X, Epsilon, "Translation X");
        Assert.AreEqual(20f, result.Y, Epsilon, "Translation Y");
        Assert.AreEqual(30f, result.Z, Epsilon, "Translation Z");
        Assert.AreEqual(1f, result.W, Epsilon, "W unchanged");
    }

    [TestMethod]
    public void HLSL_MulMatrixVector_WouldBeDifferent()
    {
        // HLSL: mul(matrix, vector) - matrix * column vector
        // This would require matrix transpose or different interpretation
        
        // For our shader code using mul(float4(worldPos, 1.0), View):
        // - We pass row-major matrices from C#
        // - HLSL reads them as row-major (default in DX)
        // - mul(row_vector, matrix) gives correct result
        
        // This test verifies our understanding
        Matrix4x4 rotation = Matrix4x4.CreateRotationZ(MathF.PI / 2); // 90 degrees
        Vector4 v = new(1, 0, 0, 1); // Point on X axis

        // Row-vector * matrix (our convention)
        Vector4 rowVectorResult = Vector4.Transform(v, rotation);

        // Column-vector * matrix would give different result
        // Matrix * column_vector = (row_vector * Matrix^T)^T
        Vector4 colVectorResult = Vector4.Transform(v, Matrix4x4.Transpose(rotation));

        // Results should be different (rotated in opposite directions)
        Assert.AreNotEqual(rowVectorResult.Y, colVectorResult.Y,
            "Row-vector and column-vector multiplication should give different results");
    }

    #endregion

    #region Constant Buffer Alignment

    [TestMethod]
    public void CameraConstants_ProperAlignment()
    {
        // Constant buffers require 16-byte alignment for float4x4
        // Our CameraConstants struct has two Matrix4x4 (View and Projection)
        
        int matrixSize = Marshal.SizeOf<Matrix4x4>();
        Assert.AreEqual(64, matrixSize, "Matrix4x4 should be 64 bytes");
        Assert.AreEqual(0, matrixSize % 16, "Matrix4x4 size should be multiple of 16");
    }

    [TestMethod]
    public void ConstantBuffer_TwoMatrices_Layout()
    {
        // Simulate our cbuffer Camera layout:
        // cbuffer Camera : register(b0)
        // {
        //     float4x4 View;       // offset 0, size 64
        //     float4x4 Projection; // offset 64, size 64
        // };
        
        // Total size should be 128 bytes
        int expectedSize = 2 * Marshal.SizeOf<Matrix4x4>();
        Assert.AreEqual(128, expectedSize, "Two matrices = 128 bytes");
    }

    #endregion

    #region Transformation Order

    [TestMethod]
    public void TransformOrder_WorldViewProjection()
    {
        // Our shader does:
        // float4 viewPos = mul(float4(worldPos, 1.0), View);
        // o.Position = mul(viewPos, Projection);
        //
        // This is equivalent to: worldPos * View * Projection
        
        Vector3 worldPos = new(5, 5, 5);
        
        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(
            new Vector3(0, 0, -10),
            Vector3.Zero,
            Vector3.UnitY);
        
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, 0.1f, 100f);

        // Step by step (shader method)
        Vector4 viewPos = Vector4.Transform(new Vector4(worldPos, 1), view);
        Vector4 clipPos = Vector4.Transform(viewPos, projection);

        // Combined (efficient method)
        Matrix4x4 viewProj = view * projection;
        Vector4 clipPos2 = Vector4.Transform(new Vector4(worldPos, 1), viewProj);

        // Should be identical
        Assert.AreEqual(clipPos.X, clipPos2.X, Epsilon);
        Assert.AreEqual(clipPos.Y, clipPos2.Y, Epsilon);
        Assert.AreEqual(clipPos.Z, clipPos2.Z, Epsilon);
        Assert.AreEqual(clipPos.W, clipPos2.W, Epsilon);
    }

    [TestMethod]
    public void TransformOrder_WithInstanceTransform()
    {
        // Our node shader does:
        // float3 worldPos = input.InstancePos + input.Position * input.InstanceRadius;
        // float4 viewPos = mul(float4(worldPos, 1.0), View);
        // o.Position = mul(viewPos, Projection);
        
        Vector3 instancePos = new(10, 0, 0);
        float instanceRadius = 2f;
        Vector3 localPos = new(0, 1, 0); // Top of sphere
        
        // Calculate world position
        Vector3 worldPos = instancePos + localPos * instanceRadius;
        
        Assert.AreEqual(10f, worldPos.X, Epsilon, "World X");
        Assert.AreEqual(2f, worldPos.Y, Epsilon, "World Y (local Y scaled)");
        Assert.AreEqual(0f, worldPos.Z, Epsilon, "World Z");
    }

    #endregion

    #region Projection Matrix HLSL Behavior

    [TestMethod]
    public void ProjectionMatrix_ClipSpaceWDivide()
    {
        // After vertex shader, GPU performs perspective divide: pos.xyz / pos.w
        // This converts clip space to NDC
        
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, 0.1f, 100f);

        // Point in view space - ensure it's within the frustum
        // With FOV=45°, at z=10, visible X range ? ±tan(22.5°)*10 ? ±4.14
        // Use a point well within the frustum
        Vector4 viewPos = new(2, 2, 10, 1);
        
        // Transform to clip space
        Vector4 clipPos = Vector4.Transform(viewPos, projection);
        
        // Perspective divide
        Vector3 ndc = new(clipPos.X / clipPos.W, clipPos.Y / clipPos.W, clipPos.Z / clipPos.W);

        // NDC should be in reasonable range for a point inside frustum
        Assert.IsTrue(ndc.X > -1 && ndc.X < 1, $"NDC X out of range: {ndc.X}");
        Assert.IsTrue(ndc.Y > -1 && ndc.Y < 1, $"NDC Y out of range: {ndc.Y}");
        Assert.IsTrue(ndc.Z >= 0 && ndc.Z <= 1, $"NDC Z out of range: {ndc.Z}");
    }

    [TestMethod]
    public void ProjectionMatrix_WEqualsViewSpaceZ()
    {
        // For LH projection with M34=1 and M44=0:
        // clipPos.w = viewPos.z * 1 + 1 * 0 = viewPos.z
        
        Matrix4x4 projection = CameraMatrixHelper.CreatePerspectiveReverseZ(
            MathF.PI / 4f, 1f, 0.1f, 100f);

        float[] testZValues = [0.5f, 1f, 5f, 10f, 50f];

        foreach (float viewZ in testZValues)
        {
            Vector4 viewPos = new(0, 0, viewZ, 1);
            Vector4 clipPos = Vector4.Transform(viewPos, projection);

            Assert.AreEqual(viewZ, clipPos.W, Epsilon,
                $"Clip W should equal view Z. For viewZ={viewZ}, got W={clipPos.W}");
        }
    }

    #endregion

    #region Row-Major vs Column-Major

    [TestMethod]
    public void RowMajor_TranslationInLastRow()
    {
        // In row-major format with row-vector multiplication:
        // Translation is stored in M41, M42, M43
        
        Matrix4x4 translation = Matrix4x4.CreateTranslation(10, 20, 30);
        
        Assert.AreEqual(10f, translation.M41, Epsilon, "Translation X in M41");
        Assert.AreEqual(20f, translation.M42, Epsilon, "Translation Y in M42");
        Assert.AreEqual(30f, translation.M43, Epsilon, "Translation Z in M43");
        Assert.AreEqual(1f, translation.M44, Epsilon, "M44 = 1 for affine");
    }

    [TestMethod]
    public void RowMajor_RotationAxesInRows()
    {
        // In row-major format:
        // Row 0 = right vector (transformed X axis)
        // Row 1 = up vector (transformed Y axis)  
        // Row 2 = forward vector (transformed Z axis)
        
        float angle = MathF.PI / 2; // 90 degrees around Y
        Matrix4x4 rotY = Matrix4x4.CreateRotationY(angle);

        // After 90-degree Y rotation:
        // X axis -> -Z (approximately)
        // Y axis -> Y (unchanged)
        // Z axis -> X (approximately)
        
        Assert.AreEqual(0f, rotY.M11, Epsilon, "M11 ? 0 after 90° Y rotation");
        Assert.AreEqual(1f, rotY.M22, Epsilon, "M22 = 1 (Y unchanged)");
        Assert.AreEqual(0f, rotY.M33, Epsilon, "M33 ? 0 after 90° Y rotation");
    }

    #endregion

    #region View Matrix Specifics

    [TestMethod]
    public void ViewMatrix_CameraPositionNotDirectlyVisible()
    {
        // View matrix transforms world -> view space
        // Camera world position is encoded in the last row after rotation
        
        Vector3 cameraPos = new(10, 5, -20);
        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, Vector3.Zero, Vector3.UnitY);

        // Transform origin (world 0,0,0) to view space
        // It should be at (0, 0, distance) relative to camera
        Vector4 originInView = Vector4.Transform(new Vector4(0, 0, 0, 1), view);
        
        // Origin should be in front of camera (positive Z in LH view)
        Assert.IsTrue(originInView.Z > 0, "Origin should be in front of camera");
    }

    [TestMethod]
    public void ViewMatrix_InverseGivesCameraToWorld()
    {
        // View^-1 transforms view space -> world space
        // View^-1 * (0,0,0,1) = camera position
        
        Vector3 cameraPos = new(10, 5, -20);
        Matrix4x4 view = CameraMatrixHelper.CreateLookAt(cameraPos, Vector3.Zero, Vector3.UnitY);
        
        bool inverted = Matrix4x4.Invert(view, out Matrix4x4 viewInverse);
        Assert.IsTrue(inverted, "View matrix should be invertible");

        // Transform view-space origin to world
        Vector4 viewOrigin = new(0, 0, 0, 1);
        Vector4 worldPos = Vector4.Transform(viewOrigin, viewInverse);

        Assert.AreEqual(cameraPos.X, worldPos.X, 0.01f, "Camera X");
        Assert.AreEqual(cameraPos.Y, worldPos.Y, 0.01f, "Camera Y");
        Assert.AreEqual(cameraPos.Z, worldPos.Z, 0.01f, "Camera Z");
    }

    #endregion

    #region HLSL row_major Verification

    [TestMethod]
    public void HlslRowMajor_CorrectMemoryLayoutForCBuffer()
    {
        // When using row_major in HLSL, the matrix memory layout matches C#:
        // C# Matrix4x4 stores in row-major: M11, M12, M13, M14, M21, M22, ...
        // HLSL row_major float4x4 expects the same layout
        
        // The key difference:
        // - Without row_major: HLSL reads column-by-column, so M11,M21,M31,M41 become first column
        // - With row_major: HLSL reads row-by-row, so M11,M12,M13,M14 become first row
        
        // Create a simple view matrix
        Matrix4x4 view = Matrix4x4.CreateLookAtLeftHanded(
            new Vector3(0, 0, -5),  // Camera at Z=-5
            Vector3.Zero,           // Looking at origin
            Vector3.UnitY);         // Up is Y
        
        // With row_major and row-vector multiplication (mul(v, M)):
        // The translation should be in the last row (M41, M42, M43)
        // For LookAt, translation = -dot(axis, eye) for each axis
        
        // Verify the matrix structure is what HLSL expects
        // The W row (M41-M44) contains the translation component
        Assert.AreEqual(1f, view.M44, Epsilon, "M44 should be 1 for affine transforms");
        
        // M43 should contain the Z translation which should be positive
        // (camera at Z=-5 looking at origin means +5 translation in view space)
        Assert.IsTrue(view.M43 > 0, $"M43 should be positive for camera behind origin. Got {view.M43}");
    }

    [TestMethod]
    public void HlslRowMajor_MulVectorMatrix_CorrectTransform()
    {
        // In HLSL with row_major: mul(vector, matrix) does row-vector multiplication
        // This matches C# Vector4.Transform(v, m)
        
        Matrix4x4 translation = Matrix4x4.CreateTranslation(10, 20, 30);
        Vector4 point = new(0, 0, 0, 1); // Origin
        
        // C# transform
        Vector4 transformed = Vector4.Transform(point, translation);
        
        // Should move point to (10, 20, 30)
        Assert.AreEqual(10f, transformed.X, Epsilon);
        Assert.AreEqual(20f, transformed.Y, Epsilon);
        Assert.AreEqual(30f, transformed.Z, Epsilon);
        Assert.AreEqual(1f, transformed.W, Epsilon);
    }

    [TestMethod]
    public void HlslRowMajor_ViewProjection_TransformPointCorrectly()
    {
        // Full transform pipeline test: World -> View -> Clip -> NDC
        
        Vector3 cameraPos = new(0, 0, -5);
        Vector3 target = Vector3.Zero;
        
        Matrix4x4 view = CameraMatrixHelper.CreateOrbitCamera(target, 5f, 0f, 0f);
        Matrix4x4 proj = CameraMatrixHelper.CreatePerspectiveReverseZ(MathF.PI / 4f, 1f, 0.1f, 100f);
        
        // Point at origin (camera target)
        Vector4 worldPos = new(0, 0, 0, 1);
        
        // Transform to clip space (simulating HLSL mul(mul(pos, View), Projection))
        Vector4 viewPos = Vector4.Transform(worldPos, view);
        Vector4 clipPos = Vector4.Transform(viewPos, proj);
        
        // After perspective divide, origin should be at center of screen (NDC X=0, Y=0)
        Vector3 ndc = new(clipPos.X / clipPos.W, clipPos.Y / clipPos.W, clipPos.Z / clipPos.W);
        
        Assert.AreEqual(0f, ndc.X, 0.01f, $"NDC X should be 0 for centered point. Got {ndc.X}");
        Assert.AreEqual(0f, ndc.Y, 0.01f, $"NDC Y should be 0 for centered point. Got {ndc.Y}");
        
        // NDC Z should be between 0 and 1 for Reverse-Z
        Assert.IsTrue(ndc.Z >= 0f && ndc.Z <= 1f, $"NDC Z should be in [0,1]. Got {ndc.Z}");
    }

    #endregion
}
