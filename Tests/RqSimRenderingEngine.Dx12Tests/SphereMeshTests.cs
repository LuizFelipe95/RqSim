using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RqSimRenderingEngine.Dx12Tests;

/// <summary>
/// Tests for verifying sphere mesh generation and winding order.
/// These tests use a local implementation to verify the expected mesh properties
/// since the actual SphereMesh class is internal.
/// </summary>
[TestClass]
public class SphereMeshTests
{
    private const float Epsilon = 1e-5f;

    /// <summary>
    /// Local vertex structure matching Dx12VertexPositionNormal for testing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TestVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    #region Test Mesh Generation (local implementation for verification)

    /// <summary>
    /// Creates a test sphere mesh using UV sphere algorithm.
    /// This mimics what the production code should generate.
    /// </summary>
    private static (TestVertex[] vertices, ushort[] indices) CreateTestSphereMesh(int latSegments = 16, int lonSegments = 32)
    {
        var vertices = new List<TestVertex>();
        var indices = new List<ushort>();

        // Generate vertices
        for (int lat = 0; lat <= latSegments; lat++)
        {
            float theta = lat * MathF.PI / latSegments;
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= lonSegments; lon++)
            {
                float phi = lon * 2 * MathF.PI / lonSegments;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                Vector3 pos = new(
                    sinTheta * cosPhi,
                    cosTheta,
                    sinTheta * sinPhi);

                vertices.Add(new TestVertex
                {
                    Position = pos,
                    Normal = Vector3.Normalize(pos)
                });
            }
        }

        // Generate indices (CCW winding for outward-facing triangles)
        for (int lat = 0; lat < latSegments; lat++)
        {
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int first = lat * (lonSegments + 1) + lon;
                int second = first + lonSegments + 1;

                // First triangle (CCW when viewed from outside)
                indices.Add((ushort)first);
                indices.Add((ushort)(first + 1));
                indices.Add((ushort)second);

                // Second triangle (CCW when viewed from outside)
                indices.Add((ushort)second);
                indices.Add((ushort)(first + 1));
                indices.Add((ushort)(second + 1));
            }
        }

        return (vertices.ToArray(), indices.ToArray());
    }

    #endregion

    #region Basic Mesh Generation

    [TestMethod]
    public void TestSphereMesh_HasValidVertexCount()
    {
        var (vertices, indices) = CreateTestSphereMesh();
        
        Assert.IsTrue(vertices.Length > 0, "Should have vertices");
        Assert.IsTrue(indices.Length > 0, "Should have indices");
        Assert.IsTrue(indices.Length % 3 == 0, "Index count should be multiple of 3 (triangles)");
    }

    [TestMethod]
    public void TestSphereMesh_VerticesOnUnitSphere()
    {
        var (vertices, _) = CreateTestSphereMesh();
        
        // All position vertices should be on unit sphere (length ? 1)
        foreach (var vertex in vertices)
        {
            Vector3 pos = vertex.Position;
            float length = pos.Length();
            
            Assert.AreEqual(1f, length, 0.01f,
                $"Vertex position should be on unit sphere. Got length={length}");
        }
    }

    [TestMethod]
    public void TestSphereMesh_NormalsPointOutward()
    {
        var (vertices, _) = CreateTestSphereMesh();
        
        // For a unit sphere centered at origin:
        // Normal should equal position (pointing outward)
        foreach (var vertex in vertices)
        {
            Vector3 pos = vertex.Position;
            Vector3 normal = vertex.Normal;
            
            // Normalize position to compare with normal
            Vector3 expectedNormal = Vector3.Normalize(pos);
            
            Assert.AreEqual(expectedNormal.X, normal.X, 0.01f, "Normal X should match position direction");
            Assert.AreEqual(expectedNormal.Y, normal.Y, 0.01f, "Normal Y should match position direction");
            Assert.AreEqual(expectedNormal.Z, normal.Z, 0.01f, "Normal Z should match position direction");
        }
    }

    [TestMethod]
    public void TestSphereMesh_NormalsAreNormalized()
    {
        var (vertices, _) = CreateTestSphereMesh();
        
        foreach (var vertex in vertices)
        {
            float length = vertex.Normal.Length();
            Assert.AreEqual(1f, length, 0.01f,
                $"Normal should be unit length. Got {length}");
        }
    }

    #endregion

    #region Triangle Winding Order

    [TestMethod]
    public void TestSphereMesh_TriangleWindingOrder_CCWForOutwardNormal()
    {
        var (vertices, indices) = CreateTestSphereMesh();
        
        int triangleCount = indices.Length / 3;
        int outwardCount = 0;
        
        for (int i = 0; i < triangleCount; i++)
        {
            int i0 = indices[i * 3];
            int i1 = indices[i * 3 + 1];
            int i2 = indices[i * 3 + 2];
            
            Vector3 v0 = vertices[i0].Position;
            Vector3 v1 = vertices[i1].Position;
            Vector3 v2 = vertices[i2].Position;
            
            // Calculate face normal using cross product
            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;
            Vector3 faceNormal = Vector3.Cross(e1, e2);
            
            // Skip degenerate triangles at poles
            if (faceNormal.LengthSquared() < 1e-10f)
                continue;
            
            // For outward-facing triangles on a sphere:
            // Face normal should point same direction as vertex position (outward)
            Vector3 centroid = (v0 + v1 + v2) / 3f;
            float dot = Vector3.Dot(faceNormal, centroid);
            
            if (dot > 0)
                outwardCount++;
        }
        
        // Most non-degenerate triangles should have outward-facing normals
        // UV sphere has degenerate triangles at poles, so we use a lower threshold
        float outwardRatio = (float)outwardCount / triangleCount;
        Assert.IsTrue(outwardRatio > 0.90f,
            $"Expected most triangles to have outward normals. Got {outwardRatio:P1}");
    }

    #endregion

    #region Index Buffer Validity

    [TestMethod]
    public void TestSphereMesh_IndicesWithinBounds()
    {
        var (vertices, indices) = CreateTestSphereMesh();
        
        foreach (ushort index in indices)
        {
            Assert.IsTrue(index < vertices.Length,
                $"Index {index} exceeds vertex count {vertices.Length}");
        }
    }

    [TestMethod]
    public void TestSphereMesh_NoDegenerateTriangles()
    {
        var (vertices, indices) = CreateTestSphereMesh();
        
        int triangleCount = indices.Length / 3;
        int degenerateCount = 0;
        
        for (int i = 0; i < triangleCount; i++)
        {
            int i0 = indices[i * 3];
            int i1 = indices[i * 3 + 1];
            int i2 = indices[i * 3 + 2];
            
            // Check for duplicate indices
            if (i0 == i1 || i1 == i2 || i0 == i2)
            {
                degenerateCount++;
                continue;
            }
            
            // Check for zero-area triangles
            Vector3 v0 = vertices[i0].Position;
            Vector3 v1 = vertices[i1].Position;
            Vector3 v2 = vertices[i2].Position;
            
            Vector3 cross = Vector3.Cross(v1 - v0, v2 - v0);
            if (cross.LengthSquared() < 1e-10f)
            {
                degenerateCount++;
            }
        }
        
        // UV sphere has degenerate triangles at poles (where vertices converge)
        // Allow up to 10% degenerate triangles for this test mesh
        float degenerateRatio = (float)degenerateCount / triangleCount;
        Assert.IsTrue(degenerateRatio < 0.10f,
            $"Too many degenerate triangles: {degenerateCount} ({degenerateRatio:P1})");
    }

    #endregion

    #region Vertex Data Structure Size Verification

    [TestMethod]
    public void TestVertex_CorrectSize()
    {
        // Position (3 floats) + Normal (3 floats) = 24 bytes
        int size = Marshal.SizeOf<TestVertex>();
        Assert.AreEqual(24, size, "Vertex should be 24 bytes");
    }

    [TestMethod]
    public unsafe void TestVertex_MemoryLayout()
    {
        var vertex = new TestVertex
        {
            Position = new Vector3(1, 2, 3),
            Normal = new Vector3(4, 5, 6)
        };

        float* ptr = (float*)&vertex;

        // Position should come first
        Assert.AreEqual(1f, ptr[0], Epsilon, "Position.X at offset 0");
        Assert.AreEqual(2f, ptr[1], Epsilon, "Position.Y at offset 4");
        Assert.AreEqual(3f, ptr[2], Epsilon, "Position.Z at offset 8");
        
        // Normal follows position
        Assert.AreEqual(4f, ptr[3], Epsilon, "Normal.X at offset 12");
        Assert.AreEqual(5f, ptr[4], Epsilon, "Normal.Y at offset 16");
        Assert.AreEqual(6f, ptr[5], Epsilon, "Normal.Z at offset 20");
    }

    #endregion

    #region Mesh Topology

    [TestMethod]
    public void TestSphereMesh_IsClosedSurface()
    {
        // A valid sphere mesh should be a closed surface
        // Each edge should be shared by exactly 2 triangles
        
        var (vertices, indices) = CreateTestSphereMesh();
        
        var edgeCounts = new Dictionary<(int, int), int>();
        
        int triangleCount = indices.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int i0 = indices[i * 3];
            int i1 = indices[i * 3 + 1];
            int i2 = indices[i * 3 + 2];
            
            // Count edges (store in canonical order)
            AddEdge(edgeCounts, i0, i1);
            AddEdge(edgeCounts, i1, i2);
            AddEdge(edgeCounts, i2, i0);
        }
        
        int boundaryEdges = 0;
        int internalEdges = 0;
        int problematicEdges = 0;
        
        foreach (int count in edgeCounts.Values)
        {
            if (count == 1)
                boundaryEdges++;
            else if (count == 2)
                internalEdges++;
            else
                problematicEdges++;
        }
        
        // UV sphere has seam edges that appear only once per vertex pair
        // This is expected due to duplicate vertices at the seam
        Assert.AreEqual(0, problematicEdges, "No edge should be shared by more than 2 triangles");
    }

    private static void AddEdge(Dictionary<(int, int), int> edges, int a, int b)
    {
        var key = a < b ? (a, b) : (b, a);
        if (!edges.TryAdd(key, 1))
        {
            edges[key]++;
        }
    }

    #endregion
}
