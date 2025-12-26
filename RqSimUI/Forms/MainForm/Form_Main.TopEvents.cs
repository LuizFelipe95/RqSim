using RqSimForms.Events;
using RqSimForms.ProcessesDispatcher.Contracts;
using RqSimForms.ProcessesDispatcher.IPC;
using RqSimUI.Forms.PartialForms;
using RqSimUI.FormSimAPI.Interfaces;

namespace RqSimForms;

/// <summary>
/// Helper class for combobox items with PhysicsEventType and display name.
/// </summary>
internal sealed class EventTypeItem(PhysicsEventType? eventType, string displayName)
{
    public PhysicsEventType? EventType { get; } = eventType;
    public string DisplayName { get; } = displayName;

    public override string ToString() => DisplayName;
}

public partial class Form_Main
{
    /// <summary>
    /// Store for physics verification events.
    /// </summary>
    private readonly PhysicsEventStore _eventStore = new();

    /// <summary>
    /// Whether live update is enabled for events list.
    /// </summary>
    private bool _eventsLiveUpdateEnabled;

    /// <summary>
    /// Current filter type for events (null = all events).
    /// </summary>
    private PhysicsEventType? _currentEventFilter;

    /// <summary>
    /// TabPage for TopEvents panel.
    /// </summary>
    private TabPage? tabPage_TopEvents;

    /// <summary>
    /// Initializes the TopEvents UI components and event handlers.
    /// Call this from the main Form initialization.
    /// </summary>
    private void InitializeTopEvents()
    {
        // Create TabPage for TopEvents
        tabPage_TopEvents = new TabPage
        {
            Text = "Physics Events",
            Name = "tabPage_TopEvents",
            Padding = new Padding(3)
        };

        // Create main layout panel
        TableLayoutPanel tlpTopEvents = new()
        {
            Name = "tlpTopEvents",
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(4)
        };

        tlpTopEvents.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tlpTopEvents.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Create toolbar panel
        FlowLayoutPanel flpToolbar = new()
        {
            Name = "flpTopEventsToolbar",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        // Initialize controls
        comboBox_TopEvents_EventType = new ComboBox
        {
            Name = "comboBox_TopEvents_EventType",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200,
            Margin = new Padding(3)
        };
        
        // Add event types with descriptive names
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(null, "All Events"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.MassGap, "MassGap - Yang-Mills spectral gap"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.SpectralDimension, "SpectralDimension - d_S ? 4D"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.SpeedOfLightIsotropy, "SpeedOfLight - Lieb-Robinson"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.RicciFlatness, "RicciFlatness - Ricci ? 0"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.HolographicAreaLaw, "HolographicAreaLaw - S ~ Area"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.HausdorffDimension, "HausdorffDimension - d_H"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.ClusterTransition, "ClusterTransition - phase"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.EnergyViolation, "EnergyViolation - constraint"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.AutoTuningAdjustment, "AutoTuning - parameter adj"));
        comboBox_TopEvents_EventType.Items.Add(new EventTypeItem(PhysicsEventType.Milestone, "Milestone - simulation"));
        
        comboBox_TopEvents_EventType.DisplayMember = "DisplayName";
        comboBox_TopEvents_EventType.SelectedIndex = 0;
        comboBox_TopEvents_EventType.SelectedIndexChanged += comboBox_TopEvents_EventType_SelectedIndexChanged;

        checkBox_TopEvents_LiveUpdate = new CheckBox
        {
            Name = "checkBox_TopEvents_LiveUpdate",
            Text = "Live Update",
            AutoSize = true,
            Margin = new Padding(8, 6, 3, 3),
            Checked = true
        };
        checkBox_TopEvents_LiveUpdate.CheckedChanged += checkBox_TopEvents_LiveUpdate_CheckedChanged;
        _eventsLiveUpdateEnabled = true;

        buttonTopEvents_Refresh = new Button
        {
            Name = "buttonTopEvents_Refresh",
            Text = "Refresh",
            Width = 80,
            Margin = new Padding(3)
        };
        buttonTopEvents_Refresh.Click += buttonTopEvents_Refresh_Click;

        buttonTopEvents_Clear = new Button
        {
            Name = "buttonTopEvents_Clear",
            Text = "Clear",
            Width = 80,
            Margin = new Padding(3)
        };
        buttonTopEvents_Clear.Click += buttonTopEvents_Clear_Click;

        buttonTopEvents_SaveJson = new Button
        {
            Name = "buttonTopEvents_SaveJson",
            Text = "Export JSON",
            Width = 100,
            Margin = new Padding(3)
        };
        buttonTopEvents_SaveJson.Click += buttonTopEvents_SaveJson_Click;

        // Add controls to toolbar
        flpToolbar.Controls.Add(comboBox_TopEvents_EventType);
        flpToolbar.Controls.Add(checkBox_TopEvents_LiveUpdate);
        flpToolbar.Controls.Add(buttonTopEvents_Refresh);
        flpToolbar.Controls.Add(buttonTopEvents_Clear);
        flpToolbar.Controls.Add(buttonTopEvents_SaveJson);

        // Create ListView
        listView_TopEvents = new ListView
        {
            Name = "listView_TopEvents",
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false
        };

        // Create column headers
        columnHeader_TopEventType = new ColumnHeader
        {
            Text = "Event Type",
            Width = 140
        };
        columnHeader_TopEventDecs = new ColumnHeader
        {
            Text = "Description",
            Width = 320
        };
        columnHeader_TopEventParams = new ColumnHeader
        {
            Text = "Parameters",
            Width = 250
        };

        listView_TopEvents.Columns.AddRange(new[] {
            columnHeader_TopEventType,
            columnHeader_TopEventDecs,
            columnHeader_TopEventParams
        });

        listView_TopEvents.SelectedIndexChanged += listView_TopEvents_SelectedIndexChanged;

        // Assemble layout
        tlpTopEvents.Controls.Add(flpToolbar, 0, 0);
        tlpTopEvents.Controls.Add(listView_TopEvents, 0, 1);

        tabPage_TopEvents.Controls.Add(tlpTopEvents);

        // Add to TabControl (insert after Charts tab if it exists)
        int insertIndex = tabControl_Main.TabPages.Count;
        for (int i = 0; i < tabControl_Main.TabPages.Count; i++)
        {
            if (tabControl_Main.TabPages[i].Name == "tabPage_Charts")
            {
                insertIndex = i + 1;
                break;
            }
        }
        tabControl_Main.TabPages.Insert(insertIndex, tabPage_TopEvents);

        // Subscribe to event store notifications
        _eventStore.EventAdded += OnPhysicsEventAdded;
        _eventStore.EventsCleared += OnPhysicsEventsCleared;
    }

    /// <summary>
    /// Adds a physics verification event to the store.
    /// </summary>
    public void AddPhysicsEvent(PhysicsVerificationEvent evt)
    {
        _eventStore.Add(evt);
    }

    /// <summary>
    /// Gets the physics event store for external access.
    /// </summary>
    public PhysicsEventStore PhysicsEvents => _eventStore;

    /// <summary>
    /// Logs a mass gap measurement event.
    /// </summary>
    public void LogMassGapEvent(long step, double gapValue, double? targetGap = null)
    {
        AddPhysicsEvent(PhysicsVerificationEvent.MassGap(step, gapValue, targetGap));
    }

    /// <summary>
    /// Logs a spectral dimension measurement event.
    /// </summary>
    public void LogSpectralDimensionEvent(long step, double dS, double confidence)
    {
        AddPhysicsEvent(PhysicsVerificationEvent.SpectralDimension(step, dS, confidence));
    }

    /// <summary>
    /// Logs a speed of light isotropy event.
    /// </summary>
    public void LogSpeedOfLightEvent(long step, double velocity, double variance)
    {
        AddPhysicsEvent(PhysicsVerificationEvent.SpeedOfLightIsotropy(step, velocity, variance));
    }

    /// <summary>
    /// Logs a Ricci flatness event.
    /// </summary>
    public void LogRicciFlatnessEvent(long step, double avgCurvature)
    {
        AddPhysicsEvent(PhysicsVerificationEvent.RicciFlatness(step, avgCurvature));
    }

    /// <summary>
    /// Logs a holographic area law event.
    /// </summary>
    public void LogHolographicAreaLawEvent(long step, double entropy, double area, double volume)
    {
        AddPhysicsEvent(PhysicsVerificationEvent.HolographicAreaLaw(step, entropy, area, volume));
    }

    /// <summary>
    /// Logs an auto-tuning adjustment event.
    /// </summary>
    public void LogAutoTuningEvent(long step, string parameter, double oldValue, double newValue)
    {
        AddPhysicsEvent(PhysicsVerificationEvent.AutoTuningAdjustment(step, parameter, oldValue, newValue));
    }

    private void OnPhysicsEventAdded(object? sender, PhysicsVerificationEvent evt)
    {
        if (!_eventsLiveUpdateEnabled)
        {
            return;
        }

        // Marshal to UI thread if needed
        if (InvokeRequired)
        {
            BeginInvoke(() => AddEventToListView(evt));
        }
        else
        {
            AddEventToListView(evt);
        }
    }

    private void OnPhysicsEventsCleared(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => listView_TopEvents.Items.Clear());
        }
        else
        {
            listView_TopEvents.Items.Clear();
        }
    }

    private void AddEventToListView(PhysicsVerificationEvent evt)
    {
        // Check filter
        if (_currentEventFilter.HasValue && evt.EventType != _currentEventFilter.Value)
        {
            return;
        }

        ListViewItem item = CreateListViewItem(evt);
        listView_TopEvents.Items.Insert(0, item); // Add at top (newest first)

        // Limit displayed items
        while (listView_TopEvents.Items.Count > 500)
        {
            listView_TopEvents.Items.RemoveAt(listView_TopEvents.Items.Count - 1);
        }
    }

    private static ListViewItem CreateListViewItem(PhysicsVerificationEvent evt)
    {
        ListViewItem item = new(evt.EventType.ToString());
        item.SubItems.Add(evt.Description);
        item.SubItems.Add(evt.ParametersDisplay);
        item.Tag = evt;

        // Color coding based on severity
        item.BackColor = evt.Severity switch
        {
            2 => Color.FromArgb(255, 200, 200), // Critical - light red
            1 => Color.FromArgb(255, 255, 200), // Warning - light yellow
            _ => Color.White
        };

        return item;
    }

    private void RefreshEventListView()
    {
        listView_TopEvents.BeginUpdate();
        try
        {
            listView_TopEvents.Items.Clear();

            var events = _eventStore.GetFiltered(_currentEventFilter, limit: 500);

            foreach (var evt in events.Reverse()) // Show newest first
            {
                listView_TopEvents.Items.Add(CreateListViewItem(evt));
            }
        }
        finally
        {
            listView_TopEvents.EndUpdate();
        }
    }

    private async void buttonTopEvents_SaveJson_Click(object? sender, EventArgs e)
    {
        try
        {
            using SaveFileDialog dialog = new()
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json",
                FileName = $"physics_events_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                await _eventStore.ExportToFileAsync(dialog.FileName, _currentEventFilter).ConfigureAwait(true);
                MessageBox.Show(
                    $"Exported {_eventStore.Count} events to:\n{dialog.FileName}",
                    "Export Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to export events: {ex.Message}",
                "Export Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void buttonTopEvents_Refresh_Click(object? sender, EventArgs e)
    {
        RefreshEventListView();
    }

    private void buttonTopEvents_Clear_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show(
            "Clear all physics events?",
            "Confirm Clear",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _eventStore.Clear();
        }
    }

    private void checkBox_TopEvents_LiveUpdate_CheckedChanged(object? sender, EventArgs e)
    {
        _eventsLiveUpdateEnabled = checkBox_TopEvents_LiveUpdate.Checked;

        if (_eventsLiveUpdateEnabled)
        {
            // Refresh when enabling live update
            RefreshEventListView();
        }
    }

    private void comboBox_TopEvents_EventType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (comboBox_TopEvents_EventType.SelectedItem is EventTypeItem selectedItem)
        {
            _currentEventFilter = selectedItem.EventType;
        }
        else
        {
            _currentEventFilter = null;
        }

        RefreshEventListView();
    }

    private void listView_TopEvents_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (listView_TopEvents.SelectedItems.Count > 0 &&
            listView_TopEvents.SelectedItems[0].Tag is PhysicsVerificationEvent evt)
        {
            // Show detailed info in status bar or tooltip
            string details = $"Step {evt.Timestamp}: {evt.EventType} - Value: {evt.Value:G4}";
            if (evt.SecondaryValue.HasValue)
            {
                details += $", Secondary: {evt.SecondaryValue.Value:G4}";
            }

            // Update status bar if available
            if (statusLabelSteps != null)
            {
                statusLabelSteps.Text = details;
            }
        }
    }

    /// <summary>
    /// Gets summary statistics of events by type.
    /// </summary>
    public Dictionary<PhysicsEventType, int> GetEventSummary()
    {
        return _eventStore.GetEventCounts();
    }

    /// <summary>
    /// Gets the latest event of a specific type.
    /// </summary>
    public PhysicsVerificationEvent? GetLatestEvent(PhysicsEventType type)
    {
        return _eventStore.GetLatest(type);
    }
}

































































































