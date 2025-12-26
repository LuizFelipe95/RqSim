using RqSimForms.Forms.Interfaces;
using RqSimGraphEngine.Experiments;
using RQSimulation;
using RQSimulation.Analysis;
using RQSimulation.GPUOptimized;
using System.Text;
using System.Text.Json;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RqSimForms;

public partial class Form_Main
{
    private TextBox synthesisTextBox; // Promoted to field

    // Synthesis data shortcuts
    private List<(int volume, double deltaMass)>? synthesisData { get => _simApi.SynthesisData; set => _simApi.SynthesisData = value; }
    private int synthesisCount { get => _simApi.SynthesisCount; set => _simApi.SynthesisCount = value; }
    private int fissionCount { get => _simApi.FissionCount; set => _simApi.FissionCount = value; }

    private void InitializeSynthesisTab()
    {
        // ????????? ??????? ?? ??? ?????: ????? ??????, ?????? ?????
        TableLayoutPanel layoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F)); // ????????? ?????
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F)); // ??????

        // TextBox ??? ?????????? ???????
        synthesisTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 9F),
            BackColor = Color.White,
            ForeColor = Color.Black,
            Name = "synthesisTextBox"
        };

        // Panel ??? ???????
        Panel chartPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Name = "synthesisChartPanel"
        };
        chartPanel.Paint += SynthesisChartPanel_Paint;

        layoutPanel.Controls.Add(synthesisTextBox, 0, 0);
        layoutPanel.Controls.Add(chartPanel, 0, 1);

        tabPage_Sythnesis.Controls.Add(layoutPanel);
    }

    private void AnalyzeSynthesis(List<string> avalancheStats)
    {
        if (InvokeRequired)
        {
            Invoke(() => AnalyzeSynthesis(avalancheStats));
            return;
        }

        var synthesisTextBox = tabPage_Sythnesis.Controls
            .OfType<TableLayoutPanel>().FirstOrDefault()?
            .Controls.OfType<TextBox>().FirstOrDefault();

        if (synthesisTextBox == null || avalancheStats.Count < 2)
            return;

        // ?????? CSV ??????
        string[] headers = avalancheStats[0].Split(',');
        int volumeIdx = Array.IndexOf(headers, "volume");
        int deltaMassIdx = Array.IndexOf(headers, "deltaHeavyMass");

        if (volumeIdx < 0 || deltaMassIdx < 0)
        {
            synthesisTextBox.Text = "??????: ?? ??????? ??????? volume ??? deltaHeavyMass ? CSV";
            return;
        }

        synthesisData = new List<(int, double)>();
        synthesisCount = 0;
        fissionCount = 0;
        int neutralCount = 0;

        double totalSynthesisMass = 0.0;
        double totalFissionMass = 0.0;
        double maxSynthesis = double.MinValue;
        double maxFission = double.MaxValue;

        for (int i = 1; i < avalancheStats.Count; i++)
        {
            string[] fields = avalancheStats[i].Split(',');

            if (fields.Length <= Math.Max(volumeIdx, deltaMassIdx))
                continue;

            if (int.TryParse(fields[volumeIdx], out int volume) &&
                double.TryParse(fields[deltaMassIdx], out double deltaMass))
            {
                synthesisData.Add((volume, deltaMass));

                if (deltaMass > 0.001)
                {
                    synthesisCount++;
                    totalSynthesisMass += deltaMass;
                    if (deltaMass > maxSynthesis) maxSynthesis = deltaMass;
                }
                else if (deltaMass < -0.001)
                {
                    fissionCount++;
                    totalFissionMass += Math.Abs(deltaMass);
                    if (deltaMass < maxFission) maxFission = deltaMass;
                }
                else
                {
                    neutralCount++;
                }
            }
        }

        // ????????? ????????? ?????
        var sb = new StringBuilder();
        sb.AppendLine("=== ?????? ???????/??????? ?????? ????????? ===\n");
        sb.AppendLine($"????? ????? ????????????????: {avalancheStats.Count - 1}");
        sb.AppendLine();

        sb.AppendLine("--- ????????????? ????? ---");
        sb.AppendLine($"? ?????? (deltaHeavyMass > 0):      {synthesisCount,4} ????? ({100.0 * synthesisCount / (avalancheStats.Count - 1):F1}%)");
        sb.AppendLine($"? ?????? (deltaHeavyMass < 0):      {fissionCount,4} ????? ({100.0 * fissionCount / (avalancheStats.Count - 1):F1}%)");
        sb.AppendLine($"? ??????????? (|deltaMass| < 0.001): {neutralCount,4} ????? ({100.0 * neutralCount / (avalancheStats.Count - 1):F1}%)");
        sb.AppendLine();

        sb.AppendLine("--- ?????????? ?? ????? ---");
        sb.AppendLine($"????????? ????? ?????????????:   {totalSynthesisMass,8:F2}");
        sb.AppendLine($"????????? ????? ?????????:       {totalFissionMass,8:F2}");
        sb.AppendLine($"?????? ??????? ?????:            {totalSynthesisMass - totalFissionMass,8:F2}");
        sb.AppendLine();

        if (synthesisCount > 0)
        {
            sb.AppendLine($"???????????? ?????? ?? ??????:   {maxSynthesis,8:F2}");
            sb.AppendLine($"??????? ?????? (??? ?????????):  {totalSynthesisMass / synthesisCount,8:F2}");
        }

        if (fissionCount > 0)
        {
            sb.AppendLine($"???????????? ?????? ?? ??????:   {maxFission,8:F2}");
            sb.AppendLine($"??????? ?????? (??? ?????????):  {-totalFissionMass / fissionCount,8:F2}");
        }

        sb.AppendLine();
        sb.AppendLine("--- ???????? ? ??????? ---");
        sb.AppendLine("?????? > ?????? ? ???????????? (???????????? ?????? ?????????)");
        sb.AppendLine("?????? > ?????? ? ???????????? (?????????? ????????)");
        sb.AppendLine();
        sb.AppendLine("?????? ???? ?????????? ??????????? deltaHeavyMass ?? ?????? ??????.");
        sb.AppendLine("??????? ????? (y=0) ????????? ?????? ? ??????.");

        synthesisTextBox.Text = sb.ToString();

        // ?????????????? ??????
        var chartPanel = tabPage_Sythnesis.Controls
            .OfType<TableLayoutPanel>().FirstOrDefault()?
            .Controls.OfType<Panel>().FirstOrDefault();
        chartPanel?.Invalidate();
    }

    private void SynthesisChartPanel_Paint(object? sender, PaintEventArgs e)
    {
        if (synthesisData == null || synthesisData.Count == 0)
        {
            // ?????????? ?????????, ???? ?????? ???
            e.Graphics.DrawString("?????? ????? ???????? ????? ?????????? ?????????",
                new Font("Arial", 12), Brushes.Gray, 10, 10);
            return;
        }

        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        Panel panel = (Panel)sender!;
        int width = panel.Width;
        int height = panel.Height;

        // ???????
        int marginLeft = 60;
        int marginRight = 20;
        int marginTop = 20;
        int marginBottom = 50;

        int plotWidth = width - marginLeft - marginRight;
        int plotHeight = height - marginTop - marginBottom;

        if (plotWidth <= 0 || plotHeight <= 0)
            return;

        // ??????? ????????? ??????
        int minVolume = synthesisData.Min(d => d.volume);
        int maxVolume = synthesisData.Max(d => d.volume);
        double minDelta = synthesisData.Min(d => d.deltaMass);
        double maxDelta = synthesisData.Max(d => d.deltaMass);

        // ????????? ????????? ?????
        double deltaRange = Math.Max(Math.Abs(maxDelta), Math.Abs(minDelta)) * 1.1;
        minDelta = -deltaRange;
        maxDelta = deltaRange;

        int volumeRange = maxVolume - minVolume;
        if (volumeRange == 0) volumeRange = 1;

        // ?????? ???
        using (Pen axisPen = new Pen(Color.Black, 2))
        {
            // Y-???
            g.DrawLine(axisPen, marginLeft, marginTop, marginLeft, height - marginBottom);
            // X-???
            g.DrawLine(axisPen, marginLeft, height - marginBottom, width - marginRight, height - marginBottom);
        }

        // ??????? ????? y=0 (????????? ?????? ? ??????)
        int zeroY = marginTop + (int)(plotHeight * (maxDelta / (maxDelta - minDelta)));
        using (Pen zeroPen = new Pen(Color.Red, 2))
        {
            zeroPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            g.DrawLine(zeroPen, marginLeft, zeroY, width - marginRight, zeroY);
        }

        // ??????? ????
        using (Font axisFont = new Font("Arial", 10))
        {
            // Y-???
            g.DrawString("Delta Heavy Mass", axisFont, Brushes.Black, 5, marginTop + plotHeight / 2 - 30);
            g.DrawString($"{maxDelta:F1}", axisFont, Brushes.Black, 5, marginTop);
            g.DrawString("0", axisFont, Brushes.Red, 5, zeroY - 7);
            g.DrawString($"{minDelta:F1}", axisFont, Brushes.Black, 5, height - marginBottom - 15);

            // X-???
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("Avalanche Volume", axisFont, Brushes.Black,
                marginLeft + plotWidth / 2, height - marginBottom + 30, sf);
            g.DrawString($"{minVolume}", axisFont, Brushes.Black, marginLeft, height - marginBottom + 5);
            g.DrawString($"{maxVolume}", axisFont, Brushes.Black, width - marginRight - 30, height - marginBottom + 5);
        }

        // ?????? ?????
        foreach (var (volume, deltaMass) in synthesisData)
        {
            int x = marginLeft + (int)((volume - minVolume) * plotWidth / (double)volumeRange);
            int y = marginTop + (int)((maxDelta - deltaMass) * plotHeight / (maxDelta - minDelta));

            Color pointColor = deltaMass > 0 ? Color.Blue : (deltaMass < 0 ? Color.OrangeRed : Color.Gray);
            int pointSize = 4;

            using (Brush pointBrush = new SolidBrush(pointColor))
            {
                g.FillEllipse(pointBrush, x - pointSize / 2, y - pointSize / 2, pointSize, pointSize);
            }
        }

        // ???????
        int legendX = width - marginRight - 150;
        int legendY = marginTop + 10;

        using (Font legendFont = new Font("Arial", 9))
        {
            g.FillRectangle(Brushes.Blue, legendX, legendY, 10, 10);
            g.DrawString("?????? (? > 0)", legendFont, Brushes.Black, legendX + 15, legendY - 2);

            g.FillRectangle(Brushes.OrangeRed, legendX, legendY + 20, 10, 10);
            g.DrawString("?????? (? < 0)", legendFont, Brushes.Black, legendX + 15, legendY + 18);
        }
    }
}
