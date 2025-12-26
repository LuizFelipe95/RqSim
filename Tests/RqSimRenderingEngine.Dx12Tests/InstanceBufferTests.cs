using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RqSimRenderingEngine.Dx12Tests;

/// <summary>
/// Tests for verifying instance buffer data layout and correctness.
/// Ensures per-instance data (position, radius, color) is correctly structured.
/// Uses a local test struct since the actual Dx12NodeInstance is internal.
/// </summary>
[TestClass]
public class InstanceBufferTests
{
    private const float Epsilon = 1e-5f;

    /// <summary>
    /// Local struct matching the expected Dx12NodeInstance layout for testing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TestNodeInstance
    {
        public Vector3 Position;
        public float Radius;
        public Vector4 Color;
    }

    #region TestNodeInstance Structure

    [TestMethod]
    public void TestNodeInstance_CorrectSize()
    {
        // Position (3 floats) + Radius (1 float) + Color (4 floats) = 32 bytes
        int size = Marshal.SizeOf<TestNodeInstance>();
        Assert.AreEqual(32, size, "TestNodeInstance should be 32 bytes");
    }

    [TestMethod]
    public unsafe void TestNodeInstance_MemoryLayout()
    {
        var instance = new TestNodeInstance
        {
            Position = new Vector3(1, 2, 3),
            Radius = 4,
            Color = new Vector4(5, 6, 7, 8)
        };

        float* ptr = (float*)&instance;

        // Verify memory layout matches input element descriptions:
        // INSTANCEPOS (float3) at offset 0
        Assert.AreEqual(1f, ptr[0], Epsilon, "Position.X at offset 0");
        Assert.AreEqual(2f, ptr[1], Epsilon, "Position.Y at offset 4");
        Assert.AreEqual(3f, ptr[2], Epsilon, "Position.Z at offset 8");
        
        // INSTANCERADIUS (float) at offset 12
        Assert.AreEqual(4f, ptr[3], Epsilon, "Radius at offset 12");
        
        // INSTANCECOLOR (float4) at offset 16
        Assert.AreEqual(5f, ptr[4], Epsilon, "Color.X at offset 16");
        Assert.AreEqual(6f, ptr[5], Epsilon, "Color.Y at offset 20");
        Assert.AreEqual(7f, ptr[6], Epsilon, "Color.Z at offset 24");
        Assert.AreEqual(8f, ptr[7], Epsilon, "Color.W at offset 28");
    }

    [TestMethod]
    public void TestNodeInstance_AlignedFor16ByteBoundary()
    {
        // Instance data should be aligned for efficient GPU access
        int size = Marshal.SizeOf<TestNodeInstance>();
        Assert.AreEqual(0, size % 16, "Instance size should be multiple of 16 bytes");
    }

    #endregion

    #region Instance Data Validation

    [TestMethod]
    public void InstanceData_PositionRange()
    {
        // Typical scene: positions within reasonable bounds
        var instances = CreateTestInstances(100);
        
        foreach (var instance in instances)
        {
            Assert.IsFalse(float.IsNaN(instance.Position.X), "Position X should not be NaN");
            Assert.IsFalse(float.IsNaN(instance.Position.Y), "Position Y should not be NaN");
            Assert.IsFalse(float.IsNaN(instance.Position.Z), "Position Z should not be NaN");
            Assert.IsFalse(float.IsInfinity(instance.Position.X), "Position X should not be Infinity");
            Assert.IsFalse(float.IsInfinity(instance.Position.Y), "Position Y should not be Infinity");
            Assert.IsFalse(float.IsInfinity(instance.Position.Z), "Position Z should not be Infinity");
        }
    }

    [TestMethod]
    public void InstanceData_RadiusPositive()
    {
        var instances = CreateTestInstances(100);
        
        foreach (var instance in instances)
        {
            Assert.IsTrue(instance.Radius > 0, $"Radius should be positive. Got {instance.Radius}");
            Assert.IsFalse(float.IsNaN(instance.Radius), "Radius should not be NaN");
            Assert.IsFalse(float.IsInfinity(instance.Radius), "Radius should not be Infinity");
        }
    }

    [TestMethod]
    public void InstanceData_ColorInRange()
    {
        var instances = CreateTestInstances(100);
        
        foreach (var instance in instances)
        {
            Assert.IsTrue(instance.Color.X >= 0 && instance.Color.X <= 1,
                $"Color R should be [0,1]. Got {instance.Color.X}");
            Assert.IsTrue(instance.Color.Y >= 0 && instance.Color.Y <= 1,
                $"Color G should be [0,1]. Got {instance.Color.Y}");
            Assert.IsTrue(instance.Color.Z >= 0 && instance.Color.Z <= 1,
                $"Color B should be [0,1]. Got {instance.Color.Z}");
            Assert.IsTrue(instance.Color.W >= 0 && instance.Color.W <= 1,
                $"Color A should be [0,1]. Got {instance.Color.W}");
        }
    }

    #endregion

    #region Instance Transform Calculation

    [TestMethod]
    public void InstanceTransform_LocalToWorld()
    {
        // Shader does: worldPos = instancePos + localPos * instanceRadius
        
        Vector3 instancePos = new(10, 20, 30);
        float instanceRadius = 5f;
        Vector3 localPos = new(1, 0, 0); // Point on +X of unit sphere
        
        Vector3 worldPos = instancePos + localPos * instanceRadius;
        
        Assert.AreEqual(15f, worldPos.X, Epsilon, "World X = 10 + 1*5");
        Assert.AreEqual(20f, worldPos.Y, Epsilon, "World Y = 20 + 0*5");
        Assert.AreEqual(30f, worldPos.Z, Epsilon, "World Z = 30 + 0*5");
    }

    [TestMethod]
    public void InstanceTransform_ZeroRadius_DegenerateCase()
    {
        // Zero radius would collapse sphere to a point
        Vector3 instancePos = new(10, 20, 30);
        float instanceRadius = 0f;
        Vector3 localPos = new(1, 1, 1);
        
        Vector3 worldPos = instancePos + localPos * instanceRadius;
        
        // All local positions collapse to instance position
        Assert.AreEqual(instancePos.X, worldPos.X, Epsilon);
        Assert.AreEqual(instancePos.Y, worldPos.Y, Epsilon);
        Assert.AreEqual(instancePos.Z, worldPos.Z, Epsilon);
    }

    [TestMethod]
    public void InstanceTransform_NegativeRadius_InvertsNormals()
    {
        // Negative radius flips the sphere inside-out
        // This is a potential issue for rendering
        
        float instanceRadius = -5f;
        Vector3 localPos = new(1, 0, 0);
        
        // With negative radius, +X local becomes -X world offset
        Vector3 scaledLocal = localPos * instanceRadius;
        Assert.AreEqual(-5f, scaledLocal.X, Epsilon, "Negative radius inverts direction");
        
        // This is usually a bug - radius should be positive
    }

    #endregion

    #region Buffer Upload Simulation

    [TestMethod]
    public unsafe void InstanceBuffer_CopyCorrectly()
    {
        var instances = CreateTestInstances(10);
        
        // Simulate buffer copy
        int sizeInBytes = instances.Length * Marshal.SizeOf<TestNodeInstance>();
        byte[] buffer = new byte[sizeInBytes];
        
        fixed (TestNodeInstance* src = instances)
        fixed (byte* dst = buffer)
        {
            Buffer.MemoryCopy(src, dst, sizeInBytes, sizeInBytes);
        }
        
        // Verify data integrity
        fixed (byte* ptr = buffer)
        {
            TestNodeInstance* readBack = (TestNodeInstance*)ptr;
            
            for (int i = 0; i < instances.Length; i++)
            {
                Assert.AreEqual(instances[i].Position.X, readBack[i].Position.X, Epsilon);
                Assert.AreEqual(instances[i].Position.Y, readBack[i].Position.Y, Epsilon);
                Assert.AreEqual(instances[i].Position.Z, readBack[i].Position.Z, Epsilon);
                Assert.AreEqual(instances[i].Radius, readBack[i].Radius, Epsilon);
                Assert.AreEqual(instances[i].Color.X, readBack[i].Color.X, Epsilon);
                Assert.AreEqual(instances[i].Color.Y, readBack[i].Color.Y, Epsilon);
                Assert.AreEqual(instances[i].Color.Z, readBack[i].Color.Z, Epsilon);
                Assert.AreEqual(instances[i].Color.W, readBack[i].Color.W, Epsilon);
            }
        }
    }

    [TestMethod]
    public void InstanceBuffer_StrideMatches()
    {
        // Vertex buffer view stride must match struct size
        int structSize = Marshal.SizeOf<TestNodeInstance>();
        int expectedStride = 32; // Per input element descriptions
        
        Assert.AreEqual(expectedStride, structSize,
            "Struct size must match expected stride for VertexBufferView");
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void InstanceBuffer_SingleInstance()
    {
        var instances = new TestNodeInstance[]
        {
            new()
            {
                Position = Vector3.Zero,
                Radius = 1f,
                Color = new Vector4(1, 0, 0, 1)
            }
        };
        
        Assert.AreEqual(1, instances.Length);
        Assert.AreEqual(1f, instances[0].Radius, Epsilon);
    }

    [TestMethod]
    public void InstanceBuffer_LargeCount()
    {
        // Test with many instances (typical scene might have 10K+ nodes)
        int count = 10000;
        var instances = CreateTestInstances(count);
        
        Assert.AreEqual(count, instances.Length);
        
        // Verify no overflow in size calculation
        long totalSize = (long)count * Marshal.SizeOf<TestNodeInstance>();
        Assert.IsTrue(totalSize < int.MaxValue, "Total size should fit in int for buffer creation");
    }

    [TestMethod]
    public void InstanceBuffer_ZeroInstances()
    {
        var instances = Array.Empty<TestNodeInstance>();
        Assert.AreEqual(0, instances.Length);
        
        // Should handle zero instances gracefully
    }

    #endregion

    #region Helper Methods

    private static TestNodeInstance[] CreateTestInstances(int count)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var instances = new TestNodeInstance[count];
        
        for (int i = 0; i < count; i++)
        {
            instances[i] = new TestNodeInstance
            {
                Position = new Vector3(
                    (float)(random.NextDouble() * 20 - 10),
                    (float)(random.NextDouble() * 20 - 10),
                    (float)(random.NextDouble() * 20 - 10)),
                Radius = (float)(random.NextDouble() * 0.5 + 0.1),
                Color = new Vector4(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    1f)
            };
        }
        
        return instances;
    }

    #endregion
}
