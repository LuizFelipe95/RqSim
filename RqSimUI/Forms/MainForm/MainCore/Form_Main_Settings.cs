namespace RqSimForms;

/// <summary>
/// Partial class for Form_Main - Settings persistence methods.
/// Handles loading settings on startup and saving on close.
/// </summary>
partial class Form_Main
{
    private FormSettings? _formSettings;

    /// <summary>
    /// Load saved settings and apply to UI controls.
    /// Call this at the END of Form_Main_Load after all controls are initialized.
    /// </summary>


    private static void SetNumericValueSafe(NumericUpDown? control, decimal value)
    {
        if (control is null) return;
        
        var originalMinimum = control.Minimum;
        var originalMaximum = control.Maximum;

        try
        {
            // Round value to match DecimalPlaces setting to avoid validation errors
            if (control.DecimalPlaces >= 0)
            {
                value = Math.Round(value, control.DecimalPlaces, MidpointRounding.AwayFromZero);
            }

            // Временно расширяем диапазон, чтобы Value присвоился без ArgumentOutOfRangeException,
            // даже если Designer/другой код позже пересчитает Minimum/Maximum.
            control.Minimum = decimal.MinValue;
            control.Maximum = decimal.MaxValue;

            control.Value = value;
        }
        catch (ArgumentOutOfRangeException)
        {
            // На всякий случай (например, NaN/Infinity через конверсию),
            // выбираем безопасный дефолт.
            control.Value = originalMinimum;
        }
        catch (Exception)
        {
            // Catch any other exceptions and use minimum as safe default
            control.Value = originalMinimum;
        }
        finally
        {
            control.Minimum = originalMinimum;
            control.Maximum = originalMaximum;

            // Финальный кламп под реальный диапазон.
            var clamped = Math.Clamp(control.Value, control.Minimum, control.Maximum);
            if (control.Value != clamped)
                control.Value = clamped;
        }
    }


    private void LoadAndApplySettings()
    {
        _formSettings = FormSettings.Load();

        try
        {
            // === Simulation Parameters ===
            SetNumericValueSafe(numNodeCount, _formSettings.NodeCount);
            SetNumericValueSafe(numTargetDegree, _formSettings.TargetDegree);
            SetNumericValueSafe(numInitialExcitedProb, (decimal)_formSettings.InitialExcitedProb);
            SetNumericValueSafe(numLambdaState, (decimal)_formSettings.LambdaState);
            SetNumericValueSafe(numTemperature, (decimal)_formSettings.Temperature);
            SetNumericValueSafe(numEdgeTrialProb, (decimal)_formSettings.EdgeTrialProb);
            SetNumericValueSafe(numMeasurementThreshold, (decimal)_formSettings.MeasurementThreshold);
            SetNumericValueSafe(numTotalSteps, _formSettings.TotalSteps);
            SetNumericValueSafe(numFractalLevels, _formSettings.FractalLevels);
            SetNumericValueSafe(numFractalBranchFactor, _formSettings.FractalBranchFactor);
            checkBox_AutoTuning.Checked = _formSettings.AutoTuning;

            // === Physics Constants ===
            SetNumericValueSafe(numInitialEdgeProb, (decimal)_formSettings.InitialEdgeProb);
            SetNumericValueSafe(numGravitationalCoupling, (decimal)_formSettings.GravitationalCoupling);
            SetNumericValueSafe(numVacuumEnergyScale, (decimal)_formSettings.VacuumEnergyScale);
            SetNumericValueSafe(numDecoherenceRate, (decimal)_formSettings.DecoherenceRate);
            SetNumericValueSafe(numHotStartTemperature, (decimal)_formSettings.HotStartTemperature);
            SetNumericValueSafe(numAdaptiveThresholdSigma, (decimal)_formSettings.AdaptiveThresholdSigma);
            SetNumericValueSafe(numWarmupDuration, _formSettings.WarmupDuration);
            SetNumericValueSafe(numGravityTransitionDuration, (decimal)_formSettings.GravityTransitionDuration);

            // === GPU Settings ===
            checkBox_EnableGPU.Checked = _formSettings.EnableGPU;
            checkBox_UseMultiGPU.Checked = _formSettings.UseMultiGPU;
            if (comboBox_GPUComputeEngine.Items.Count > _formSettings.GPUComputeEngineIndex)
                comboBox_GPUComputeEngine.SelectedIndex = _formSettings.GPUComputeEngineIndex;
            if (comboBox_GPUIndex.Items.Count > _formSettings.GPUIndex)
                comboBox_GPUIndex.SelectedIndex = _formSettings.GPUIndex;

            // === UI Settings ===
            SetNumericValueSafe(numericUpDown_MaxFPS, _formSettings.MaxFPS);
            SetNumericValueSafe(numericUpDown1, _formSettings.CPUThreads);
            checkBox_AutoScrollSysConsole.Checked = _formSettings.AutoScrollConsole;
            chkShowHeavyOnly.Checked = _formSettings.ShowHeavyOnly;
            if (cmbWeightThreshold.Items.Count > _formSettings.WeightThresholdIndex)
                cmbWeightThreshold.SelectedIndex = _formSettings.WeightThresholdIndex;
            if (comboBox_Presets.Items.Count > _formSettings.PresetIndex)
                comboBox_Presets.SelectedIndex = _formSettings.PresetIndex;
            if (comboBox_Experiments.Items.Count > _formSettings.ExperimentIndex)
                comboBox_Experiments.SelectedIndex = _formSettings.ExperimentIndex;

            // === Mode Settings (Science Mode) ===
            if (checkBox_ScienceSimMode is not null)
            {
                checkBox_ScienceSimMode.Checked = _formSettings.ScienceMode;
            }
            _scienceModeEnabled = _formSettings.ScienceMode;
            _useOllivierRicci = _formSettings.UseOllivierRicciCurvature;
            _enableConservation = _formSettings.EnableConservationValidation;
            _useGpuAnisotropy = _formSettings.UseGpuAnisotropy;

            // === Window State ===
            if (!_formSettings.IsMaximized)
            {
                var screen = Screen.FromPoint(new Point(_formSettings.WindowX, _formSettings.WindowY));
                if (screen.WorkingArea.Contains(_formSettings.WindowX, _formSettings.WindowY))
                {
                    StartPosition = FormStartPosition.Manual;
                    Location = new Point(_formSettings.WindowX, _formSettings.WindowY);
                }
                Width = Math.Max(800, _formSettings.WindowWidth);
                Height = Math.Max(600, _formSettings.WindowHeight);
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }

            if (tabControl_Main.TabCount > _formSettings.SelectedTabIndex)
                tabControl_Main.SelectedIndex = _formSettings.SelectedTabIndex;

            RestoreBackgroundPluginsFromSettings();
        }
        catch (Exception ex)
        {
            AppendSysConsole($"[Settings] Warning: Could not fully restore settings: {ex.Message}\n");
        }
    }

    /*
        private void LoadAndApplySettings()
        {
            _formSettings = FormSettings.Load();

            try
            {
                // === Simulation Parameters ===
                SetNumericValueSafe(numNodeCount, _formSettings.NodeCount);
                SetNumericValueSafe(numTargetDegree, _formSettings.TargetDegree);
                SetNumericValueSafe(numInitialExcitedProb, (decimal)_formSettings.InitialExcitedProb);
                SetNumericValueSafe(numLambdaState, (decimal)_formSettings.LambdaState);
                SetNumericValueSafe(numTemperature, (decimal)_formSettings.Temperature);
                SetNumericValueSafe(numEdgeTrialProb, (decimal)_formSettings.EdgeTrialProb);
                SetNumericValueSafe(numMeasurementThreshold, (decimal)_formSettings.MeasurementThreshold);
                SetNumericValueSafe(numTotalSteps, _formSettings.TotalSteps);
                SetNumericValueSafe(numFractalLevels, _formSettings.FractalLevels);
                SetNumericValueSafe(numFractalBranchFactor, _formSettings.FractalBranchFactor);
                checkBox_AutoTuning.Checked = _formSettings.AutoTuning;

                // === Physics Constants ===
                SetNumericValueSafe(numInitialEdgeProb, (decimal)_formSettings.InitialEdgeProb);
                SetNumericValueSafe(numGravitationalCoupling, (decimal)_formSettings.GravitationalCoupling);
                SetNumericValueSafe(numVacuumEnergyScale, (decimal)_formSettings.VacuumEnergyScale);
                SetNumericValueSafe(numDecoherenceRate, (decimal)_formSettings.DecoherenceRate);
                SetNumericValueSafe(numHotStartTemperature, (decimal)_formSettings.HotStartTemperature);
                SetNumericValueSafe(numAdaptiveThresholdSigma, (decimal)_formSettings.AdaptiveThresholdSigma);
                SetNumericValueSafe(numWarmupDuration, _formSettings.WarmupDuration);
                SetNumericValueSafe(numGravityTransitionDuration, (decimal)_formSettings.GravityTransitionDuration);

                SetNumericValueSafe(numericUpDown_MaxFPS, _formSettings.MaxFPS);
                SetNumericValueSafe(numericUpDown1, _formSettings.CPUThreads);

                // === Physics Modules ===
                chkQuantumDriven.Checked = _formSettings.UseQuantumDriven;
                chkSpacetimePhysics.Checked = _formSettings.UseSpacetimePhysics;
                chkSpinorField.Checked = _formSettings.UseSpinorField;
                chkVacuumFluctuations.Checked = _formSettings.UseVacuumFluctuations;
                chkBlackHolePhysics.Checked = _formSettings.UseBlackHolePhysics;
                chkYangMillsGauge.Checked = _formSettings.UseYangMillsGauge;
                chkEnhancedKleinGordon.Checked = _formSettings.UseEnhancedKleinGordon;
                chkInternalTime.Checked = _formSettings.UseInternalTime;
                chkSpectralGeometry.Checked = _formSettings.UseSpectralGeometry;
                chkQuantumGraphity.Checked = _formSettings.UseQuantumGraphity;
                chkRelationalTime.Checked = _formSettings.UseRelationalTime;
                chkRelationalYangMills.Checked = _formSettings.UseRelationalYangMills;
                chkNetworkGravity.Checked = _formSettings.UseNetworkGravity;
                chkUnifiedPhysicsStep.Checked = _formSettings.UseUnifiedPhysicsStep;
                chkEnforceGaugeConstraints.Checked = _formSettings.UseEnforceGaugeConstraints;
                chkCausalRewiring.Checked = _formSettings.UseCausalRewiring;
                chkTopologicalProtection.Checked = _formSettings.UseTopologicalProtection;
                chkValidateEnergyConservation.Checked = _formSettings.UseValidateEnergyConservation;
                chkMexicanHatPotential.Checked = _formSettings.UseMexicanHatPotential;
                chkGeometryMomenta.Checked = _formSettings.UseGeometryMomenta;
                chkTopologicalCensorship.Checked = _formSettings.UseTopologicalCensorship;

                // === GPU Settings ===
                checkBox_EnableGPU.Checked = _formSettings.EnableGPU;
                checkBox_UseMultiGPU.Checked = _formSettings.UseMultiGPU;
                if (comboBox_GPUComputeEngine.Items.Count > _formSettings.GPUComputeEngineIndex)
                    comboBox_GPUComputeEngine.SelectedIndex = _formSettings.GPUComputeEngineIndex;
                if (comboBox_GPUIndex.Items.Count > _formSettings.GPUIndex)
                    comboBox_GPUIndex.SelectedIndex = _formSettings.GPUIndex;

                // === UI Settings ===
                numericUpDown_MaxFPS.Value = Math.Clamp(_formSettings.MaxFPS, (int)numericUpDown_MaxFPS.Minimum, (int)numericUpDown_MaxFPS.Maximum);
                numericUpDown1.Value = Math.Clamp(_formSettings.CPUThreads, (int)numericUpDown1.Minimum, (int)numericUpDown1.Maximum);
                checkBox_AutoScrollSysConsole.Checked = _formSettings.AutoScrollConsole;
                chkShowHeavyOnly.Checked = _formSettings.ShowHeavyOnly;
                if (cmbWeightThreshold.Items.Count > _formSettings.WeightThresholdIndex)
                    cmbWeightThreshold.SelectedIndex = _formSettings.WeightThresholdIndex;
                if (comboBox_Presets.Items.Count > _formSettings.PresetIndex)
                    comboBox_Presets.SelectedIndex = _formSettings.PresetIndex;
                if (comboBox_Experiments.Items.Count > _formSettings.ExperimentIndex)
                    comboBox_Experiments.SelectedIndex = _formSettings.ExperimentIndex;

                // === Window State ===
                if (!_formSettings.IsMaximized)
                {
                    // Only restore position if within screen bounds
                    var screen = Screen.FromPoint(new Point(_formSettings.WindowX, _formSettings.WindowY));
                    if (screen.WorkingArea.Contains(_formSettings.WindowX, _formSettings.WindowY))
                    {
                        StartPosition = FormStartPosition.Manual;
                        Location = new Point(_formSettings.WindowX, _formSettings.WindowY);
                    }
                    Width = Math.Max(800, _formSettings.WindowWidth);
                    Height = Math.Max(600, _formSettings.WindowHeight);
                }
                else
                {
                    WindowState = FormWindowState.Maximized;
                }

                if (tabControl1.TabCount > _formSettings.SelectedTabIndex)
                    tabControl1.SelectedIndex = _formSettings.SelectedTabIndex;

                // === Restore Background Plugins ===
                RestoreBackgroundPluginsFromSettings();
            }
            catch (Exception ex)
            {
                // Log but don't fail - use defaults
                AppendSysConsole($"[Settings] Warning: Could not fully restore settings: {ex.Message}\n");
            }
        }*/

    /// <summary>
    /// Save current UI settings to file.
    /// Call this in OnFormClosing before base call.
    /// </summary>
    private void SaveCurrentSettings()
    {
        _formSettings ??= new FormSettings();

        try
        {
            // === Simulation Parameters ===
            _formSettings.NodeCount = (int)numNodeCount.Value;
            _formSettings.TargetDegree = (int)numTargetDegree.Value;
            _formSettings.InitialExcitedProb = (double)numInitialExcitedProb.Value;
            _formSettings.LambdaState = (double)numLambdaState.Value;
            _formSettings.Temperature = (double)numTemperature.Value;
            _formSettings.EdgeTrialProb = (double)numEdgeTrialProb.Value;
            _formSettings.MeasurementThreshold = (double)numMeasurementThreshold.Value;
            _formSettings.TotalSteps = (int)numTotalSteps.Value;
            _formSettings.FractalLevels = (int)numFractalLevels.Value;
            _formSettings.FractalBranchFactor = (int)numFractalBranchFactor.Value;
            _formSettings.AutoTuning = checkBox_AutoTuning.Checked;

            // === Physics Constants ===
            _formSettings.InitialEdgeProb = (double)numInitialEdgeProb.Value;
            _formSettings.GravitationalCoupling = (double)numGravitationalCoupling.Value;
            _formSettings.VacuumEnergyScale = (double)numVacuumEnergyScale.Value;
            _formSettings.DecoherenceRate = (double)numDecoherenceRate.Value;
            _formSettings.HotStartTemperature = (double)numHotStartTemperature.Value;
            _formSettings.AdaptiveThresholdSigma = (double)numAdaptiveThresholdSigma.Value;
            _formSettings.WarmupDuration = (int)numWarmupDuration.Value;
            _formSettings.GravityTransitionDuration = (double)numGravityTransitionDuration.Value;

            // === Physics Modules ===
            _formSettings.UseQuantumDriven = chkQuantumDriven.Checked;
            _formSettings.UseSpacetimePhysics = chkSpacetimePhysics.Checked;
            _formSettings.UseSpinorField = chkSpinorField.Checked;
            _formSettings.UseVacuumFluctuations = chkVacuumFluctuations.Checked;
            _formSettings.UseBlackHolePhysics = chkBlackHolePhysics.Checked;
            _formSettings.UseYangMillsGauge = chkYangMillsGauge.Checked;
            _formSettings.UseEnhancedKleinGordon = chkEnhancedKleinGordon.Checked;
            _formSettings.UseInternalTime = chkInternalTime.Checked;
            _formSettings.UseSpectralGeometry = chkSpectralGeometry.Checked;
            _formSettings.UseQuantumGraphity = chkQuantumGraphity.Checked;
            _formSettings.UseRelationalTime = chkRelationalTime.Checked;
            _formSettings.UseRelationalYangMills = chkRelationalYangMills.Checked;
            _formSettings.UseNetworkGravity = chkNetworkGravity.Checked;
            _formSettings.UseUnifiedPhysicsStep = chkUnifiedPhysicsStep.Checked;
            _formSettings.UseEnforceGaugeConstraints = chkEnforceGaugeConstraints.Checked;
            _formSettings.UseCausalRewiring = chkCausalRewiring.Checked;
            _formSettings.UseTopologicalProtection = chkTopologicalProtection.Checked;
            _formSettings.UseValidateEnergyConservation = chkValidateEnergyConservation.Checked;
            _formSettings.UseMexicanHatPotential = chkMexicanHatPotential.Checked;
            _formSettings.UseGeometryMomenta = chkGeometryMomenta.Checked;
            _formSettings.UseTopologicalCensorship = chkTopologicalCensorship.Checked;

            // === GPU Settings ===
            _formSettings.EnableGPU = checkBox_EnableGPU.Checked;
            _formSettings.UseMultiGPU = checkBox_UseMultiGPU.Checked;
            _formSettings.GPUComputeEngineIndex = comboBox_GPUComputeEngine.SelectedIndex >= 0 ? comboBox_GPUComputeEngine.SelectedIndex : 0;
            _formSettings.GPUIndex = comboBox_GPUIndex.SelectedIndex >= 0 ? comboBox_GPUIndex.SelectedIndex : 0;

            // === UI Settings ===
            _formSettings.MaxFPS = (int)numericUpDown_MaxFPS.Value;
            _formSettings.CPUThreads = (int)numericUpDown1.Value;
            _formSettings.AutoScrollConsole = checkBox_AutoScrollSysConsole.Checked;
            _formSettings.ShowHeavyOnly = chkShowHeavyOnly.Checked;
            _formSettings.WeightThresholdIndex = cmbWeightThreshold.SelectedIndex >= 0 ? cmbWeightThreshold.SelectedIndex : 0;
            _formSettings.PresetIndex = comboBox_Presets.SelectedIndex >= 0 ? comboBox_Presets.SelectedIndex : 0;
            _formSettings.ExperimentIndex = comboBox_Experiments.SelectedIndex >= 0 ? comboBox_Experiments.SelectedIndex : 0;

            // === Mode Settings (Science Mode) ===
            _formSettings.ScienceMode = checkBox_ScienceSimMode?.Checked ?? false;
            _formSettings.UseOllivierRicciCurvature = _useOllivierRicci;
            _formSettings.EnableConservationValidation = _enableConservation;
            _formSettings.UseGpuAnisotropy = _useGpuAnisotropy;

            // === Window State ===
            _formSettings.IsMaximized = WindowState == FormWindowState.Maximized;
            if (!_formSettings.IsMaximized)
            {
                _formSettings.WindowWidth = Width;
                _formSettings.WindowHeight = Height;
                _formSettings.WindowX = Location.X;
                _formSettings.WindowY = Location.Y;
            }
            _formSettings.SelectedTabIndex = tabControl_Main.SelectedIndex;

            // === Background Plugins ===
            _formSettings.EnabledBackgroundPlugins.Clear();
            _formSettings.PluginGpuAssignments.Clear();
            _formSettings.PluginKernelCounts.Clear();
            foreach (var plugin in _activeBackgroundPlugins)
            {
                string typeName = plugin.PluginType.FullName ?? plugin.PluginType.Name;
                _formSettings.EnabledBackgroundPlugins.Add(typeName);
                _formSettings.PluginGpuAssignments[typeName] = plugin.GpuIndex;
                _formSettings.PluginKernelCounts[typeName] = plugin.KernelCount;
            }

            _formSettings.Save();
        }
        catch
        {
            // Silently fail - settings persistence is not critical
        }
    }
}
