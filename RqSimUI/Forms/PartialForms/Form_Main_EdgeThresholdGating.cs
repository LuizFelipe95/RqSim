namespace RqSimForms;

/// <summary>
/// Partial class for Edge Threshold gating logic.
/// Part of Phase 4 of uni-pipeline implementation.
/// Handles edge threshold field declarations and Science mode gating.
/// </summary>
partial class Form_Main
{
    // === Edge Threshold Fields ===
    
    /// <summary>
    /// Current edge threshold value (0.0 to 1.0).
    /// Used for filtering edges in visualization and physics.
    /// </summary>
    private double _edgeThresholdValue = 0.1;
    
    /// <summary>
    /// TrackBar control for edge threshold adjustment.
    /// Range: 0-100, maps to 0.0-1.0.
    /// </summary>
    private TrackBar? _trkEdgeThreshold;
    
    /// <summary>
    /// Label displaying current edge threshold value.
    /// </summary>
    private Label? _lblEdgeThresholdValue;

    /// <summary>
    /// Initializes edge threshold controls in the main form.
    /// Call this after form initialization.
    /// </summary>
    private void InitializeEdgeThresholdControls()
    {
        if (_trkEdgeThreshold is not null)
        {
            _trkEdgeThreshold.Minimum = 0;
            _trkEdgeThreshold.Maximum = 100;
            _trkEdgeThreshold.Value = (int)(_edgeThresholdValue * 100);
            _trkEdgeThreshold.ValueChanged += TrkEdgeThreshold_ValueChanged;
        }
        
        if (_lblEdgeThresholdValue is not null)
        {
            _lblEdgeThresholdValue.Text = _edgeThresholdValue.ToString("F2");
        }
        
        // Apply initial Science mode state
        UpdateEdgeThresholdAvailability();
    }

    /// <summary>
    /// Handler for edge threshold trackbar value changes.
    /// </summary>
    private void TrkEdgeThreshold_ValueChanged(object? sender, EventArgs e)
    {
        if (_trkEdgeThreshold is null) return;
        
        _edgeThresholdValue = _trkEdgeThreshold.Value / 100.0;
        
        if (_lblEdgeThresholdValue is not null)
        {
            _lblEdgeThresholdValue.Text = _edgeThresholdValue.ToString("F2");
        }
        
        // Sync to CSR visualization
        SyncEdgeThresholdToCsrVisualization();
    }

    /// <summary>
    /// Updates edge threshold control availability based on Science mode.
    /// In Science mode, edge threshold is locked to prevent manual adjustments.
    /// </summary>
    private void UpdateEdgeThresholdAvailability()
    {
        bool scienceModeEnabled = checkBox_ScienceSimMode?.Checked ?? false;
        
        if (_trkEdgeThreshold is not null)
        {
            _trkEdgeThreshold.Enabled = !scienceModeEnabled;
        }
        
        if (_lblEdgeThresholdValue is not null)
        {
            _lblEdgeThresholdValue.ForeColor = scienceModeEnabled 
                ? System.Drawing.Color.Gray 
                : System.Drawing.SystemColors.ControlText;
        }
        
        // Also update CSR visualization trackbar
        if (_csrTrackEdgeThreshold is not null)
        {
            _csrTrackEdgeThreshold.Enabled = !scienceModeEnabled;
        }
    }

    /// <summary>
    /// Sets edge threshold value programmatically.
    /// Updates both UI controls and internal state.
    /// </summary>
    /// <param name="value">Threshold value (0.0 to 1.0)</param>
    public void SetEdgeThreshold(double value)
    {
        _edgeThresholdValue = Math.Clamp(value, 0.0, 1.0);
        
        if (_trkEdgeThreshold is not null)
        {
            int intValue = (int)(_edgeThresholdValue * 100);
            if (_trkEdgeThreshold.Value != intValue)
            {
                _trkEdgeThreshold.Value = intValue;
            }
        }
        
        if (_lblEdgeThresholdValue is not null)
        {
            _lblEdgeThresholdValue.Text = _edgeThresholdValue.ToString("F2");
        }
        
        // Sync to CSR visualization
        SyncEdgeThresholdToCsrVisualization();
    }

    /// <summary>
    /// Gets current edge threshold value.
    /// </summary>
    public double GetEdgeThreshold() => _edgeThresholdValue;
}
