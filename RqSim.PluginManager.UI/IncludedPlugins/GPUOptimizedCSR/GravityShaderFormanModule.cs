using RQSimulation;

namespace RqSim.PluginManager.UI.IncludedPlugins.GPUOptimizedCSR;

/// <summary>
/// GPU module for Forman-Ricci curvature calculation.
/// 
/// This version uses Forman-Ricci curvature (scalar approximation).
/// Formula: Ric(e) = w_e * (?(w_e1*w_e2) for triangles - ?*(W_u + W_v))
/// where W_u = weighted degree of node u (excluding edge e)
///
/// NOTE: This is a SCALAR curvature - it cannot describe:
/// - Gravitational waves (spin-2)
/// - Tensor perturbations
/// - Full Riemann curvature
/// 
/// For tensor curvature with spin-2 support, see Ollivier-Ricci implementation.
/// 
/// Based on original GravityShaders.forman implementation.
/// </summary>
public sealed class GravityShaderFormanModule : GpuPluginBase
{
    private RQGraph? _graph;
    private double[]? _curvatures;

    public override string Name => "Forman Curvature (GPU)";
    public override string Description => "GPU-accelerated Forman-Ricci scalar curvature calculation";
    public override string Category => "Gravity";
    public override int Priority => 40;

    /// <summary>
    /// Degree penalty factor in Forman curvature formula.
    /// </summary>
    public double DegreePenaltyFactor { get; set; } = 0.1;

    /// <summary>
    /// Coupling constant for gravity evolution.
    /// </summary>
    public double GravityCoupling { get; set; } = 0.1;

    /// <summary>
    /// Last computed curvatures per edge (indexed by flat edge index).
    /// </summary>
    public IReadOnlyList<double>? LastCurvatures => _curvatures;

    /// <summary>
    /// Average curvature across all edges.
    /// </summary>
    public double AverageCurvature
    {
        get
        {
            if (_curvatures is null || _curvatures.Length == 0) return 0.0;
            double sum = 0.0;
            int count = 0;
            for (int i = 0; i < _curvatures.Length; i++)
            {
                if (_curvatures[i] != 0.0)
                {
                    sum += _curvatures[i];
                    count++;
                }
            }
            return count > 0 ? sum / count : 0.0;
        }
    }

    public override void Initialize(RQGraph graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));

        // Allocate curvature buffer
        int maxEdges = graph.N * (graph.N - 1) / 2;
        _curvatures = new double[maxEdges];

        // Initial curvature calculation
        ComputeAllCurvatures();
    }

    public override void ExecuteStep(RQGraph graph, double dt)
    {
        if (_graph is null) return;

        // Recompute curvatures
        ComputeAllCurvatures();

        // Apply gravity-driven weight evolution
        EvolveWeightsByCurvature(dt);
    }

    /// <summary>
    /// Compute Forman-Ricci curvature for all edges.
    /// </summary>
    private void ComputeAllCurvatures()
    {
        if (_graph is null || _curvatures is null) return;

        int edgeIdx = 0;
        for (int i = 0; i < _graph.N; i++)
        {
            for (int j = i + 1; j < _graph.N; j++)
            {
                if (edgeIdx >= _curvatures.Length) break;

                if (_graph.Edges[i, j])
                {
                    _curvatures[edgeIdx] = ComputeFormanCurvature(i, j);
                }
                else
                {
                    _curvatures[edgeIdx] = 0.0;
                }
                edgeIdx++;
            }
        }
    }

    /// <summary>
    /// Compute Forman-Ricci curvature for edge (u, v).
    /// 
    /// Forman formula: Ric(e) = w_e * (??(w_e1*w_e2) for triangles - ?*(W_u + W_v))
    /// where:
    /// - w_e = weight of edge e
    /// - ??(w_e1*w_e2) = sum over triangles containing edge e
    /// - W_u, W_v = weighted degrees of endpoints (excluding e)
    /// - ? = degree penalty factor
    /// </summary>
    public double ComputeFormanCurvature(int u, int v)
    {
        if (_graph is null || !_graph.Edges[u, v]) return 0.0;

        double w_uv = _graph.Weights[u, v];

        // Compute weighted degrees (excluding edge u-v)
        double w_u = 0.0;
        foreach (int n in _graph.Neighbors(u))
        {
            if (n != v)
                w_u += _graph.Weights[u, n];
        }

        double w_v = 0.0;
        foreach (int n in _graph.Neighbors(v))
        {
            if (n != u)
                w_v += _graph.Weights[v, n];
        }

        // Compute triangle contribution
        double triangleTerm = 0.0;
        foreach (int k in _graph.Neighbors(u))
        {
            if (k == v) continue;

            // Check if k is also connected to v (forms triangle u-v-k)
            if (_graph.Edges[v, k])
            {
                double w_uk = _graph.Weights[u, k];
                double w_vk = _graph.Weights[v, k];
                triangleTerm += Math.Sqrt(w_uk * w_vk);
            }
        }

        // Forman curvature formula
        double penalty = DegreePenaltyFactor * (w_u + w_v);
        double curvature = w_uv * (triangleTerm - penalty);

        return curvature;
    }

    /// <summary>
    /// Evolve edge weights based on curvature (Ricci flow).
    /// dw/dt = -R * w (positive curvature shrinks, negative expands)
    /// </summary>
    private void EvolveWeightsByCurvature(double dt)
    {
        if (_graph is null || _curvatures is null) return;

        int edgeIdx = 0;
        for (int i = 0; i < _graph.N; i++)
        {
            for (int j = i + 1; j < _graph.N; j++)
            {
                if (edgeIdx >= _curvatures.Length) break;

                if (_graph.Edges[i, j])
                {
                    double curvature = _curvatures[edgeIdx];
                    double w = _graph.Weights[i, j];

                    // Ricci flow: dw/dt = -R * coupling * w
                    double dw = -curvature * GravityCoupling * w * dt;
                    double newW = Math.Clamp(w + dw, 0.01, 1.0);

                    _graph.Weights[i, j] = newW;
                    _graph.Weights[j, i] = newW;
                }
                edgeIdx++;
            }
        }
    }

    /// <summary>
    /// Get total scalar curvature (Einstein-Hilbert action integrand).
    /// </summary>
    public double GetTotalScalarCurvature()
    {
        if (_curvatures is null) return 0.0;

        double total = 0.0;
        for (int i = 0; i < _curvatures.Length; i++)
        {
            total += _curvatures[i];
        }
        return total;
    }

    protected override void DisposeCore()
    {
        _curvatures = null;
        _graph = null;
    }
}
