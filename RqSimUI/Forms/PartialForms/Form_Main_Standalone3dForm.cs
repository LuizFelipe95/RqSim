using RqSim3DForm;
using RQSimulation;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    private Form_Rsim3DForm? _standalone3DForm;

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
        RQGraph? graph = _simApi?.ActiveGraph ?? _simApi?.SimulationEngine?.Graph;
        
        if (graph is null || graph.N == 0)
        {
            return new GraphRenderData(null, null, null, null, null, 0, 0);
        }

        int n = graph.N;
        var nodeX = new float[n];
        var nodeY = new float[n];
        var nodeZ = new float[n];
        var states = new NodeState[n];

        // Use SpectralX/Y/Z if available
        bool hasSpectral = graph.SpectralX is not null && graph.SpectralX.Length == n;

        if (hasSpectral)
        {
            for (int i = 0; i < n; i++)
            {
                nodeX[i] = (float)graph.SpectralX![i];
                nodeY[i] = (float)graph.SpectralY![i];
                nodeZ[i] = (float)graph.SpectralZ![i];
                states[i] = graph.State[i];
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
                nodeX[i] = (gx - gridSize / 2f) * spacing;
                nodeY[i] = (gy - gridSize / 2f) * spacing;
                nodeZ[i] = 0f;
                states[i] = graph.State[i];
            }
        }

        // Build edges - collect all edges, threshold filtering will be done in standalone form
        // This matches CSR behavior where edges are filtered during rendering
        var edges = new List<(int, int, float)>(n * 4);
        int step = Math.Max(1, n / 500); // Sample edges for large graphs (same as CSR)

        for (int i = 0; i < n; i += step)
        {
            foreach (int j in graph.Neighbors(i))
            {
                if (j > i) // Avoid duplicate edges
                {
                    float w = (float)graph.Weights[i, j];
                    // Pass all edges with weight > 0, threshold filtering happens in renderer
                    if (w > 0.01f)
                    {
                        edges.Add((i, j, w));
                    }
                }
            }
        }

        return new GraphRenderData(nodeX, nodeY, nodeZ, states, edges, n, graph.SmoothedSpectralDimension);
    }
}




