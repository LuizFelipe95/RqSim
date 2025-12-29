using System.Numerics;
using System.Linq;
using ImGuiNET;
using RqSimRenderingEngine.Abstractions;
using RqSimRenderingEngine.Rendering.Backend.DX12;
using RqSimRenderingEngine.Rendering.Backend.DX12.Rendering;
using RQSimulation;
using Vortice.Mathematics;

namespace RqSimForms;

public partial class Form_Main
{
    private DateTime _csrLastFrameTime = DateTime.Now;
    private float _csrFps;
    private int _csrFrameCount;
    private DateTime _csrFpsUpdateTime = DateTime.Now;

    // Cached graph data for rendering
    private int _csrNodeCount;
    private int _csrEdgeCount;
    private float[]? _csrNodeX;
    private float[]? _csrNodeY;
    private float[]? _csrNodeZ;
    private NodeState[]? _csrNodeStates;
    private List<(int u, int v, float w)>? _csrEdgeList;
    private double _csrSpectralDim;
    private DateTime _csrLastDebugLog = DateTime.MinValue;

    private void TimerCsr3D_Tick(object? sender, EventArgs e)
    {
        if (!_csrVisualizationInitialized) return;
        if (_csrRenderHost is null) return;
        if (tabControl_Main.SelectedTab == tabPage_3DVisualCSR)
            RenderCsrVisualizationFrame();
    }

    /// <summary>
    /// Clears all cached graph data and resets the renderer buffers.
    /// Called when starting a new simulation to prevent "ghost" data.
    /// </summary>
    private void ClearCsrVisualizationData()
    {
        _csrNodeCount = 0;
        _csrEdgeCount = 0;
        _csrNodeX = null;
        _csrNodeY = null;
        _csrNodeZ = null;
        _csrNodeStates = null;
        _csrEdgeList?.Clear();

        // Clear the cached graph from previous session to prevent "ghost" overlay
        _simApi?.ClearCachedGraph();

        // Reset manifold embedding state
        ResetManifoldEmbedding();

        // Clear renderer buffers
        if (_csrRenderHost is Dx12RenderHost dx12Host)
        {
            dx12Host.SetNodeInstances(Array.Empty<Dx12NodeInstance>(), 0);
            dx12Host.SetEdgeVertices(Array.Empty<Dx12LineVertex>(), 0);
        }

        // Clear standalone form if open
        _standalone3DForm?.ClearData();

        // Force overlay update
        _csrIsWaitingForData = true;
        UpdateCsrWaitingLabelVisibility(true);
    }

    private void RenderCsrVisualizationFrame()
    {
        if (_csrRenderHost is null) return;

        // If device is lost, stop trying to render
        if (_csrDx12Host?.IsDeviceLost == true)
        {
            // Only log once per second to avoid spam
            if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
            {
                System.Diagnostics.Debug.WriteLine("[CSR Render] Skipping frame - DX12 device lost");
                _csrLastDebugLog = DateTime.Now;
            }
            return;
        }

        bool beganFrame = false;

        try
        {
            // Throttled diagnostics: confirm which host is active and panel state
            if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
            {
                var hostType = _csrRenderHost.GetType().FullName ?? _csrRenderHost.GetType().Name;
                var dx12 = _csrRenderHost is Dx12RenderHost;
                var hwnd = _csrRenderPanel?.Handle ?? IntPtr.Zero;
                System.Diagnostics.Debug.WriteLine($"[CSR Render] Host={hostType}, IsDx12={dx12}, HWND=0x{hwnd.ToInt64():X}, Size={_csrRenderPanel?.Width}x{_csrRenderPanel?.Height}, Visible={_csrRenderPanel?.Visible}");
            }

            // BeginFrame first so we always have a chance to present even if simulation throws
            _csrRenderHost.BeginFrame();
            beganFrame = true;

            if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
            {
                System.Diagnostics.Debug.WriteLine("[CSR Render] BeginFrame called");
            }

            // Update cached graph data from simulation (best-effort)
            try
            {
                UpdateCsrGraphData();
            }
            catch (Exception ex)
            {
                if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
                {
                    System.Diagnostics.Debug.WriteLine($"[CSR Render] UpdateCsrGraphData threw: {ex.GetType().Name}: {ex.Message}");
                }
                // Keep last valid cached data to avoid blank frames
            }

            // Clear with dark background
            if (_csrDx12Host is not null)
            {
                _csrDx12Host.Clear(new Color4(0.02f, 0.02f, 0.05f, 1f));
            }

            // Draw 3D content via ImGui draw lists
            DrawCsr3DContent();

            // Draw ImGui overlay with graph info
            DrawCsrImGuiOverlay();

            UpdateCsrStats();
        }
        catch (Exception ex)
        {
            // Avoid log spam; show full frame errors at most once per second
            if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
            {
                System.Diagnostics.Debug.WriteLine($"[CSR Render] Frame error: {ex.GetType().Name}: {ex.Message}");
            }
        }
        finally
        {
            if (beganFrame)
            {
                try
                {
                    _csrRenderHost.EndFrame();
                }
                catch (Exception ex)
                {
                    if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CSR Render] EndFrame error: {ex.GetType().Name}: {ex.Message}");
                    }
                }

                if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
                {
                    System.Diagnostics.Debug.WriteLine("[CSR Render] EndFrame called");
                    _csrLastDebugLog = DateTime.Now;
                }
            }
        }
    }

    private void UpdateCsrGraphData()
    {
        RQGraph? graph = null;

        // First, check if we have an external simulation via shared memory
        var externalNodes = _lifeCycleManager?.GetExternalRenderNodes();
        if (externalNodes is not null && externalNodes.Length > 0)
        {
            // Use external simulation data from shared memory
            UpdateCsrFromExternalNodes(externalNodes);
            return;
        }

        // Use ActiveGraph which persists after simulation ends
        graph = _simApi?.ActiveGraph;

        // If we have a running simulation, also cache the graph
        if (_simApi?.SimulationEngine?.Graph is not null)
        {
            _simApi.CacheActiveGraph(_simApi.SimulationEngine.Graph);
            graph = _simApi.SimulationEngine.Graph;
        }

        if (graph is null)
        {
            // Throttled debug logging (once per second max)
            if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
            {
                System.Diagnostics.Debug.WriteLine($"[CSR Render] Graph is null. ActiveGraph={_simApi?.ActiveGraph is not null}");
            }

            // Generate test data if no graph available (verifies render pipeline)
            if (_csrNodeCount == 0)
            {
                GenerateTestVisualizationData();
            }
            return;
        }

        try
        {
            int n = graph.N;
            if (n == 0)
            {
                // Keep existing cached data if simulation is unstable; don't force blank
                _csrNodeCount = 0;
                _csrEdgeCount = 0;
                return;
            }

            _csrNodeCount = n;
            _csrSpectralDim = graph.SmoothedSpectralDimension;

            // Resize arrays if needed
            if (_csrNodeX is null || _csrNodeX.Length != n)
            {
                _csrNodeX = new float[n];
                _csrNodeY = new float[n];
                _csrNodeZ = new float[n];
                _csrNodeStates = new NodeState[n];
            }

            // Use SpectralX/Y/Z if available, otherwise fall back to 2D Coordinates
            bool hasSpectral = graph.SpectralX is not null && graph.SpectralX.Length == n;

#pragma warning disable CS0618 // Coordinates is obsolete but needed for visualization
            bool hasCoords = graph.Coordinates is not null && graph.Coordinates.Length == n;
#pragma warning restore CS0618

            // 1. Position Initialization (Manifold or Standard)
            if (_enableManifoldEmbedding)
            {
                if (NeedsManifoldInitialization(n))
                {
                    InitializeManifoldPositions(graph);
                }

                for (int i = 0; i < n; i++)
                {
                    _csrNodeX[i] = _embeddingPositionX![i];
                    _csrNodeY[i] = _embeddingPositionY![i];
                    _csrNodeZ[i] = _embeddingPositionZ![i];
                    _csrNodeStates![i] = graph.State[i];
                }
            }
            else
            {
                if (hasSpectral)
                {
                    for (int i = 0; i < n; i++)
                    {
                        _csrNodeX[i] = (float)graph.SpectralX![i];
                        _csrNodeY[i] = (float)graph.SpectralY![i];
                        _csrNodeZ[i] = (float)graph.SpectralZ![i];
                        _csrNodeStates![i] = graph.State[i];
                    }
                }
                else if (hasCoords)
                {
#pragma warning disable CS0618
                    for (int i = 0; i < n; i++)
                    {
                        _csrNodeX[i] = (float)graph.Coordinates[i].X;
                        _csrNodeY[i] = (float)graph.Coordinates[i].Y;
                        _csrNodeZ[i] = 0f; // 2D coordinates, Z = 0
                        _csrNodeStates![i] = graph.State[i];
                    }
#pragma warning restore CS0618
                }
                else
                {
                    // No coordinates - generate simple grid layout
                    int gridSize = (int)Math.Ceiling(Math.Sqrt(n));
                    float spacing = 2.0f;
                    for (int i = 0; i < n; i++)
                    {
                        int gx = i % gridSize;
                        int gy = i / gridSize;
                        _csrNodeX[i] = (gx - gridSize / 2f) * spacing;
                        _csrNodeY[i] = (gy - gridSize / 2f) * spacing;
                        _csrNodeZ[i] = 0f;
                        _csrNodeStates![i] = graph.State[i];
                    }
                }
            }

            // Build edge list using Neighbors() method
            _csrEdgeList ??= new List<(int, int, float)>(n * 4);
            _csrEdgeList.Clear();

            // Log coordinate range for debugging (once per second)
            if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
            {
                float minX = _csrNodeX.Take(n).Min();
                float maxX = _csrNodeX.Take(n).Max();
                float minY = _csrNodeY!.Take(n).Min();
                float maxY = _csrNodeY.Take(n).Max();
                float rangeX = maxX - minX;
                float rangeY = maxY - minY;
                System.Diagnostics.Debug.WriteLine($"[CSR Data] Coordinate range: X=[{minX:F3}, {maxX:F3}] (range={rangeX:F3}), Y=[{minY:F3}, {maxY:F3}] (range={rangeY:F3}), HasSpectral={hasSpectral}, HasCoords={hasCoords}");
            }

            // Collect edges if showing edges OR if manifold embedding is enabled
            if (_csrShowEdges || _enableManifoldEmbedding)
            {
                double threshold = _csrEdgeWeightThreshold;
                int step = Math.Max(1, n / 500); // Sample edges for large graphs

                for (int i = 0; i < n; i += step)
                {
                    foreach (int j in graph.Neighbors(i))
                    {
                        if (j > i)
                        {
                            float w = (float)graph.Weights[i, j];
                            if (w >= threshold)
                            {
                                _csrEdgeList.Add((i, j, w));
                            }
                        }
                    }
                }
            }
            _csrEdgeCount = _csrEdgeList.Count;

            // 2. Update Manifold Physics
            if (_enableManifoldEmbedding)
            {
                UpdateManifoldEmbedding(n, _csrNodeX, _csrNodeY, _csrNodeZ, _csrEdgeList);
            }

            // 3. Update Cluster IDs (for Clusters visualization mode)
            if (_csrVisMode == CsrVisualizationMode.Clusters)
            {
                UpdateCsrClusterIds(graph, n);
            }

            // 4. Update Target Metrics (only if overlay is shown)
            if (_showTargetOverlay)
            {
                UpdateTargetMetrics(graph);
            }
        }
        catch (Exception ex)
        {
            // Keep last valid cached data; don't force blank.
            if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
            {
                System.Diagnostics.Debug.WriteLine($"[CSR Render] UpdateCsrGraphData error: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Updates CSR graph data from external simulation nodes received via shared memory.
    /// This is used when UI is connected to console server mode.
    /// </summary>
    private void UpdateCsrFromExternalNodes(RqSimForms.ProcessesDispatcher.Contracts.RenderNode[] nodes)
    {
        int n = nodes.Length;
        if (n == 0)
        {
            _csrNodeCount = 0;
            _csrEdgeCount = 0;
            return;
        }

        _csrNodeCount = n;

        // Resize arrays if needed
        if (_csrNodeX is null || _csrNodeX.Length != n)
        {
            _csrNodeX = new float[n];
            _csrNodeY = new float[n];
            _csrNodeZ = new float[n];
            _csrNodeStates = new NodeState[n];
        }

        // Copy positions from external nodes
        for (int i = 0; i < n; i++)
        {
            _csrNodeX[i] = nodes[i].X;
            _csrNodeY[i] = nodes[i].Y;
            _csrNodeZ[i] = nodes[i].Z;
            
            // Determine node state from color (R channel indicates excited state)
            // Console sends: R=0.2 for normal, higher for excited
            _csrNodeStates![i] = nodes[i].R > 0.5f ? NodeState.Excited : NodeState.Rest;
        }

        // Clear edge list - external mode doesn't provide edge data via shared memory
        _csrEdgeList ??= new List<(int, int, float)>();
        _csrEdgeList.Clear();
        _csrEdgeCount = 0;

        // Throttled debug logging
        if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
        {
            float minX = _csrNodeX.Min();
            float maxX = _csrNodeX.Max();
            float minY = _csrNodeY.Min();
            float maxY = _csrNodeY.Max();
            System.Diagnostics.Debug.WriteLine($"[CSR External] Loaded {n} nodes from shared memory. X=[{minX:F2},{maxX:F2}], Y=[{minY:F2},{maxY:F2}]");
        }
    }

    private void DrawCsr3DContent()
    {
        // Safety check: don't call ImGui if device is lost
        if (_csrDx12Host?.IsDeviceLost == true)
        {
            System.Diagnostics.Debug.WriteLine("[CSR Draw] Skipping - DX12 device lost");
            return;
        }

        if (_csrNodeCount == 0 || _csrNodeX is null || _csrCamera is null)
        {
            // Draw waiting overlay when no data available (matching standalone behavior)
            DrawCsrWaitingOverlayIfNeeded();
            return;
        }

        // Switch between rendering modes (matching Form_Rsim3DForm.DrawGraph)
        if (_csrRenderMode3D == CsrRenderMode3D.Gpu3D)
        {
            RenderCsrSceneGpu3D();
            return;
        }

        // === ImGui 2D Mode (CPU-based legacy rendering) ===
        // Use foreground draw list instead of background - it draws ON TOP of everything
        var drawList = ImGui.GetForegroundDrawList();
        int panelW = _csrRenderPanel?.Width ?? 800;
        int panelH = _csrRenderPanel?.Height ?? 600;
        float cx = panelW / 2f;
        float cy = panelH / 2f;

        // Simple 3D to 2D projection using orbit camera
        float cosYaw = MathF.Cos(_csrCamera.Yaw);
        float sinYaw = MathF.Sin(_csrCamera.Yaw);
        float cosPitch = MathF.Cos(_csrCamera.Pitch);
        float sinPitch = MathF.Sin(_csrCamera.Pitch);

        // Limit node count for stackalloc
        int count = Math.Min(_csrNodeCount, 8000);
        
        // First pass: compute data bounds and center
        float dataMinX = float.MaxValue, dataMaxX = float.MinValue;
        float dataMinY = float.MaxValue, dataMaxY = float.MinValue;
        float sumX = 0, sumY = 0;
        
        Span<Vector2> rotatedPos = stackalloc Vector2[count];
        Span<float> depths = stackalloc float[count];
        
        for (int i = 0; i < count; i++)
        {
            float x = _csrNodeX[i];
            float y = _csrNodeY![i];
            float z = _csrNodeZ![i];

            // Rotate around Y (yaw)
            float rx = x * cosYaw - z * sinYaw;
            float rz = x * sinYaw + z * cosYaw;

            // Rotate around X (pitch)
            float ry = y * cosPitch - rz * sinPitch;
            float rz2 = y * sinPitch + rz * cosPitch;

            rotatedPos[i] = new Vector2(rx, ry);
            depths[i] = rz2;
            
            dataMinX = Math.Min(dataMinX, rx);
            dataMaxX = Math.Max(dataMaxX, rx);
            dataMinY = Math.Min(dataMinY, ry);
            dataMaxY = Math.Max(dataMaxY, ry);
            sumX += rx;
            sumY += ry;
        }
        
        // Calculate auto-scale to fit data in panel with margin
        float dataRangeX = dataMaxX - dataMinX;
        float dataRangeY = dataMaxY - dataMinY;
        float dataRange = Math.Max(dataRangeX, dataRangeY);
        if (dataRange < 0.001f) dataRange = 1f; // Prevent division by zero
        
        float margin = 0.1f; // 10% margin on each side
        float availableSize = Math.Min(panelW, panelH) * (1f - 2f * margin);
        float autoScale = availableSize / dataRange;
        
        // Apply zoom from camera (zoom = 100 is default, larger = zoomed out)
        float zoomFactor = 100f / Math.Max(_csrCamera.Distance, 1f);
        float scale = autoScale * zoomFactor;
        
        // Calculate data center for centering
        float dataCenterX = sumX / count;
        float dataCenterY = sumY / count;
        
        // Second pass: convert to screen coordinates
        Span<Vector2> screenPos = stackalloc Vector2[count];
        float minScreenX = float.MaxValue, maxScreenX = float.MinValue;
        float minScreenY = float.MaxValue, maxScreenY = float.MinValue;
        
        for (int i = 0; i < count; i++)
        {
            // Center data around origin, then scale and offset to screen center
            float screenX = cx + (rotatedPos[i].X - dataCenterX) * scale;
            float screenY = cy - (rotatedPos[i].Y - dataCenterY) * scale;
            screenPos[i] = new Vector2(screenX, screenY);
            
            minScreenX = Math.Min(minScreenX, screenX);
            maxScreenX = Math.Max(maxScreenX, screenX);
            minScreenY = Math.Min(minScreenY, screenY);
            maxScreenY = Math.Max(maxScreenY, screenY);
        }

        // Debug: log screen bounds once per second
        if ((DateTime.Now - _csrLastDebugLog).TotalSeconds > 1)
        {
            System.Diagnostics.Debug.WriteLine($"[CSR Draw] Nodes={count}, AutoScale={autoScale:F1}, Scale={scale:F1}, ScreenBounds=[{minScreenX:F0},{minScreenY:F0}]-[{maxScreenX:F0},{maxScreenY:F0}], Panel={panelW}x{panelH}");
            _csrLastDebugLog = DateTime.Now;
        }

        // Draw edges first (behind nodes)
        if (_csrShowEdges && _csrEdgeList is not null)
        {
            foreach (var (u, v, w) in _csrEdgeList)
            {
                if (u < count && v < count)
                {
                    // Use mode-based edge styling
                    var (edgeColor, thickness) = GetCsrEdgeStyle(u, v, w);
                    uint col = ImGui.ColorConvertFloat4ToU32(edgeColor);
                    drawList.AddLine(screenPos[u], screenPos[v], col, thickness);
                }
            }
        }

        // Draw nodes as circles - use mode-based coloring
        for (int i = 0; i < count; i++)
        {
            // Use mode-based node coloring
            Vector4 color = GetCsrNodeColor(i);

            float depthFactor = 1f - Math.Clamp(depths[i] / 10f, -0.5f, 0.5f);
            float radius = 1.3f + depthFactor * 1f; // Reduced radius (was 4f + 3f, now ~3x smaller)

            uint col = ImGui.ColorConvertFloat4ToU32(color);
            drawList.AddCircleFilled(screenPos[i], radius, col);
        }

        // Update FPS counter
        _csrFrameCount++;
        if ((DateTime.Now - _csrFpsUpdateTime).TotalSeconds >= 1.0)
        {
            _csrFps = _csrFrameCount / (float)(DateTime.Now - _csrFpsUpdateTime).TotalSeconds;
            _csrFrameCount = 0;
            _csrFpsUpdateTime = DateTime.Now;
        }
    }

    private void DrawCsrImGuiOverlay()
    {
        // Safety check: don't call ImGui if device is lost
        if (_csrDx12Host?.IsDeviceLost == true)
            return;

        // Controls hint at bottom - shifted right to avoid overlapping with left panel scrollbar
        // Leave extra space for the left controls panel + metrics panel.
        float leftMargin = Math.Max(280f, (_csrControlsHostPanel?.Width ?? 0) + 40f);
        ImGui.SetNextWindowPos(new Vector2(leftMargin, (float)((_csrRenderPanel?.Height ?? 600) - 50)), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.5f);
        if (ImGui.Begin("##Controls", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1f), "LMB: Rotate | Wheel: Zoom");
        }
        ImGui.End();
    }

    private void CsrRenderPanel_Resize(object? sender, EventArgs e)
    {
        if (_csrRenderHost is not null && _csrRenderPanel is not null)
            _csrRenderHost.Resize(Math.Max(_csrRenderPanel.Width, 1), Math.Max(_csrRenderPanel.Height, 1));
    }

    /// <summary>
    /// Generate synthetic test data to verify the render pipeline works.
    /// Creates a simple circular arrangement of nodes with edges.
    /// </summary>
    private void GenerateTestVisualizationData()
    {
        const int testNodeCount = 50;
        
        _csrNodeCount = testNodeCount;
        _csrNodeX = new float[testNodeCount];
        _csrNodeY = new float[testNodeCount];
        _csrNodeZ = new float[testNodeCount];
        _csrNodeStates = new NodeState[testNodeCount];

        // Generate circular layout
        float radius = 15f;
        for (int i = 0; i < testNodeCount; i++)
        {
            float angle = 2f * MathF.PI * i / testNodeCount;
            _csrNodeX[i] = radius * MathF.Cos(angle);
            _csrNodeY[i] = radius * MathF.Sin(angle);
            _csrNodeZ[i] = 0f;
            _csrNodeStates[i] = i % 3 == 0 ? NodeState.Excited : 
                                i % 3 == 1 ? NodeState.Refractory : NodeState.Rest;
        }

        // Generate edges connecting adjacent nodes
        _csrEdgeList ??= new List<(int, int, float)>(testNodeCount * 2);
        _csrEdgeList.Clear();

        for (int i = 0; i < testNodeCount; i++)
        {
            int next = (i + 1) % testNodeCount;
            _csrEdgeList.Add((i, next, 0.5f));
            
            // Also connect to opposite node for cross pattern
            int opposite = (i + testNodeCount / 2) % testNodeCount;
            if (i < testNodeCount / 2)
            {
                _csrEdgeList.Add((i, opposite, 0.3f));
            }
        }

        _csrEdgeCount = _csrEdgeList.Count;
        _csrSpectralDim = 2.0; // Test value

        System.Diagnostics.Debug.WriteLine($"[CSR Render] Generated test data: {testNodeCount} nodes, {_csrEdgeCount} edges");
    }
}
