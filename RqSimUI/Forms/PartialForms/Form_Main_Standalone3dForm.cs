using RqSim3DForm;
using RQSimulation;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    private Form_Rsim3DForm? _standalone3DForm;

    // Standalone DX12 provider cache to reduce allocations/GC pressure
    private float[]? _standaloneNodeX;
    private float[]? _standaloneNodeY;
    private float[]? _standaloneNodeZ;
    private NodeState[]? _standaloneStates;
    private List<(int, int, float)>? _standaloneEdges;
    private int _standaloneCachedN;
    private int _standaloneEdgeRebuildCounter;

    // Cached spectral dimension for display continuity
    private double _standaloneCachedSpectralDim = double.NaN;

    /// <summary>
    /// Opens or closes standalone 3D visualization form.
    /// </summary>
    private void checkBox_StanaloneDX12Form_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is not CheckBox checkBox) return;

        if (checkBox.Checked)
        {
            // Open standalone form if not already open
            if (_standalone3DForm is null || _standalone3DForm.IsDisposed)
            {
                _standalone3DForm = new Form_Rsim3DForm();
                _standalone3DForm.SetDataProvider(() => GetGraphDataForStandalone3D());
                _standalone3DForm.FormClosed += (s, args) =>
                {
                    _standalone3DForm = null;
                    // Uncheck the checkbox when form is closed
                    if (!checkBox.IsDisposed)
                    {
                        checkBox.Checked = false;
                    }
                };
                _standalone3DForm.Show();
                AppendSysConsole("[3D] Standalone visualization form opened\n");
            }
            else
            {
                // Bring existing form to front
                _standalone3DForm.BringToFront();
                _standalone3DForm.Focus();
            }
        }
        else
        {
            // Close standalone form
            if (_standalone3DForm is not null && !_standalone3DForm.IsDisposed)
            {
                _standalone3DForm.Close();
                _standalone3DForm = null;
                AppendSysConsole("[3D] Standalone visualization form closed\n");
            }
        }
    }

    /// <summary>
    /// Provides graph data for standalone 3D form.
    /// </summary>
    private GraphRenderData GetGraphDataForStandalone3D()
    {
        // Prefer live graph while simulation is running; fall back to cached ActiveGraph only when engine graph is unavailable.
        RQGraph? graph = _simApi?.SimulationEngine?.Graph ?? _simApi?.ActiveGraph;

        if (graph is null)
        {
            _standaloneCachedN = 0;
            _standaloneEdges?.Clear();
            _standaloneCachedSpectralDim = double.NaN;
            return new GraphRenderData(null, null, null, null, null, 0, 0);
        }

        int n = graph.N;
        if (n <= 0)
        {
            _standaloneCachedN = 0;
            _standaloneEdges?.Clear();
            _standaloneCachedSpectralDim = double.NaN;
            return new GraphRenderData(null, null, null, null, null, 0, 0);
        }

        if (_standaloneNodeX is null || _standaloneNodeX.Length != n)
        {
            _standaloneNodeX = new float[n];
            _standaloneNodeY = new float[n];
            _standaloneNodeZ = new float[n];
            _standaloneStates = new NodeState[n];
            _standaloneCachedN = n;
            _standaloneEdgeRebuildCounter = 0;
            _standaloneCachedSpectralDim = double.NaN; // Reset cached spectral dim on graph size change
        }

        // Use SpectralX/Y/Z if available
        bool hasSpectral = graph.SpectralX is not null && graph.SpectralX.Length == n;

        if (hasSpectral)
        {
            for (int i = 0; i < n; i++)
            {
                _standaloneNodeX![i] = (float)graph.SpectralX![i];
                _standaloneNodeY![i] = (float)graph.SpectralY![i];
                _standaloneNodeZ![i] = (float)graph.SpectralZ![i];
                _standaloneStates![i] = graph.State[i];
            }
        }
        else
        {
            // Fallback to grid layout
            int gridSize = (int)Math.Ceiling(Math.Sqrt(n));
            float spacing = 2.0f;
            for (int i = 0; i < n; i++)
            {
                int gx = i % gridSize;
                int gy = i / gridSize;
                _standaloneNodeX![i] = (gx - gridSize / 2f) * spacing;
                _standaloneNodeY![i] = (gy - gridSize / 2f) * spacing;
                _standaloneNodeZ![i] = 0f;
                _standaloneStates![i] = graph.State[i];
            }
        }

        // Build edges.
        // This is expensive for large graphs, so we reuse a cached list and rebuild periodically.
        // Rendering-side threshold filtering still applies.
        _standaloneEdges ??= new List<(int, int, float)>(n * 4);

        bool graphSizeChanged = _standaloneCachedN != n;
        _standaloneEdgeRebuildCounter++;
        int rebuildPeriodFrames = _simApi?.SimulationEngine?.Graph is not null ? 4 : 30; // Rebuild more often while running, less often when stopped

        if (graphSizeChanged || _standaloneEdges.Count == 0 || _standaloneEdgeRebuildCounter >= rebuildPeriodFrames)
        {
            _standaloneEdgeRebuildCounter = 0;
            _standaloneCachedN = n;

            _standaloneEdges.Clear();

            int step = Math.Max(1, n / 500); // Sample edges for large graphs (same as CSR)
            for (int i = 0; i < n; i += step)
            {
                foreach (int j in graph.Neighbors(i))
                {
                    if (j > i)
                    {
                        float w = (float)graph.Weights[i, j];
                        if (w > 0.01f)
                        {
                            _standaloneEdges.Add((i, j, w));
                        }
                    }
                }
            }
        }

        // Get spectral dimension - use SmoothedSpectralDimension only (cheap property read)
        double spectralDim = graph.SmoothedSpectralDimension;
        
        // Cache valid values for display continuity when SmoothedSpectralDimension temporarily returns NaN
        if (!double.IsNaN(spectralDim) && spectralDim > 0)
        {
            _standaloneCachedSpectralDim = spectralDim;
        }
        else if (!double.IsNaN(_standaloneCachedSpectralDim))
        {
            // Use last valid cached value
            spectralDim = _standaloneCachedSpectralDim;
        }
        // Otherwise spectralDim remains NaN - that's fine, UI will show "---"

        return new GraphRenderData(_standaloneNodeX, _standaloneNodeY, _standaloneNodeZ, _standaloneStates, _standaloneEdges, n, spectralDim);
    }
}


















