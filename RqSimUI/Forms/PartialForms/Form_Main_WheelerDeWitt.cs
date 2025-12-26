using RQSimulation;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - Wheeler-DeWitt Constraint Controls.
/// Provides UI for displaying Wheeler-DeWitt constraint parameters (read-only reference).
/// 
/// PHYSICS BACKGROUND:
/// The Wheeler-DeWitt equation ?|?? = 0 is the quantum version of the 
/// Hamiltonian constraint in general relativity. It states that the total
/// Hamiltonian of the universe is zero - time does not flow externally,
/// but emerges from correlations within the wavefunction.
/// </summary>
public partial class Form_Main
{
    /// <summary>
    /// Extends InitializeAdvancedPhysicsControls with Wheeler-DeWitt section.
    /// Displays Wheeler-DeWitt constraint parameters as read-only reference.
    /// Call this after InitializeAdvancedPhysicsControls().
    /// </summary>
    private void InitializeWheelerDeWittControls()
    {
        int startRow = tlpPhysicsConstants.RowCount;

        // === Wheeler-DeWitt Section Header ===
        tlpPhysicsConstants.RowCount = startRow + 1;
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        var headerLabel = new Label
        {
            Text = "??? Wheeler-DeWitt Constraint (H?0) ???",
            AutoSize = true,
            ForeColor = Color.DarkRed,
            Font = new Font(Font, FontStyle.Bold),
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(0, 8, 0, 2)
        };
        tlpPhysicsConstants.Controls.Add(headerLabel, 0, startRow);
        tlpPhysicsConstants.SetColumnSpan(headerLabel, 2);
        startRow++;

        // === WDW Gravitational Coupling ===
        AddWDWDisplayRow("WDW ? (Grav. Coupling):", 
            $"{PhysicsConstants.WheelerDeWittConstants.GravitationalCoupling:F3}", 
            ref startRow,
            "Matter-geometry coupling in Hamiltonian constraint: ? = 8?G");

        // === WDW Constraint Tolerance ===
        AddWDWDisplayRow("WDW Constraint Tolerance:", 
            $"{PhysicsConstants.WheelerDeWittConstants.ConstraintTolerance:F4}", 
            ref startRow,
            "Tolerance for H_total ? 0 constraint satisfaction");

        // === WDW Lagrange Multiplier ===
        AddWDWDisplayRow("WDW Lagrange ?:", 
            $"{PhysicsConstants.WheelerDeWittConstants.ConstraintLagrangeMultiplier:F1}", 
            ref startRow,
            "Lagrange multiplier for constraint enforcement strength");

        // === WDW Max Allowed Violation ===
        AddWDWDisplayRow("Max Allowed Violation:", 
            $"{PhysicsConstants.WheelerDeWittConstants.MaxAllowedViolation:F2}", 
            ref startRow,
            "Maximum constraint violation before move is rejected");

        // === WDW Strict Mode ===
        var strictModeText = PhysicsConstants.WheelerDeWittConstants.EnableStrictMode 
            ? "? ENABLED" : "? disabled";
        var strictModeColor = PhysicsConstants.WheelerDeWittConstants.EnableStrictMode 
            ? Color.DarkGreen : Color.Gray;
        AddWDWDisplayRow("WDW Strict Mode:", 
            strictModeText,
            ref startRow,
            "When enabled, external energy injection is forbidden",
            strictModeColor);

        // === WDW Violation Logging ===
        var loggingText = PhysicsConstants.WheelerDeWittConstants.EnableViolationLogging 
            ? "? ENABLED" : "? disabled";
        var loggingColor = PhysicsConstants.WheelerDeWittConstants.EnableViolationLogging 
            ? Color.DarkGreen : Color.Gray;
        AddWDWDisplayRow("Violation Logging:", 
            loggingText,
            ref startRow,
            "Log constraint violations for diagnostics",
            loggingColor);
    }

    private void AddWDWDisplayRow(string labelText, string value, ref int row, 
        string? tooltip = null, Color? valueColor = null)
    {
        tlpPhysicsConstants.RowCount = row + 1;
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };

        var valueLabel = new Label
        {
            Text = value,
            AutoSize = true,
            ForeColor = valueColor ?? Color.DarkBlue,
            Font = new Font("Consolas", 9F),
            Anchor = AnchorStyles.Left
        };

        if (tooltip != null)
        {
            var tt = new ToolTip
            {
                AutoPopDelay = 10000,
                InitialDelay = 300,
                ShowAlways = true
            };
            tt.SetToolTip(valueLabel, tooltip);
            tt.SetToolTip(label, tooltip);
        }

        tlpPhysicsConstants.Controls.Add(label, 0, row);
        tlpPhysicsConstants.Controls.Add(valueLabel, 1, row);
        row++;
    }

    /// <summary>
    /// Placeholder for backward compatibility with existing code.
    /// </summary>
    private void InitializeWheelerDeWittDisplay()
    {
        // Redirects to new implementation
        // InitializeWheelerDeWittControls() is called from InitializeAllSettingsControls()
    }
}