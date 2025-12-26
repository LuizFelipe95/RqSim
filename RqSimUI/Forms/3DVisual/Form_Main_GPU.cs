using RqSimForms.Forms.Interfaces;
using RQSimulation;
using RQSimulation.Analysis;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using RqSimUI.FormSimAPI.Interfaces;

namespace RqSimForms;

public partial class Form_Main
{
    /// <summary>
    /// Инициализирует контролы GPU: заполняет comboBox_GPUIndex доступными устройствами
    /// </summary>
    private void InitializeGpuControls()
    {
        comboBox_GPUIndex.Items.Clear();

        try
        {
            // Проверяем доступность GPU через ComputeSharp
            var defaultDevice = ComputeSharp.GraphicsDevice.GetDefault();
            _simApi.GpuAvailable = defaultDevice != null;

            if (_simApi.GpuAvailable)
            {
                // Добавляем устройство по умолчанию
                comboBox_GPUIndex.Items.Add($"0: {defaultDevice.Name}");

                // Пытаемся получить все устройства
                int deviceIndex = 0;
                foreach (var device in ComputeSharp.GraphicsDevice.EnumerateDevices())
                {
                    if (deviceIndex > 0) // Первое уже добавлено
                    {
                        comboBox_GPUIndex.Items.Add($"{deviceIndex}: {device.Name}");
                    }
                    deviceIndex++;
                }

                comboBox_GPUIndex.SelectedIndex = 0;
                AppendSysConsole($"[GPU] Обнаружено устройств: {comboBox_GPUIndex.Items.Count}\n");
                AppendSysConsole($"[GPU] Устройство по умолчанию: {defaultDevice.Name}\n");
            }
            else
            {
                comboBox_GPUIndex.Items.Add("Нет доступных GPU");
                comboBox_GPUIndex.SelectedIndex = 0;
                checkBox_EnableGPU.Checked = false;
                checkBox_EnableGPU.Enabled = false;
            }
        }
        catch (Exception ex)
        {
            _simApi.GpuAvailable = false;
            comboBox_GPUIndex.Items.Add("GPU недоступен");
            comboBox_GPUIndex.SelectedIndex = 0;
            checkBox_EnableGPU.Checked = false;
            checkBox_EnableGPU.Enabled = false;
            AppendSysConsole($"[GPU] Ошибка инициализации: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Проверяет доступность GPU и возвращает true если можно использовать GPU ускорение
    /// </summary>
    private bool CanUseGpu()
    {
        return _simApi.GpuAvailable && checkBox_EnableGPU.Checked;
    }

    private void checkBox_EnableGPU_CheckedChanged(object sender, EventArgs e)
    {
        // Здесь можно добавить логику для обработки изменения состояния GPU
        AppendSysConsole($"GPU ускорение {(checkBox_EnableGPU.Checked ? "включено" : "выключено")}.\n");
    }

    // Helper to append to GPU console
    private void AppendGPUConsole(string text)
    {
        if (textBox_SimConsole.InvokeRequired)
        {
            textBox_SimConsole.BeginInvoke(new Action(() => AppendGPUConsole(text)));
        }
        else
        {
            if (textBox_SimConsole.TextLength > 50000)
            {
                textBox_SimConsole.Clear();
                textBox_SimConsole.AppendText("[Console cleared due to size limit]\n");
            }
            textBox_SimConsole.AppendText(text);
            if (checkBox_AutoScrollSysConsole.Checked)
            {
                textBox_SimConsole.SelectionStart = textBox_SimConsole.TextLength;
                textBox_SimConsole.ScrollToCaret();
            }
        }
    }

    private void button_CPUtoGPUCompare_Click(object sender, EventArgs e)
    {
        if (_isModernRunning)
        {
            MessageBox.Show("Please stop the current simulation first.");
            return;
        }

        var config = GetConfigFromUI();

        // Disable buttons
        button_CPUtoGPUCompare.Enabled = false;
        button_RunModernSim.Enabled = false;

        // Start timer to see logs
        if (_uiUpdateTimer == null)
        {
            _uiUpdateTimer = new System.Windows.Forms.Timer();
            _uiUpdateTimer.Interval = 100;
            _uiUpdateTimer.Tick += UiUpdateTimer_Tick;
            _uiUpdateTimer.Start();
        }

        Task.Run(async () =>
        {
            try
            {
                string report = await LogStatistics.CompareCpuGpu(config, async (cfg, useGpu) =>
                {
                    return await Task.Run(() =>
                    {
                        var stats = new LogStatistics.RunStats
                        {
                            DeviceType = useGpu ? "GPU" : "CPU"
                        };

                        // Redirect logs for this run
                        Action<string> logAction = useGpu ? LogStatistics.LogGPU : LogStatistics.LogCPU;

                        // We need to invoke on UI thread to change _simApi callback safely if it was not thread safe, 
                        // but here we are in a task. _simApi.OnConsoleLog is a delegate.
                        // We should be careful not to overwrite it if main thread uses it, but disabled buttons.
                        var oldLog = _simApi.OnConsoleLog;
                        _simApi.OnConsoleLog = logAction;

                        try
                        {
                            var sw = System.Diagnostics.Stopwatch.StartNew();

                            // Initialize Session
                            string? gpuDeviceName = null;
                            if (useGpu)
                            {
                                Invoke(new Action(() => gpuDeviceName = comboBox_GPUIndex.SelectedItem as string));
                            }

                            var filters = new DisplayFilters();

                            _simApi.CreateSession(useGpu, gpuDeviceName, filters);
                            _simApi.InitializeSimulation(cfg);
                            _simApi.InitializeLiveConfig(cfg);

                            // Force enable spectrum logging for comparison
                            _simApi.SpectrumLoggingEnabled = true;

                            int totalEvents = cfg.TotalSteps * cfg.NodeCount;
                            var cts = new CancellationTokenSource();

                            // Run Simulation
                            _simApi.RunParallelEventBasedLoop(cts.Token, totalEvents, useParallel: true, useGpu: useGpu);

                            sw.Stop();
                            stats.TotalTimeMs = sw.ElapsedMilliseconds;
                            stats.TotalSteps = _simApi.LiveTotalSteps;

                            // Capture metrics
                            stats.FinalExcited = _simApi.LiveExcited;
                            stats.FinalHeavyMass = _simApi.LiveHeavyMass;
                            stats.FinalLargestCluster = _simApi.LiveLargestCluster;
                            stats.FinalEnergy = _simApi.LiveQNorm;
                            stats.FinalSpectralDimension = _simApi.LiveSpectralDim;
                            stats.SpectralDimensionHistory = _simApi.SeriesSpectralDimension.ToList();
                        }
                        catch (Exception ex)
                        {
                            logAction($"Error in {stats.DeviceType} run: {ex.Message}");
                        }
                        finally
                        {
                            _simApi.OnConsoleLog = oldLog;
                            _simApi.Cleanup();
                        }

                        return stats;
                    });
                });

                // Update UI with report
                BeginInvoke(new Action(() =>
                {
                    if (synthesisTextBox != null)
                    {
                        synthesisTextBox.Text = report;
                        // Switch to Synthesis tab to show results
                        tabControl_Main.SelectedTab = tabPage_Sythnesis;
                    }
                }));
            }
            catch (Exception ex)
            {
                LogStatistics.LogCPU($"Comparison Error: {ex.Message}");
            }
            finally
            {
                BeginInvoke(new Action(() =>
                {
                    button_CPUtoGPUCompare.Enabled = true;
                    button_RunModernSim.Enabled = true;
                }));
            }
        });
    }
}

