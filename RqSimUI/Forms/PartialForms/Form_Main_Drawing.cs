using Microsoft.VisualBasic.ApplicationServices;
using RQSimulation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    private void DrawingPanel_Paint(object? sender, PaintEventArgs e)
    {
        if (canvasBitmap != null)
        {
            try
            {
                e.Graphics.DrawImage(canvasBitmap, 0, 0);
            }
            catch (Exception ex)
            {
                // Draw error on panel
                e.Graphics.Clear(Color.Red);
                e.Graphics.DrawString($"Paint error: {ex.Message}", SystemFonts.DefaultFont, Brushes.White, 10, 10);
            }
        }
        else
        {
            // Draw placeholder when no bitmap
            e.Graphics.Clear(Color.LightGray);
            e.Graphics.DrawString("No graph bitmap", SystemFonts.DefaultFont, Brushes.Black, 10, 10);
        }
    }

    /// <summary>
    /// Optimized graph drawing with thread-safe GDI resources.
    /// FIX: Creates fresh bitmap AND fresh GDI objects each time to avoid race conditions.
    /// GDI objects (Pen, Brush) are NOT thread-safe - must create new ones in background thread!
    /// 
    /// FIX 2: All early returns inside Task.Run now go through finally block to reset _isDrawing flag.
    /// </summary>
    private void DrawGraph()
    {
        var graph = _simulationEngine?.Graph;
        if (graph == null) return;

        // Non-blocking lock - if already drawing, skip this frame
        if (Interlocked.CompareExchange(ref _isDrawing, 1, 0) != 0) return;

        int panelWidth = drawingPanel.Width;
        int panelHeight = drawingPanel.Height;

        // Validate panel dimensions before starting async work
        if (panelWidth <= 0 || panelHeight <= 0)
        {
            Interlocked.Exchange(ref _isDrawing, 0);
            return;
        }

        // Capture display settings (read on UI thread)
        bool useDynamic = _useDynamicCoords;
        bool showHeavyOnly = _displayShowHeavyOnly;
        double weightThreshold = _displayWeightThreshold;

        Task.Run(() =>
        {
            Bitmap? produced = null;
            Pen[]? localPens = null;
            SolidBrush[]? localBrushes = null;
            Pen? blackPen = null;

            try
            {
                // Re-check graph availability (could be disposed during async transition)
                var localGraph = _simulationEngine?.Graph;
                if (localGraph == null)
                {
                    // Early exit - finally will reset _isDrawing
                    return;
                }

                if (useDynamic) NormalizeToCircle();
                BuildNodePositions();
                var localPositions = _cachedNodePositions;
                if (localPositions == null || localPositions.Length == 0)
                {
                    // Early exit - finally will reset _isDrawing
                    return;
                }

                HashSet<int>? heavyNodes = null;
                if (showHeavyOnly)
                {
                    try
                    {
                        var clusters = localGraph.GetStrongCorrelationClusters(localGraph.GetAdaptiveHeavyThreshold());
                        heavyNodes = new HashSet<int>(clusters.SelectMany(c => c));
                        if (heavyNodes.Count == 0) heavyNodes = null;
                    }
                    catch { heavyNodes = null; }
                }

                double effectiveWeightThreshold = weightThreshold;
                if (effectiveWeightThreshold >= 0.95) effectiveWeightThreshold = 0.0;

                // Create thread-local GDI objects (CRITICAL for thread safety)
                localPens = CreateEdgePenSet();
                localBrushes = CreateNodeBrushSet();
                blackPen = new Pen(Color.Black, 1f);

                // Create fresh bitmap for this frame
                produced = new Bitmap(panelWidth, panelHeight);
                using (var g = Graphics.FromImage(produced))
                {
                    g.Clear(Color.White);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    int n = localGraph.N;

                    // Draw edges using thread-local Pens
                    for (int i = 0; i < n; i++)
                    {
                        if (heavyNodes != null && !heavyNodes.Contains(i)) continue;
                        if (i >= localPositions.Length) continue;
                        var p1 = localPositions[i];
                        foreach (int j in localGraph.Neighbors(i))
                        {
                            if (j <= i) continue;
                            if (heavyNodes != null && !heavyNodes.Contains(j)) continue;
                            if (j >= localPositions.Length) continue;
                            double w = localGraph.Weights[i, j];
                            if (w < effectiveWeightThreshold) continue;

                            var pen = GetEdgePenFromSet(localPens, w);
                            var p2 = localPositions[j];
                            g.DrawLine(pen, p1, p2);
                        }
                    }

                    // Draw nodes using thread-local Brushes
                    int nodeCountToDraw = heavyNodes == null ? localGraph.N : heavyNodes.Count;
                    float nodeSize = Math.Max(4, Math.Min(11, 500f / Math.Max(1, nodeCountToDraw)));
                    for (int i = 0; i < localGraph.N; i++)
                    {
                        if (heavyNodes != null && !heavyNodes.Contains(i)) continue;
                        if (i >= localPositions.Length) continue;
                        var p = localPositions[i];

                        var brush = GetNodeBrushFromSet(localBrushes, localGraph, i);
                        g.FillEllipse(brush, p.X - nodeSize / 2, p.Y - nodeSize / 2, nodeSize, nodeSize);
                        g.DrawEllipse(blackPen, p.X - nodeSize / 2, p.Y - nodeSize / 2, nodeSize, nodeSize);
                    }
                }

                // Pass bitmap to UI thread for display
                if (!IsDisposed && IsHandleCreated)
                {
                    var bitmapToDisplay = produced;
                    produced = null; // Transfer ownership to UI thread

                    BeginInvoke(new Action(() =>
                    {
                        if (IsDisposed)
                        {
                            bitmapToDisplay?.Dispose();
                            return;
                        }

                        // Swap bitmaps
                        var oldBitmap = canvasBitmap;
                        canvasBitmap = bitmapToDisplay;
                        oldBitmap?.Dispose();

                        // Force immediate repaint
                        drawingPanel.Invalidate();
                        drawingPanel.Update();
                    }));
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    try
                    {
                        BeginInvoke(new Action(() => AppendSysConsole($"[DrawGraph] error: {ex.Message}\n")));
                    }
                    catch { /* Ignore if form is closing */ }
                }
            }
            finally
            {
                // Dispose thread-local GDI resources
                if (localPens != null)
                {
                    foreach (var pen in localPens) pen?.Dispose();
                }
                if (localBrushes != null)
                {
                    foreach (var brush in localBrushes) brush?.Dispose();
                }
                blackPen?.Dispose();

                // Dispose bitmap if not transferred to UI thread
                produced?.Dispose();

                // CRITICAL: Always reset drawing flag - this was the bug!
                Interlocked.Exchange(ref _isDrawing, 0);
            }
        });
    }

    /// <summary>
    /// Draws graph visualization from external simulation data (RenderNode[] from shared memory).
    /// Used when _isExternalSimulation is true.
    /// </summary>
    private void DrawExternalGraph()
    {
        // Non-blocking lock - if already drawing, skip this frame
        if (Interlocked.CompareExchange(ref _isDrawing, 1, 0) != 0) return;

        int panelWidth = drawingPanel.Width;
        int panelHeight = drawingPanel.Height;

        if (panelWidth <= 0 || panelHeight <= 0)
        {
            Interlocked.Exchange(ref _isDrawing, 0);
            return;
        }

        // Capture external nodes data
        var nodes = _externalNodesBuffer;
        if (nodes == null || nodes.Length == 0)
        {
            Interlocked.Exchange(ref _isDrawing, 0);
            return;
        }

        // Read header for node count
        if (!_externalReader.TryReadHeader(out var header) || header.NodeCount <= 0)
        {
            Interlocked.Exchange(ref _isDrawing, 0);
            return;
        }

        int nodeCount = Math.Min(header.NodeCount, nodes.Length);

        Task.Run(() =>
        {
            Bitmap? produced = null;
            Pen? blackPen = null;

            try
            {
                produced = new Bitmap(panelWidth, panelHeight);

                using (var g = Graphics.FromImage(produced))
                {
                    g.Clear(Color.White);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    blackPen = new Pen(Color.Black, 1f);

                    // Calculate bounds for normalization
                    float minX = float.MaxValue, maxX = float.MinValue;
                    float minY = float.MaxValue, maxY = float.MinValue;

                    for (int i = 0; i < nodeCount; i++)
                    {
                        var node = nodes[i];
                        if (node.X < minX) minX = node.X;
                        if (node.X > maxX) maxX = node.X;
                        if (node.Y < minY) minY = node.Y;
                        if (node.Y > maxY) maxY = node.Y;
                    }

                    float rangeX = maxX - minX;
                    float rangeY = maxY - minY;
                    if (rangeX < 0.001f) rangeX = 1f;
                    if (rangeY < 0.001f) rangeY = 1f;

                    float margin = 30f;
                    float scaleX = (panelWidth - 2 * margin) / rangeX;
                    float scaleY = (panelHeight - 2 * margin) / rangeY;
                    float scale = Math.Min(scaleX, scaleY);

                    float offsetX = margin + (panelWidth - 2 * margin - rangeX * scale) / 2 - minX * scale;
                    float offsetY = margin + (panelHeight - 2 * margin - rangeY * scale) / 2 - minY * scale;

                    // Draw nodes
                    float nodeSize = Math.Max(4f, Math.Min(20f, 400f / (float)Math.Sqrt(nodeCount)));

                    for (int i = 0; i < nodeCount; i++)
                    {
                        var node = nodes[i];
                        float px = node.X * scale + offsetX;
                        float py = node.Y * scale + offsetY;

                        // Use RGB from RenderNode (clamped to 0-255)
                        int r = Math.Clamp((int)(node.R * 255), 0, 255);
                        int g2 = Math.Clamp((int)(node.G * 255), 0, 255);
                        int b = Math.Clamp((int)(node.B * 255), 0, 255);

                        using var brush = new SolidBrush(Color.FromArgb(r, g2, b));
                        g.FillEllipse(brush, px - nodeSize / 2, py - nodeSize / 2, nodeSize, nodeSize);
                        g.DrawEllipse(blackPen, px - nodeSize / 2, py - nodeSize / 2, nodeSize, nodeSize);
                    }

                    // Draw info text
                    string info = $"External Simulation: {nodeCount} nodes";
                    g.DrawString(info, SystemFonts.DefaultFont, Brushes.Black, 5, 5);
                }

                // Pass bitmap to UI thread
                if (!IsDisposed && IsHandleCreated)
                {
                    var bitmapToDisplay = produced;
                    produced = null;

                    BeginInvoke(new Action(() =>
                    {
                        if (IsDisposed)
                        {
                            bitmapToDisplay?.Dispose();
                            return;
                        }

                        var oldBitmap = canvasBitmap;
                        canvasBitmap = bitmapToDisplay;
                        oldBitmap?.Dispose();

                        drawingPanel.Invalidate();
                        drawingPanel.Update();
                    }));
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    try
                    {
                        BeginInvoke(new Action(() => AppendSysConsole($"[DrawExternalGraph] error: {ex.Message}\n")));
                    }
                    catch { /* Ignore if form is closing */ }
                }
            }
            finally
            {
                blackPen?.Dispose();
                produced?.Dispose();
                Interlocked.Exchange(ref _isDrawing, 0);
            }
        });
    }

    private void button_ForceRedrawGraphImage_Click(object sender, EventArgs e)
    {
        // Force synchronous redraw

        // CRITICAL: Stop the UI timer temporarily so our diagnostic bitmap is visible
        bool timerWasRunning = _uiUpdateTimer?.Enabled ?? false;
        _uiUpdateTimer?.Stop();

        // Check simulation engine state
        var simEngine = _simApi.SimulationEngine;
        if (simEngine == null)
        {
            AppendSysConsole("[ForceRedraw] SimulationEngine is null - cannot draw graph\n");
            return;
        }

        var graph = simEngine.Graph;
        if (graph == null)
        {
            AppendSysConsole("[ForceRedraw] Graph is null - cannot draw graph\n");
            return;
        }

        // Force reset drawing flag to ensure we can draw
        Interlocked.Exchange(ref _isDrawing, 0);

        // SYNCHRONOUS graph drawing (not Task.Run)
        try
        {
            int panelWidth = drawingPanel.Width;
            int panelHeight = drawingPanel.Height;

            if (panelWidth <= 0 || panelHeight <= 0)
            {
                AppendSysConsole($"[ForceRedraw] Invalid panel dimensions: {panelWidth}x{panelHeight}\n");
                return;
            }

            // Build node positions
            BuildNodePositions();
            var localPositions = _cachedNodePositions;

            if (localPositions == null || localPositions.Length == 0)
            {
                AppendSysConsole("[ForceRedraw] localPositions is null or empty\n");
                return;
            }

            // Create bitmap synchronously
            var newBitmap = new Bitmap(panelWidth, panelHeight);

            using (var g = Graphics.FromImage(newBitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int n = graph.N;

                // Draw edges
                for (int i = 0; i < n && i < localPositions.Length; i++)
                {
                    var p1 = localPositions[i];
                    foreach (int j in graph.Neighbors(i))
                    {
                        if (j <= i) continue;
                        if (j >= localPositions.Length) continue;

                        double w = graph.Weights[i, j];
                        if (w < _displayWeightThreshold) continue;

                        var pen = GetEdgePen(w);
                        var p2 = localPositions[j];
                        g.DrawLine(pen, p1, p2);
                    }
                }

                // Draw nodes
                float nodeSize = Math.Max(4, Math.Min(11, 500f / Math.Max(1, n)));
                for (int i = 0; i < n && i < localPositions.Length; i++)
                {
                    var p = localPositions[i];
                    var brush = GetNodeBrush(graph, i);
                    g.FillEllipse(brush, p.X - nodeSize / 2, p.Y - nodeSize / 2, nodeSize, nodeSize);
                    g.DrawEllipse(Pens.Black, p.X - nodeSize / 2, p.Y - nodeSize / 2, nodeSize, nodeSize);
                }
            }

            // Swap bitmaps SYNCHRONOUSLY
            var oldBitmap = canvasBitmap;
            canvasBitmap = newBitmap;
            oldBitmap?.Dispose();

            // Force repaint
            drawingPanel.Invalidate();
            drawingPanel.Update();

            AppendSysConsole($"[ForceRedraw] Graph redrawn: {graph.N} nodes, step {_simApi.LiveStep}\n");
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[ForceRedraw] Error: {ex.Message}\n");
        }

        // Also force chart repaint
        panelOnChart?.Invalidate();
        panelHeavyChart?.Invalidate();
        panelClusterChart?.Invalidate();
        panelEnergyChart?.Invalidate();

        // Force refresh the entire tab
        tabPage_GUI.Refresh();

        // Restart timer if it was running (with 2 second delay)
        if (timerWasRunning && _uiUpdateTimer != null)
        {
            var restartTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            restartTimer.Tick += (s, args) =>
            {
                restartTimer.Stop();
                restartTimer.Dispose();
                if (_isModernRunning && !_simApi.SimulationComplete)
                {
                    _uiUpdateTimer?.Start();
                    AppendSysConsole("[UI] Timer restarted after force redraw\n");
                }
            };
            restartTimer.Start();
        }
    }

    private Pen[] CreateEdgePenSet()
    {
        // Thread-safe pen set for background drawing (matches DrawingOptimizations colors/weights)
        return
        [
            new Pen(Color.FromArgb(220, 255, 0, 0), 3f),      // VeryStrong > 0.7
            new Pen(Color.FromArgb(200, 255, 69, 0), 2.5f),   // Strong > 0.5
            new Pen(Color.FromArgb(180, 255, 140, 0), 2f),    // Medium > 0.3
            new Pen(Color.FromArgb(120, 218, 165, 32), 1.2f), // Weak > 0.15
            new Pen(Color.FromArgb(80, 160, 160, 160), 0.6f)  // VeryWeak
        ];
    }

    private SolidBrush[] CreateNodeBrushSet()
    {
        // Thread-safe brush set for background drawing (matches DrawingOptimizations colors)
        // Index: [0]=Excited, [1]=Refractory, [2]=Rest, [3]=Composite, [4]=Clock
        return
        [
            new SolidBrush(Color.Red),       // Excited
            new SolidBrush(Color.Orange),    // Refractory
            new SolidBrush(Color.LightBlue), // Rest
            new SolidBrush(Color.DarkRed),   // Composite
            new SolidBrush(Color.Blue)       // Clock
        ];
    }

    private static Pen GetEdgePenFromSet(Pen[] localPens, double w)
    {
        // Same thresholds as GetEdgePen in DrawingOptimizations.cs
        return w switch
        {
            > 0.7 => localPens[0],  // VeryStrong
            > 0.5 => localPens[1],  // Strong
            > 0.3 => localPens[2],  // Medium
            > 0.15 => localPens[3], // Weak
            _ => localPens[4]       // VeryWeak
        };
    }

    private static SolidBrush GetNodeBrushFromSet(SolidBrush[] localBrushes, RQGraph localGraph, int i)
    {
        // Same logic as GetNodeBrush in DrawingOptimizations.cs
        if (localGraph.PhysicsProperties[i].IsClock)
            return localBrushes[4]; // Clock = Blue
        if (localGraph.PhysicsProperties[i].Type == ParticleType.Composite)
            return localBrushes[3]; // Composite = DarkRed

        return localGraph.State[i] switch
        {
            NodeState.Excited => localBrushes[0],    // Red
            NodeState.Refractory => localBrushes[1], // Orange
            _ => localBrushes[2]                     // Rest = LightBlue
        };
    }

    private void NormalizeToCircle()
    {
        if (_simulationEngine?.Graph == null || _simulationEngine.Graph.Coordinates == null) return;
        int n = _simulationEngine.Graph.N;
        var coords = _simulationEngine.Graph.Coordinates;
        // ?????????? ?? ???????? ????, ????? ????????? ????????????? ???????
        var indexed = coords.Select((c, i) => (i, angle: Math.Atan2(c.Y, c.X))).OrderBy(x => x.angle).ToArray();
        for (int k = 0; k < indexed.Length; k++)
        {
            double ang = 2 * Math.PI * k / n;
            coords[indexed[k].i] = (Math.Cos(ang), Math.Sin(ang));
        }
    }

    private void BuildNodePositions()
    {
        if (_simulationEngine?.Graph == null) return;
        var graph = _simulationEngine.Graph;
        int n = graph.N; if (n <= 0) { _cachedNodePositions = null; return; }
        _cachedNodePositions = new PointF[n];
        int w = Math.Max(drawingPanel.Width, 1); int h = Math.Max(drawingPanel.Height, 1);
        float margin = 20f;
        if (_useDynamicCoords && graph.Coordinates != null && graph.Coordinates.Length == n)
        {
            double minX = graph.Coordinates.Min(c => c.X);
            double maxX = graph.Coordinates.Max(c => c.X);
            double minY = graph.Coordinates.Min(c => c.Y);
            double maxY = graph.Coordinates.Max(c => c.Y);
            double centerX = (minX + maxX) * 0.5; double centerY = (minY + maxY) * 0.5;
            // uniform scale using max radial distance to avoid flattening into a line
            double maxR = 0.0;
            for (int i = 0; i < n; i++) { var (cx, cy) = graph.Coordinates[i]; double dx = cx - centerX; double dy = cy - centerY; double r = Math.Sqrt(dx * dx + dy * dy); if (r > maxR) maxR = r; }
            if (maxR < 1e-9) maxR = 1.0;
            float radiusPixels = (Math.Min(w, h) - 2 * margin) * 0.5f;
            double scale = radiusPixels / maxR;
            for (int i = 0; i < n; i++)
            {
                var (cx, cy) = graph.Coordinates[i];
                double dx = cx - centerX; double dy = cy - centerY;
                float x = (float)(w / 2.0 + dx * scale);
                float y = (float)(h / 2.0 + dy * scale);
                _cachedNodePositions[i] = new PointF(x, y);
            }
        }
        else
        {
            // stable circle layout
            float radius = (Math.Min(w, h) - 2 * margin) * 0.5f;
            for (int i = 0; i < n; i++)
            {
                double ang = 2 * Math.PI * i / n;
                float x = w / 2f + radius * (float)Math.Cos(ang);
                float y = h / 2f + radius * (float)Math.Sin(ang);
                _cachedNodePositions[i] = new PointF(x, y);
            }
        }
    }
}
