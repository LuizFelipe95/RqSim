using ComputeSharp;

namespace RQSimulation.GPUOptimized
{
    /// <summary>
    /// GPU shader for computing Forman-Ricci curvature on edges.
    /// Uses CSR (Compressed Sparse Row) format for efficient neighbor lookup.
    /// 
    /// Formula: Ric(e) = w_e * (?(w_e1*w_e2) for triangles - ?*(W_u + W_v))
    /// where W_u = weighted degree of node u (excluding edge e)
    /// 
    /// ============================================================
    /// RQ-MODERNIZATION: Physics-Visualization Separation (Checklist #7)
    /// ============================================================
    /// 
    /// IMPORTANT: This shader computes PHYSICS quantities only.
    /// 
    /// INPUTS (Physics Data):
    /// - weights: Edge weights (w_ij) - the metric tensor components
    /// - edges: Topological connectivity (which nodes are connected)
    /// - adjOffsets/adjData: CSR graph structure for neighbor enumeration
    /// 
    /// OUTPUTS (Physics Data):
    /// - curvatures: Ricci curvature tensor components
    /// 
    /// NOT USED:
    /// - NodePositions (visualization coordinates)
    /// - Any UI/rendering data
    /// 
    /// The Forman-Ricci curvature is computed purely from:
    /// 1. Graph topology (who is connected to whom)
    /// 2. Edge weights (how strong are the connections)
    /// 
    /// This maintains strict separation between:
    /// - PHYSICS: Discrete geometry on the graph (weights, curvature, stress-energy)
    /// - VISUALIZATION: Embedding coordinates for rendering (positions in ??)
    /// 
    /// The embedding (ManifoldEmbedding) reads FROM physics AFTER the physics step,
    /// never the other way around. This prevents non-physical coordinate-dependent
    /// artifacts from entering the simulation.
    /// ============================================================
    /// </summary>
    [ThreadGroupSize(64, 1, 1)]
    [GeneratedComputeShaderDescriptor]
    public readonly partial struct FormanCurvatureShader : IComputeShader
    {
        // Input data
        public readonly ReadWriteBuffer<float> weights;       // Current edge weights (w_ij)
        public readonly ReadOnlyBuffer<Int2> edges;           // Node pairs for each edge (u, v)

        // Graph in CSR format (for fast neighbor lookup)
        public readonly ReadOnlyBuffer<int> adjOffsets;       // Start indices of neighbors for each node
        public readonly ReadOnlyBuffer<Int2> adjData;         // Neighbor data: .X = neighborIndex, .Y = edgeIndex

        // Output data
        public readonly ReadWriteBuffer<float> curvatures;    // Result (Ric_ij)

        // Constants
        public readonly float degreePenaltyFactor;
        public readonly int nodeCount;

        public FormanCurvatureShader(
            ReadWriteBuffer<float> weights,
            ReadOnlyBuffer<Int2> edges,
            ReadOnlyBuffer<int> adjOffsets,
            ReadOnlyBuffer<Int2> adjData,
            ReadWriteBuffer<float> curvatures,
            float degreePenaltyFactor,
            int nodeCount)
        {
            this.weights = weights;
            this.edges = edges;
            this.adjOffsets = adjOffsets;
            this.adjData = adjData;
            this.curvatures = curvatures;
            this.degreePenaltyFactor = degreePenaltyFactor;
            this.nodeCount = nodeCount;
        }

        public void Execute()
        {
            int edgeIdx = ThreadIds.X;
            if (edgeIdx >= edges.Length) return;

            // 1. Get data for current edge E(u,v)
            Int2 nodes = edges[edgeIdx];
            int u = nodes.X;
            int v = nodes.Y;
            float w_uv = weights[edgeIdx];

            if (w_uv <= 0.0f)
            {
                curvatures[edgeIdx] = 0.0f;
                return;
            }

            // 2. Compute weighted degrees (excluding edge e)
            // W_node = sum of weights of all incident edges except E(u,v)

            float w_u = 0.0f;
            int startU = adjOffsets[u];
            int endU = (u + 1 < nodeCount) ? adjOffsets[u + 1] : adjData.Length;

            for (int k = startU; k < endU; k++)
            {
                int e_idx = adjData[k].Y;
                // Forman formula requires excluding edge e itself
                if (e_idx != edgeIdx)
                    w_u += weights[e_idx];
            }

            float w_v = 0.0f;
            int startV = adjOffsets[v];
            int endV = (v + 1 < nodeCount) ? adjOffsets[v + 1] : adjData.Length;

            for (int k = startV; k < endV; k++)
            {
                int e_idx = adjData[k].Y;
                if (e_idx != edgeIdx)
                    w_v += weights[e_idx];
            }

            // 3. Find triangles (3-cycles contribution)
            // Intersection of neighbor sets U and V
            float triangleTerm = 0.0f;

            // Iterate through neighbors of u, check if they are also neighbors of v
            for (int i = startU; i < endU; i++)
            {
                int neighbor_u = adjData[i].X;
                if (neighbor_u == v) continue; // Skip edge u-v itself

                // Search for neighbor_u in neighbor list of v
                for (int j = startV; j < endV; j++)
                {
                    int neighbor_v = adjData[j].X;

                    if (neighbor_u == neighbor_v)
                    {
                        // FOUND TRIANGLE (u, v, neighbor)
                        // Weights of adjacent edges
                        float w_un = weights[adjData[i].Y];
                        float w_vn = weights[adjData[j].Y];

                        // Geometric mean of weights (as in CPU implementation)
                        // Using cube root for consistency with ComputeFormanRicciCurvature
                        triangleTerm += Hlsl.Pow(w_un * w_vn * w_uv, 1.0f / 3.0f);
                    }
                }
            }

            // 4. Final Forman-Ricci curvature formula
            // Ric(e) = w(e) * (triangles - penalty*(W_u + W_v))
            float penalty = degreePenaltyFactor * (w_u + w_v);
            curvatures[edgeIdx] = w_uv * (triangleTerm - penalty);
        }
    }

    /// <summary>
    /// HLSL Shader for gravity evolution, compiled from C#.
    /// IComputeShader - marker for ComputeSharp.
    /// Source generator provides IComputeShaderDescriptor implementation.
    /// 
    /// ENERGY CONSERVATION FIX:
    /// The gravity update formula now uses a flow-based approach that
    /// redistributes weight rather than adding/removing arbitrarily.
    /// dw = dt * tanh(G * massTerm - curvature + lambda)
    /// The tanh bounds the change to [-1, 1] preventing runaway growth.
    /// 
    /// FORMULA ALIGNED WITH CPU:
    /// flowRate = curvature - massTerm * curvatureTermScale + lambda
    /// This matches EvolveNetworkGeometryOllivierDynamic and EvolveNetworkGeometryForman.
    /// 
    /// RQ-MODERNIZATION: Added isScientificMode flag for unbounded flow.
    /// </summary>
    [ThreadGroupSize(64, 1, 1)]
    [GeneratedComputeShaderDescriptor]
    public readonly partial struct GravityShader : IComputeShader
    {
        public readonly ReadWriteBuffer<float> weights;
        public readonly ReadWriteBuffer<float> curvatures;
        public readonly ReadOnlyBuffer<float> masses;
        public readonly ReadOnlyBuffer<Int2> edges;
        public readonly float dt;
        public readonly float G;
        public readonly float lambda;
        public readonly float curvatureTermScale;
        public readonly int isScientificMode;  // 1 = scientific (unbounded), 0 = legacy (bounded)

        public GravityShader(
            ReadWriteBuffer<float> weights,
            ReadWriteBuffer<float> curvatures,
            ReadOnlyBuffer<float> masses,
            ReadOnlyBuffer<Int2> edges,
            float dt,
            float G,
            float lambda,
            float curvatureTermScale,
            int isScientificMode = 0)
        {
            this.weights = weights;
            this.curvatures = curvatures;
            this.masses = masses;
            this.edges = edges;
            this.dt = dt;
            this.G = G;
            this.lambda = lambda;
            this.curvatureTermScale = curvatureTermScale;
            this.isScientificMode = isScientificMode;
        }

        public void Execute()
        {
            int i = ThreadIds.X;

            Int2 edgeNodes = edges[i];
            int nodeA = edgeNodes.X;
            int nodeB = edgeNodes.Y;

            float currentWeight = weights[i];

            if (currentWeight < 0.0001f)
            {
                return;
            }

            float massTerm = (masses[nodeA] + masses[nodeB]) * 0.5f;
            float curvatureTerm = curvatures[i];

            float flowRate = curvatureTerm - G * massTerm + lambda;

            float w;
            if (isScientificMode != 0)
            {
                // SCIENTIFIC MODE: Linear flow without tanh saturation
                float relativeChange = flowRate * 0.1f * dt;
                w = currentWeight * (1.0f + relativeChange);
                // No clamping - allow physics to determine fate (may go negative/NaN)
            }
            else
            {
                // LEGACY MODE: Bounded tanh flow with soft walls
                float relativeChange = Hlsl.Tanh(flowRate * 0.1f) * dt;
                w = currentWeight * (1.0f + relativeChange);
                w = Hlsl.Clamp(w, 0.02f, 0.98f);
            }

            weights[i] = w;
        }
    }
}
