using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace RQSimulation.Core.Plugins;

/// <summary>
/// GPU synchronization interface for resource barrier transitions.
/// Implemented by GpuSyncManager in the rendering engine.
/// </summary>
public interface IGpuSyncManager
{
    /// <summary>
    /// Transition shared buffers from read state to write state for compute shaders.
    /// Call before GPU physics modules execute.
    /// </summary>
    void TransitionToCompute();

    /// <summary>
    /// Transition shared buffers from write state to read state for rendering.
    /// Call after GPU physics modules complete.
    /// </summary>
    void TransitionToRender();

    /// <summary>
    /// Wait for compute operations to complete before rendering.
    /// </summary>
    void WaitForComputeComplete();
}

/// <summary>
/// Event args for module errors.
/// </summary>
public class ModuleErrorEventArgs : EventArgs
{
    public IPhysicsModule Module { get; }
    public Exception Exception { get; }
    public string Phase { get; }

    public ModuleErrorEventArgs(IPhysicsModule module, Exception exception, string phase)
    {
        Module = module;
        Exception = exception;
        Phase = phase;
    }
}

/// <summary>
/// Event args for pipeline logging.
/// </summary>
public class PipelineLogEventArgs : EventArgs
{
    public string Message { get; }
    public DateTime Timestamp { get; }

    public PipelineLogEventArgs(string message)
    {
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Manages the ordered collection of physics modules in the simulation pipeline.
/// 
/// Features:
/// - Register/unregister modules dynamically
/// - Reorder modules (move up/down)
/// - Enable/disable modules at runtime
/// - Initialize all modules at simulation start
/// - Execute all enabled modules each frame
/// - Observable collection for UI binding
/// - GPU synchronization via resource barriers
/// - Zero-copy Span execution for ISpanPhysicsModule implementations
/// - Dynamic physics parameters from UI (see PhysicsPipeline.DynamicParams.cs)
/// 
/// Thread safety: Not thread-safe. Call from UI/main thread only.
/// </summary>
public partial class PhysicsPipeline : INotifyPropertyChanged
{
    private readonly ObservableCollection<IPhysicsModule> _modules = [];
    private IGpuSyncManager? _gpuSyncManager;
    private bool _isInitialized;
    private int _executionCount;

    /// <summary>
    /// Observable collection of registered modules.
    /// Bind to this in UI for live updates.
    /// </summary>
    public ReadOnlyObservableCollection<IPhysicsModule> Modules { get; }

    /// <summary>
    /// Total number of registered modules.
    /// </summary>
    public int Count => _modules.Count;

    /// <summary>
    /// Whether InitializeAll has been called.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Number of times ExecuteFrame has been called.
    /// </summary>
    public int ExecutionCount => _executionCount;

    /// <summary>
    /// Event raised when a module encounters an error during execution.
    /// </summary>
    public event EventHandler<ModuleErrorEventArgs>? ModuleError;

    /// <summary>
    /// Event raised for logging/diagnostics.
    /// </summary>
    public event EventHandler<PipelineLogEventArgs>? Log;

    public event PropertyChangedEventHandler? PropertyChanged;

    public PhysicsPipeline()
    {
        Modules = new ReadOnlyObservableCollection<IPhysicsModule>(_modules);
        _modules.CollectionChanged += OnModulesChanged;
    }

    /// <summary>
    /// Creates pipeline with GPU sync manager for resource barrier coordination.
    /// </summary>
    /// <param name="gpuSyncManager">GPU synchronization manager (can be null for CPU-only pipelines)</param>
    public PhysicsPipeline(IGpuSyncManager? gpuSyncManager) : this()
    {
        _gpuSyncManager = gpuSyncManager;
    }

    /// <summary>
    /// Sets or updates the GPU sync manager.
    /// </summary>
    public void SetGpuSyncManager(IGpuSyncManager? gpuSyncManager)
    {
        _gpuSyncManager = gpuSyncManager;
        RaiseLog($"GPU sync manager {(gpuSyncManager is not null ? "attached" : "detached")}");
    }

    /// <summary>
    /// Registers a module at the end of the pipeline.
    /// </summary>
    public void RegisterModule(IPhysicsModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        if (_modules.Any(m => m.Name == module.Name))
        {
            RaiseLog($"Module '{module.Name}' already registered, skipping");
            return;
        }

        _modules.Add(module);
        RaiseLog($"Registered module: {module.Name} [{module.Category}]");
    }

    /// <summary>
    /// Registers a module at a specific index.
    /// </summary>
    public void RegisterModuleAt(IPhysicsModule module, int index)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        index = Math.Min(index, _modules.Count);
        _modules.Insert(index, module);
        RaiseLog($"Registered module at {index}: {module.Name}");
    }

    /// <summary>
    /// Removes a module from the pipeline.
    /// </summary>
    public bool RemoveModule(IPhysicsModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        bool removed = _modules.Remove(module);
        if (removed)
        {
            module.Cleanup();
            RaiseLog($"Removed module: {module.Name}");
        }
        return removed;
    }

    /// <summary>
    /// Removes a module by name.
    /// </summary>
    public bool RemoveModule(string name)
    {
        var module = _modules.FirstOrDefault(m => m.Name == name);
        return module is not null && RemoveModule(module);
    }

    /// <summary>
    /// Removes all modules from the pipeline.
    /// </summary>
    public void Clear()
    {
        foreach (var module in _modules)
        {
            module.Cleanup();
        }
        _modules.Clear();
        _isInitialized = false;
        _executionCount = 0;
        RaiseLog("Pipeline cleared");
    }

    /// <summary>
    /// Moves a module up (earlier execution) in the pipeline.
    /// </summary>
    public bool MoveUp(IPhysicsModule module)
    {
        int index = _modules.IndexOf(module);
        if (index <= 0) return false;

        _modules.Move(index, index - 1);
        return true;
    }

    /// <summary>
    /// Moves a module down (later execution) in the pipeline.
    /// </summary>
    public bool MoveDown(IPhysicsModule module)
    {
        int index = _modules.IndexOf(module);
        if (index < 0 || index >= _modules.Count - 1) return false;

        _modules.Move(index, index + 1);
        return true;
    }

    /// <summary>
    /// Moves a module to a specific index.
    /// </summary>
    public bool MoveTo(IPhysicsModule module, int newIndex)
    {
        int currentIndex = _modules.IndexOf(module);
        if (currentIndex < 0) return false;

        newIndex = Math.Clamp(newIndex, 0, _modules.Count - 1);
        if (currentIndex == newIndex) return false;

        _modules.Move(currentIndex, newIndex);
        return true;
    }

    /// <summary>
    /// Gets a module by name.
    /// </summary>
    public IPhysicsModule? GetModule(string name)
        => _modules.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets all modules in a category.
    /// </summary>
    public IEnumerable<IPhysicsModule> GetModulesByCategory(string category)
        => _modules.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets all enabled modules in execution order.
    /// </summary>
    public IEnumerable<IPhysicsModule> GetEnabledModules()
        => _modules.Where(m => m.IsEnabled);

    /// <summary>
    /// Initializes all enabled modules. Call once at simulation start.
    /// </summary>
    public void InitializeAll(RQGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        RaiseLog($"Initializing {_modules.Count(m => m.IsEnabled)} enabled modules...");

        foreach (var module in _modules.Where(m => m.IsEnabled))
        {
            try
            {
                module.Initialize(graph);
                RaiseLog($"  Initialized: {module.Name}");
            }
            catch (Exception ex)
            {
                RaiseError(module, ex, "Initialize");
                // Continue with other modules
            }
        }

        _isInitialized = true;
        _executionCount = 0;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInitialized)));
    }

    /// <summary>
    /// Executes all enabled modules for one simulation frame.
    /// Modules are executed in order: Stage (Preparation->Forces->Integration->PostProcess), 
    /// then by ModuleGroup (grouped modules execute atomically), then Priority within each stage.
    /// GPU modules are wrapped with resource barrier transitions.
    /// </summary>
    public void ExecuteFrame(RQGraph graph, double dt)
    {
        ArgumentNullException.ThrowIfNull(graph);

        // Get enabled modules sorted by Stage, then ModuleGroup, then Priority
        var enabledModules = _modules
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.Stage)
            .ThenBy(m => m.ModuleGroup ?? string.Empty) // Ungrouped modules come first
            .ThenBy(m => m.Priority)
            .ToList();

        // Group modules by Stage and ModuleGroup for atomic execution
        var groupedByStage = enabledModules
            .GroupBy(m => m.Stage)
            .OrderBy(g => g.Key);

        foreach (var stageGroup in groupedByStage)
        {
            // Within each stage, process by module groups
            var moduleGroups = stageGroup
                .GroupBy(m => m.ModuleGroup ?? $"__ungrouped_{m.Name}") // Each ungrouped module is its own "group"
                .ToList();

            foreach (var group in moduleGroups)
            {
                var modulesInGroup = group.OrderBy(m => m.Priority).ToList();
                bool isRealGroup = modulesInGroup.Count > 1 || 
                                   (modulesInGroup.Count == 1 && modulesInGroup[0].ModuleGroup != null);
                
                // Determine group execution mode (use first module's setting)
                var groupMode = modulesInGroup.FirstOrDefault()?.GroupMode ?? GroupExecutionMode.Sequential;
                
                // Separate by execution type
                var gpuModules = modulesInGroup.Where(m => m.ExecutionType == ExecutionType.GPU).ToList();
                var cpuModules = modulesInGroup.Where(m => m.ExecutionType == ExecutionType.SynchronousCPU).ToList();
                var asyncModules = modulesInGroup.Where(m => m.ExecutionType == ExecutionType.AsynchronousTask).ToList();

                // Execute GPU modules with barrier protection
                if (gpuModules.Count > 0)
                {
                    _gpuSyncManager?.TransitionToCompute();
                    
                    if (groupMode == GroupExecutionMode.Parallel && gpuModules.Count > 1)
                    {
                        // GPU modules run sequentially on same device, but can overlap with CPU work
                        foreach (var module in gpuModules)
                        {
                            ExecuteModuleSafe(module, graph, dt);
                        }
                    }
                    else
                    {
                        foreach (var module in gpuModules)
                        {
                            ExecuteModuleSafe(module, graph, dt);
                        }
                    }
                    
                    _gpuSyncManager?.TransitionToRender();
                    _gpuSyncManager?.WaitForComputeComplete();
                }

                // Execute CPU modules based on group mode
                if (groupMode == GroupExecutionMode.Parallel && cpuModules.Count > 1)
                {
                    // Parallel execution within group
                    var tasks = cpuModules.Select(module =>
                        Task.Run(() => ExecuteModuleSafe(module, graph, dt))
                    ).ToArray();
                    Task.WaitAll(tasks);
                }
                else
                {
                    // Sequential execution
                    foreach (var module in cpuModules)
                    {
                        ExecuteModuleSafe(module, graph, dt);
                    }
                }

                // Execute async modules (always parallel within group)
                if (asyncModules.Count > 0)
                {
                    var tasks = asyncModules.Select(module =>
                        Task.Run(() => ExecuteModuleSafe(module, graph, dt))
                    ).ToArray();
                    Task.WaitAll(tasks); // Wait for group completion (atomic)
                }
            }
        }

        _executionCount++;
    }

    /// <summary>
    /// Executes all enabled modules for one frame, async version.
    /// </summary>
    public async Task ExecuteFrameAsync(RQGraph graph, double dt, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(graph);

        // Get enabled modules sorted by Stage then Priority
        var enabledModules = _modules
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.Stage)
            .ThenBy(m => m.Priority)
            .ToList();

        var gpuModules = enabledModules.Where(m => m.ExecutionType == ExecutionType.GPU).ToList();
        var syncModules = enabledModules.Where(m => m.ExecutionType == ExecutionType.SynchronousCPU);
        var asyncModules = enabledModules.Where(m => m.ExecutionType == ExecutionType.AsynchronousTask);

        // GPU first with barriers
        if (gpuModules.Count > 0)
        {
            _gpuSyncManager?.TransitionToCompute();

            foreach (var module in gpuModules)
            {
                ct.ThrowIfCancellationRequested();
                ExecuteModuleSafe(module, graph, dt);
            }

            _gpuSyncManager?.TransitionToRender();
            _gpuSyncManager?.WaitForComputeComplete();
        }

        // Sync CPU
        foreach (var module in syncModules)
        {
            ct.ThrowIfCancellationRequested();
            ExecuteModuleSafe(module, graph, dt);
        }

        // Async concurrently
        var tasks = asyncModules.Select(module =>
            Task.Run(() => ExecuteModuleSafe(module, graph, dt), ct)
        ).ToArray();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        _executionCount++;
    }

    /// <summary>
    /// Cleans up all modules. Call when simulation stops.
    /// </summary>
    public void CleanupAll()
    {
        foreach (var module in _modules)
        {
            try
            {
                module.Cleanup();
            }
            catch (Exception ex)
            {
                RaiseError(module, ex, "Cleanup");
            }
        }
        _isInitialized = false;
        RaiseLog("Pipeline cleanup complete");
    }

    /// <summary>
    /// Sorts modules by execution stage and priority.
    /// Order: Stage (Preparation -> Forces -> Integration -> PostProcess), then Priority (lower first).
    /// </summary>
    public void SortByPriority()
    {
        var sorted = _modules
            .OrderBy(m => m.Stage)
            .ThenBy(m => m.Priority)
            .ToList();

        _modules.Clear();
        foreach (var module in sorted)
        {
            _modules.Add(module);
        }
        
        RaiseLog("Pipeline sorted by stage and priority");
    }

    /// <summary>
    /// Sorts modules by execution stage, execution type, then priority.
    /// Use this for deterministic ordering that also groups by execution type.
    /// </summary>
    public void SortByStageAndType()
    {
        var sorted = _modules
            .OrderBy(m => m.Stage)
            .ThenBy(m => m.ExecutionType)
            .ThenBy(m => m.Priority)
            .ToList();

        _modules.Clear();
        foreach (var module in sorted)
        {
            _modules.Add(module);
        }
        
        RaiseLog("Pipeline sorted by stage, type, and priority");
    }

    private void ExecuteModuleSafe(IPhysicsModule module, RQGraph graph, double dt)
    {
        try
        {
            // Use zero-copy Span execution if module supports it
            if (module is ISpanPhysicsModule spanModule && graph.EdgePhaseU1 is not null)
            {
                // Get Span views of the underlying arrays for zero-copy access
                Span<double> weightsSpan = graph.Weights.AsSpan();
                Span<double> phasesSpan = graph.EdgePhaseU1.AsSpan();
                ReadOnlySpan<bool> edgesSpan = graph.Edges.AsReadOnlySpan();
                
                spanModule.ExecuteSpan(weightsSpan, phasesSpan, edgesSpan, graph.N, dt);
            }
            else
            {
                // Fall back to standard execution
                module.ExecuteStep(graph, dt);
            }
        }
        catch (Exception ex)
        {
            RaiseError(module, ex, "ExecuteStep");
        }
    }

    private void RaiseError(IPhysicsModule module, Exception ex, string phase)
    {
        ModuleError?.Invoke(this, new ModuleErrorEventArgs(module, ex, phase));
        RaiseLog($"ERROR in {module.Name}.{phase}: {ex.Message}");
    }

    private void RaiseLog(string message)
    {
        Log?.Invoke(this, new PipelineLogEventArgs(message));
    }

    private void OnModulesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }
}
