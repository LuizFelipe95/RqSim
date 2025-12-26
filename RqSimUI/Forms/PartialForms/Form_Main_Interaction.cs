using System;
using System.Drawing;
using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    private int FindNodeAtPosition(Point clickPos)
    {
        if (_simulationEngine?.Graph == null) return -1;
        var graph = _simulationEngine.Graph;
        int n = graph.N;
        if (n <= 0) return -1;
        // ensure positions exist
        if (_cachedNodePositions == null || _cachedNodePositions.Length != n)
            BuildNodePositions();
        float hitRadius = Math.Max(12f, 500f / Math.Max(50, n));
        float hitRadiusSq = hitRadius * hitRadius;
        int best = -1; float bestDist = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            var p = _cachedNodePositions[i];
            float dx = clickPos.X - p.X; float dy = clickPos.Y - p.Y; float d2 = dx * dx + dy * dy;
            if (d2 < hitRadiusSq && d2 < bestDist) { bestDist = d2; best = i; }
        }
        return best;
    }

    private void DrawingPanel_MouseClick(object? sender, MouseEventArgs e)
    {
        if (_simulationEngine?.Graph == null || !_isModernRunning)
            return;

        // Находим ближайший узел к клику и возбудем его
        int nodeIndex = FindNodeAtPosition(e.Location);
        if (nodeIndex >= 0)
        {
            _simulationEngine.Graph.FlipNodeWithNeighbors(nodeIndex);
            AppendSimConsole($"Импульс в узел {nodeIndex}\n");
        }
    }

    private void ToolViewGraph_Click(object? sender, EventArgs e) => tabControl_Main.SelectedTab = tabPage_GUI;
    private void ToolViewConsole_Click(object? sender, EventArgs e) => tabControl_Main.SelectedTab = tabPage_Console;
    private void ToolViewAnalysis_Click(object? sender, EventArgs e) => tabControl_Main.SelectedTab = tabPage_Sythnesis;
    private void ToolViewSummary_Click(object? sender, EventArgs e) => tabControl_Main.SelectedTab = tabPage_Summary;
}
