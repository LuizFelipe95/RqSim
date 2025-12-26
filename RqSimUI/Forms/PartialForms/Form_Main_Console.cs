using System;

namespace RqSimForms;

public partial class Form_Main
{
    private System.Windows.Forms.Timer? _sysConsoleLiveTimer;
    private System.Windows.Forms.Timer? _simConsoleLiveTimer;

    private void button_SysConsole_CopyToClipboard_Click(object? sender, EventArgs e)
    {
        if (IsDisposed || textBox_SysConsole is null || textBox_SysConsole.IsDisposed)
            return;

        try
        {
            Clipboard.SetText(textBox_SysConsole.Text);
        }
        catch
        {
            // Clipboard can fail if locked by another process; ignore.
        }
    }

    private void button_SysConsole_Clear_Click(object? sender, EventArgs e)
    {
        _sysConsoleLines.Clear();

        if (IsDisposed || textBox_SysConsole is null || textBox_SysConsole.IsDisposed)
            return;

        if (textBox_SysConsole.InvokeRequired)
        {
            if (!textBox_SysConsole.IsHandleCreated) return;
            textBox_SysConsole.BeginInvoke(new Action(() => button_SysConsole_Clear_Click(sender, e)));
            return;
        }

        textBox_SysConsole.Clear();
    }

    private void comboBox_SysConsole_OutType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _sysConsoleOutType = comboBox_SysConsole_OutType?.SelectedIndex switch
        {
            1 => SysConsoleOutType.Info,
            2 => SysConsoleOutType.Warning,
            3 => SysConsoleOutType.Error,
            4 => SysConsoleOutType.Dispatcher,
            5 => SysConsoleOutType.GPU,
            6 => SysConsoleOutType.IO,
            _ => SysConsoleOutType.All,
        };

        RefreshSysConsole();
    }

    private void button_SysConsole_Refresh_Click(object? sender, EventArgs e)
    {
        RefreshSysConsole();

        if (checkBox_AutoScrollSysConsole is not null && checkBox_AutoScrollSysConsole.Checked)
            ScrollSysConsoleToBottom();
    }

    private void button_SimConsole_CopyToClipboard_Click(object? sender, EventArgs e)
    {
        if (IsDisposed || textBox_SimConsole is null || textBox_SimConsole.IsDisposed)
            return;

        try
        {
            Clipboard.SetText(textBox_SimConsole.Text);
        }
        catch
        {
            // Clipboard can fail if locked by another process; ignore.
        }
    }

    private void button_SimConsole_Refresh_Click(object? sender, EventArgs e)
    {
        RefreshSimConsole();

        if (checkBox_AutoScrollSimConsole is not null && checkBox_AutoScrollSimConsole.Checked)
            ScrollSimConsoleToBottom();
    }

    private void button_SimConsole_Clear_Click(object? sender, EventArgs e)
    {
        _simConsoleLines.Clear();

        if (IsDisposed || textBox_SimConsole is null || textBox_SimConsole.IsDisposed)
            return;

        if (textBox_SimConsole.InvokeRequired)
        {
            if (!textBox_SimConsole.IsHandleCreated) return;
            textBox_SimConsole.BeginInvoke(new Action(() => button_SimConsole_Clear_Click(sender, e)));
            return;
        }

        textBox_SimConsole.Clear();
    }

    private void checkBox_SysConsole_LiveUpdate_CheckedChanged(object sender, EventArgs e)
    {
        if (IsDisposed)
            return;

        if (sender is not CheckBox cb || cb.IsDisposed)
            return;

        ToggleSysConsoleLiveUpdate(cb.Checked);
    }

    private void checkBox_SimConsole_LiveUpdate_CheckedChanged(object sender, EventArgs e)
    {
        if (IsDisposed)
            return;

        if (sender is not CheckBox cb || cb.IsDisposed)
            return;

        ToggleSimConsoleLiveUpdate(cb.Checked);
    }

    private void checkBox_AutoScrollSysConsole_CheckedChanged(object sender, EventArgs e)
    {
        // AutoScroll only. If enabled, jump to bottom immediately.
        if (IsDisposed || textBox_SysConsole is null || textBox_SysConsole.IsDisposed)
            return;

        if (checkBox_AutoScrollSysConsole is not null && checkBox_AutoScrollSysConsole.Checked)
            ScrollSysConsoleToBottom();
    }

    private void checkBox_AutoScrollSimConsole_CheckedChanged(object sender, EventArgs e)
    {
        // AutoScroll only. If enabled, jump to bottom immediately.
        if (IsDisposed || textBox_SimConsole is null || textBox_SimConsole.IsDisposed)
            return;

        if (checkBox_AutoScrollSimConsole is not null && checkBox_AutoScrollSimConsole.Checked)
            ScrollSimConsoleToBottom();
    }

    private void ToggleSysConsoleLiveUpdate(bool enabled)
    {
        if (!enabled)
        {
            _sysConsoleLiveTimer?.Stop();
            return;
        }

        _sysConsoleLiveTimer ??= new System.Windows.Forms.Timer
        {
            Interval = 150,
            Enabled = false,
        };
        _sysConsoleLiveTimer.Tick -= SysConsoleLiveTimer_Tick;
        _sysConsoleLiveTimer.Tick += SysConsoleLiveTimer_Tick;

        _sysConsoleLiveTimer.Start();
    }

    private void ToggleSimConsoleLiveUpdate(bool enabled)
    {
        if (!enabled)
        {
            _simConsoleLiveTimer?.Stop();
            return;
        }

        _simConsoleLiveTimer ??= new System.Windows.Forms.Timer
        {
            Interval = 150,
            Enabled = false,
        };
        _simConsoleLiveTimer.Tick -= SimConsoleLiveTimer_Tick;
        _simConsoleLiveTimer.Tick += SimConsoleLiveTimer_Tick;

        _simConsoleLiveTimer.Start();
    }

    private void SysConsoleLiveTimer_Tick(object? sender, EventArgs e)
    {
        // Live update is UI-thread only (Timer). Keep work short.
        try
        {
            DrainSysConsoleSources();

            if (checkBox_AutoScrollSysConsole is not null && checkBox_AutoScrollSysConsole.Checked)
                ScrollSysConsoleToBottom();
        }
        catch
        {
            // Avoid crashing UI on logging timer faults.
            _sysConsoleLiveTimer?.Stop();
        }
    }

    private void SimConsoleLiveTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            DrainSimConsoleSources();

            if (checkBox_AutoScrollSimConsole is not null && checkBox_AutoScrollSimConsole.Checked)
                ScrollSimConsoleToBottom();
        }
        catch
        {
            _simConsoleLiveTimer?.Stop();
        }
    }

    private void DrainSysConsoleSources()
    {
        // If you have additional sys sources later, add them here.
        // Currently, sys console updates are mostly direct `AppendSysLog(...)` calls.
    }

    private void DrainSimConsoleSources()
    {
        // Drain buffered simulation logs (CPU/GPU) into sim console.
        foreach (var line in RQSimulation.Analysis.LogStatistics.FetchCpuLogs())
            AppendSimLog(line);

        foreach (var line in RQSimulation.Analysis.LogStatistics.FetchGpuLogs())
            AppendSimLog(line);
    }

    private void AppendSimConsole(string text)
    {
        if (IsDisposed || textBox_SimConsole == null || textBox_SimConsole.IsDisposed)
            return;

        if (_simConsoleBuffer is not null)
        {
            _simConsoleBuffer.Append(text);
        }
        else
        {
            textBox_SimConsole.AppendText(text);
        }

        if (checkBox_AutoScrollSimConsole is not null && checkBox_AutoScrollSimConsole.Checked)
            ScrollSimConsoleToBottom();
    }

    private void AppendSysConsole(string text)
    {
        if (IsDisposed || textBox_SysConsole == null || textBox_SysConsole.IsDisposed)
            return;

        if (_sysConsoleBuffer is not null)
        {
            _sysConsoleBuffer.Append(text);
        }
        else
        {
            textBox_SysConsole.AppendText(text);
        }

        if (checkBox_AutoScrollSysConsole is not null && checkBox_AutoScrollSysConsole.Checked)
            ScrollSysConsoleToBottom();
    }

    private void ScrollSysConsoleToBottom()
    {
        if (textBox_SysConsole is null || textBox_SysConsole.IsDisposed)
            return;

        if (textBox_SysConsole.InvokeRequired)
        {
            if (!textBox_SysConsole.IsHandleCreated) return;
            textBox_SysConsole.BeginInvoke(new Action(ScrollSysConsoleToBottom));
            return;
        }

        textBox_SysConsole.SelectionStart = textBox_SysConsole.Text.Length;
        textBox_SysConsole.ScrollToCaret();
    }

    private void ScrollSimConsoleToBottom()
    {
        if (textBox_SimConsole is null || textBox_SimConsole.IsDisposed)
            return;

        if (textBox_SimConsole.InvokeRequired)
        {
            if (!textBox_SimConsole.IsHandleCreated) return;
            textBox_SimConsole.BeginInvoke(new Action(ScrollSimConsoleToBottom));
            return;
        }

        textBox_SimConsole.SelectionStart = textBox_SimConsole.Text.Length;
        textBox_SimConsole.ScrollToCaret();
    }

    private void AppendSummary(string text)
    {
        if (summaryTextBox == null) return;
        if (summaryTextBox.InvokeRequired)
        {
            summaryTextBox.Invoke(() => AppendSummary(text));
            return;
        }
        summaryTextBox.AppendText(text + (text.EndsWith("\n") ? string.Empty : "\n"));
    }

    private void AppendSysLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var (category, normalized) = ClassifySysMessage(message);

        lock (_sysConsoleLines)
        {
            _sysConsoleLines.Add(new ConsoleLine(DateTime.UtcNow, category, normalized));
            if (_sysConsoleLines.Count > MaxConsoleLines)
                _sysConsoleLines.RemoveRange(0, _sysConsoleLines.Count - MaxConsoleLines);
        }

        if (!PassSysFilter(category))
            return;

        AppendSysConsole(normalized);
    }

    private void AppendSimLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var (category, normalized) = ClassifySimMessage(message);

        lock (_simConsoleLines)
        {
            _simConsoleLines.Add(new ConsoleLine(DateTime.UtcNow, category, normalized));
            if (_simConsoleLines.Count > MaxConsoleLines)
                _simConsoleLines.RemoveRange(0, _simConsoleLines.Count - MaxConsoleLines);
        }

        if (!PassSimFilter(category))
            return;

        AppendSimConsole(normalized);
    }

    private void RefreshSysConsole()
    {
        if (IsDisposed || textBox_SysConsole is null || textBox_SysConsole.IsDisposed)
            return;

        if (textBox_SysConsole.InvokeRequired)
        {
            if (!textBox_SysConsole.IsHandleCreated) return;
            textBox_SysConsole.BeginInvoke(new Action(RefreshSysConsole));
            return;
        }

        textBox_SysConsole.Clear();

        List<ConsoleLine> snapshot;
        lock (_sysConsoleLines)
        {
            snapshot = new List<ConsoleLine>(_sysConsoleLines);
        }

        foreach (var line in snapshot)
        {
            if (PassSysFilter(line.Category))
                textBox_SysConsole.AppendText(line.Message);
        }

        if (checkBox_AutoScrollSysConsole is not null && checkBox_AutoScrollSysConsole.Checked)
            ScrollSysConsoleToBottom();
    }

    private void RefreshSimConsole()
    {
        if (IsDisposed || textBox_SimConsole is null || textBox_SimConsole.IsDisposed)
            return;

        if (textBox_SimConsole.InvokeRequired)
        {
            if (!textBox_SimConsole.IsHandleCreated) return;
            textBox_SimConsole.BeginInvoke(new Action(RefreshSimConsole));
            return;
        }

        textBox_SimConsole.Clear();

        List<ConsoleLine> snapshot;
        lock (_simConsoleLines)
        {
            snapshot = new List<ConsoleLine>(_simConsoleLines);
        }

        foreach (var line in snapshot)
        {
            if (PassSimFilter(line.Category))
                textBox_SimConsole.AppendText(line.Message);
        }

        if (checkBox_AutoScrollSimConsole is not null && checkBox_AutoScrollSimConsole.Checked)
            ScrollSimConsoleToBottom();
    }

    private static (string Category, string Normalized) ClassifySysMessage(string message)
    {
        // Ensure newline termination.
        var m = message.EndsWith("\n", StringComparison.Ordinal) ? message : message + "\n";

        if (m.Contains("[ERROR]", StringComparison.OrdinalIgnoreCase) || m.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
            return (nameof(SysConsoleOutType.Error), PrefixIfMissing(nameof(SysConsoleOutType.Error), m));

        if (m.Contains("[WARN]", StringComparison.OrdinalIgnoreCase) || m.Contains("WARNING", StringComparison.OrdinalIgnoreCase))
            return (nameof(SysConsoleOutType.Warning), PrefixIfMissing(nameof(SysConsoleOutType.Warning), m));

        if (m.StartsWith("[Dispatcher]", StringComparison.OrdinalIgnoreCase) || m.Contains("Dispatcher", StringComparison.OrdinalIgnoreCase))
            return (nameof(SysConsoleOutType.Dispatcher), PrefixIfMissing(nameof(SysConsoleOutType.Dispatcher), m));

        if (m.Contains("GPU", StringComparison.OrdinalIgnoreCase))
            return (nameof(SysConsoleOutType.GPU), PrefixIfMissing(nameof(SysConsoleOutType.GPU), m));

        if (m.Contains("[IO]", StringComparison.OrdinalIgnoreCase) || m.Contains("File", StringComparison.OrdinalIgnoreCase) || m.Contains("Path", StringComparison.OrdinalIgnoreCase))
            return (nameof(SysConsoleOutType.IO), PrefixIfMissing(nameof(SysConsoleOutType.IO), m));

        return (nameof(SysConsoleOutType.Info), PrefixIfMissing(nameof(SysConsoleOutType.Info), m));
    }

    private static (string Category, string Normalized) ClassifySimMessage(string message)
    {
        var m = message.EndsWith("\n", StringComparison.Ordinal) ? message : message + "\n";

        if (m.Contains("[Pipeline ERROR]", StringComparison.OrdinalIgnoreCase) || m.Contains("[ERROR]", StringComparison.OrdinalIgnoreCase))
            return (nameof(SimConsoleOutType.Error), PrefixIfMissing(nameof(SimConsoleOutType.Error), m));

        if (m.Contains("[Pipeline]", StringComparison.OrdinalIgnoreCase))
            return (nameof(SimConsoleOutType.Pipeline), PrefixIfMissing(nameof(SimConsoleOutType.Pipeline), m));

        if (m.Contains("Physics", StringComparison.OrdinalIgnoreCase) || m.Contains("Module", StringComparison.OrdinalIgnoreCase))
            return (nameof(SimConsoleOutType.Physics), PrefixIfMissing(nameof(SimConsoleOutType.Physics), m));

        if (m.Contains("Metric", StringComparison.OrdinalIgnoreCase) || m.Contains("Spectral", StringComparison.OrdinalIgnoreCase))
            return (nameof(SimConsoleOutType.Metrics), PrefixIfMissing(nameof(SimConsoleOutType.Metrics), m));

        if (m.Contains("WARNING", StringComparison.OrdinalIgnoreCase) || m.Contains("[WARN]", StringComparison.OrdinalIgnoreCase))
            return (nameof(SimConsoleOutType.Warning), PrefixIfMissing(nameof(SimConsoleOutType.Warning), m));

        return (nameof(SimConsoleOutType.Info), PrefixIfMissing(nameof(SimConsoleOutType.Info), m));
    }

    private bool PassSysFilter(string category)
    {
        if (_sysConsoleOutType == SysConsoleOutType.All)
            return true;

        return category == _sysConsoleOutType.ToString();
    }

    private bool PassSimFilter(string category)
    {
        if (_simConsoleOutType == SimConsoleOutType.All)
            return true;

        return category == _simConsoleOutType.ToString();
    }

    private static string PrefixIfMissing(string category, string message)
    {
        var prefix = $"[{category}] ";
        var withPrefix = message.StartsWith("[", StringComparison.Ordinal) ? message : prefix + message;

        // Always separate console entries by a blank line.
        withPrefix = withPrefix.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!withPrefix.EndsWith("\n", StringComparison.Ordinal))
            withPrefix += "\n";
        if (!withPrefix.EndsWith("\n\n", StringComparison.Ordinal))
            withPrefix += "\n";

        return withPrefix;
    }

    private static string WrapToWidth(string text, int width) => text;
}