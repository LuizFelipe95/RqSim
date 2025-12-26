using System.Numerics;
using RqSimRenderingEngine.Rendering.Backend.DX12.Rendering;
using ImGuiNET;
using Vortice.Mathematics;

namespace RqSim3DForm;

public partial class Form_Rsim3DForm
{
    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        if (_renderHost is null || _dx12Host?.IsDeviceLost == true)
            return;

        try
        {
            UpdateGraphData();

            // Apply manifold embedding if enabled
            ApplyManifoldEmbedding();

            // Update target metrics
            if (_showTargetOverlay && _nodeCount > 0)
            {
                var data = new GraphRenderData(_nodeX, _nodeY, _nodeZ, _nodeStates, _edges, _nodeCount, _spectralDim);
                UpdateTargetMetrics(data);
            }

            _renderHost.BeginFrame();
            _dx12Host?.Clear(new Color4(0.02f, 0.02f, 0.05f, 1f));

            DrawGraph();
            DrawImGuiOverlay();

            _renderHost.EndFrame();

            UpdateFps();
        }
        catch (Exception ex)
        {
            if ((DateTime.Now - _lastDebugLog).TotalSeconds > 1)
            {
                System.Diagnostics.Debug.WriteLine($"[3DForm] Render error: {ex.Message}");
                _lastDebugLog = DateTime.Now;
            }
        }
    }

    private void UpdateGraphData()
    {
        if (_getGraphData is null) return;

        try
        {
            var data = _getGraphData();
            
            // Don't overwrite positions if manifold embedding is active and we already have data
            // Manifold embedding modifies _nodeX/Y/Z in place and we need to preserve those changes
            bool preservePositions = _enableManifoldEmbedding && _embeddingInitialized && 
                                     _nodeCount == data.NodeCount && _nodeCount > 0;

            if (!preservePositions)
            {
                _nodeX = data.NodeX;
                _nodeY = data.NodeY;
                _nodeZ = data.NodeZ;
            }
            
            // Always update states and edges (they come from simulation)
            _nodeStates = data.States;
            _edges = data.Edges;
            _nodeCount = data.NodeCount;
            _spectralDim = data.SpectralDimension;
        }
        catch
        {
            // Keep last valid data
        }
    }

    private void DrawGraph()
    {
        if (_nodeCount == 0 || _nodeX is null) 
        {
            DrawNoDataMessage();
            return;
        }

        // Switch between rendering modes
        if (_renderMode == RenderMode3D.Gpu3D)
        {
            RenderSceneGpu3D();
            return;
        }

        // === ImGui 2D Mode (CPU-based legacy rendering) ===
        var drawList = ImGui.GetForegroundDrawList();
        int panelW = _renderPanel?.Width ?? 800;
        int panelH = _renderPanel?.Height ?? 600;
        float cx = panelW / 2f;
        float cy = panelH / 2f;

        // Camera rotation
        float cosYaw = MathF.Cos(_cameraYaw);
        float sinYaw = MathF.Sin(_cameraYaw);
        float cosPitch = MathF.Cos(_cameraPitch);
        float sinPitch = MathF.Sin(_cameraPitch);

        int count = Math.Min(_nodeCount, 10000);

        // First pass: transform and find bounds
        Span<Vector2> screenPos = stackalloc Vector2[count];
        Span<float> depths = stackalloc float[count];
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float sumX = 0, sumY = 0;

        for (int i = 0; i < count; i++)
        {
            float x = _nodeX[i];
            float y = _nodeY![i];
            float z = _nodeZ![i];

            // Rotate around Y (yaw)
            float rx = x * cosYaw - z * sinYaw;
            float rz = x * sinYaw + z * cosYaw;

            // Rotate around X (pitch)
            float ry = y * cosPitch - rz * sinPitch;
            float rz2 = y * sinPitch + rz * cosPitch;

            screenPos[i] = new Vector2(rx, ry);
            depths[i] = rz2;

            minX = Math.Min(minX, rx);
            maxX = Math.Max(maxX, rx);
            minY = Math.Min(minY, ry);
            maxY = Math.Max(maxY, ry);
            sumX += rx;
            sumY += ry;
        }

        // Calculate auto-scale
        float dataRange = Math.Max(maxX - minX, maxY - minY);
        if (dataRange < 0.001f) dataRange = 1f;

        float availableSize = Math.Min(panelW, panelH) * 0.8f;
        float autoScale = availableSize / dataRange;
        float zoomFactor = 100f / Math.Max(_cameraDistance, 1f);
        float scale = autoScale * zoomFactor;

        float dataCenterX = sumX / count;
        float dataCenterY = sumY / count;

        // Convert to final screen positions
        for (int i = 0; i < count; i++)
        {
            float sx = cx + (screenPos[i].X - dataCenterX) * scale;
            float sy = cy - (screenPos[i].Y - dataCenterY) * scale;
            screenPos[i] = new Vector2(sx, sy);
        }

        // Draw edges first (behind nodes) - use mode-based styling
        if (_showEdges && _edges is not null)
        {
            foreach (var (u, v, w) in _edges)
            {
                if (u < count && v < count && w >= _edgeWeightThreshold)
                {
                    var (edgeColor, thickness) = GetEdgeStyle(u, v, w);
                    uint col = ImGui.ColorConvertFloat4ToU32(edgeColor);
                    drawList.AddLine(screenPos[u], screenPos[v], col, thickness);
                }
            }
        }

        // Draw nodes as circles - use mode-based coloring
        for (int i = 0; i < count; i++)
        {
            Vector4 color = GetNodeColor(i);
            float depthFactor = 1f - Math.Clamp(depths[i] / 10f, -0.5f, 0.5f);
            float radius = 1.3f + depthFactor * 1f; // Reduced radius (matching CSR)

            uint col = ImGui.ColorConvertFloat4ToU32(color);
            drawList.AddCircleFilled(screenPos[i], radius, col);
        }
    }

    private void DrawNoDataMessage()
    {
        var drawList = ImGui.GetForegroundDrawList();
        int panelW = _renderPanel?.Width ?? 800;
        int panelH = _renderPanel?.Height ?? 600;

        string msg = "Waiting for simulation data...";
        var textSize = ImGui.CalcTextSize(msg);
        var pos = new Vector2((panelW - textSize.X) / 2, (panelH - textSize.Y) / 2);

        drawList.AddText(pos, ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.5f, 0.5f, 1f)), msg);
    }

    private void DrawImGuiOverlay()
    {
        // Stats overlay
        ImGui.SetNextWindowPos(new Vector2(230, 10), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.7f);
        if (ImGui.Begin("##Stats", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.TextColored(new Vector4(0.8f, 0.9f, 1f, 1f), $"Nodes: {_nodeCount}");
            ImGui.TextColored(new Vector4(0.8f, 0.9f, 1f, 1f), $"Edges: {_edges?.Count ?? 0}");
            ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f), $"d_S: {_spectralDim:F3}");
            ImGui.TextColored(new Vector4(0.5f, 1f, 0.5f, 1f), $"FPS: {_fps:F0}");
            
            if (_enableManifoldEmbedding)
            {
                ImGui.TextColored(new Vector4(0.3f, 1f, 1f, 1f), "Manifold: ON");
            }
        }
        ImGui.End();

        // Controls hint
        ImGui.SetNextWindowPos(new Vector2(230, (float)((_renderPanel?.Height ?? 600) - 35)), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.5f);
        if (ImGui.Begin("##Controls", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1f), "LMB: Rotate | Wheel: Zoom");
        }
        ImGui.End();
    }

    private void UpdateFps()
    {
        _frameCount++;
        if ((DateTime.Now - _fpsUpdateTime).TotalSeconds >= 1.0)
        {
            _fps = _frameCount / (float)(DateTime.Now - _fpsUpdateTime).TotalSeconds;
            _frameCount = 0;
            _fpsUpdateTime = DateTime.Now;

            UpdateStatsLabel();
        }
    }

    private void UpdateStatsLabel()
    {
        if (_statsLabel is null) return;

        string manifoldStatus = _enableManifoldEmbedding ? " [Manifold]" : "";
        string stats = $"Nodes: {_nodeCount}\n" +
                       $"Edges: {_edges?.Count ?? 0}\n" +
                       $"d_S: {_spectralDim:F3}\n" +
                       $"FPS: {_fps:F0}\n" +
                       $"Mode: {_visMode}{manifoldStatus}";

        if (_statsLabel.InvokeRequired)
            _statsLabel.Invoke(() => _statsLabel.Text = stats);
        else
            _statsLabel.Text = stats;
    }
}

