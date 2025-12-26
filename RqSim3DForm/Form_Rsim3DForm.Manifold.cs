using RQSimulation;

namespace RqSim3DForm;

/// <summary>
/// Manifold Embedding functionality for standalone 3D visualization.
/// Force-directed graph layout based on RQ-hypothesis principles.
/// </summary>
public partial class Form_Rsim3DForm
{
    // === Manifold Embedding State ===
    private bool _enableManifoldEmbedding = true; // Enabled by default
    private bool _embeddingInitialized;
    private float[]? _embeddingPositionX;
    private float[]? _embeddingPositionY;
    private float[]? _embeddingPositionZ;
    private float[]? _embeddingVelocityX;
    private float[]? _embeddingVelocityY;
    private float[]? _embeddingVelocityZ;

    // === Manifold Physics Constants (matched to original GDI+ version) ===
    private const double ManifoldRepulsionFactor = 0.5;  // Was 50.0 - way too high!
    private const double ManifoldSpringFactor = 0.8;     // Was 0.5
    private const double ManifoldDamping = 0.85;
    private const double ManifoldDeltaTime = 0.05;       // Was 0.016 - increased for faster convergence

    /// <summary>
    /// Resets all manifold embedding state.
    /// </summary>
    private void ResetManifoldEmbedding()
    {
        _embeddingInitialized = false;
        _embeddingPositionX = null;
        _embeddingPositionY = null;
        _embeddingPositionZ = null;
        _embeddingVelocityX = null;
        _embeddingVelocityY = null;
        _embeddingVelocityZ = null;
    }

    /// <summary>
    /// Initializes manifold embedding positions from current coordinates.
    /// </summary>
    private void InitializeManifoldPositions(int n, float[] x, float[] y, float[] z)
    {
        _embeddingPositionX = new float[n];
        _embeddingPositionY = new float[n];
        _embeddingPositionZ = new float[n];
        _embeddingVelocityX = new float[n];
        _embeddingVelocityY = new float[n];
        _embeddingVelocityZ = new float[n];

        for (int i = 0; i < n; i++)
        {
            _embeddingPositionX[i] = x[i];
            _embeddingPositionY[i] = y[i];
            _embeddingPositionZ[i] = z[i];
        }

        _embeddingInitialized = true;
    }

    /// <summary>
    /// Checks if manifold embedding needs (re)initialization.
    /// </summary>
    private bool NeedsManifoldInitialization(int nodeCount)
    {
        if (!_embeddingInitialized) return true;
        if (_embeddingPositionX == null || _embeddingPositionX.Length != nodeCount) return true;
        return false;
    }

    /// <summary>
    /// Updates manifold embedding positions based on force-directed layout.
    /// </summary>
    private void UpdateManifoldEmbedding(int n, float[] x, float[] y, float[] z, List<(int u, int v, float w)>? edges)
    {
        if (_embeddingVelocityX == null || _embeddingPositionX == null) return;
        if (_embeddingVelocityX.Length != n || _embeddingPositionX.Length != n) return;
        if (edges == null) return;

        float[] forceX = new float[n];
        float[] forceY = new float[n];
        float[] forceZ = new float[n];

        // Calculate center of mass
        float comX = 0, comY = 0, comZ = 0;
        for (int i = 0; i < n; i++)
        {
            comX += x[i];
            comY += y[i];
            comZ += z[i];
        }
        comX /= n;
        comY /= n;
        comZ /= n;

        // 1. Global repulsion from center (prevents collapse)
        for (int i = 0; i < n; i++)
        {
            float dx = x[i] - comX;
            float dy = y[i] - comY;
            float dz = z[i] - comZ;
            float dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz) + 0.1f;

            float repulsion = (float)(ManifoldRepulsionFactor / (dist * dist));
            forceX[i] += dx / dist * repulsion;
            forceY[i] += dy / dist * repulsion;
            forceZ[i] += dz / dist * repulsion;
        }

        // 2. Spring attraction along edges
        foreach (var (u, v, w) in edges)
        {
            if (u >= n || v >= n) continue;

            float dx = x[v] - x[u];
            float dy = y[v] - y[u];
            float dz = z[v] - z[u];
            float dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz) + 0.01f;

            float targetDist = 1.0f / (w + 0.1f);
            float springForce = (float)(ManifoldSpringFactor * w * (dist - targetDist));

            float fx = dx / dist * springForce;
            float fy = dy / dist * springForce;
            float fz = dz / dist * springForce;

            forceX[u] += fx;
            forceY[u] += fy;
            forceZ[u] += fz;
            forceX[v] -= fx;
            forceY[v] -= fy;
            forceZ[v] -= fz;
        }

        // 3. Integration with damping
        float dt = (float)ManifoldDeltaTime;
        float damping = (float)ManifoldDamping;

        for (int i = 0; i < n; i++)
        {
            _embeddingVelocityX[i] = (_embeddingVelocityX[i] + forceX[i] * dt) * damping;
            _embeddingVelocityY![i] = (_embeddingVelocityY[i] + forceY[i] * dt) * damping;
            _embeddingVelocityZ![i] = (_embeddingVelocityZ[i] + forceZ[i] * dt) * damping;

            _embeddingPositionX[i] += _embeddingVelocityX[i] * dt;
            _embeddingPositionY![i] += _embeddingVelocityY[i] * dt;
            _embeddingPositionZ![i] += _embeddingVelocityZ[i] * dt;

            // Copy to output
            x[i] = _embeddingPositionX[i];
            y[i] = _embeddingPositionY[i];
            z[i] = _embeddingPositionZ[i];
        }
    }

    /// <summary>
    /// Applies manifold embedding to graph data if enabled.
    /// </summary>
    private void ApplyManifoldEmbedding()
    {
        if (!_enableManifoldEmbedding) return;
        if (_nodeX == null || _nodeY == null || _nodeZ == null) return;
        if (_nodeCount == 0) return;

        if (NeedsManifoldInitialization(_nodeCount))
        {
            InitializeManifoldPositions(_nodeCount, _nodeX, _nodeY, _nodeZ);
        }

        // Filter edges by threshold for manifold physics (matching CSR behavior)
        // Manifold should only use edges above threshold for force calculations
        List<(int u, int v, float w)>? filteredEdges = null;
        if (_edges is not null)
        {
            filteredEdges = new List<(int, int, float)>(_edges.Count);
            foreach (var (u, v, w) in _edges)
            {
                if (w >= _edgeWeightThreshold)
                {
                    filteredEdges.Add((u, v, w));
                }
            }
        }

        UpdateManifoldEmbedding(_nodeCount, _nodeX, _nodeY, _nodeZ, filteredEdges);
    }
}
