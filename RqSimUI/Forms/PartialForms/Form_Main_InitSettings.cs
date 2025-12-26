using System.Windows.Forms;

namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - Settings initialization calls.
/// This file consolidates all Settings tab initialization in one place.
/// </summary>
public partial class Form_Main
{
    /// <summary>
    /// Initializes all Settings tab controls.
    /// Call this from Form_Main constructor after InitializeComponent() and before other initializations.
    /// This method calls all Settings-related initialization methods in correct order.
    /// </summary>
    private void InitializeAllSettingsControls()
    {
        // 1. Initialize Graph Health controls (adds to tlpPhysicsConstants)
        InitializeGraphHealthControls();

        // 2. Initialize RQ-Hypothesis Experimental Flags (adds to flpPhysics)
        InitializeRQExperimentalFlagsControls();

        // 3. Initialize Advanced Physics controls (adds more to tlpPhysicsConstants)
        InitializeAdvancedPhysicsControls();

        // 4. Initialize All Physics Constants display panel (read-only reference)
        InitializeAllPhysicsConstantsDisplay();

        // 5. Sync UI with PhysicsConstants defaults
        SyncUIWithPhysicsConstants();
    }
}