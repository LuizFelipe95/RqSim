namespace RqSimForms;

partial class Form_Main : Form
{
    private void InitializeComponent()
    {
        ListViewGroup listViewGroup1 = new ListViewGroup("GPU", HorizontalAlignment.Left);
        ListViewGroup listViewGroup2 = new ListViewGroup("Top Events", HorizontalAlignment.Left);
        tabControl_Main = new TabControl();
        tabPage_Summary = new TabPage();
        groupBox_MultiGpu_Settings = new GroupBox();
        button_RemoveGpuBackgroundPluginToPipeline = new Button();
        button_AddGpuBackgroundPluginToPipeline = new Button();
        label_BackgroundPipelineGPU = new Label();
        comboBox_BackgroundPipelineGPU = new ComboBox();
        label_BackgroundPipelineGPU_Kernels = new Label();
        numericUpDown_BackgroundPluginGPUKernels = new NumericUpDown();
        listView_AnaliticsGPU = new ListView();
        columnHeader_GPU = new ColumnHeader();
        columnHeader_Algorithm = new ColumnHeader();
        columnHeader_GPUKernels = new ColumnHeader();
        label_RenderingGPU = new Label();
        checkBox_UseMultiGPU = new CheckBox();
        label_MultiGPU_ActivePhysxGPU = new Label();
        label_GPUMode = new Label();
        checkBox_EnableGPU = new CheckBox();
        comboBox_GPUComputeEngine = new ComboBox();
        comboBox_GPUIndex = new ComboBox();
        comboBox_MultiGpu_PhysicsGPU = new ComboBox();
        grpLiveMetrics = new GroupBox();
        tlpLiveMetrics = new TableLayoutPanel();
        lblGlobalNbr = new Label();
        valGlobalNbr = new Label();
        lblGlobalSpont = new Label();
        valGlobalSpont = new Label();
        lblStrongEdges = new Label();
        valStrongEdges = new Label();
        lblLargestCluster = new Label();
        valLargestCluster = new Label();
        lblHeavyMass = new Label();
        valHeavyMass = new Label();
        lblSpectrumInfo = new Label();
        valSpectrumInfo = new Label();
        lblLightSpeed = new Label();
        valLightSpeed = new Label();
        grpRunStats = new GroupBox();
        tlpRunStats = new TableLayoutPanel();
        lblExcitedAvg = new Label();
        valExcitedAvg = new Label();
        lblExcitedMax = new Label();
        valExcitedMax = new Label();
        lblAvalancheCount = new Label();
        valAvalancheCount = new Label();
        lblMeasurementStatus = new Label();
        valMeasurementStatus = new Label();
        valCurrentStep = new Label();
        valTotalSteps = new Label();
        lblTotalSteps = new Label();
        lblCurrentStep = new Label();
        grpDashboard = new GroupBox();
        tlpDashboard = new TableLayoutPanel();
        lblDashNodes = new Label();
        valDashNodes = new Label();
        lblDashTotalSteps = new Label();
        valDashTotalSteps = new Label();
        lblDashCurrentStep = new Label();
        valDashCurrentStep = new Label();
        lblDashExcited = new Label();
        valDashExcited = new Label();
        lblDashHeavyMass = new Label();
        valDashHeavyMass = new Label();
        lblDashLargestCluster = new Label();
        valDashLargestCluster = new Label();
        lblDashStrongEdges = new Label();
        valDashStrongEdges = new Label();
        lblDashPhase = new Label();
        valDashPhase = new Label();
        lblDashQNorm = new Label();
        valDashQNorm = new Label();
        lblDashEntanglement = new Label();
        valDashEntanglement = new Label();
        lblDashCorrelation = new Label();
        valDashCorrelation = new Label();
        lblDashStatus = new Label();
        valDashStatus = new Label();
        lblDashSpectralDim = new Label();
        valDashSpectralDim = new Label();
        lblDashEffectiveG = new Label();
        valDashEffectiveG = new Label();
        lblDashGSuppression = new Label();
        valDashGSuppression = new Label();
        lblDashNetworkTemp = new Label();
        valDashNetworkTemp = new Label();
        grpEvents = new GroupBox();
        buttonTopEvents_Refresh = new Button();
        checkBox_TopEvents_LiveUpdate = new CheckBox();
        comboBox_TopEvents_EventType = new ComboBox();
        buttonTopEvents_Clear = new Button();
        listView_TopEvents = new ListView();
        columnHeader_TopEventType = new ColumnHeader();
        columnHeader_TopEventDecs = new ColumnHeader();
        columnHeader_TopEventParams = new ColumnHeader();
        buttonTopEvents_SaveJson = new Button();
        lvEvents = new ListView();
        colEventStep = new ColumnHeader();
        colEventType = new ColumnHeader();
        colEventDetail = new ColumnHeader();
        splitpanels_Add = new SplitContainer();
        label_ParamPresets = new Label();
        comboBox_Presets = new ComboBox();
        lblExperiments = new Label();
        comboBox_Experiments = new ComboBox();
        chkShowHeavyOnly = new CheckBox();
        btnSnapshotImage = new Button();
        button_CPUtoGPUCompare = new Button();
        button_ForceRedrawGraphImage = new Button();
        button_SaveSimReult = new Button();
        cmbWeightThreshold = new ComboBox();
        tabPage_Settings = new TabPage();
        settingsMainLayout = new TableLayoutPanel();
        grpPhysicsModules = new GroupBox();
        flpPhysics = new FlowLayoutPanel();
        chkQuantumDriven = new CheckBox();
        chkSpacetimePhysics = new CheckBox();
        chkSpinorField = new CheckBox();
        chkVacuumFluctuations = new CheckBox();
        chkBlackHolePhysics = new CheckBox();
        chkYangMillsGauge = new CheckBox();
        chkEnhancedKleinGordon = new CheckBox();
        chkInternalTime = new CheckBox();
        chkSpectralGeometry = new CheckBox();
        chkQuantumGraphity = new CheckBox();
        chkRelationalTime = new CheckBox();
        chkRelationalYangMills = new CheckBox();
        chkNetworkGravity = new CheckBox();
        chkUnifiedPhysicsStep = new CheckBox();
        chkEnforceGaugeConstraints = new CheckBox();
        chkCausalRewiring = new CheckBox();
        chkTopologicalProtection = new CheckBox();
        chkValidateEnergyConservation = new CheckBox();
        chkMexicanHatPotential = new CheckBox();
        chkGeometryMomenta = new CheckBox();
        chkTopologicalCensorship = new CheckBox();
        grpPhysicsConstants = new GroupBox();
        tlpPhysicsConstants = new TableLayoutPanel();
        numGravityTransitionDuration = new NumericUpDown();
        lblInitialEdgeProb = new Label();
        numInitialEdgeProb = new NumericUpDown();
        lblGravitationalCoupling = new Label();
        numGravitationalCoupling = new NumericUpDown();
        lblVacuumEnergyScale = new Label();
        numVacuumEnergyScale = new NumericUpDown();
        numDecoherenceRate = new NumericUpDown();
        numHotStartTemperature = new NumericUpDown();
        lblDecoherenceRate = new Label();
        lblHotStartTemperature = new Label();
        lblAdaptiveThresholdSigma = new Label();
        numAdaptiveThresholdSigma = new NumericUpDown();
        lblWarmupDuration = new Label();
        numWarmupDuration = new NumericUpDown();
        lblGravityTransitionDuration = new Label();
        valAnnealingTimeConstant = new Label();
        grpSimParams = new GroupBox();
        tlpSimParams = new TableLayoutPanel();
        lblNodeCount = new Label();
        numNodeCount = new NumericUpDown();
        lblTargetDegree = new Label();
        numTargetDegree = new NumericUpDown();
        lblInitialExcitedProb = new Label();
        numInitialExcitedProb = new NumericUpDown();
        lblLambdaState = new Label();
        numLambdaState = new NumericUpDown();
        lblTemperature = new Label();
        numTemperature = new NumericUpDown();
        lblEdgeTrialProb = new Label();
        numEdgeTrialProb = new NumericUpDown();
        lblMeasurementThreshold = new Label();
        numMeasurementThreshold = new NumericUpDown();
        lblTotalStepsSettings = new Label();
        numTotalSteps = new NumericUpDown();
        lblFractalLevels = new Label();
        numFractalLevels = new NumericUpDown();
        numFractalBranchFactor = new NumericUpDown();
        lblFractalBranchFactor = new Label();
        tabPage_UniPipelineState = new TabPage();
        _tlp_UniPipeline_Main = new TableLayoutPanel();
        _tlpLeft = new TableLayoutPanel();
        _dgvModules = new DataGridView();
        _colEnabled = new DataGridViewCheckBoxColumn();
        _colName = new DataGridViewTextBoxColumn();
        _colCategory = new DataGridViewTextBoxColumn();
        _colStage = new DataGridViewTextBoxColumn();
        _colType = new DataGridViewTextBoxColumn();
        _colPriority = new DataGridViewTextBoxColumn();
        _colModuleGroup = new DataGridViewTextBoxColumn();
        _flpButtons = new FlowLayoutPanel();
        _btnMoveUp = new Button();
        _btnMoveDown = new Button();
        _btnRemove = new Button();
        _btnLoadDll = new Button();
        _btnAddBuiltIn = new Button();
        _btnSaveConfig = new Button();
        _btnLoadConfig = new Button();
        _grpProperties = new GroupBox();
        _tlpProperties = new TableLayoutPanel();
        _lblModuleName = new Label();
        _txtModuleName = new TextBox();
        _lblDescription = new Label();
        _txtDescription = new TextBox();
        _lblExecutionType = new Label();
        _cmbExecutionType = new ComboBox();
        _flpGpuTopologySettings = new FlowLayoutPanel();
        label_GpuEngineUniPipeline = new Label();
        comboBox_GpuEngineUniPipeline = new ComboBox();
        label_TopologyMode = new Label();
        comboBox_TopologyMode = new ComboBox();
        checkBox_ScienceSimMode = new CheckBox();
        _flpDialogButtons = new FlowLayoutPanel();
        tabPage_GUI = new TabPage();
        tabPage_Charts = new TabPage();
        tlpCharts = new TableLayoutPanel();
        grpChartExcited = new GroupBox();
        grpChartHeavy = new GroupBox();
        grpChartCluster = new GroupBox();
        grpChartEnergy = new GroupBox();
        tabPage_Console = new TabPage();
        summaryTextBox = new TextBox();
        textBox_HostSessionErrors = new TextBox();
        checkBox_SysConsole_LiveUpdate = new CheckBox();
        checkBox_SimConsole_LiveUpdate = new CheckBox();
        button_SimConsole_Refresh = new Button();
        comboBox_SimConsole_OutType = new ComboBox();
        button_SimConsole_Clear = new Button();
        button_SimConsole_CopyToClipboard = new Button();
        checkBox_AutoScrollSimConsole = new CheckBox();
        button_SysConsole_Refresh = new Button();
        comboBox_SysConsole_OutType = new ComboBox();
        button_SysConsole_Clear = new Button();
        button_SysConsole_CopyToClipboard = new Button();
        checkBox_AutoScrollSysConsole = new CheckBox();
        textBox_SimConsole = new TextBox();
        textBox_SysConsole = new TextBox();
        tabPage_Sythnesis = new TabPage();
        tabPage_Experiments = new TabPage();
        tabPage_3DVisual = new TabPage();
        tabPage_3DVisualCSR = new TabPage();
        _btnCancel = new Button();
        _btnApply = new Button();
        _btnOK = new Button();
        button_ApplyPipelineConfSet = new Button();
        btnExpornShortJson = new Button();
        checkBox_AutoTuning = new CheckBox();
        label_MaxFPS = new Label();
        numericUpDown_MaxFPS = new NumericUpDown();
        label_CPUThreads = new Label();
        numericUpDown1 = new NumericUpDown();
        modernSimTextBox = new TextBox();
        button_RunModernSim = new Button();
        button_Plugins = new Button();
        button_BindConsoleSession = new Button();
        checkBox_StanaloneDX12Form = new CheckBox();
        button_TerminateSimSession = new Button();
        statusStrip1 = new StatusStrip();
        statusLabelSteps = new ToolStripStatusLabel();
        statusLabelHeavyMass = new ToolStripStatusLabel();
        statusLabelExcited = new ToolStripStatusLabel();
        tabControl_Main.SuspendLayout();
        tabPage_Summary.SuspendLayout();
        groupBox_MultiGpu_Settings.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDown_BackgroundPluginGPUKernels).BeginInit();
        grpLiveMetrics.SuspendLayout();
        tlpLiveMetrics.SuspendLayout();
        grpRunStats.SuspendLayout();
        tlpRunStats.SuspendLayout();
        grpDashboard.SuspendLayout();
        tlpDashboard.SuspendLayout();
        grpEvents.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitpanels_Add).BeginInit();
        splitpanels_Add.Panel1.SuspendLayout();
        splitpanels_Add.SuspendLayout();
        tabPage_Settings.SuspendLayout();
        settingsMainLayout.SuspendLayout();
        grpPhysicsModules.SuspendLayout();
        flpPhysics.SuspendLayout();
        grpPhysicsConstants.SuspendLayout();
        tlpPhysicsConstants.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numGravityTransitionDuration).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numInitialEdgeProb).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numGravitationalCoupling).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numVacuumEnergyScale).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numDecoherenceRate).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numHotStartTemperature).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numAdaptiveThresholdSigma).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numWarmupDuration).BeginInit();
        grpSimParams.SuspendLayout();
        tlpSimParams.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numNodeCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTargetDegree).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numInitialExcitedProb).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numLambdaState).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTemperature).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numEdgeTrialProb).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numMeasurementThreshold).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTotalSteps).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numFractalLevels).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numFractalBranchFactor).BeginInit();
        tabPage_UniPipelineState.SuspendLayout();
        _tlp_UniPipeline_Main.SuspendLayout();
        _tlpLeft.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_dgvModules).BeginInit();
        _flpButtons.SuspendLayout();
        _grpProperties.SuspendLayout();
        _tlpProperties.SuspendLayout();
        _flpGpuTopologySettings.SuspendLayout();
        tabPage_Charts.SuspendLayout();
        tlpCharts.SuspendLayout();
        tabPage_Console.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDown_MaxFPS).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
        statusStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // tabControl_Main
        // 
        tabControl_Main.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        tabControl_Main.Controls.Add(tabPage_Summary);
        tabControl_Main.Controls.Add(tabPage_Settings);
        tabControl_Main.Controls.Add(tabPage_UniPipelineState);
        tabControl_Main.Controls.Add(tabPage_GUI);
        tabControl_Main.Controls.Add(tabPage_Charts);
        tabControl_Main.Controls.Add(tabPage_Console);
        tabControl_Main.Controls.Add(tabPage_Sythnesis);
        tabControl_Main.Controls.Add(tabPage_Experiments);
        tabControl_Main.Controls.Add(tabPage_3DVisual);
        tabControl_Main.Controls.Add(tabPage_3DVisualCSR);
        tabControl_Main.Location = new Point(-3, 41);
        tabControl_Main.Name = "tabControl_Main";
        tabControl_Main.SelectedIndex = 0;
        tabControl_Main.Size = new Size(1350, 753);
        tabControl_Main.TabIndex = 0;
        // 
        // tabPage_Summary
        // 
        tabPage_Summary.Controls.Add(groupBox_MultiGpu_Settings);
        tabPage_Summary.Controls.Add(grpLiveMetrics);
        tabPage_Summary.Controls.Add(grpRunStats);
        tabPage_Summary.Controls.Add(grpDashboard);
        tabPage_Summary.Controls.Add(grpEvents);
        tabPage_Summary.Controls.Add(splitpanels_Add);
        tabPage_Summary.Location = new Point(4, 24);
        tabPage_Summary.Name = "tabPage_Summary";
        tabPage_Summary.Size = new Size(1342, 725);
        tabPage_Summary.TabIndex = 12;
        tabPage_Summary.Text = "Main Control";
        tabPage_Summary.UseVisualStyleBackColor = true;
        // 
        // groupBox_MultiGpu_Settings
        // 
        groupBox_MultiGpu_Settings.Controls.Add(button_RemoveGpuBackgroundPluginToPipeline);
        groupBox_MultiGpu_Settings.Controls.Add(button_AddGpuBackgroundPluginToPipeline);
        groupBox_MultiGpu_Settings.Controls.Add(label_BackgroundPipelineGPU);
        groupBox_MultiGpu_Settings.Controls.Add(comboBox_BackgroundPipelineGPU);
        groupBox_MultiGpu_Settings.Controls.Add(label_BackgroundPipelineGPU_Kernels);
        groupBox_MultiGpu_Settings.Controls.Add(numericUpDown_BackgroundPluginGPUKernels);
        groupBox_MultiGpu_Settings.Controls.Add(listView_AnaliticsGPU);
        groupBox_MultiGpu_Settings.Controls.Add(label_RenderingGPU);
        groupBox_MultiGpu_Settings.Controls.Add(checkBox_UseMultiGPU);
        groupBox_MultiGpu_Settings.Controls.Add(label_MultiGPU_ActivePhysxGPU);
        groupBox_MultiGpu_Settings.Controls.Add(label_GPUMode);
        groupBox_MultiGpu_Settings.Controls.Add(checkBox_EnableGPU);
        groupBox_MultiGpu_Settings.Controls.Add(comboBox_GPUComputeEngine);
        groupBox_MultiGpu_Settings.Controls.Add(comboBox_GPUIndex);
        groupBox_MultiGpu_Settings.Controls.Add(comboBox_MultiGpu_PhysicsGPU);
        groupBox_MultiGpu_Settings.Location = new Point(930, 6);
        groupBox_MultiGpu_Settings.Name = "groupBox_MultiGpu_Settings";
        groupBox_MultiGpu_Settings.Size = new Size(392, 703);
        groupBox_MultiGpu_Settings.TabIndex = 33;
        groupBox_MultiGpu_Settings.TabStop = false;
        groupBox_MultiGpu_Settings.Text = "GPU \\ Multi-GPU Settings";
        // 
        // button_RemoveGpuBackgroundPluginToPipeline
        // 
        button_RemoveGpuBackgroundPluginToPipeline.Location = new Point(126, 671);
        button_RemoveGpuBackgroundPluginToPipeline.Name = "button_RemoveGpuBackgroundPluginToPipeline";
        button_RemoveGpuBackgroundPluginToPipeline.Size = new Size(114, 22);
        button_RemoveGpuBackgroundPluginToPipeline.TabIndex = 45;
        button_RemoveGpuBackgroundPluginToPipeline.Text = "Remove Plugin";
        button_RemoveGpuBackgroundPluginToPipeline.UseVisualStyleBackColor = true;
        button_RemoveGpuBackgroundPluginToPipeline.Click += button_RemoveGpuBackgroundPluginToPipeline_Click;
        // 
        // button_AddGpuBackgroundPluginToPipeline
        // 
        button_AddGpuBackgroundPluginToPipeline.Location = new Point(6, 671);
        button_AddGpuBackgroundPluginToPipeline.Name = "button_AddGpuBackgroundPluginToPipeline";
        button_AddGpuBackgroundPluginToPipeline.Size = new Size(114, 22);
        button_AddGpuBackgroundPluginToPipeline.TabIndex = 44;
        button_AddGpuBackgroundPluginToPipeline.Text = "Add to Pipeline";
        button_AddGpuBackgroundPluginToPipeline.UseVisualStyleBackColor = true;
        button_AddGpuBackgroundPluginToPipeline.Click += button_AddGpuBackgroundPluginToPipeline_Click;
        // 
        // label_BackgroundPipelineGPU
        // 
        label_BackgroundPipelineGPU.AutoSize = true;
        label_BackgroundPipelineGPU.Location = new Point(5, 161);
        label_BackgroundPipelineGPU.Name = "label_BackgroundPipelineGPU";
        label_BackgroundPipelineGPU.Size = new Size(145, 15);
        label_BackgroundPipelineGPU.TabIndex = 43;
        label_BackgroundPipelineGPU.Text = "Background Pipeline GPU:";
        // 
        // comboBox_BackgroundPipelineGPU
        // 
        comboBox_BackgroundPipelineGPU.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_BackgroundPipelineGPU.Location = new Point(7, 179);
        comboBox_BackgroundPipelineGPU.Name = "comboBox_BackgroundPipelineGPU";
        comboBox_BackgroundPipelineGPU.Size = new Size(232, 23);
        comboBox_BackgroundPipelineGPU.TabIndex = 42;
        comboBox_BackgroundPipelineGPU.SelectedIndexChanged += comboBox_BackgroundPipelineGPU_SelectedIndexChanged;
        // 
        // label_BackgroundPipelineGPU_Kernels
        // 
        label_BackgroundPipelineGPU_Kernels.AutoSize = true;
        label_BackgroundPipelineGPU_Kernels.Location = new Point(242, 161);
        label_BackgroundPipelineGPU_Kernels.Name = "label_BackgroundPipelineGPU_Kernels";
        label_BackgroundPipelineGPU_Kernels.Size = new Size(74, 15);
        label_BackgroundPipelineGPU_Kernels.TabIndex = 41;
        label_BackgroundPipelineGPU_Kernels.Text = "GPU Kernels:";
        // 
        // numericUpDown_BackgroundPluginGPUKernels
        // 
        numericUpDown_BackgroundPluginGPUKernels.Location = new Point(245, 179);
        numericUpDown_BackgroundPluginGPUKernels.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
        numericUpDown_BackgroundPluginGPUKernels.Minimum = new decimal(new int[] { 10000, 0, 0, 0 });
        numericUpDown_BackgroundPluginGPUKernels.Name = "numericUpDown_BackgroundPluginGPUKernels";
        numericUpDown_BackgroundPluginGPUKernels.Size = new Size(131, 23);
        numericUpDown_BackgroundPluginGPUKernels.TabIndex = 40;
        numericUpDown_BackgroundPluginGPUKernels.Value = new decimal(new int[] { 10000, 0, 0, 0 });
        // 
        // listView_AnaliticsGPU
        // 
        listView_AnaliticsGPU.CheckBoxes = true;
        listView_AnaliticsGPU.Columns.AddRange(new ColumnHeader[] { columnHeader_GPU, columnHeader_Algorithm, columnHeader_GPUKernels });
        listView_AnaliticsGPU.FullRowSelect = true;
        listView_AnaliticsGPU.GridLines = true;
        listViewGroup1.Header = "GPU";
        listViewGroup1.Name = "listViewGroup_GPU";
        listView_AnaliticsGPU.Groups.AddRange(new ListViewGroup[] { listViewGroup1 });
        listView_AnaliticsGPU.Location = new Point(7, 208);
        listView_AnaliticsGPU.Name = "listView_AnaliticsGPU";
        listView_AnaliticsGPU.Size = new Size(369, 457);
        listView_AnaliticsGPU.TabIndex = 39;
        listView_AnaliticsGPU.UseCompatibleStateImageBehavior = false;
        listView_AnaliticsGPU.View = View.Details;
        // 
        // columnHeader_GPU
        // 
        columnHeader_GPU.Text = "GPU";
        // 
        // columnHeader_Algorithm
        // 
        columnHeader_Algorithm.Text = "Algorithm\\Plugin";
        columnHeader_Algorithm.Width = 200;
        // 
        // columnHeader_GPUKernels
        // 
        columnHeader_GPUKernels.Text = "Kernels\\Threads";
        // 
        // label_RenderingGPU
        // 
        label_RenderingGPU.AutoSize = true;
        label_RenderingGPU.Location = new Point(7, 25);
        label_RenderingGPU.Name = "label_RenderingGPU";
        label_RenderingGPU.Size = new Size(107, 15);
        label_RenderingGPU.TabIndex = 38;
        label_RenderingGPU.Text = "3D Rendering GPU:";
        // 
        // checkBox_UseMultiGPU
        // 
        checkBox_UseMultiGPU.AutoSize = true;
        checkBox_UseMultiGPU.Location = new Point(8, 129);
        checkBox_UseMultiGPU.Name = "checkBox_UseMultiGPU";
        checkBox_UseMultiGPU.Size = new Size(120, 19);
        checkBox_UseMultiGPU.TabIndex = 31;
        checkBox_UseMultiGPU.Text = "Multi GPU Cluster";
        // 
        // label_MultiGPU_ActivePhysxGPU
        // 
        label_MultiGPU_ActivePhysxGPU.AutoSize = true;
        label_MultiGPU_ActivePhysxGPU.Location = new Point(7, 76);
        label_MultiGPU_ActivePhysxGPU.Name = "label_MultiGPU_ActivePhysxGPU";
        label_MultiGPU_ActivePhysxGPU.Size = new Size(141, 15);
        label_MultiGPU_ActivePhysxGPU.TabIndex = 37;
        label_MultiGPU_ActivePhysxGPU.Text = "Graph atomic physx GPU:";
        // 
        // label_GPUMode
        // 
        label_GPUMode.AutoSize = true;
        label_GPUMode.Location = new Point(243, 76);
        label_GPUMode.Name = "label_GPUMode";
        label_GPUMode.Size = new Size(120, 15);
        label_GPUMode.TabIndex = 30;
        label_GPUMode.Text = "GPU Compute Mode:";
        // 
        // checkBox_EnableGPU
        // 
        checkBox_EnableGPU.AutoSize = true;
        checkBox_EnableGPU.Checked = true;
        checkBox_EnableGPU.CheckState = CheckState.Checked;
        checkBox_EnableGPU.Location = new Point(246, 41);
        checkBox_EnableGPU.Name = "checkBox_EnableGPU";
        checkBox_EnableGPU.Size = new Size(87, 19);
        checkBox_EnableGPU.TabIndex = 28;
        checkBox_EnableGPU.Text = "Enable GPU";
        // 
        // comboBox_GPUComputeEngine
        // 
        comboBox_GPUComputeEngine.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_GPUComputeEngine.Items.AddRange(new object[] { "Auto", "Original (Dense GPU)", "CSR (Sparse GPU)", "CPU Only" });
        comboBox_GPUComputeEngine.Location = new Point(245, 93);
        comboBox_GPUComputeEngine.Name = "comboBox_GPUComputeEngine";
        comboBox_GPUComputeEngine.Size = new Size(131, 23);
        comboBox_GPUComputeEngine.TabIndex = 29;
        comboBox_GPUComputeEngine.SelectedIndexChanged += comboBox_GPUComputeEngine_SelectedIndexChanged;
        // 
        // comboBox_GPUIndex
        // 
        comboBox_GPUIndex.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_GPUIndex.Items.AddRange(new object[] { "All (w>0.0)", "w>0.15", "w>0.3", "w>0.5", "w>0.7", "w>0.9" });
        comboBox_GPUIndex.Location = new Point(8, 42);
        comboBox_GPUIndex.Name = "comboBox_GPUIndex";
        comboBox_GPUIndex.Size = new Size(232, 23);
        comboBox_GPUIndex.TabIndex = 15;
        // 
        // comboBox_MultiGpu_PhysicsGPU
        // 
        comboBox_MultiGpu_PhysicsGPU.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_MultiGpu_PhysicsGPU.Items.AddRange(new object[] { "All (w>0.0)", "w>0.15", "w>0.3", "w>0.5", "w>0.7", "w>0.9" });
        comboBox_MultiGpu_PhysicsGPU.Location = new Point(7, 93);
        comboBox_MultiGpu_PhysicsGPU.Name = "comboBox_MultiGpu_PhysicsGPU";
        comboBox_MultiGpu_PhysicsGPU.Size = new Size(232, 23);
        comboBox_MultiGpu_PhysicsGPU.TabIndex = 32;
        // 
        // grpLiveMetrics
        // 
        grpLiveMetrics.Controls.Add(tlpLiveMetrics);
        grpLiveMetrics.Location = new Point(354, 6);
        grpLiveMetrics.Name = "grpLiveMetrics";
        grpLiveMetrics.Size = new Size(252, 156);
        grpLiveMetrics.TabIndex = 0;
        grpLiveMetrics.TabStop = false;
        grpLiveMetrics.Text = "Live Metrics";
        // 
        // tlpLiveMetrics
        // 
        tlpLiveMetrics.ColumnCount = 2;
        tlpLiveMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        tlpLiveMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        tlpLiveMetrics.Controls.Add(lblGlobalNbr, 0, 0);
        tlpLiveMetrics.Controls.Add(valGlobalNbr, 1, 0);
        tlpLiveMetrics.Controls.Add(lblGlobalSpont, 0, 1);
        tlpLiveMetrics.Controls.Add(valGlobalSpont, 1, 1);
        tlpLiveMetrics.Controls.Add(lblStrongEdges, 0, 2);
        tlpLiveMetrics.Controls.Add(valStrongEdges, 1, 2);
        tlpLiveMetrics.Controls.Add(lblLargestCluster, 0, 3);
        tlpLiveMetrics.Controls.Add(valLargestCluster, 1, 3);
        tlpLiveMetrics.Controls.Add(lblHeavyMass, 0, 4);
        tlpLiveMetrics.Controls.Add(valHeavyMass, 1, 4);
        tlpLiveMetrics.Controls.Add(lblSpectrumInfo, 0, 5);
        tlpLiveMetrics.Controls.Add(valSpectrumInfo, 1, 5);
        tlpLiveMetrics.Controls.Add(lblLightSpeed, 0, 6);
        tlpLiveMetrics.Controls.Add(valLightSpeed, 1, 6);
        tlpLiveMetrics.Location = new Point(3, 19);
        tlpLiveMetrics.Name = "tlpLiveMetrics";
        tlpLiveMetrics.RowCount = 7;
        tlpLiveMetrics.RowStyles.Add(new RowStyle());
        tlpLiveMetrics.RowStyles.Add(new RowStyle());
        tlpLiveMetrics.RowStyles.Add(new RowStyle());
        tlpLiveMetrics.RowStyles.Add(new RowStyle());
        tlpLiveMetrics.RowStyles.Add(new RowStyle());
        tlpLiveMetrics.RowStyles.Add(new RowStyle());
        tlpLiveMetrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpLiveMetrics.Size = new Size(249, 129);
        tlpLiveMetrics.TabIndex = 0;
        // 
        // lblGlobalNbr
        // 
        lblGlobalNbr.AutoSize = true;
        lblGlobalNbr.Location = new Point(3, 0);
        lblGlobalNbr.Name = "lblGlobalNbr";
        lblGlobalNbr.Size = new Size(67, 15);
        lblGlobalNbr.TabIndex = 0;
        lblGlobalNbr.Text = "Global Nbr:";
        // 
        // valGlobalNbr
        // 
        valGlobalNbr.AutoSize = true;
        valGlobalNbr.Location = new Point(152, 0);
        valGlobalNbr.Name = "valGlobalNbr";
        valGlobalNbr.Size = new Size(34, 15);
        valGlobalNbr.TabIndex = 1;
        valGlobalNbr.Text = "0.000";
        // 
        // lblGlobalSpont
        // 
        lblGlobalSpont.AutoSize = true;
        lblGlobalSpont.Location = new Point(3, 15);
        lblGlobalSpont.Name = "lblGlobalSpont";
        lblGlobalSpont.Size = new Size(78, 15);
        lblGlobalSpont.TabIndex = 2;
        lblGlobalSpont.Text = "Global Spont:";
        // 
        // valGlobalSpont
        // 
        valGlobalSpont.AutoSize = true;
        valGlobalSpont.Location = new Point(152, 15);
        valGlobalSpont.Name = "valGlobalSpont";
        valGlobalSpont.Size = new Size(34, 15);
        valGlobalSpont.TabIndex = 3;
        valGlobalSpont.Text = "0.000";
        // 
        // lblStrongEdges
        // 
        lblStrongEdges.AutoSize = true;
        lblStrongEdges.Location = new Point(3, 30);
        lblStrongEdges.Name = "lblStrongEdges";
        lblStrongEdges.Size = new Size(79, 15);
        lblStrongEdges.TabIndex = 4;
        lblStrongEdges.Text = "Strong edges:";
        // 
        // valStrongEdges
        // 
        valStrongEdges.AutoSize = true;
        valStrongEdges.Location = new Point(152, 30);
        valStrongEdges.Name = "valStrongEdges";
        valStrongEdges.Size = new Size(13, 15);
        valStrongEdges.TabIndex = 5;
        valStrongEdges.Text = "0";
        // 
        // lblLargestCluster
        // 
        lblLargestCluster.AutoSize = true;
        lblLargestCluster.Location = new Point(3, 45);
        lblLargestCluster.Name = "lblLargestCluster";
        lblLargestCluster.Size = new Size(86, 15);
        lblLargestCluster.TabIndex = 6;
        lblLargestCluster.Text = "Largest cluster:";
        // 
        // valLargestCluster
        // 
        valLargestCluster.AutoSize = true;
        valLargestCluster.Location = new Point(152, 45);
        valLargestCluster.Name = "valLargestCluster";
        valLargestCluster.Size = new Size(13, 15);
        valLargestCluster.TabIndex = 7;
        valLargestCluster.Text = "0";
        // 
        // lblHeavyMass
        // 
        lblHeavyMass.AutoSize = true;
        lblHeavyMass.Location = new Point(3, 60);
        lblHeavyMass.Name = "lblHeavyMass";
        lblHeavyMass.Size = new Size(73, 15);
        lblHeavyMass.TabIndex = 8;
        lblHeavyMass.Text = "Heavy mass:";
        // 
        // valHeavyMass
        // 
        valHeavyMass.AutoSize = true;
        valHeavyMass.Location = new Point(152, 60);
        valHeavyMass.Name = "valHeavyMass";
        valHeavyMass.Size = new Size(22, 15);
        valHeavyMass.TabIndex = 9;
        valHeavyMass.Text = "0.0";
        // 
        // lblSpectrumInfo
        // 
        lblSpectrumInfo.AutoSize = true;
        lblSpectrumInfo.Location = new Point(3, 75);
        lblSpectrumInfo.Name = "lblSpectrumInfo";
        lblSpectrumInfo.Size = new Size(86, 15);
        lblSpectrumInfo.TabIndex = 10;
        lblSpectrumInfo.Text = "Spectrum logs:";
        // 
        // valSpectrumInfo
        // 
        valSpectrumInfo.AutoSize = true;
        valSpectrumInfo.Location = new Point(152, 75);
        valSpectrumInfo.Name = "valSpectrumInfo";
        valSpectrumInfo.Size = new Size(22, 15);
        valSpectrumInfo.TabIndex = 11;
        valSpectrumInfo.Text = "off";
        // 
        // lblLightSpeed
        // 
        lblLightSpeed.AutoSize = true;
        lblLightSpeed.Location = new Point(3, 90);
        lblLightSpeed.Name = "lblLightSpeed";
        lblLightSpeed.Size = new Size(35, 15);
        lblLightSpeed.TabIndex = 12;
        lblLightSpeed.Text = "c_eff:";
        // 
        // valLightSpeed
        // 
        valLightSpeed.AutoSize = true;
        valLightSpeed.Location = new Point(152, 90);
        valLightSpeed.Name = "valLightSpeed";
        valLightSpeed.Size = new Size(13, 15);
        valLightSpeed.TabIndex = 13;
        valLightSpeed.Text = "0";
        // 
        // grpRunStats
        // 
        grpRunStats.Controls.Add(tlpRunStats);
        grpRunStats.Location = new Point(354, 169);
        grpRunStats.Name = "grpRunStats";
        grpRunStats.Size = new Size(252, 217);
        grpRunStats.TabIndex = 0;
        grpRunStats.TabStop = false;
        grpRunStats.Text = "Run Summary";
        // 
        // tlpRunStats
        // 
        tlpRunStats.ColumnCount = 2;
        tlpRunStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        tlpRunStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        tlpRunStats.Controls.Add(lblExcitedAvg, 0, 2);
        tlpRunStats.Controls.Add(valExcitedAvg, 1, 2);
        tlpRunStats.Controls.Add(lblExcitedMax, 0, 3);
        tlpRunStats.Controls.Add(valExcitedMax, 1, 3);
        tlpRunStats.Controls.Add(lblAvalancheCount, 0, 4);
        tlpRunStats.Controls.Add(valAvalancheCount, 1, 4);
        tlpRunStats.Controls.Add(lblMeasurementStatus, 0, 5);
        tlpRunStats.Controls.Add(valMeasurementStatus, 1, 5);
        tlpRunStats.Controls.Add(valCurrentStep, 1, 0);
        tlpRunStats.Controls.Add(valTotalSteps, 1, 1);
        tlpRunStats.Controls.Add(lblTotalSteps, 0, 1);
        tlpRunStats.Controls.Add(lblCurrentStep, 0, 0);
        tlpRunStats.Dock = DockStyle.Fill;
        tlpRunStats.Location = new Point(3, 19);
        tlpRunStats.Name = "tlpRunStats";
        tlpRunStats.RowCount = 6;
        tlpRunStats.RowStyles.Add(new RowStyle());
        tlpRunStats.RowStyles.Add(new RowStyle());
        tlpRunStats.RowStyles.Add(new RowStyle());
        tlpRunStats.RowStyles.Add(new RowStyle());
        tlpRunStats.RowStyles.Add(new RowStyle());
        tlpRunStats.RowStyles.Add(new RowStyle());
        tlpRunStats.Size = new Size(246, 195);
        tlpRunStats.TabIndex = 0;
        // 
        // lblExcitedAvg
        // 
        lblExcitedAvg.AutoSize = true;
        lblExcitedAvg.Location = new Point(3, 30);
        lblExcitedAvg.Name = "lblExcitedAvg";
        lblExcitedAvg.Size = new Size(71, 15);
        lblExcitedAvg.TabIndex = 4;
        lblExcitedAvg.Text = "Avg excited:";
        // 
        // valExcitedAvg
        // 
        valExcitedAvg.AutoSize = true;
        valExcitedAvg.Location = new Point(150, 30);
        valExcitedAvg.Name = "valExcitedAvg";
        valExcitedAvg.Size = new Size(28, 15);
        valExcitedAvg.TabIndex = 5;
        valExcitedAvg.Text = "0.00";
        // 
        // lblExcitedMax
        // 
        lblExcitedMax.AutoSize = true;
        lblExcitedMax.Location = new Point(3, 45);
        lblExcitedMax.Name = "lblExcitedMax";
        lblExcitedMax.Size = new Size(72, 15);
        lblExcitedMax.TabIndex = 6;
        lblExcitedMax.Text = "Max excited:";
        // 
        // valExcitedMax
        // 
        valExcitedMax.AutoSize = true;
        valExcitedMax.Location = new Point(150, 45);
        valExcitedMax.Name = "valExcitedMax";
        valExcitedMax.Size = new Size(13, 15);
        valExcitedMax.TabIndex = 7;
        valExcitedMax.Text = "0";
        // 
        // lblAvalancheCount
        // 
        lblAvalancheCount.AutoSize = true;
        lblAvalancheCount.Location = new Point(3, 60);
        lblAvalancheCount.Name = "lblAvalancheCount";
        lblAvalancheCount.Size = new Size(70, 15);
        lblAvalancheCount.TabIndex = 8;
        lblAvalancheCount.Text = "Avalanches:";
        // 
        // valAvalancheCount
        // 
        valAvalancheCount.AutoSize = true;
        valAvalancheCount.Location = new Point(150, 60);
        valAvalancheCount.Name = "valAvalancheCount";
        valAvalancheCount.Size = new Size(13, 15);
        valAvalancheCount.TabIndex = 9;
        valAvalancheCount.Text = "0";
        // 
        // lblMeasurementStatus
        // 
        lblMeasurementStatus.AutoSize = true;
        lblMeasurementStatus.Location = new Point(3, 75);
        lblMeasurementStatus.Name = "lblMeasurementStatus";
        lblMeasurementStatus.Size = new Size(83, 15);
        lblMeasurementStatus.TabIndex = 10;
        lblMeasurementStatus.Text = "Measurement:";
        // 
        // valMeasurementStatus
        // 
        valMeasurementStatus.AutoSize = true;
        valMeasurementStatus.Location = new Point(150, 75);
        valMeasurementStatus.Name = "valMeasurementStatus";
        valMeasurementStatus.Size = new Size(29, 15);
        valMeasurementStatus.TabIndex = 11;
        valMeasurementStatus.Text = "N/A";
        // 
        // valCurrentStep
        // 
        valCurrentStep.AutoSize = true;
        valCurrentStep.Location = new Point(150, 0);
        valCurrentStep.Name = "valCurrentStep";
        valCurrentStep.Size = new Size(13, 15);
        valCurrentStep.TabIndex = 3;
        valCurrentStep.Text = "0";
        // 
        // valTotalSteps
        // 
        valTotalSteps.AutoSize = true;
        valTotalSteps.Location = new Point(150, 15);
        valTotalSteps.Name = "valTotalSteps";
        valTotalSteps.Size = new Size(13, 15);
        valTotalSteps.TabIndex = 1;
        valTotalSteps.Text = "0";
        // 
        // lblTotalSteps
        // 
        lblTotalSteps.AutoSize = true;
        lblTotalSteps.Location = new Point(3, 15);
        lblTotalSteps.Name = "lblTotalSteps";
        lblTotalSteps.Size = new Size(66, 15);
        lblTotalSteps.TabIndex = 0;
        lblTotalSteps.Text = "Total steps:";
        // 
        // lblCurrentStep
        // 
        lblCurrentStep.AutoSize = true;
        lblCurrentStep.Location = new Point(3, 0);
        lblCurrentStep.Name = "lblCurrentStep";
        lblCurrentStep.Size = new Size(75, 15);
        lblCurrentStep.TabIndex = 2;
        lblCurrentStep.Text = "Current step:";
        // 
        // grpDashboard
        // 
        grpDashboard.Anchor = AnchorStyles.None;
        grpDashboard.Controls.Add(tlpDashboard);
        grpDashboard.Location = new Point(17, 5);
        grpDashboard.Margin = new Padding(5);
        grpDashboard.Name = "grpDashboard";
        grpDashboard.Size = new Size(331, 380);
        grpDashboard.TabIndex = 9;
        grpDashboard.TabStop = false;
        grpDashboard.Text = "Real-Time Dashboard";
        // 
        // tlpDashboard
        // 
        tlpDashboard.ColumnCount = 2;
        tlpDashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        tlpDashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        tlpDashboard.Controls.Add(lblDashNodes, 0, 0);
        tlpDashboard.Controls.Add(valDashNodes, 1, 0);
        tlpDashboard.Controls.Add(lblDashTotalSteps, 0, 1);
        tlpDashboard.Controls.Add(valDashTotalSteps, 1, 1);
        tlpDashboard.Controls.Add(lblDashCurrentStep, 0, 2);
        tlpDashboard.Controls.Add(valDashCurrentStep, 1, 2);
        tlpDashboard.Controls.Add(lblDashExcited, 0, 3);
        tlpDashboard.Controls.Add(valDashExcited, 1, 3);
        tlpDashboard.Controls.Add(lblDashHeavyMass, 0, 4);
        tlpDashboard.Controls.Add(valDashHeavyMass, 1, 4);
        tlpDashboard.Controls.Add(lblDashLargestCluster, 0, 5);
        tlpDashboard.Controls.Add(valDashLargestCluster, 1, 5);
        tlpDashboard.Controls.Add(lblDashStrongEdges, 0, 6);
        tlpDashboard.Controls.Add(valDashStrongEdges, 1, 6);
        tlpDashboard.Controls.Add(lblDashPhase, 0, 7);
        tlpDashboard.Controls.Add(valDashPhase, 1, 7);
        tlpDashboard.Controls.Add(lblDashQNorm, 0, 8);
        tlpDashboard.Controls.Add(valDashQNorm, 1, 8);
        tlpDashboard.Controls.Add(lblDashEntanglement, 0, 9);
        tlpDashboard.Controls.Add(valDashEntanglement, 1, 9);
        tlpDashboard.Controls.Add(lblDashCorrelation, 0, 10);
        tlpDashboard.Controls.Add(valDashCorrelation, 1, 10);
        tlpDashboard.Controls.Add(lblDashStatus, 0, 11);
        tlpDashboard.Controls.Add(valDashStatus, 1, 11);
        tlpDashboard.Controls.Add(lblDashSpectralDim, 0, 12);
        tlpDashboard.Controls.Add(valDashSpectralDim, 1, 12);
        tlpDashboard.Controls.Add(lblDashEffectiveG, 0, 13);
        tlpDashboard.Controls.Add(valDashEffectiveG, 1, 13);
        tlpDashboard.Controls.Add(lblDashGSuppression, 0, 14);
        tlpDashboard.Controls.Add(valDashGSuppression, 1, 14);
        tlpDashboard.Controls.Add(lblDashNetworkTemp, 0, 15);
        tlpDashboard.Controls.Add(valDashNetworkTemp, 1, 15);
        tlpDashboard.Dock = DockStyle.Fill;
        tlpDashboard.Location = new Point(3, 19);
        tlpDashboard.Name = "tlpDashboard";
        tlpDashboard.RowCount = 16;
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpDashboard.Size = new Size(325, 358);
        tlpDashboard.TabIndex = 0;
        // 
        // lblDashNodes
        // 
        lblDashNodes.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashNodes.AutoSize = true;
        lblDashNodes.Location = new Point(3, 2);
        lblDashNodes.Name = "lblDashNodes";
        lblDashNodes.Size = new Size(189, 15);
        lblDashNodes.TabIndex = 0;
        lblDashNodes.Text = "Nodes:";
        // 
        // valDashNodes
        // 
        valDashNodes.AutoSize = true;
        valDashNodes.Location = new Point(198, 0);
        valDashNodes.Name = "valDashNodes";
        valDashNodes.Size = new Size(13, 15);
        valDashNodes.TabIndex = 1;
        valDashNodes.Text = "0";
        // 
        // lblDashTotalSteps
        // 
        lblDashTotalSteps.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashTotalSteps.AutoSize = true;
        lblDashTotalSteps.Location = new Point(3, 22);
        lblDashTotalSteps.Name = "lblDashTotalSteps";
        lblDashTotalSteps.Size = new Size(189, 15);
        lblDashTotalSteps.TabIndex = 2;
        lblDashTotalSteps.Text = "Total Steps:";
        // 
        // valDashTotalSteps
        // 
        valDashTotalSteps.AutoSize = true;
        valDashTotalSteps.Location = new Point(198, 20);
        valDashTotalSteps.Name = "valDashTotalSteps";
        valDashTotalSteps.Size = new Size(13, 15);
        valDashTotalSteps.TabIndex = 3;
        valDashTotalSteps.Text = "0";
        // 
        // lblDashCurrentStep
        // 
        lblDashCurrentStep.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashCurrentStep.AutoSize = true;
        lblDashCurrentStep.Location = new Point(3, 42);
        lblDashCurrentStep.Name = "lblDashCurrentStep";
        lblDashCurrentStep.Size = new Size(189, 15);
        lblDashCurrentStep.TabIndex = 4;
        lblDashCurrentStep.Text = "Current Step:";
        // 
        // valDashCurrentStep
        // 
        valDashCurrentStep.AutoSize = true;
        valDashCurrentStep.Location = new Point(198, 40);
        valDashCurrentStep.Name = "valDashCurrentStep";
        valDashCurrentStep.Size = new Size(13, 15);
        valDashCurrentStep.TabIndex = 5;
        valDashCurrentStep.Text = "0";
        // 
        // lblDashExcited
        // 
        lblDashExcited.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashExcited.AutoSize = true;
        lblDashExcited.Location = new Point(3, 62);
        lblDashExcited.Name = "lblDashExcited";
        lblDashExcited.Size = new Size(189, 15);
        lblDashExcited.TabIndex = 6;
        lblDashExcited.Text = "Excited:";
        // 
        // valDashExcited
        // 
        valDashExcited.AutoSize = true;
        valDashExcited.Location = new Point(198, 60);
        valDashExcited.Name = "valDashExcited";
        valDashExcited.Size = new Size(13, 15);
        valDashExcited.TabIndex = 7;
        valDashExcited.Text = "0";
        // 
        // lblDashHeavyMass
        // 
        lblDashHeavyMass.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashHeavyMass.AutoSize = true;
        lblDashHeavyMass.Location = new Point(3, 82);
        lblDashHeavyMass.Name = "lblDashHeavyMass";
        lblDashHeavyMass.Size = new Size(189, 15);
        lblDashHeavyMass.TabIndex = 8;
        lblDashHeavyMass.Text = "Heavy Mass:";
        // 
        // valDashHeavyMass
        // 
        valDashHeavyMass.AutoSize = true;
        valDashHeavyMass.Location = new Point(198, 80);
        valDashHeavyMass.Name = "valDashHeavyMass";
        valDashHeavyMass.Size = new Size(28, 15);
        valDashHeavyMass.TabIndex = 9;
        valDashHeavyMass.Text = "0.00";
        // 
        // lblDashLargestCluster
        // 
        lblDashLargestCluster.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashLargestCluster.AutoSize = true;
        lblDashLargestCluster.Location = new Point(3, 102);
        lblDashLargestCluster.Name = "lblDashLargestCluster";
        lblDashLargestCluster.Size = new Size(189, 15);
        lblDashLargestCluster.TabIndex = 10;
        lblDashLargestCluster.Text = "Largest Cluster:";
        // 
        // valDashLargestCluster
        // 
        valDashLargestCluster.AutoSize = true;
        valDashLargestCluster.Location = new Point(198, 100);
        valDashLargestCluster.Name = "valDashLargestCluster";
        valDashLargestCluster.Size = new Size(13, 15);
        valDashLargestCluster.TabIndex = 11;
        valDashLargestCluster.Text = "0";
        // 
        // lblDashStrongEdges
        // 
        lblDashStrongEdges.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashStrongEdges.AutoSize = true;
        lblDashStrongEdges.Location = new Point(3, 122);
        lblDashStrongEdges.Name = "lblDashStrongEdges";
        lblDashStrongEdges.Size = new Size(189, 15);
        lblDashStrongEdges.TabIndex = 12;
        lblDashStrongEdges.Text = "Strong Edges:";
        // 
        // valDashStrongEdges
        // 
        valDashStrongEdges.AutoSize = true;
        valDashStrongEdges.Location = new Point(198, 120);
        valDashStrongEdges.Name = "valDashStrongEdges";
        valDashStrongEdges.Size = new Size(13, 15);
        valDashStrongEdges.TabIndex = 13;
        valDashStrongEdges.Text = "0";
        // 
        // lblDashPhase
        // 
        lblDashPhase.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashPhase.AutoSize = true;
        lblDashPhase.Location = new Point(3, 142);
        lblDashPhase.Name = "lblDashPhase";
        lblDashPhase.Size = new Size(189, 15);
        lblDashPhase.TabIndex = 14;
        lblDashPhase.Text = "Phase:";
        // 
        // valDashPhase
        // 
        valDashPhase.AutoSize = true;
        valDashPhase.Location = new Point(198, 140);
        valDashPhase.Name = "valDashPhase";
        valDashPhase.Size = new Size(36, 15);
        valDashPhase.TabIndex = 15;
        valDashPhase.Text = "Quiet";
        // 
        // lblDashQNorm
        // 
        lblDashQNorm.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashQNorm.AutoSize = true;
        lblDashQNorm.Location = new Point(3, 162);
        lblDashQNorm.Name = "lblDashQNorm";
        lblDashQNorm.Size = new Size(189, 15);
        lblDashQNorm.TabIndex = 16;
        lblDashQNorm.Text = "Q-Norm:";
        // 
        // valDashQNorm
        // 
        valDashQNorm.AutoSize = true;
        valDashQNorm.Location = new Point(198, 160);
        valDashQNorm.Name = "valDashQNorm";
        valDashQNorm.Size = new Size(34, 15);
        valDashQNorm.TabIndex = 17;
        valDashQNorm.Text = "0.000";
        // 
        // lblDashEntanglement
        // 
        lblDashEntanglement.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashEntanglement.AutoSize = true;
        lblDashEntanglement.Location = new Point(3, 182);
        lblDashEntanglement.Name = "lblDashEntanglement";
        lblDashEntanglement.Size = new Size(189, 15);
        lblDashEntanglement.TabIndex = 18;
        lblDashEntanglement.Text = "Entanglement:";
        // 
        // valDashEntanglement
        // 
        valDashEntanglement.AutoSize = true;
        valDashEntanglement.Location = new Point(198, 180);
        valDashEntanglement.Name = "valDashEntanglement";
        valDashEntanglement.Size = new Size(34, 15);
        valDashEntanglement.TabIndex = 19;
        valDashEntanglement.Text = "0.000";
        // 
        // lblDashCorrelation
        // 
        lblDashCorrelation.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashCorrelation.AutoSize = true;
        lblDashCorrelation.Location = new Point(3, 202);
        lblDashCorrelation.Name = "lblDashCorrelation";
        lblDashCorrelation.Size = new Size(189, 15);
        lblDashCorrelation.TabIndex = 20;
        lblDashCorrelation.Text = "Correlation:";
        // 
        // valDashCorrelation
        // 
        valDashCorrelation.AutoSize = true;
        valDashCorrelation.Location = new Point(198, 200);
        valDashCorrelation.Name = "valDashCorrelation";
        valDashCorrelation.Size = new Size(34, 15);
        valDashCorrelation.TabIndex = 21;
        valDashCorrelation.Text = "0.000";
        // 
        // lblDashStatus
        // 
        lblDashStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDashStatus.AutoSize = true;
        lblDashStatus.Location = new Point(3, 222);
        lblDashStatus.Name = "lblDashStatus";
        lblDashStatus.Size = new Size(189, 15);
        lblDashStatus.TabIndex = 22;
        lblDashStatus.Text = "Status:";
        // 
        // valDashStatus
        // 
        valDashStatus.AutoSize = true;
        valDashStatus.Location = new Point(198, 220);
        valDashStatus.Name = "valDashStatus";
        valDashStatus.Size = new Size(39, 15);
        valDashStatus.TabIndex = 23;
        valDashStatus.Text = "Ready";
        // 
        // lblDashSpectralDim
        // 
        lblDashSpectralDim.Location = new Point(3, 240);
        lblDashSpectralDim.Name = "lblDashSpectralDim";
        lblDashSpectralDim.Size = new Size(100, 20);
        lblDashSpectralDim.TabIndex = 24;
        lblDashSpectralDim.Text = "SpectralDim";
        // 
        // valDashSpectralDim
        // 
        valDashSpectralDim.Location = new Point(198, 240);
        valDashSpectralDim.Name = "valDashSpectralDim";
        valDashSpectralDim.Size = new Size(100, 20);
        valDashSpectralDim.TabIndex = 25;
        // 
        // lblDashEffectiveG
        // 
        lblDashEffectiveG.Location = new Point(3, 260);
        lblDashEffectiveG.Name = "lblDashEffectiveG";
        lblDashEffectiveG.Size = new Size(100, 20);
        lblDashEffectiveG.TabIndex = 26;
        lblDashEffectiveG.Text = "EffectiveG";
        // 
        // valDashEffectiveG
        // 
        valDashEffectiveG.Location = new Point(198, 260);
        valDashEffectiveG.Name = "valDashEffectiveG";
        valDashEffectiveG.Size = new Size(100, 20);
        valDashEffectiveG.TabIndex = 27;
        // 
        // lblDashGSuppression
        // 
        lblDashGSuppression.Location = new Point(3, 280);
        lblDashGSuppression.Name = "lblDashGSuppression";
        lblDashGSuppression.Size = new Size(100, 20);
        lblDashGSuppression.TabIndex = 28;
        lblDashGSuppression.Text = "GSuppression";
        // 
        // valDashGSuppression
        // 
        valDashGSuppression.Location = new Point(198, 280);
        valDashGSuppression.Name = "valDashGSuppression";
        valDashGSuppression.Size = new Size(100, 20);
        valDashGSuppression.TabIndex = 29;
        // 
        // lblDashNetworkTemp
        // 
        lblDashNetworkTemp.Location = new Point(3, 300);
        lblDashNetworkTemp.Name = "lblDashNetworkTemp";
        lblDashNetworkTemp.Size = new Size(100, 23);
        lblDashNetworkTemp.TabIndex = 30;
        lblDashNetworkTemp.Text = "NetworkTemp";
        // 
        // valDashNetworkTemp
        // 
        valDashNetworkTemp.Location = new Point(198, 300);
        valDashNetworkTemp.Name = "valDashNetworkTemp";
        valDashNetworkTemp.Size = new Size(100, 23);
        valDashNetworkTemp.TabIndex = 31;
        // 
        // grpEvents
        // 
        grpEvents.Controls.Add(buttonTopEvents_Refresh);
        grpEvents.Controls.Add(checkBox_TopEvents_LiveUpdate);
        grpEvents.Controls.Add(comboBox_TopEvents_EventType);
        grpEvents.Controls.Add(buttonTopEvents_Clear);
        grpEvents.Controls.Add(listView_TopEvents);
        grpEvents.Controls.Add(buttonTopEvents_SaveJson);
        grpEvents.Controls.Add(lvEvents);
        grpEvents.Location = new Point(18, 388);
        grpEvents.Name = "grpEvents";
        grpEvents.Size = new Size(589, 321);
        grpEvents.TabIndex = 8;
        grpEvents.TabStop = false;
        grpEvents.Text = "Important Events";
        // 
        // buttonTopEvents_Refresh
        // 
        buttonTopEvents_Refresh.Location = new Point(124, 280);
        buttonTopEvents_Refresh.Name = "buttonTopEvents_Refresh";
        buttonTopEvents_Refresh.Size = new Size(86, 25);
        buttonTopEvents_Refresh.TabIndex = 51;
        buttonTopEvents_Refresh.Text = "Refresh";
        buttonTopEvents_Refresh.UseVisualStyleBackColor = true;
        buttonTopEvents_Refresh.Click += buttonTopEvents_Refresh_Click;
        // 
        // checkBox_TopEvents_LiveUpdate
        // 
        checkBox_TopEvents_LiveUpdate.AutoSize = true;
        checkBox_TopEvents_LiveUpdate.Location = new Point(15, 23);
        checkBox_TopEvents_LiveUpdate.Name = "checkBox_TopEvents_LiveUpdate";
        checkBox_TopEvents_LiveUpdate.Size = new Size(87, 19);
        checkBox_TopEvents_LiveUpdate.TabIndex = 50;
        checkBox_TopEvents_LiveUpdate.Text = "Live update";
        checkBox_TopEvents_LiveUpdate.CheckedChanged += checkBox_TopEvents_LiveUpdate_CheckedChanged;
        // 
        // comboBox_TopEvents_EventType
        // 
        comboBox_TopEvents_EventType.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_TopEvents_EventType.Items.AddRange(new object[] { "| `MassGap` | Yang-Mills spectral gap detection | λ₁ - λ₀ eigenvalue separation |", "| `SpectralDimension` | Dimensional emergence measurement | d_S convergence toward 4D |", "| `SpeedOfLightIsotropy` | Lieb-Robinson bounds verification | Signal velocity variance |", "| `RicciFlatness` | Vacuum curvature measurement | Average Ricci → 0 |", "| `HolographicAreaLaw` | Entropy scaling verification | S ~ Area vs S ~ Volume |", "| `HausdorffDimension` | Geometric dimension via ball growth | d_H measurement |", "| `ClusterTransition` | Giant cluster phase transitions | Cluster ratio thresholds |", "| `AutoTuningAdjustment` | Parameter optimization events | Before/after values |" });
        comboBox_TopEvents_EventType.Location = new Point(108, 19);
        comboBox_TopEvents_EventType.Name = "comboBox_TopEvents_EventType";
        comboBox_TopEvents_EventType.Size = new Size(468, 23);
        comboBox_TopEvents_EventType.TabIndex = 49;
        comboBox_TopEvents_EventType.SelectedIndexChanged += comboBox_TopEvents_EventType_SelectedIndexChanged;
        // 
        // buttonTopEvents_Clear
        // 
        buttonTopEvents_Clear.Location = new Point(217, 280);
        buttonTopEvents_Clear.Name = "buttonTopEvents_Clear";
        buttonTopEvents_Clear.Size = new Size(86, 25);
        buttonTopEvents_Clear.TabIndex = 48;
        buttonTopEvents_Clear.Text = "Clear";
        buttonTopEvents_Clear.UseVisualStyleBackColor = true;
        buttonTopEvents_Clear.Click += buttonTopEvents_Clear_Click;
        // 
        // listView_TopEvents
        // 
        listView_TopEvents.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        listView_TopEvents.CheckBoxes = true;
        listView_TopEvents.Columns.AddRange(new ColumnHeader[] { columnHeader_TopEventType, columnHeader_TopEventDecs, columnHeader_TopEventParams });
        listView_TopEvents.FullRowSelect = true;
        listView_TopEvents.GridLines = true;
        listViewGroup2.Header = "Top Events";
        listViewGroup2.Name = "listViewGroup_TopEvents";
        listView_TopEvents.Groups.AddRange(new ListViewGroup[] { listViewGroup2 });
        listView_TopEvents.Location = new Point(6, 57);
        listView_TopEvents.Name = "listView_TopEvents";
        listView_TopEvents.Size = new Size(371, 222);
        listView_TopEvents.Sorting = SortOrder.Ascending;
        listView_TopEvents.TabIndex = 46;
        listView_TopEvents.UseCompatibleStateImageBehavior = false;
        listView_TopEvents.View = View.Details;
        listView_TopEvents.SelectedIndexChanged += listView_TopEvents_SelectedIndexChanged;
        // 
        // columnHeader_TopEventType
        // 
        columnHeader_TopEventType.Text = "Top Events Type";
        // 
        // columnHeader_TopEventDecs
        // 
        columnHeader_TopEventDecs.Text = "Description";
        columnHeader_TopEventDecs.Width = 200;
        // 
        // columnHeader_TopEventParams
        // 
        columnHeader_TopEventParams.Text = "Prarams";
        // 
        // buttonTopEvents_SaveJson
        // 
        buttonTopEvents_SaveJson.Location = new Point(4, 280);
        buttonTopEvents_SaveJson.Name = "buttonTopEvents_SaveJson";
        buttonTopEvents_SaveJson.Size = new Size(114, 25);
        buttonTopEvents_SaveJson.TabIndex = 47;
        buttonTopEvents_SaveJson.Text = "Save top events";
        buttonTopEvents_SaveJson.UseVisualStyleBackColor = true;
        buttonTopEvents_SaveJson.Click += buttonTopEvents_SaveJson_Click;
        // 
        // lvEvents
        // 
        lvEvents.Anchor = AnchorStyles.None;
        lvEvents.Columns.AddRange(new ColumnHeader[] { colEventStep, colEventType, colEventDetail });
        lvEvents.FullRowSelect = true;
        lvEvents.Location = new Point(383, 76);
        lvEvents.Name = "lvEvents";
        lvEvents.Size = new Size(256, 152);
        lvEvents.TabIndex = 0;
        lvEvents.UseCompatibleStateImageBehavior = false;
        lvEvents.View = View.Details;
        // 
        // colEventStep
        // 
        colEventStep.Text = "Step";
        // 
        // colEventType
        // 
        colEventType.Text = "Type";
        colEventType.Width = 80;
        // 
        // colEventDetail
        // 
        colEventDetail.Text = "Detail";
        colEventDetail.Width = 130;
        // 
        // splitpanels_Add
        // 
        splitpanels_Add.Location = new Point(612, 13);
        splitpanels_Add.Name = "splitpanels_Add";
        splitpanels_Add.Orientation = Orientation.Horizontal;
        // 
        // splitpanels_Add.Panel1
        // 
        splitpanels_Add.Panel1.AutoScroll = true;
        splitpanels_Add.Panel1.BackgroundImageLayout = ImageLayout.None;
        splitpanels_Add.Panel1.Controls.Add(label_ParamPresets);
        splitpanels_Add.Panel1.Controls.Add(comboBox_Presets);
        splitpanels_Add.Panel1.Controls.Add(lblExperiments);
        splitpanels_Add.Panel1.Controls.Add(comboBox_Experiments);
        splitpanels_Add.Panel1.Controls.Add(chkShowHeavyOnly);
        splitpanels_Add.Panel1.Controls.Add(btnSnapshotImage);
        splitpanels_Add.Panel1.Controls.Add(button_CPUtoGPUCompare);
        splitpanels_Add.Panel1.Controls.Add(button_ForceRedrawGraphImage);
        splitpanels_Add.Panel1.Controls.Add(button_SaveSimReult);
        splitpanels_Add.Panel1.Controls.Add(cmbWeightThreshold);
        splitpanels_Add.Size = new Size(310, 696);
        splitpanels_Add.SplitterDistance = 240;
        splitpanels_Add.TabIndex = 6;
        // 
        // label_ParamPresets
        // 
        label_ParamPresets.AutoSize = true;
        label_ParamPresets.Location = new Point(19, 83);
        label_ParamPresets.Name = "label_ParamPresets";
        label_ParamPresets.Size = new Size(47, 15);
        label_ParamPresets.TabIndex = 27;
        label_ParamPresets.Text = "Presets:";
        // 
        // comboBox_Presets
        // 
        comboBox_Presets.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_Presets.Items.AddRange(new object[] { "Nodes 100", "Nodes 200", "Nodes 300", "Nodes 400", "Nodes 500", "Nodes 800", "Nodes 1000" });
        comboBox_Presets.Location = new Point(21, 99);
        comboBox_Presets.Name = "comboBox_Presets";
        comboBox_Presets.Size = new Size(130, 23);
        comboBox_Presets.TabIndex = 20;
        // 
        // lblExperiments
        // 
        lblExperiments.AutoSize = true;
        lblExperiments.Location = new Point(16, 16);
        lblExperiments.Name = "lblExperiments";
        lblExperiments.Size = new Size(69, 15);
        lblExperiments.TabIndex = 23;
        lblExperiments.Text = "Experiment:";
        // 
        // comboBox_Experiments
        // 
        comboBox_Experiments.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_Experiments.Location = new Point(21, 34);
        comboBox_Experiments.Name = "comboBox_Experiments";
        comboBox_Experiments.Size = new Size(267, 23);
        comboBox_Experiments.TabIndex = 22;
        // 
        // chkShowHeavyOnly
        // 
        chkShowHeavyOnly.AutoSize = true;
        chkShowHeavyOnly.Location = new Point(23, 61);
        chkShowHeavyOnly.Name = "chkShowHeavyOnly";
        chkShowHeavyOnly.Size = new Size(128, 19);
        chkShowHeavyOnly.TabIndex = 1;
        chkShowHeavyOnly.Text = "Heavy clusters only";
        // 
        // btnSnapshotImage
        // 
        btnSnapshotImage.AutoSize = true;
        btnSnapshotImage.Location = new Point(156, 130);
        btnSnapshotImage.Name = "btnSnapshotImage";
        btnSnapshotImage.Size = new Size(132, 25);
        btnSnapshotImage.TabIndex = 4;
        btnSnapshotImage.Text = "Snapshot";
        btnSnapshotImage.Click += BtnSnapshotImage_Click;
        // 
        // button_CPUtoGPUCompare
        // 
        button_CPUtoGPUCompare.AutoSize = true;
        button_CPUtoGPUCompare.Location = new Point(19, 201);
        button_CPUtoGPUCompare.Name = "button_CPUtoGPUCompare";
        button_CPUtoGPUCompare.Size = new Size(185, 25);
        button_CPUtoGPUCompare.TabIndex = 10;
        button_CPUtoGPUCompare.Text = "DEBUG! Run GPU\\CPU compare";
        button_CPUtoGPUCompare.Click += button_CPUtoGPUCompare_Click;
        // 
        // button_ForceRedrawGraphImage
        // 
        button_ForceRedrawGraphImage.AutoSize = true;
        button_ForceRedrawGraphImage.Location = new Point(19, 130);
        button_ForceRedrawGraphImage.Name = "button_ForceRedrawGraphImage";
        button_ForceRedrawGraphImage.Size = new Size(132, 25);
        button_ForceRedrawGraphImage.TabIndex = 17;
        button_ForceRedrawGraphImage.Text = "Force Redraw Graph";
        button_ForceRedrawGraphImage.Click += button_ForceRedrawGraphImage_Click;
        // 
        // button_SaveSimReult
        // 
        button_SaveSimReult.AutoSize = true;
        button_SaveSimReult.Location = new Point(156, 98);
        button_SaveSimReult.Name = "button_SaveSimReult";
        button_SaveSimReult.Size = new Size(132, 25);
        button_SaveSimReult.TabIndex = 8;
        button_SaveSimReult.Text = "Save Sim Results";
        button_SaveSimReult.Click += button_SaveSimReult_Click;
        // 
        // cmbWeightThreshold
        // 
        cmbWeightThreshold.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbWeightThreshold.Items.AddRange(new object[] { "All (w>0.0)", "w>0.15", "w>0.3", "w>0.5", "w>0.7", "w>0.9" });
        cmbWeightThreshold.Location = new Point(157, 64);
        cmbWeightThreshold.Name = "cmbWeightThreshold";
        cmbWeightThreshold.Size = new Size(131, 23);
        cmbWeightThreshold.TabIndex = 0;
        // 
        // tabPage_Settings
        // 
        tabPage_Settings.AutoScroll = true;
        tabPage_Settings.Controls.Add(settingsMainLayout);
        tabPage_Settings.Location = new Point(4, 24);
        tabPage_Settings.Name = "tabPage_Settings";
        tabPage_Settings.Size = new Size(1342, 725);
        tabPage_Settings.TabIndex = 14;
        tabPage_Settings.Text = "Settings";
        tabPage_Settings.UseVisualStyleBackColor = true;
        // 
        // settingsMainLayout
        // 
        settingsMainLayout.Anchor = AnchorStyles.None;
        settingsMainLayout.ColumnCount = 3;
        settingsMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 61.3023949F));
        settingsMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20.2095814F));
        settingsMainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18.4131737F));
        settingsMainLayout.Controls.Add(grpPhysicsModules, 0, 0);
        settingsMainLayout.Controls.Add(grpPhysicsConstants, 2, 0);
        settingsMainLayout.Controls.Add(grpSimParams, 1, 0);
        settingsMainLayout.Location = new Point(3, 0);
        settingsMainLayout.Name = "settingsMainLayout";
        settingsMainLayout.RowCount = 1;
        settingsMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        settingsMainLayout.Size = new Size(1336, 580);
        settingsMainLayout.TabIndex = 0;
        // 
        // grpPhysicsModules
        // 
        grpPhysicsModules.Anchor = AnchorStyles.Left;
        grpPhysicsModules.Controls.Add(flpPhysics);
        grpPhysicsModules.Location = new Point(5, 5);
        grpPhysicsModules.Margin = new Padding(5);
        grpPhysicsModules.Name = "grpPhysicsModules";
        grpPhysicsModules.Size = new Size(809, 570);
        grpPhysicsModules.TabIndex = 1;
        grpPhysicsModules.TabStop = false;
        grpPhysicsModules.Text = "Physics Modules";
        // 
        // flpPhysics
        // 
        flpPhysics.Controls.Add(chkQuantumDriven);
        flpPhysics.Controls.Add(chkSpacetimePhysics);
        flpPhysics.Controls.Add(chkSpinorField);
        flpPhysics.Controls.Add(chkVacuumFluctuations);
        flpPhysics.Controls.Add(chkBlackHolePhysics);
        flpPhysics.Controls.Add(chkYangMillsGauge);
        flpPhysics.Controls.Add(chkEnhancedKleinGordon);
        flpPhysics.Controls.Add(chkInternalTime);
        flpPhysics.Controls.Add(chkSpectralGeometry);
        flpPhysics.Controls.Add(chkQuantumGraphity);
        flpPhysics.Controls.Add(chkRelationalTime);
        flpPhysics.Controls.Add(chkRelationalYangMills);
        flpPhysics.Controls.Add(chkNetworkGravity);
        flpPhysics.Controls.Add(chkUnifiedPhysicsStep);
        flpPhysics.Controls.Add(chkEnforceGaugeConstraints);
        flpPhysics.Controls.Add(chkCausalRewiring);
        flpPhysics.Controls.Add(chkTopologicalProtection);
        flpPhysics.Controls.Add(chkValidateEnergyConservation);
        flpPhysics.Controls.Add(chkMexicanHatPotential);
        flpPhysics.Controls.Add(chkGeometryMomenta);
        flpPhysics.Controls.Add(chkTopologicalCensorship);
        flpPhysics.Dock = DockStyle.Right;
        flpPhysics.FlowDirection = FlowDirection.TopDown;
        flpPhysics.Location = new Point(6, 19);
        flpPhysics.Name = "flpPhysics";
        flpPhysics.Size = new Size(800, 548);
        flpPhysics.TabIndex = 0;
        // 
        // chkQuantumDriven
        // 
        chkQuantumDriven.AutoSize = true;
        chkQuantumDriven.Checked = true;
        chkQuantumDriven.CheckState = CheckState.Checked;
        chkQuantumDriven.Location = new Point(3, 3);
        chkQuantumDriven.Name = "chkQuantumDriven";
        chkQuantumDriven.Size = new Size(148, 19);
        chkQuantumDriven.TabIndex = 0;
        chkQuantumDriven.Text = "Quantum Driven States";
        // 
        // chkSpacetimePhysics
        // 
        chkSpacetimePhysics.AutoSize = true;
        chkSpacetimePhysics.Checked = true;
        chkSpacetimePhysics.CheckState = CheckState.Checked;
        chkSpacetimePhysics.Location = new Point(3, 28);
        chkSpacetimePhysics.Name = "chkSpacetimePhysics";
        chkSpacetimePhysics.Size = new Size(123, 19);
        chkSpacetimePhysics.TabIndex = 1;
        chkSpacetimePhysics.Text = "Spacetime Physics";
        // 
        // chkSpinorField
        // 
        chkSpinorField.AutoSize = true;
        chkSpinorField.Checked = true;
        chkSpinorField.CheckState = CheckState.Checked;
        chkSpinorField.Location = new Point(3, 53);
        chkSpinorField.Name = "chkSpinorField";
        chkSpinorField.Size = new Size(88, 19);
        chkSpinorField.TabIndex = 2;
        chkSpinorField.Text = "Spinor Field";
        // 
        // chkVacuumFluctuations
        // 
        chkVacuumFluctuations.AutoSize = true;
        chkVacuumFluctuations.Checked = true;
        chkVacuumFluctuations.CheckState = CheckState.Checked;
        chkVacuumFluctuations.Location = new Point(3, 78);
        chkVacuumFluctuations.Name = "chkVacuumFluctuations";
        chkVacuumFluctuations.Size = new Size(137, 19);
        chkVacuumFluctuations.TabIndex = 3;
        chkVacuumFluctuations.Text = "Vacuum Fluctuations";
        // 
        // chkBlackHolePhysics
        // 
        chkBlackHolePhysics.AutoSize = true;
        chkBlackHolePhysics.Checked = true;
        chkBlackHolePhysics.CheckState = CheckState.Checked;
        chkBlackHolePhysics.Location = new Point(3, 103);
        chkBlackHolePhysics.Name = "chkBlackHolePhysics";
        chkBlackHolePhysics.Size = new Size(124, 19);
        chkBlackHolePhysics.TabIndex = 4;
        chkBlackHolePhysics.Text = "Black Hole Physics";
        // 
        // chkYangMillsGauge
        // 
        chkYangMillsGauge.AutoSize = true;
        chkYangMillsGauge.Checked = true;
        chkYangMillsGauge.CheckState = CheckState.Checked;
        chkYangMillsGauge.Location = new Point(3, 128);
        chkYangMillsGauge.Name = "chkYangMillsGauge";
        chkYangMillsGauge.Size = new Size(119, 19);
        chkYangMillsGauge.TabIndex = 5;
        chkYangMillsGauge.Text = "Yang-Mills Gauge";
        // 
        // chkEnhancedKleinGordon
        // 
        chkEnhancedKleinGordon.AutoSize = true;
        chkEnhancedKleinGordon.Checked = true;
        chkEnhancedKleinGordon.CheckState = CheckState.Checked;
        chkEnhancedKleinGordon.Location = new Point(3, 153);
        chkEnhancedKleinGordon.Name = "chkEnhancedKleinGordon";
        chkEnhancedKleinGordon.Size = new Size(152, 19);
        chkEnhancedKleinGordon.TabIndex = 6;
        chkEnhancedKleinGordon.Text = "Enhanced Klein-Gordon";
        // 
        // chkInternalTime
        // 
        chkInternalTime.AutoSize = true;
        chkInternalTime.Checked = true;
        chkInternalTime.CheckState = CheckState.Checked;
        chkInternalTime.Location = new Point(3, 178);
        chkInternalTime.Name = "chkInternalTime";
        chkInternalTime.Size = new Size(186, 19);
        chkInternalTime.TabIndex = 7;
        chkInternalTime.Text = "Internal Time (Page-Wootters)";
        // 
        // chkSpectralGeometry
        // 
        chkSpectralGeometry.AutoSize = true;
        chkSpectralGeometry.Checked = true;
        chkSpectralGeometry.CheckState = CheckState.Checked;
        chkSpectralGeometry.Location = new Point(3, 203);
        chkSpectralGeometry.Name = "chkSpectralGeometry";
        chkSpectralGeometry.Size = new Size(123, 19);
        chkSpectralGeometry.TabIndex = 8;
        chkSpectralGeometry.Text = "Spectral Geometry";
        // 
        // chkQuantumGraphity
        // 
        chkQuantumGraphity.AutoSize = true;
        chkQuantumGraphity.Checked = true;
        chkQuantumGraphity.CheckState = CheckState.Checked;
        chkQuantumGraphity.Location = new Point(3, 228);
        chkQuantumGraphity.Name = "chkQuantumGraphity";
        chkQuantumGraphity.Size = new Size(125, 19);
        chkQuantumGraphity.TabIndex = 9;
        chkQuantumGraphity.Text = "Quantum Graphity";
        // 
        // chkRelationalTime
        // 
        chkRelationalTime.AutoSize = true;
        chkRelationalTime.Checked = true;
        chkRelationalTime.CheckState = CheckState.Checked;
        chkRelationalTime.Location = new Point(3, 253);
        chkRelationalTime.Name = "chkRelationalTime";
        chkRelationalTime.Size = new Size(108, 19);
        chkRelationalTime.TabIndex = 10;
        chkRelationalTime.Text = "Relational Time";
        // 
        // chkRelationalYangMills
        // 
        chkRelationalYangMills.AutoSize = true;
        chkRelationalYangMills.Checked = true;
        chkRelationalYangMills.CheckState = CheckState.Checked;
        chkRelationalYangMills.Location = new Point(3, 278);
        chkRelationalYangMills.Name = "chkRelationalYangMills";
        chkRelationalYangMills.Size = new Size(137, 19);
        chkRelationalYangMills.TabIndex = 11;
        chkRelationalYangMills.Text = "Relational Yang-Mills";
        // 
        // chkNetworkGravity
        // 
        chkNetworkGravity.AutoSize = true;
        chkNetworkGravity.Checked = true;
        chkNetworkGravity.CheckState = CheckState.Checked;
        chkNetworkGravity.Location = new Point(3, 303);
        chkNetworkGravity.Name = "chkNetworkGravity";
        chkNetworkGravity.Size = new Size(111, 19);
        chkNetworkGravity.TabIndex = 12;
        chkNetworkGravity.Text = "Network Gravity";
        // 
        // chkUnifiedPhysicsStep
        // 
        chkUnifiedPhysicsStep.AutoSize = true;
        chkUnifiedPhysicsStep.Checked = true;
        chkUnifiedPhysicsStep.CheckState = CheckState.Checked;
        chkUnifiedPhysicsStep.Location = new Point(3, 328);
        chkUnifiedPhysicsStep.Name = "chkUnifiedPhysicsStep";
        chkUnifiedPhysicsStep.Size = new Size(132, 19);
        chkUnifiedPhysicsStep.TabIndex = 13;
        chkUnifiedPhysicsStep.Text = "Unified Physics Step";
        // 
        // chkEnforceGaugeConstraints
        // 
        chkEnforceGaugeConstraints.AutoSize = true;
        chkEnforceGaugeConstraints.Checked = true;
        chkEnforceGaugeConstraints.CheckState = CheckState.Checked;
        chkEnforceGaugeConstraints.Location = new Point(3, 353);
        chkEnforceGaugeConstraints.Name = "chkEnforceGaugeConstraints";
        chkEnforceGaugeConstraints.Size = new Size(166, 19);
        chkEnforceGaugeConstraints.TabIndex = 14;
        chkEnforceGaugeConstraints.Text = "Enforce Gauge Constraints";
        // 
        // chkCausalRewiring
        // 
        chkCausalRewiring.AutoSize = true;
        chkCausalRewiring.Checked = true;
        chkCausalRewiring.CheckState = CheckState.Checked;
        chkCausalRewiring.Location = new Point(3, 378);
        chkCausalRewiring.Name = "chkCausalRewiring";
        chkCausalRewiring.Size = new Size(110, 19);
        chkCausalRewiring.TabIndex = 15;
        chkCausalRewiring.Text = "Causal Rewiring";
        // 
        // chkTopologicalProtection
        // 
        chkTopologicalProtection.AutoSize = true;
        chkTopologicalProtection.Checked = true;
        chkTopologicalProtection.CheckState = CheckState.Checked;
        chkTopologicalProtection.Location = new Point(3, 403);
        chkTopologicalProtection.Name = "chkTopologicalProtection";
        chkTopologicalProtection.Size = new Size(146, 19);
        chkTopologicalProtection.TabIndex = 16;
        chkTopologicalProtection.Text = "Topological Protection";
        // 
        // chkValidateEnergyConservation
        // 
        chkValidateEnergyConservation.AutoSize = true;
        chkValidateEnergyConservation.Checked = true;
        chkValidateEnergyConservation.CheckState = CheckState.Checked;
        chkValidateEnergyConservation.Location = new Point(3, 428);
        chkValidateEnergyConservation.Name = "chkValidateEnergyConservation";
        chkValidateEnergyConservation.Size = new Size(179, 19);
        chkValidateEnergyConservation.TabIndex = 17;
        chkValidateEnergyConservation.Text = "Validate Energy Conservation";
        // 
        // chkMexicanHatPotential
        // 
        chkMexicanHatPotential.AutoSize = true;
        chkMexicanHatPotential.Checked = true;
        chkMexicanHatPotential.CheckState = CheckState.Checked;
        chkMexicanHatPotential.Location = new Point(3, 453);
        chkMexicanHatPotential.Name = "chkMexicanHatPotential";
        chkMexicanHatPotential.Size = new Size(142, 19);
        chkMexicanHatPotential.TabIndex = 18;
        chkMexicanHatPotential.Text = "Mexican Hat Potential";
        // 
        // chkGeometryMomenta
        // 
        chkGeometryMomenta.AutoSize = true;
        chkGeometryMomenta.Checked = true;
        chkGeometryMomenta.CheckState = CheckState.Checked;
        chkGeometryMomenta.Location = new Point(3, 478);
        chkGeometryMomenta.Name = "chkGeometryMomenta";
        chkGeometryMomenta.Size = new Size(133, 19);
        chkGeometryMomenta.TabIndex = 20;
        chkGeometryMomenta.Text = "Geometry Momenta";
        // 
        // chkTopologicalCensorship
        // 
        chkTopologicalCensorship.AutoSize = true;
        chkTopologicalCensorship.Checked = true;
        chkTopologicalCensorship.CheckState = CheckState.Checked;
        chkTopologicalCensorship.Location = new Point(3, 503);
        chkTopologicalCensorship.Name = "chkTopologicalCensorship";
        chkTopologicalCensorship.Size = new Size(150, 19);
        chkTopologicalCensorship.TabIndex = 21;
        chkTopologicalCensorship.Text = "Topological Censorship";
        // 
        // grpPhysicsConstants
        // 
        grpPhysicsConstants.Controls.Add(tlpPhysicsConstants);
        grpPhysicsConstants.Dock = DockStyle.Right;
        grpPhysicsConstants.Location = new Point(1096, 5);
        grpPhysicsConstants.Margin = new Padding(5);
        grpPhysicsConstants.Name = "grpPhysicsConstants";
        grpPhysicsConstants.Size = new Size(235, 570);
        grpPhysicsConstants.TabIndex = 2;
        grpPhysicsConstants.TabStop = false;
        grpPhysicsConstants.Text = "Physics Constants";
        // 
        // tlpPhysicsConstants
        // 
        tlpPhysicsConstants.AutoSize = true;
        tlpPhysicsConstants.ColumnCount = 2;
        tlpPhysicsConstants.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 73.3333359F));
        tlpPhysicsConstants.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26.666666F));
        tlpPhysicsConstants.Controls.Add(numGravityTransitionDuration, 1, 3);
        tlpPhysicsConstants.Controls.Add(lblInitialEdgeProb, 0, 0);
        tlpPhysicsConstants.Controls.Add(numInitialEdgeProb, 1, 0);
        tlpPhysicsConstants.Controls.Add(lblGravitationalCoupling, 0, 1);
        tlpPhysicsConstants.Controls.Add(numGravitationalCoupling, 1, 1);
        tlpPhysicsConstants.Controls.Add(lblVacuumEnergyScale, 0, 2);
        tlpPhysicsConstants.Controls.Add(numVacuumEnergyScale, 1, 2);
        tlpPhysicsConstants.Controls.Add(numDecoherenceRate, 1, 4);
        tlpPhysicsConstants.Controls.Add(numHotStartTemperature, 1, 5);
        tlpPhysicsConstants.Controls.Add(lblDecoherenceRate, 0, 4);
        tlpPhysicsConstants.Controls.Add(lblHotStartTemperature, 0, 5);
        tlpPhysicsConstants.Controls.Add(lblAdaptiveThresholdSigma, 0, 6);
        tlpPhysicsConstants.Controls.Add(numAdaptiveThresholdSigma, 1, 6);
        tlpPhysicsConstants.Controls.Add(lblWarmupDuration, 0, 7);
        tlpPhysicsConstants.Controls.Add(numWarmupDuration, 1, 7);
        tlpPhysicsConstants.Controls.Add(lblGravityTransitionDuration, 0, 3);
        tlpPhysicsConstants.Controls.Add(valAnnealingTimeConstant, 0, 8);
        tlpPhysicsConstants.Dock = DockStyle.Fill;
        tlpPhysicsConstants.Location = new Point(3, 19);
        tlpPhysicsConstants.Name = "tlpPhysicsConstants";
        tlpPhysicsConstants.RowCount = 10;
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tlpPhysicsConstants.Size = new Size(229, 548);
        tlpPhysicsConstants.TabIndex = 0;
        // 
        // numGravityTransitionDuration
        // 
        numGravityTransitionDuration.DecimalPlaces = 1;
        numGravityTransitionDuration.Dock = DockStyle.Fill;
        numGravityTransitionDuration.Increment = new decimal(new int[] { 10, 0, 0, 0 });
        numGravityTransitionDuration.Location = new Point(170, 87);
        numGravityTransitionDuration.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
        numGravityTransitionDuration.Name = "numGravityTransitionDuration";
        numGravityTransitionDuration.Size = new Size(56, 23);
        numGravityTransitionDuration.TabIndex = 18;
        numGravityTransitionDuration.Value = new decimal(new int[] { 137, 0, 0, 0 });
        // 
        // lblInitialEdgeProb
        // 
        lblInitialEdgeProb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblInitialEdgeProb.AutoSize = true;
        lblInitialEdgeProb.Location = new Point(3, 6);
        lblInitialEdgeProb.Name = "lblInitialEdgeProb";
        lblInitialEdgeProb.Size = new Size(161, 15);
        lblInitialEdgeProb.TabIndex = 0;
        lblInitialEdgeProb.Text = "Initial Edge Prob:";
        // 
        // numInitialEdgeProb
        // 
        numInitialEdgeProb.DecimalPlaces = 4;
        numInitialEdgeProb.Dock = DockStyle.Fill;
        numInitialEdgeProb.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
        numInitialEdgeProb.Location = new Point(170, 3);
        numInitialEdgeProb.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
        numInitialEdgeProb.Name = "numInitialEdgeProb";
        numInitialEdgeProb.Size = new Size(56, 23);
        numInitialEdgeProb.TabIndex = 1;
        numInitialEdgeProb.Value = new decimal(new int[] { 35, 0, 0, 196608 });
        // 
        // lblGravitationalCoupling
        // 
        lblGravitationalCoupling.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblGravitationalCoupling.AutoSize = true;
        lblGravitationalCoupling.Location = new Point(3, 34);
        lblGravitationalCoupling.Name = "lblGravitationalCoupling";
        lblGravitationalCoupling.Size = new Size(161, 15);
        lblGravitationalCoupling.TabIndex = 2;
        lblGravitationalCoupling.Text = "Gravitational Coupling (G):";
        // 
        // numGravitationalCoupling
        // 
        numGravitationalCoupling.DecimalPlaces = 4;
        numGravitationalCoupling.Dock = DockStyle.Fill;
        numGravitationalCoupling.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
        numGravitationalCoupling.Location = new Point(170, 31);
        numGravitationalCoupling.Name = "numGravitationalCoupling";
        numGravitationalCoupling.Size = new Size(56, 23);
        numGravitationalCoupling.TabIndex = 3;
        numGravitationalCoupling.Value = new decimal(new int[] { 10, 0, 0, 196608 });
        // 
        // lblVacuumEnergyScale
        // 
        lblVacuumEnergyScale.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblVacuumEnergyScale.AutoSize = true;
        lblVacuumEnergyScale.Location = new Point(3, 62);
        lblVacuumEnergyScale.Name = "lblVacuumEnergyScale";
        lblVacuumEnergyScale.Size = new Size(161, 15);
        lblVacuumEnergyScale.TabIndex = 4;
        lblVacuumEnergyScale.Text = "Vacuum Energy Scale:";
        // 
        // numVacuumEnergyScale
        // 
        numVacuumEnergyScale.DecimalPlaces = 4;
        numVacuumEnergyScale.Dock = DockStyle.Fill;
        numVacuumEnergyScale.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
        numVacuumEnergyScale.Location = new Point(170, 59);
        numVacuumEnergyScale.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
        numVacuumEnergyScale.Name = "numVacuumEnergyScale";
        numVacuumEnergyScale.Size = new Size(56, 23);
        numVacuumEnergyScale.TabIndex = 5;
        numVacuumEnergyScale.Value = new decimal(new int[] { 5, 0, 0, 327680 });
        // 
        // numDecoherenceRate
        // 
        numDecoherenceRate.DecimalPlaces = 4;
        numDecoherenceRate.Dock = DockStyle.Fill;
        numDecoherenceRate.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
        numDecoherenceRate.Location = new Point(170, 115);
        numDecoherenceRate.Maximum = new decimal(new int[] { 1, 0, 0, 65536 });
        numDecoherenceRate.Name = "numDecoherenceRate";
        numDecoherenceRate.Size = new Size(56, 23);
        numDecoherenceRate.TabIndex = 9;
        numDecoherenceRate.Value = new decimal(new int[] { 5, 0, 0, 196608 });
        // 
        // numHotStartTemperature
        // 
        numHotStartTemperature.DecimalPlaces = 1;
        numHotStartTemperature.Dock = DockStyle.Fill;
        numHotStartTemperature.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
        numHotStartTemperature.Location = new Point(170, 143);
        numHotStartTemperature.Name = "numHotStartTemperature";
        numHotStartTemperature.Size = new Size(56, 23);
        numHotStartTemperature.TabIndex = 11;
        numHotStartTemperature.Value = new decimal(new int[] { 6, 0, 0, 0 });
        // 
        // lblDecoherenceRate
        // 
        lblDecoherenceRate.Anchor = AnchorStyles.Left;
        lblDecoherenceRate.AutoSize = true;
        lblDecoherenceRate.Location = new Point(3, 118);
        lblDecoherenceRate.Name = "lblDecoherenceRate";
        lblDecoherenceRate.Size = new Size(105, 15);
        lblDecoherenceRate.TabIndex = 8;
        lblDecoherenceRate.Text = "Decoherence Rate:";
        // 
        // lblHotStartTemperature
        // 
        lblHotStartTemperature.Anchor = AnchorStyles.Left;
        lblHotStartTemperature.AutoSize = true;
        lblHotStartTemperature.Location = new Point(3, 146);
        lblHotStartTemperature.Name = "lblHotStartTemperature";
        lblHotStartTemperature.Size = new Size(127, 15);
        lblHotStartTemperature.TabIndex = 10;
        lblHotStartTemperature.Text = "Hot Start Temperature:";
        // 
        // lblAdaptiveThresholdSigma
        // 
        lblAdaptiveThresholdSigma.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblAdaptiveThresholdSigma.AutoSize = true;
        lblAdaptiveThresholdSigma.Location = new Point(3, 174);
        lblAdaptiveThresholdSigma.Name = "lblAdaptiveThresholdSigma";
        lblAdaptiveThresholdSigma.Size = new Size(161, 15);
        lblAdaptiveThresholdSigma.TabIndex = 12;
        lblAdaptiveThresholdSigma.Text = "Adaptive Threshold ?:";
        // 
        // numAdaptiveThresholdSigma
        // 
        numAdaptiveThresholdSigma.DecimalPlaces = 2;
        numAdaptiveThresholdSigma.Dock = DockStyle.Fill;
        numAdaptiveThresholdSigma.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        numAdaptiveThresholdSigma.Location = new Point(170, 171);
        numAdaptiveThresholdSigma.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
        numAdaptiveThresholdSigma.Name = "numAdaptiveThresholdSigma";
        numAdaptiveThresholdSigma.Size = new Size(56, 23);
        numAdaptiveThresholdSigma.TabIndex = 13;
        numAdaptiveThresholdSigma.Value = new decimal(new int[] { 15, 0, 0, 65536 });
        // 
        // lblWarmupDuration
        // 
        lblWarmupDuration.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblWarmupDuration.AutoSize = true;
        lblWarmupDuration.Location = new Point(3, 202);
        lblWarmupDuration.Name = "lblWarmupDuration";
        lblWarmupDuration.Size = new Size(161, 15);
        lblWarmupDuration.TabIndex = 14;
        lblWarmupDuration.Text = "Warmup Duration:";
        // 
        // numWarmupDuration
        // 
        numWarmupDuration.Dock = DockStyle.Fill;
        numWarmupDuration.Increment = new decimal(new int[] { 10, 0, 0, 0 });
        numWarmupDuration.Location = new Point(170, 199);
        numWarmupDuration.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numWarmupDuration.Name = "numWarmupDuration";
        numWarmupDuration.Size = new Size(56, 23);
        numWarmupDuration.TabIndex = 15;
        numWarmupDuration.Value = new decimal(new int[] { 200, 0, 0, 0 });
        // 
        // lblGravityTransitionDuration
        // 
        lblGravityTransitionDuration.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblGravityTransitionDuration.AutoSize = true;
        lblGravityTransitionDuration.Location = new Point(3, 90);
        lblGravityTransitionDuration.Name = "lblGravityTransitionDuration";
        lblGravityTransitionDuration.Size = new Size(161, 15);
        lblGravityTransitionDuration.TabIndex = 16;
        lblGravityTransitionDuration.Text = "Gravity Transition (1/?):";
        // 
        // valAnnealingTimeConstant
        // 
        valAnnealingTimeConstant.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        valAnnealingTimeConstant.AutoSize = true;
        valAnnealingTimeConstant.Location = new Point(3, 224);
        valAnnealingTimeConstant.Name = "valAnnealingTimeConstant";
        valAnnealingTimeConstant.Size = new Size(161, 15);
        valAnnealingTimeConstant.TabIndex = 22;
        valAnnealingTimeConstant.Text = "?_anneal = (computed)";
        valAnnealingTimeConstant.Click += valAnnealingTimeConstant_Click;
        // 
        // grpSimParams
        // 
        grpSimParams.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        grpSimParams.Controls.Add(tlpSimParams);
        grpSimParams.Location = new Point(824, 5);
        grpSimParams.Margin = new Padding(5);
        grpSimParams.Name = "grpSimParams";
        grpSimParams.Size = new Size(260, 570);
        grpSimParams.TabIndex = 0;
        grpSimParams.TabStop = false;
        grpSimParams.Text = "Simulation Parameters";
        // 
        // tlpSimParams
        // 
        tlpSimParams.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        tlpSimParams.ColumnCount = 2;
        tlpSimParams.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        tlpSimParams.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        tlpSimParams.Controls.Add(lblNodeCount, 0, 0);
        tlpSimParams.Controls.Add(numNodeCount, 1, 0);
        tlpSimParams.Controls.Add(lblTargetDegree, 0, 1);
        tlpSimParams.Controls.Add(numTargetDegree, 1, 1);
        tlpSimParams.Controls.Add(lblInitialExcitedProb, 0, 2);
        tlpSimParams.Controls.Add(numInitialExcitedProb, 1, 2);
        tlpSimParams.Controls.Add(lblLambdaState, 0, 3);
        tlpSimParams.Controls.Add(numLambdaState, 1, 3);
        tlpSimParams.Controls.Add(lblTemperature, 0, 4);
        tlpSimParams.Controls.Add(numTemperature, 1, 4);
        tlpSimParams.Controls.Add(lblEdgeTrialProb, 0, 5);
        tlpSimParams.Controls.Add(numEdgeTrialProb, 1, 5);
        tlpSimParams.Controls.Add(lblMeasurementThreshold, 0, 6);
        tlpSimParams.Controls.Add(numMeasurementThreshold, 1, 6);
        tlpSimParams.Controls.Add(lblTotalStepsSettings, 0, 7);
        tlpSimParams.Controls.Add(numTotalSteps, 1, 7);
        tlpSimParams.Controls.Add(lblFractalLevels, 0, 8);
        tlpSimParams.Controls.Add(numFractalLevels, 1, 8);
        tlpSimParams.Controls.Add(numFractalBranchFactor, 1, 9);
        tlpSimParams.Controls.Add(lblFractalBranchFactor, 0, 9);
        tlpSimParams.Location = new Point(7, 22);
        tlpSimParams.Margin = new Padding(4, 3, 3, 3);
        tlpSimParams.Name = "tlpSimParams";
        tlpSimParams.RowCount = 11;
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpSimParams.Size = new Size(247, 542);
        tlpSimParams.TabIndex = 0;
        // 
        // lblNodeCount
        // 
        lblNodeCount.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblNodeCount.AutoSize = true;
        lblNodeCount.Location = new Point(3, 2);
        lblNodeCount.Name = "lblNodeCount";
        lblNodeCount.Size = new Size(142, 15);
        lblNodeCount.TabIndex = 0;
        lblNodeCount.Text = "Node Count:";
        // 
        // numNodeCount
        // 
        numNodeCount.Dock = DockStyle.Fill;
        numNodeCount.Location = new Point(151, 3);
        numNodeCount.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        numNodeCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numNodeCount.Name = "numNodeCount";
        numNodeCount.Size = new Size(93, 23);
        numNodeCount.TabIndex = 1;
        numNodeCount.Value = new decimal(new int[] { 250, 0, 0, 0 });
        // 
        // lblTargetDegree
        // 
        lblTargetDegree.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblTargetDegree.AutoSize = true;
        lblTargetDegree.Location = new Point(3, 22);
        lblTargetDegree.Name = "lblTargetDegree";
        lblTargetDegree.Size = new Size(142, 15);
        lblTargetDegree.TabIndex = 2;
        lblTargetDegree.Text = "Target Degree:";
        // 
        // numTargetDegree
        // 
        numTargetDegree.Dock = DockStyle.Fill;
        numTargetDegree.Location = new Point(151, 23);
        numTargetDegree.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
        numTargetDegree.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
        numTargetDegree.Name = "numTargetDegree";
        numTargetDegree.Size = new Size(93, 23);
        numTargetDegree.TabIndex = 3;
        numTargetDegree.Value = new decimal(new int[] { 8, 0, 0, 0 });
        // 
        // lblInitialExcitedProb
        // 
        lblInitialExcitedProb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblInitialExcitedProb.AutoSize = true;
        lblInitialExcitedProb.Location = new Point(3, 42);
        lblInitialExcitedProb.Name = "lblInitialExcitedProb";
        lblInitialExcitedProb.Size = new Size(142, 15);
        lblInitialExcitedProb.TabIndex = 4;
        lblInitialExcitedProb.Text = "Initial Excited Prob:";
        // 
        // numInitialExcitedProb
        // 
        numInitialExcitedProb.DecimalPlaces = 2;
        numInitialExcitedProb.Dock = DockStyle.Fill;
        numInitialExcitedProb.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
        numInitialExcitedProb.Location = new Point(151, 43);
        numInitialExcitedProb.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
        numInitialExcitedProb.Name = "numInitialExcitedProb";
        numInitialExcitedProb.Size = new Size(93, 23);
        numInitialExcitedProb.TabIndex = 5;
        numInitialExcitedProb.Value = new decimal(new int[] { 10, 0, 0, 131072 });
        // 
        // lblLambdaState
        // 
        lblLambdaState.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblLambdaState.AutoSize = true;
        lblLambdaState.Location = new Point(3, 62);
        lblLambdaState.Name = "lblLambdaState";
        lblLambdaState.Size = new Size(142, 15);
        lblLambdaState.TabIndex = 6;
        lblLambdaState.Text = "Lambda State:";
        // 
        // numLambdaState
        // 
        numLambdaState.DecimalPlaces = 2;
        numLambdaState.Dock = DockStyle.Fill;
        numLambdaState.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        numLambdaState.Location = new Point(151, 63);
        numLambdaState.Name = "numLambdaState";
        numLambdaState.Size = new Size(93, 23);
        numLambdaState.TabIndex = 7;
        numLambdaState.Value = new decimal(new int[] { 5, 0, 0, 65536 });
        // 
        // lblTemperature
        // 
        lblTemperature.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblTemperature.AutoSize = true;
        lblTemperature.Location = new Point(3, 82);
        lblTemperature.Name = "lblTemperature";
        lblTemperature.Size = new Size(142, 15);
        lblTemperature.TabIndex = 8;
        lblTemperature.Text = "Temperature:";
        // 
        // numTemperature
        // 
        numTemperature.DecimalPlaces = 2;
        numTemperature.Dock = DockStyle.Fill;
        numTemperature.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        numTemperature.Location = new Point(151, 83);
        numTemperature.Name = "numTemperature";
        numTemperature.Size = new Size(93, 23);
        numTemperature.TabIndex = 9;
        numTemperature.Value = new decimal(new int[] { 100, 0, 0, 65536 });
        // 
        // lblEdgeTrialProb
        // 
        lblEdgeTrialProb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblEdgeTrialProb.AutoSize = true;
        lblEdgeTrialProb.Location = new Point(3, 102);
        lblEdgeTrialProb.Name = "lblEdgeTrialProb";
        lblEdgeTrialProb.Size = new Size(142, 15);
        lblEdgeTrialProb.TabIndex = 10;
        lblEdgeTrialProb.Text = "Edge Trial Prob:";
        // 
        // numEdgeTrialProb
        // 
        numEdgeTrialProb.DecimalPlaces = 3;
        numEdgeTrialProb.Dock = DockStyle.Fill;
        numEdgeTrialProb.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
        numEdgeTrialProb.Location = new Point(151, 103);
        numEdgeTrialProb.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
        numEdgeTrialProb.Name = "numEdgeTrialProb";
        numEdgeTrialProb.Size = new Size(93, 23);
        numEdgeTrialProb.TabIndex = 11;
        numEdgeTrialProb.Value = new decimal(new int[] { 2, 0, 0, 131072 });
        // 
        // lblMeasurementThreshold
        // 
        lblMeasurementThreshold.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblMeasurementThreshold.AutoSize = true;
        lblMeasurementThreshold.Location = new Point(3, 122);
        lblMeasurementThreshold.Name = "lblMeasurementThreshold";
        lblMeasurementThreshold.Size = new Size(142, 15);
        lblMeasurementThreshold.TabIndex = 12;
        lblMeasurementThreshold.Text = "Measurement Threshold:";
        // 
        // numMeasurementThreshold
        // 
        numMeasurementThreshold.DecimalPlaces = 3;
        numMeasurementThreshold.Dock = DockStyle.Fill;
        numMeasurementThreshold.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
        numMeasurementThreshold.Location = new Point(151, 123);
        numMeasurementThreshold.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
        numMeasurementThreshold.Name = "numMeasurementThreshold";
        numMeasurementThreshold.Size = new Size(93, 23);
        numMeasurementThreshold.TabIndex = 13;
        numMeasurementThreshold.Value = new decimal(new int[] { 30, 0, 0, 131072 });
        // 
        // lblTotalStepsSettings
        // 
        lblTotalStepsSettings.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblTotalStepsSettings.AutoSize = true;
        lblTotalStepsSettings.Location = new Point(3, 142);
        lblTotalStepsSettings.Name = "lblTotalStepsSettings";
        lblTotalStepsSettings.Size = new Size(142, 15);
        lblTotalStepsSettings.TabIndex = 14;
        lblTotalStepsSettings.Text = "Total Steps:";
        // 
        // numTotalSteps
        // 
        numTotalSteps.Dock = DockStyle.Fill;
        numTotalSteps.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        numTotalSteps.Location = new Point(151, 143);
        numTotalSteps.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
        numTotalSteps.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
        numTotalSteps.Name = "numTotalSteps";
        numTotalSteps.Size = new Size(93, 23);
        numTotalSteps.TabIndex = 15;
        numTotalSteps.Value = new decimal(new int[] { 500000, 0, 0, 0 });
        // 
        // lblFractalLevels
        // 
        lblFractalLevels.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblFractalLevels.AutoSize = true;
        lblFractalLevels.Location = new Point(4, 162);
        lblFractalLevels.Margin = new Padding(4, 0, 4, 0);
        lblFractalLevels.Name = "lblFractalLevels";
        lblFractalLevels.Size = new Size(140, 15);
        lblFractalLevels.TabIndex = 16;
        lblFractalLevels.Text = "Fractal Levels:";
        // 
        // numFractalLevels
        // 
        numFractalLevels.Dock = DockStyle.Fill;
        numFractalLevels.Location = new Point(151, 163);
        numFractalLevels.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        numFractalLevels.Name = "numFractalLevels";
        numFractalLevels.Size = new Size(93, 23);
        numFractalLevels.TabIndex = 17;
        // 
        // numFractalBranchFactor
        // 
        numFractalBranchFactor.Dock = DockStyle.Fill;
        numFractalBranchFactor.Location = new Point(151, 183);
        numFractalBranchFactor.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        numFractalBranchFactor.Name = "numFractalBranchFactor";
        numFractalBranchFactor.Size = new Size(93, 23);
        numFractalBranchFactor.TabIndex = 19;
        // 
        // lblFractalBranchFactor
        // 
        lblFractalBranchFactor.AutoSize = true;
        lblFractalBranchFactor.Location = new Point(3, 180);
        lblFractalBranchFactor.Name = "lblFractalBranchFactor";
        lblFractalBranchFactor.Size = new Size(121, 15);
        lblFractalBranchFactor.TabIndex = 18;
        lblFractalBranchFactor.Text = "Fractal Branch Factor:";
        // 
        // tabPage_UniPipelineState
        // 
        tabPage_UniPipelineState.Controls.Add(_tlp_UniPipeline_Main);
        tabPage_UniPipelineState.Location = new Point(4, 24);
        tabPage_UniPipelineState.Name = "tabPage_UniPipelineState";
        tabPage_UniPipelineState.Size = new Size(1342, 725);
        tabPage_UniPipelineState.TabIndex = 19;
        tabPage_UniPipelineState.Text = "Uni-Pipeline";
        tabPage_UniPipelineState.UseVisualStyleBackColor = true;
        // 
        // _tlp_UniPipeline_Main
        // 
        _tlp_UniPipeline_Main.ColumnCount = 2;
        _tlp_UniPipeline_Main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
        _tlp_UniPipeline_Main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        _tlp_UniPipeline_Main.Controls.Add(_tlpLeft, 0, 0);
        _tlp_UniPipeline_Main.Controls.Add(_grpProperties, 1, 0);
        _tlp_UniPipeline_Main.Controls.Add(_flpGpuTopologySettings, 0, 1);
        _tlp_UniPipeline_Main.Controls.Add(_flpDialogButtons, 1, 1);
        _tlp_UniPipeline_Main.Location = new Point(8, 8);
        _tlp_UniPipeline_Main.Margin = new Padding(3, 2, 3, 2);
        _tlp_UniPipeline_Main.Name = "_tlp_UniPipeline_Main";
        _tlp_UniPipeline_Main.RowCount = 2;
        _tlp_UniPipeline_Main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlp_UniPipeline_Main.RowStyles.Add(new RowStyle());
        _tlp_UniPipeline_Main.Size = new Size(952, 551);
        _tlp_UniPipeline_Main.TabIndex = 1;
        // 
        // _tlpLeft
        // 
        _tlpLeft.ColumnCount = 1;
        _tlpLeft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpLeft.Controls.Add(_dgvModules, 0, 0);
        _tlpLeft.Controls.Add(_flpButtons, 0, 1);
        _tlpLeft.Dock = DockStyle.Fill;
        _tlpLeft.Location = new Point(3, 2);
        _tlpLeft.Margin = new Padding(3, 2, 3, 2);
        _tlpLeft.Name = "_tlpLeft";
        _tlpLeft.RowCount = 2;
        _tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlpLeft.RowStyles.Add(new RowStyle());
        _tlpLeft.Size = new Size(612, 441);
        _tlpLeft.TabIndex = 0;
        // 
        // _dgvModules
        // 
        _dgvModules.AllowUserToAddRows = false;
        _dgvModules.AllowUserToDeleteRows = false;
        _dgvModules.AllowUserToResizeRows = false;
        _dgvModules.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _dgvModules.BackgroundColor = SystemColors.Window;
        _dgvModules.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _dgvModules.Columns.AddRange(new DataGridViewColumn[] { _colEnabled, _colName, _colCategory, _colStage, _colType, _colPriority, _colModuleGroup });
        _dgvModules.Dock = DockStyle.Fill;
        _dgvModules.Location = new Point(3, 2);
        _dgvModules.Margin = new Padding(3, 2, 3, 2);
        _dgvModules.Name = "_dgvModules";
        _dgvModules.RowHeadersVisible = false;
        _dgvModules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dgvModules.Size = new Size(606, 403);
        _dgvModules.TabIndex = 0;
        // 
        // _colEnabled
        // 
        _colEnabled.FillWeight = 10F;
        _colEnabled.HeaderText = "On";
        _colEnabled.Name = "_colEnabled";
        // 
        // _colName
        // 
        _colName.FillWeight = 30F;
        _colName.HeaderText = "Module Name";
        _colName.Name = "_colName";
        _colName.ReadOnly = true;
        // 
        // _colCategory
        // 
        _colCategory.FillWeight = 15F;
        _colCategory.HeaderText = "Category";
        _colCategory.Name = "_colCategory";
        _colCategory.ReadOnly = true;
        // 
        // _colStage
        // 
        _colStage.FillWeight = 15F;
        _colStage.HeaderText = "Stage";
        _colStage.Name = "_colStage";
        _colStage.ReadOnly = true;
        // 
        // _colType
        // 
        _colType.FillWeight = 12F;
        _colType.HeaderText = "Exec";
        _colType.Name = "_colType";
        _colType.ReadOnly = true;
        // 
        // _colPriority
        // 
        _colPriority.FillWeight = 8F;
        _colPriority.HeaderText = "Priority";
        _colPriority.Name = "_colPriority";
        _colPriority.ReadOnly = true;
        // 
        // _colModuleGroup
        // 
        _colModuleGroup.FillWeight = 15F;
        _colModuleGroup.HeaderText = "Group";
        _colModuleGroup.Name = "_colModuleGroup";
        _colModuleGroup.ReadOnly = true;
        // 
        // _flpButtons
        // 
        _flpButtons.AutoSize = true;
        _flpButtons.Controls.Add(_btnMoveUp);
        _flpButtons.Controls.Add(_btnMoveDown);
        _flpButtons.Controls.Add(_btnRemove);
        _flpButtons.Controls.Add(_btnLoadDll);
        _flpButtons.Controls.Add(_btnAddBuiltIn);
        _flpButtons.Controls.Add(_btnSaveConfig);
        _flpButtons.Controls.Add(_btnLoadConfig);
        _flpButtons.Dock = DockStyle.Fill;
        _flpButtons.Location = new Point(3, 411);
        _flpButtons.Margin = new Padding(3, 4, 3, 2);
        _flpButtons.Name = "_flpButtons";
        _flpButtons.Size = new Size(606, 28);
        _flpButtons.TabIndex = 1;
        // 
        // _btnMoveUp
        // 
        _btnMoveUp.Location = new Point(3, 2);
        _btnMoveUp.Margin = new Padding(3, 2, 3, 2);
        _btnMoveUp.Name = "_btnMoveUp";
        _btnMoveUp.Size = new Size(66, 24);
        _btnMoveUp.TabIndex = 0;
        _btnMoveUp.Text = "▲ Up";
        _btnMoveUp.UseVisualStyleBackColor = true;
        _btnMoveUp.Click += _btnMoveUp_Click;
        // 
        // _btnMoveDown
        // 
        _btnMoveDown.Location = new Point(75, 2);
        _btnMoveDown.Margin = new Padding(3, 2, 3, 2);
        _btnMoveDown.Name = "_btnMoveDown";
        _btnMoveDown.Size = new Size(66, 24);
        _btnMoveDown.TabIndex = 1;
        _btnMoveDown.Text = "▼ Down";
        _btnMoveDown.UseVisualStyleBackColor = true;
        _btnMoveDown.Click += _btnMoveDown_Click;
        // 
        // _btnRemove
        // 
        _btnRemove.Location = new Point(147, 2);
        _btnRemove.Margin = new Padding(3, 2, 3, 2);
        _btnRemove.Name = "_btnRemove";
        _btnRemove.Size = new Size(66, 24);
        _btnRemove.TabIndex = 2;
        _btnRemove.Text = "Remove";
        _btnRemove.UseVisualStyleBackColor = true;
        _btnRemove.Click += _btnRemove_Click;
        // 
        // _btnLoadDll
        // 
        _btnLoadDll.Location = new Point(219, 2);
        _btnLoadDll.Margin = new Padding(3, 2, 3, 2);
        _btnLoadDll.Name = "_btnLoadDll";
        _btnLoadDll.Size = new Size(88, 24);
        _btnLoadDll.TabIndex = 3;
        _btnLoadDll.Text = "Load DLL...";
        _btnLoadDll.UseVisualStyleBackColor = true;
        _btnLoadDll.Click += _btnLoadDll_Click;
        // 
        // _btnAddBuiltIn
        // 
        _btnAddBuiltIn.Location = new Point(313, 2);
        _btnAddBuiltIn.Margin = new Padding(3, 2, 3, 2);
        _btnAddBuiltIn.Name = "_btnAddBuiltIn";
        _btnAddBuiltIn.Size = new Size(88, 24);
        _btnAddBuiltIn.TabIndex = 4;
        _btnAddBuiltIn.Text = "Add Built-in...";
        _btnAddBuiltIn.UseVisualStyleBackColor = true;
        _btnAddBuiltIn.Click += _btnAddBuiltIn_Click;
        // 
        // _btnSaveConfig
        // 
        _btnSaveConfig.Location = new Point(407, 2);
        _btnSaveConfig.Margin = new Padding(3, 2, 3, 2);
        _btnSaveConfig.Name = "_btnSaveConfig";
        _btnSaveConfig.Size = new Size(88, 24);
        _btnSaveConfig.TabIndex = 5;
        _btnSaveConfig.Text = "Save Config...";
        _btnSaveConfig.UseVisualStyleBackColor = true;
        _btnSaveConfig.Click += _btnSaveConfig_Click;
        // 
        // _btnLoadConfig
        // 
        _btnLoadConfig.Location = new Point(501, 2);
        _btnLoadConfig.Margin = new Padding(3, 2, 3, 2);
        _btnLoadConfig.Name = "_btnLoadConfig";
        _btnLoadConfig.Size = new Size(88, 24);
        _btnLoadConfig.TabIndex = 6;
        _btnLoadConfig.Text = "Load Config...";
        _btnLoadConfig.UseVisualStyleBackColor = true;
        _btnLoadConfig.Click += _btnLoadConfig_Click;
        // 
        // _grpProperties
        // 
        _grpProperties.Controls.Add(_tlpProperties);
        _grpProperties.Dock = DockStyle.Fill;
        _grpProperties.Location = new Point(623, 2);
        _grpProperties.Margin = new Padding(5, 2, 3, 2);
        _grpProperties.Name = "_grpProperties";
        _grpProperties.Padding = new Padding(7, 6, 7, 6);
        _grpProperties.Size = new Size(326, 441);
        _grpProperties.TabIndex = 1;
        _grpProperties.TabStop = false;
        _grpProperties.Text = "Module Properties";
        // 
        // _tlpProperties
        // 
        _tlpProperties.ColumnCount = 2;
        _tlpProperties.ColumnStyles.Add(new ColumnStyle());
        _tlpProperties.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tlpProperties.Controls.Add(_lblModuleName, 0, 0);
        _tlpProperties.Controls.Add(_txtModuleName, 1, 0);
        _tlpProperties.Controls.Add(_lblDescription, 0, 1);
        _tlpProperties.Controls.Add(_txtDescription, 1, 1);
        _tlpProperties.Controls.Add(_lblExecutionType, 0, 2);
        _tlpProperties.Controls.Add(_cmbExecutionType, 1, 2);
        _tlpProperties.Dock = DockStyle.Fill;
        _tlpProperties.Location = new Point(7, 22);
        _tlpProperties.Margin = new Padding(3, 2, 3, 2);
        _tlpProperties.Name = "_tlpProperties";
        _tlpProperties.RowCount = 4;
        _tlpProperties.RowStyles.Add(new RowStyle());
        _tlpProperties.RowStyles.Add(new RowStyle());
        _tlpProperties.RowStyles.Add(new RowStyle());
        _tlpProperties.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _tlpProperties.Size = new Size(312, 413);
        _tlpProperties.TabIndex = 0;
        // 
        // _lblModuleName
        // 
        _lblModuleName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblModuleName.AutoSize = true;
        _lblModuleName.Location = new Point(3, 8);
        _lblModuleName.Margin = new Padding(3, 4, 3, 4);
        _lblModuleName.Name = "_lblModuleName";
        _lblModuleName.Size = new Size(70, 15);
        _lblModuleName.TabIndex = 0;
        _lblModuleName.Text = "Name:";
        // 
        // _txtModuleName
        // 
        _txtModuleName.Dock = DockStyle.Fill;
        _txtModuleName.Location = new Point(79, 4);
        _txtModuleName.Margin = new Padding(3, 4, 3, 4);
        _txtModuleName.Name = "_txtModuleName";
        _txtModuleName.ReadOnly = true;
        _txtModuleName.Size = new Size(230, 23);
        _txtModuleName.TabIndex = 1;
        // 
        // _lblDescription
        // 
        _lblDescription.AutoSize = true;
        _lblDescription.Location = new Point(3, 35);
        _lblDescription.Margin = new Padding(3, 4, 3, 4);
        _lblDescription.Name = "_lblDescription";
        _lblDescription.Size = new Size(70, 15);
        _lblDescription.TabIndex = 2;
        _lblDescription.Text = "Description:";
        // 
        // _txtDescription
        // 
        _txtDescription.Dock = DockStyle.Fill;
        _txtDescription.Location = new Point(79, 35);
        _txtDescription.Margin = new Padding(3, 4, 3, 4);
        _txtDescription.Multiline = true;
        _txtDescription.Name = "_txtDescription";
        _txtDescription.ReadOnly = true;
        _txtDescription.ScrollBars = ScrollBars.Vertical;
        _txtDescription.Size = new Size(230, 61);
        _txtDescription.TabIndex = 3;
        // 
        // _lblExecutionType
        // 
        _lblExecutionType.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _lblExecutionType.AutoSize = true;
        _lblExecutionType.Location = new Point(3, 108);
        _lblExecutionType.Margin = new Padding(3, 4, 3, 4);
        _lblExecutionType.Name = "_lblExecutionType";
        _lblExecutionType.Size = new Size(70, 15);
        _lblExecutionType.TabIndex = 4;
        _lblExecutionType.Text = "Execution:";
        // 
        // _cmbExecutionType
        // 
        _cmbExecutionType.Dock = DockStyle.Fill;
        _cmbExecutionType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbExecutionType.Enabled = false;
        _cmbExecutionType.Location = new Point(79, 104);
        _cmbExecutionType.Margin = new Padding(3, 4, 3, 4);
        _cmbExecutionType.Name = "_cmbExecutionType";
        _cmbExecutionType.Size = new Size(230, 23);
        _cmbExecutionType.TabIndex = 5;
        // 
        // _flpGpuTopologySettings
        // 
        _flpGpuTopologySettings.AutoSize = true;
        _flpGpuTopologySettings.Controls.Add(label_GpuEngineUniPipeline);
        _flpGpuTopologySettings.Controls.Add(comboBox_GpuEngineUniPipeline);
        _flpGpuTopologySettings.Controls.Add(label_TopologyMode);
        _flpGpuTopologySettings.Controls.Add(comboBox_TopologyMode);
        _flpGpuTopologySettings.Controls.Add(checkBox_ScienceSimMode);
        _flpGpuTopologySettings.Dock = DockStyle.Fill;
        _flpGpuTopologySettings.Location = new Point(3, 449);
        _flpGpuTopologySettings.Margin = new Padding(3, 4, 3, 2);
        _flpGpuTopologySettings.Name = "_flpGpuTopologySettings";
        _flpGpuTopologySettings.Size = new Size(612, 100);
        _flpGpuTopologySettings.TabIndex = 3;
        // 
        // label_GpuEngineUniPipeline
        // 
        label_GpuEngineUniPipeline.AutoSize = true;
        label_GpuEngineUniPipeline.Location = new Point(3, 0);
        label_GpuEngineUniPipeline.Name = "label_GpuEngineUniPipeline";
        label_GpuEngineUniPipeline.Size = new Size(72, 15);
        label_GpuEngineUniPipeline.TabIndex = 0;
        label_GpuEngineUniPipeline.Text = "GPU Engine:";
        // 
        // comboBox_GpuEngineUniPipeline
        // 
        comboBox_GpuEngineUniPipeline.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_GpuEngineUniPipeline.Items.AddRange(new object[] { "Auto (Recommend)", "Original (Dense)", "CSR (Sparse)", "CPU Only" });
        comboBox_GpuEngineUniPipeline.Location = new Point(81, 3);
        comboBox_GpuEngineUniPipeline.Name = "comboBox_GpuEngineUniPipeline";
        comboBox_GpuEngineUniPipeline.Size = new Size(140, 23);
        comboBox_GpuEngineUniPipeline.TabIndex = 1;
        // 
        // label_TopologyMode
        // 
        label_TopologyMode.AutoSize = true;
        label_TopologyMode.Location = new Point(227, 0);
        label_TopologyMode.Name = "label_TopologyMode";
        label_TopologyMode.Size = new Size(94, 15);
        label_TopologyMode.TabIndex = 2;
        label_TopologyMode.Text = "Topology Mode:";
        // 
        // comboBox_TopologyMode
        // 
        comboBox_TopologyMode.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_TopologyMode.Items.AddRange(new object[] { "CSR (Static)", "StreamCompaction (Hybrid)", "StreamCompaction (Full GPU)", "Dynamic Hard Rewiring" });
        comboBox_TopologyMode.Location = new Point(327, 3);
        comboBox_TopologyMode.Name = "comboBox_TopologyMode";
        comboBox_TopologyMode.Size = new Size(180, 23);
        comboBox_TopologyMode.TabIndex = 3;
        // 
        // checkBox_ScienceSimMode
        // 
        checkBox_ScienceSimMode.AutoSize = true;
        checkBox_ScienceSimMode.Location = new Point(3, 32);
        checkBox_ScienceSimMode.Name = "checkBox_ScienceSimMode";
        checkBox_ScienceSimMode.Size = new Size(100, 19);
        checkBox_ScienceSimMode.TabIndex = 31;
        checkBox_ScienceSimMode.Text = "Science Mode";
        // 
        // _flpDialogButtons
        // 
        _flpDialogButtons.Location = new Point(621, 448);
        _flpDialogButtons.Name = "_flpDialogButtons";
        _flpDialogButtons.Size = new Size(200, 100);
        _flpDialogButtons.TabIndex = 4;
        // 
        // tabPage_GUI
        // 
        tabPage_GUI.Location = new Point(4, 24);
        tabPage_GUI.Name = "tabPage_GUI";
        tabPage_GUI.Padding = new Padding(3);
        tabPage_GUI.Size = new Size(1342, 725);
        tabPage_GUI.TabIndex = 0;
        tabPage_GUI.Text = "2D Graph";
        tabPage_GUI.UseVisualStyleBackColor = true;
        // 
        // tabPage_Charts
        // 
        tabPage_Charts.Controls.Add(tlpCharts);
        tabPage_Charts.Location = new Point(4, 24);
        tabPage_Charts.Name = "tabPage_Charts";
        tabPage_Charts.Size = new Size(1342, 725);
        tabPage_Charts.TabIndex = 18;
        tabPage_Charts.Text = "Charts";
        tabPage_Charts.UseVisualStyleBackColor = true;
        // 
        // tlpCharts
        // 
        tlpCharts.ColumnCount = 2;
        tlpCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpCharts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpCharts.Controls.Add(grpChartExcited, 0, 0);
        tlpCharts.Controls.Add(grpChartHeavy, 1, 0);
        tlpCharts.Controls.Add(grpChartCluster, 0, 1);
        tlpCharts.Controls.Add(grpChartEnergy, 1, 1);
        tlpCharts.Dock = DockStyle.Fill;
        tlpCharts.Location = new Point(0, 0);
        tlpCharts.Name = "tlpCharts";
        tlpCharts.RowCount = 2;
        tlpCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        tlpCharts.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        tlpCharts.Size = new Size(1342, 725);
        tlpCharts.TabIndex = 0;
        // 
        // grpChartExcited
        // 
        grpChartExcited.Dock = DockStyle.Fill;
        grpChartExcited.Location = new Point(3, 3);
        grpChartExcited.Name = "grpChartExcited";
        grpChartExcited.Size = new Size(665, 356);
        grpChartExcited.TabIndex = 0;
        grpChartExcited.TabStop = false;
        grpChartExcited.Text = "Excited States";
        // 
        // grpChartHeavy
        // 
        grpChartHeavy.Dock = DockStyle.Fill;
        grpChartHeavy.Location = new Point(674, 3);
        grpChartHeavy.Name = "grpChartHeavy";
        grpChartHeavy.Size = new Size(665, 356);
        grpChartHeavy.TabIndex = 1;
        grpChartHeavy.TabStop = false;
        grpChartHeavy.Text = "Heavy Mass";
        // 
        // grpChartCluster
        // 
        grpChartCluster.Dock = DockStyle.Fill;
        grpChartCluster.Location = new Point(3, 365);
        grpChartCluster.Name = "grpChartCluster";
        grpChartCluster.Size = new Size(665, 357);
        grpChartCluster.TabIndex = 2;
        grpChartCluster.TabStop = false;
        grpChartCluster.Text = "Clusters";
        // 
        // grpChartEnergy
        // 
        grpChartEnergy.Dock = DockStyle.Fill;
        grpChartEnergy.Location = new Point(674, 365);
        grpChartEnergy.Name = "grpChartEnergy";
        grpChartEnergy.Size = new Size(665, 357);
        grpChartEnergy.TabIndex = 3;
        grpChartEnergy.TabStop = false;
        grpChartEnergy.Text = "Energy";
        // 
        // tabPage_Console
        // 
        tabPage_Console.Controls.Add(summaryTextBox);
        tabPage_Console.Controls.Add(textBox_HostSessionErrors);
        tabPage_Console.Controls.Add(checkBox_SysConsole_LiveUpdate);
        tabPage_Console.Controls.Add(checkBox_SimConsole_LiveUpdate);
        tabPage_Console.Controls.Add(button_SimConsole_Refresh);
        tabPage_Console.Controls.Add(comboBox_SimConsole_OutType);
        tabPage_Console.Controls.Add(button_SimConsole_Clear);
        tabPage_Console.Controls.Add(button_SimConsole_CopyToClipboard);
        tabPage_Console.Controls.Add(checkBox_AutoScrollSimConsole);
        tabPage_Console.Controls.Add(button_SysConsole_Refresh);
        tabPage_Console.Controls.Add(comboBox_SysConsole_OutType);
        tabPage_Console.Controls.Add(button_SysConsole_Clear);
        tabPage_Console.Controls.Add(button_SysConsole_CopyToClipboard);
        tabPage_Console.Controls.Add(checkBox_AutoScrollSysConsole);
        tabPage_Console.Controls.Add(textBox_SimConsole);
        tabPage_Console.Controls.Add(textBox_SysConsole);
        tabPage_Console.Location = new Point(4, 24);
        tabPage_Console.Name = "tabPage_Console";
        tabPage_Console.Padding = new Padding(3);
        tabPage_Console.Size = new Size(1342, 725);
        tabPage_Console.TabIndex = 1;
        tabPage_Console.Text = "Console";
        tabPage_Console.UseVisualStyleBackColor = true;
        // 
        // summaryTextBox
        // 
        summaryTextBox.Anchor = AnchorStyles.Left;
        summaryTextBox.BackColor = Color.Black;
        summaryTextBox.Font = new Font("Consolas", 9F);
        summaryTextBox.ForeColor = Color.Lime;
        summaryTextBox.Location = new Point(773, 76);
        summaryTextBox.Multiline = true;
        summaryTextBox.Name = "summaryTextBox";
        summaryTextBox.ReadOnly = true;
        summaryTextBox.ScrollBars = ScrollBars.Both;
        summaryTextBox.Size = new Size(343, 711);
        summaryTextBox.TabIndex = 65;
        // 
        // textBox_HostSessionErrors
        // 
        textBox_HostSessionErrors.Anchor = AnchorStyles.Left;
        textBox_HostSessionErrors.BackColor = Color.Black;
        textBox_HostSessionErrors.Font = new Font("Consolas", 9F);
        textBox_HostSessionErrors.ForeColor = Color.Lime;
        textBox_HostSessionErrors.Location = new Point(241, 76);
        textBox_HostSessionErrors.Multiline = true;
        textBox_HostSessionErrors.Name = "textBox_HostSessionErrors";
        textBox_HostSessionErrors.ReadOnly = true;
        textBox_HostSessionErrors.ScrollBars = ScrollBars.Both;
        textBox_HostSessionErrors.Size = new Size(343, 711);
        textBox_HostSessionErrors.TabIndex = 64;
        // 
        // checkBox_SysConsole_LiveUpdate
        // 
        checkBox_SysConsole_LiveUpdate.AutoSize = true;
        checkBox_SysConsole_LiveUpdate.Checked = true;
        checkBox_SysConsole_LiveUpdate.CheckState = CheckState.Checked;
        checkBox_SysConsole_LiveUpdate.Location = new Point(4, 4);
        checkBox_SysConsole_LiveUpdate.Name = "checkBox_SysConsole_LiveUpdate";
        checkBox_SysConsole_LiveUpdate.Size = new Size(88, 19);
        checkBox_SysConsole_LiveUpdate.TabIndex = 63;
        checkBox_SysConsole_LiveUpdate.Text = "Live Update";
        checkBox_SysConsole_LiveUpdate.CheckedChanged += checkBox_SysConsole_LiveUpdate_CheckedChanged;
        // 
        // checkBox_SimConsole_LiveUpdate
        // 
        checkBox_SimConsole_LiveUpdate.AutoSize = true;
        checkBox_SimConsole_LiveUpdate.Checked = true;
        checkBox_SimConsole_LiveUpdate.CheckState = CheckState.Checked;
        checkBox_SimConsole_LiveUpdate.Location = new Point(664, 3);
        checkBox_SimConsole_LiveUpdate.Name = "checkBox_SimConsole_LiveUpdate";
        checkBox_SimConsole_LiveUpdate.Size = new Size(88, 19);
        checkBox_SimConsole_LiveUpdate.TabIndex = 62;
        checkBox_SimConsole_LiveUpdate.Text = "Live Update";
        checkBox_SimConsole_LiveUpdate.CheckedChanged += checkBox_SimConsole_LiveUpdate_CheckedChanged;
        // 
        // button_SimConsole_Refresh
        // 
        button_SimConsole_Refresh.Location = new Point(1087, 11);
        button_SimConsole_Refresh.Name = "button_SimConsole_Refresh";
        button_SimConsole_Refresh.Size = new Size(86, 25);
        button_SimConsole_Refresh.TabIndex = 61;
        button_SimConsole_Refresh.Text = "Refresh";
        button_SimConsole_Refresh.UseVisualStyleBackColor = true;
        button_SimConsole_Refresh.Click += button_SimConsole_Refresh_Click;
        // 
        // comboBox_SimConsole_OutType
        // 
        comboBox_SimConsole_OutType.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_SimConsole_OutType.Location = new Point(810, 12);
        comboBox_SimConsole_OutType.Name = "comboBox_SimConsole_OutType";
        comboBox_SimConsole_OutType.Size = new Size(151, 23);
        comboBox_SimConsole_OutType.TabIndex = 60;
        // 
        // button_SimConsole_Clear
        // 
        button_SimConsole_Clear.Location = new Point(1180, 11);
        button_SimConsole_Clear.Name = "button_SimConsole_Clear";
        button_SimConsole_Clear.Size = new Size(86, 25);
        button_SimConsole_Clear.TabIndex = 59;
        button_SimConsole_Clear.Text = "Clear";
        button_SimConsole_Clear.UseVisualStyleBackColor = true;
        button_SimConsole_Clear.Click += button_SimConsole_Clear_Click;
        // 
        // button_SimConsole_CopyToClipboard
        // 
        button_SimConsole_CopyToClipboard.Location = new Point(967, 11);
        button_SimConsole_CopyToClipboard.Name = "button_SimConsole_CopyToClipboard";
        button_SimConsole_CopyToClipboard.Size = new Size(114, 25);
        button_SimConsole_CopyToClipboard.TabIndex = 58;
        button_SimConsole_CopyToClipboard.Text = "Copy sim data";
        button_SimConsole_CopyToClipboard.UseVisualStyleBackColor = true;
        button_SimConsole_CopyToClipboard.Click += button_SimConsole_CopyToClipboard_Click;
        // 
        // checkBox_AutoScrollSimConsole
        // 
        checkBox_AutoScrollSimConsole.AutoSize = true;
        checkBox_AutoScrollSimConsole.Checked = true;
        checkBox_AutoScrollSimConsole.CheckState = CheckState.Checked;
        checkBox_AutoScrollSimConsole.Location = new Point(664, 24);
        checkBox_AutoScrollSimConsole.Name = "checkBox_AutoScrollSimConsole";
        checkBox_AutoScrollSimConsole.Size = new Size(129, 19);
        checkBox_AutoScrollSimConsole.TabIndex = 57;
        checkBox_AutoScrollSimConsole.Text = "Auto-scroll console";
        checkBox_AutoScrollSimConsole.CheckedChanged += checkBox_AutoScrollSimConsole_CheckedChanged;
        // 
        // button_SysConsole_Refresh
        // 
        button_SysConsole_Refresh.Location = new Point(469, 11);
        button_SysConsole_Refresh.Name = "button_SysConsole_Refresh";
        button_SysConsole_Refresh.Size = new Size(86, 25);
        button_SysConsole_Refresh.TabIndex = 56;
        button_SysConsole_Refresh.Text = "Refresh";
        button_SysConsole_Refresh.UseVisualStyleBackColor = true;
        button_SysConsole_Refresh.Click += button_SysConsole_Refresh_Click;
        // 
        // comboBox_SysConsole_OutType
        // 
        comboBox_SysConsole_OutType.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox_SysConsole_OutType.Location = new Point(144, 12);
        comboBox_SysConsole_OutType.Name = "comboBox_SysConsole_OutType";
        comboBox_SysConsole_OutType.Size = new Size(162, 23);
        comboBox_SysConsole_OutType.TabIndex = 54;
        comboBox_SysConsole_OutType.SelectedIndexChanged += comboBox_SysConsole_OutType_SelectedIndexChanged;
        // 
        // button_SysConsole_Clear
        // 
        button_SysConsole_Clear.Location = new Point(562, 11);
        button_SysConsole_Clear.Name = "button_SysConsole_Clear";
        button_SysConsole_Clear.Size = new Size(86, 25);
        button_SysConsole_Clear.TabIndex = 53;
        button_SysConsole_Clear.Text = "Clear";
        button_SysConsole_Clear.UseVisualStyleBackColor = true;
        button_SysConsole_Clear.Click += button_SysConsole_Clear_Click;
        // 
        // button_SysConsole_CopyToClipboard
        // 
        button_SysConsole_CopyToClipboard.Location = new Point(312, 11);
        button_SysConsole_CopyToClipboard.Name = "button_SysConsole_CopyToClipboard";
        button_SysConsole_CopyToClipboard.Size = new Size(151, 25);
        button_SysConsole_CopyToClipboard.TabIndex = 52;
        button_SysConsole_CopyToClipboard.Text = "Copy sys data";
        button_SysConsole_CopyToClipboard.UseVisualStyleBackColor = true;
        button_SysConsole_CopyToClipboard.Click += button_SysConsole_CopyToClipboard_Click;
        // 
        // checkBox_AutoScrollSysConsole
        // 
        checkBox_AutoScrollSysConsole.AutoSize = true;
        checkBox_AutoScrollSysConsole.Checked = true;
        checkBox_AutoScrollSysConsole.CheckState = CheckState.Checked;
        checkBox_AutoScrollSysConsole.Location = new Point(4, 24);
        checkBox_AutoScrollSysConsole.Name = "checkBox_AutoScrollSysConsole";
        checkBox_AutoScrollSysConsole.Size = new Size(129, 19);
        checkBox_AutoScrollSysConsole.TabIndex = 3;
        checkBox_AutoScrollSysConsole.Text = "Auto-scroll console";
        checkBox_AutoScrollSysConsole.CheckedChanged += checkBox_AutoScrollSysConsole_CheckedChanged;
        // 
        // textBox_SimConsole
        // 
        textBox_SimConsole.Anchor = AnchorStyles.Left;
        textBox_SimConsole.BackColor = Color.Black;
        textBox_SimConsole.Font = new Font("Consolas", 9F);
        textBox_SimConsole.ForeColor = Color.Lime;
        textBox_SimConsole.Location = new Point(90, 69);
        textBox_SimConsole.Multiline = true;
        textBox_SimConsole.Name = "textBox_SimConsole";
        textBox_SimConsole.ReadOnly = true;
        textBox_SimConsole.ScrollBars = ScrollBars.Both;
        textBox_SimConsole.Size = new Size(100, 711);
        textBox_SimConsole.TabIndex = 1;
        // 
        // textBox_SysConsole
        // 
        textBox_SysConsole.Anchor = AnchorStyles.Left;
        textBox_SysConsole.BackColor = Color.Black;
        textBox_SysConsole.Font = new Font("Consolas", 9F);
        textBox_SysConsole.ForeColor = Color.Lime;
        textBox_SysConsole.Location = new Point(-4, 70);
        textBox_SysConsole.Multiline = true;
        textBox_SysConsole.Name = "textBox_SysConsole";
        textBox_SysConsole.ReadOnly = true;
        textBox_SysConsole.ScrollBars = ScrollBars.Both;
        textBox_SysConsole.Size = new Size(88, 711);
        textBox_SysConsole.TabIndex = 0;
        // 
        // tabPage_Sythnesis
        // 
        tabPage_Sythnesis.Location = new Point(4, 24);
        tabPage_Sythnesis.Name = "tabPage_Sythnesis";
        tabPage_Sythnesis.Size = new Size(1342, 725);
        tabPage_Sythnesis.TabIndex = 3;
        tabPage_Sythnesis.Text = "Synthesis";
        tabPage_Sythnesis.UseVisualStyleBackColor = true;
        // 
        // tabPage_Experiments
        // 
        tabPage_Experiments.Location = new Point(4, 24);
        tabPage_Experiments.Name = "tabPage_Experiments";
        tabPage_Experiments.Size = new Size(1342, 725);
        tabPage_Experiments.TabIndex = 15;
        tabPage_Experiments.Text = "Experiments";
        tabPage_Experiments.UseVisualStyleBackColor = true;
        // 
        // tabPage_3DVisual
        // 
        tabPage_3DVisual.Location = new Point(4, 24);
        tabPage_3DVisual.Name = "tabPage_3DVisual";
        tabPage_3DVisual.Size = new Size(1342, 725);
        tabPage_3DVisual.TabIndex = 16;
        tabPage_3DVisual.Text = "3D Visual";
        tabPage_3DVisual.UseVisualStyleBackColor = true;
        // 
        // tabPage_3DVisualCSR
        // 
        tabPage_3DVisualCSR.Location = new Point(4, 24);
        tabPage_3DVisualCSR.Name = "tabPage_3DVisualCSR";
        tabPage_3DVisualCSR.Size = new Size(1342, 725);
        tabPage_3DVisualCSR.TabIndex = 17;
        tabPage_3DVisualCSR.Text = "DX12 3D Mode";
        tabPage_3DVisualCSR.UseVisualStyleBackColor = true;
        // 
        // _btnCancel
        // 
        _btnCancel.Location = new Point(0, 0);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(75, 23);
        _btnCancel.TabIndex = 0;
        // 
        // _btnApply
        // 
        _btnApply.Location = new Point(0, 0);
        _btnApply.Name = "_btnApply";
        _btnApply.Size = new Size(75, 23);
        _btnApply.TabIndex = 0;
        // 
        // _btnOK
        // 
        _btnOK.Location = new Point(0, 0);
        _btnOK.Name = "_btnOK";
        _btnOK.Size = new Size(75, 23);
        _btnOK.TabIndex = 0;
        // 
        // button_ApplyPipelineConfSet
        // 
        button_ApplyPipelineConfSet.AutoSize = true;
        button_ApplyPipelineConfSet.Location = new Point(414, 12);
        button_ApplyPipelineConfSet.Name = "button_ApplyPipelineConfSet";
        button_ApplyPipelineConfSet.Size = new Size(132, 25);
        button_ApplyPipelineConfSet.TabIndex = 28;
        button_ApplyPipelineConfSet.Text = "Apply Plugins\\Set";
        button_ApplyPipelineConfSet.Click += button_ApplyPipelineConfSet_Click;
        // 
        // btnExpornShortJson
        // 
        btnExpornShortJson.AutoSize = true;
        btnExpornShortJson.Location = new Point(18, 155);
        btnExpornShortJson.Name = "btnExpornShortJson";
        btnExpornShortJson.Size = new Size(132, 25);
        btnExpornShortJson.TabIndex = 3;
        btnExpornShortJson.Text = "Export Short Now";
        btnExpornShortJson.Click += btnExpornShortJson_Click;
        // 
        // checkBox_AutoTuning
        // 
        checkBox_AutoTuning.AutoSize = true;
        checkBox_AutoTuning.Location = new Point(716, 19);
        checkBox_AutoTuning.Name = "checkBox_AutoTuning";
        checkBox_AutoTuning.Size = new Size(134, 19);
        checkBox_AutoTuning.TabIndex = 23;
        checkBox_AutoTuning.Text = "Auto-tuning params";
        checkBox_AutoTuning.CheckedChanged += checkBox_AutoTuning_CheckedChanged;
        // 
        // label_MaxFPS
        // 
        label_MaxFPS.AutoSize = true;
        label_MaxFPS.Location = new Point(1193, 19);
        label_MaxFPS.Name = "label_MaxFPS";
        label_MaxFPS.Size = new Size(54, 15);
        label_MaxFPS.TabIndex = 26;
        label_MaxFPS.Text = "Max FPS:";
        // 
        // numericUpDown_MaxFPS
        // 
        numericUpDown_MaxFPS.Location = new Point(1254, 12);
        numericUpDown_MaxFPS.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
        numericUpDown_MaxFPS.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numericUpDown_MaxFPS.Name = "numericUpDown_MaxFPS";
        numericUpDown_MaxFPS.Size = new Size(54, 23);
        numericUpDown_MaxFPS.TabIndex = 18;
        numericUpDown_MaxFPS.Value = new decimal(new int[] { 10, 0, 0, 0 });
        numericUpDown_MaxFPS.ValueChanged += numericUpDown_MaxFPS_ValueChanged;
        // 
        // label_CPUThreads
        // 
        label_CPUThreads.AutoSize = true;
        label_CPUThreads.Location = new Point(1018, 21);
        label_CPUThreads.Name = "label_CPUThreads";
        label_CPUThreads.Size = new Size(75, 15);
        label_CPUThreads.TabIndex = 25;
        label_CPUThreads.Text = "CPU threads:";
        // 
        // numericUpDown1
        // 
        numericUpDown1.Location = new Point(1098, 12);
        numericUpDown1.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
        numericUpDown1.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
        numericUpDown1.Name = "numericUpDown1";
        numericUpDown1.Size = new Size(56, 23);
        numericUpDown1.TabIndex = 24;
        numericUpDown1.Value = new decimal(new int[] { 8, 0, 0, 0 });
        // 
        // modernSimTextBox
        // 
        modernSimTextBox.Location = new Point(0, 0);
        modernSimTextBox.Name = "modernSimTextBox";
        modernSimTextBox.Size = new Size(100, 23);
        modernSimTextBox.TabIndex = 0;
        // 
        // button_RunModernSim
        // 
        button_RunModernSim.AutoSize = true;
        button_RunModernSim.Location = new Point(1, 12);
        button_RunModernSim.Name = "button_RunModernSim";
        button_RunModernSim.Size = new Size(132, 25);
        button_RunModernSim.TabIndex = 9;
        button_RunModernSim.Text = "Run simulation";
        button_RunModernSim.Click += button_RunSimulation_Click;
        // 
        // button_Plugins
        // 
        button_Plugins.AutoSize = true;
        button_Plugins.Location = new Point(276, 12);
        button_Plugins.Name = "button_Plugins";
        button_Plugins.Size = new Size(132, 25);
        button_Plugins.TabIndex = 26;
        button_Plugins.Text = "Plugins";
        button_Plugins.Click += button_Plugins_Click;
        // 
        // button_BindConsoleSession
        // 
        button_BindConsoleSession.AutoSize = true;
        button_BindConsoleSession.Location = new Point(552, 12);
        button_BindConsoleSession.Name = "button_BindConsoleSession";
        button_BindConsoleSession.Size = new Size(146, 25);
        button_BindConsoleSession.TabIndex = 28;
        button_BindConsoleSession.Text = "Bind Console Session";
        button_BindConsoleSession.Click += button_BindConsoleSession_Click;
        // 
        // checkBox_StanaloneDX12Form
        // 
        checkBox_StanaloneDX12Form.AutoSize = true;
        checkBox_StanaloneDX12Form.Location = new Point(856, 19);
        checkBox_StanaloneDX12Form.Name = "checkBox_StanaloneDX12Form";
        checkBox_StanaloneDX12Form.Size = new Size(143, 19);
        checkBox_StanaloneDX12Form.TabIndex = 29;
        checkBox_StanaloneDX12Form.Text = "standalone Dx12 Form";
        checkBox_StanaloneDX12Form.CheckedChanged += checkBox_StanaloneDX12Form_CheckedChanged;
        // 
        // button_TerminateSimSession
        // 
        button_TerminateSimSession.AutoSize = true;
        button_TerminateSimSession.Location = new Point(138, 12);
        button_TerminateSimSession.Name = "button_TerminateSimSession";
        button_TerminateSimSession.Size = new Size(132, 25);
        button_TerminateSimSession.TabIndex = 30;
        button_TerminateSimSession.Text = "Terminate Sim";
        button_TerminateSimSession.Click += button_TerminateSimSession_Click;
        // 
        // statusStrip1
        // 
        statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabelSteps, statusLabelHeavyMass, statusLabelExcited });
        statusStrip1.Location = new Point(0, 797);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new Size(1359, 22);
        statusStrip1.TabIndex = 31;
        statusStrip1.Text = "statusStrip1";
        // 
        // statusLabelSteps
        // 
        statusLabelSteps.Name = "statusLabelSteps";
        statusLabelSteps.Size = new Size(34, 17);
        statusLabelSteps.Text = "steps";
        // 
        // statusLabelHeavyMass
        // 
        statusLabelHeavyMass.Name = "statusLabelHeavyMass";
        statusLabelHeavyMass.Size = new Size(34, 17);
        statusLabelHeavyMass.Text = "mass";
        // 
        // statusLabelExcited
        // 
        statusLabelExcited.Name = "statusLabelExcited";
        statusLabelExcited.Size = new Size(43, 17);
        statusLabelExcited.Text = "existed";
        // 
        // Form_Main
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        ClientSize = new Size(1359, 819);
        Controls.Add(statusStrip1);
        Controls.Add(button_TerminateSimSession);
        Controls.Add(checkBox_StanaloneDX12Form);
        Controls.Add(button_BindConsoleSession);
        Controls.Add(checkBox_AutoTuning);
        Controls.Add(label_MaxFPS);
        Controls.Add(button_Plugins);
        Controls.Add(numericUpDown_MaxFPS);
        Controls.Add(label_CPUThreads);
        Controls.Add(button_RunModernSim);
        Controls.Add(numericUpDown1);
        Controls.Add(tabControl_Main);
        Controls.Add(button_ApplyPipelineConfSet);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximumSize = new Size(1375, 900);
        Name = "Form_Main";
        Text = "RQ-Sim";
        Load += Form_Main_Load;
        tabControl_Main.ResumeLayout(false);
        tabPage_Summary.ResumeLayout(false);
        groupBox_MultiGpu_Settings.ResumeLayout(false);
        groupBox_MultiGpu_Settings.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDown_BackgroundPluginGPUKernels).EndInit();
        grpLiveMetrics.ResumeLayout(false);
        tlpLiveMetrics.ResumeLayout(false);
        tlpLiveMetrics.PerformLayout();
        grpRunStats.ResumeLayout(false);
        tlpRunStats.ResumeLayout(false);
        tlpRunStats.PerformLayout();
        grpDashboard.ResumeLayout(false);
        tlpDashboard.ResumeLayout(false);
        tlpDashboard.PerformLayout();
        grpEvents.ResumeLayout(false);
        grpEvents.PerformLayout();
        splitpanels_Add.Panel1.ResumeLayout(false);
        splitpanels_Add.Panel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitpanels_Add).EndInit();
        splitpanels_Add.ResumeLayout(false);
        tabPage_Settings.ResumeLayout(false);
        settingsMainLayout.ResumeLayout(false);
        grpPhysicsModules.ResumeLayout(false);
        flpPhysics.ResumeLayout(false);
        flpPhysics.PerformLayout();
        grpPhysicsConstants.ResumeLayout(false);
        grpPhysicsConstants.PerformLayout();
        tlpPhysicsConstants.ResumeLayout(false);
        tlpPhysicsConstants.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numGravityTransitionDuration).EndInit();
        ((System.ComponentModel.ISupportInitialize)numInitialEdgeProb).EndInit();
        ((System.ComponentModel.ISupportInitialize)numGravitationalCoupling).EndInit();
        ((System.ComponentModel.ISupportInitialize)numVacuumEnergyScale).EndInit();
        ((System.ComponentModel.ISupportInitialize)numDecoherenceRate).EndInit();
        ((System.ComponentModel.ISupportInitialize)numHotStartTemperature).EndInit();
        ((System.ComponentModel.ISupportInitialize)numAdaptiveThresholdSigma).EndInit();
        ((System.ComponentModel.ISupportInitialize)numWarmupDuration).EndInit();
        grpSimParams.ResumeLayout(false);
        tlpSimParams.ResumeLayout(false);
        tlpSimParams.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numNodeCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTargetDegree).EndInit();
        ((System.ComponentModel.ISupportInitialize)numInitialExcitedProb).EndInit();
        ((System.ComponentModel.ISupportInitialize)numLambdaState).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTemperature).EndInit();
        ((System.ComponentModel.ISupportInitialize)numEdgeTrialProb).EndInit();
        ((System.ComponentModel.ISupportInitialize)numMeasurementThreshold).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTotalSteps).EndInit();
        ((System.ComponentModel.ISupportInitialize)numFractalLevels).EndInit();
        ((System.ComponentModel.ISupportInitialize)numFractalBranchFactor).EndInit();
        tabPage_UniPipelineState.ResumeLayout(false);
        _tlp_UniPipeline_Main.ResumeLayout(false);
        _tlp_UniPipeline_Main.PerformLayout();
        _tlpLeft.ResumeLayout(false);
        _tlpLeft.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_dgvModules).EndInit();
        _flpButtons.ResumeLayout(false);
        _grpProperties.ResumeLayout(false);
        _tlpProperties.ResumeLayout(false);
        _tlpProperties.PerformLayout();
        _flpGpuTopologySettings.ResumeLayout(false);
        _flpGpuTopologySettings.PerformLayout();
        tabPage_Charts.ResumeLayout(false);
        tlpCharts.ResumeLayout(false);
        tabPage_Console.ResumeLayout(false);
        tabPage_Console.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDown_MaxFPS).EndInit();
        ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
        statusStrip1.ResumeLayout(false);
        statusStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

}

