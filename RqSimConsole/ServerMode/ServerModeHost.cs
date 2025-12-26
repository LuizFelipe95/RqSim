using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using RQSimulation;
using RQSimulation.Core.Infrastructure;
using RQSimulation.Core.Scheduler;
using RQSimulation.GPUOptimized;

namespace RqSimConsole.ServerMode;

internal sealed partial class ServerModeHost : IDisposable
{
    private const string ControlPipeName = "RqSim_Control_Pipe";
    private const string SharedMemoryMapName = "RqSim_Shared_Memory";
    private const long SharedMemoryCapacityBytes = 50L * 1024L * 1024L;

    private static readonly int HeaderSize = System.Runtime.InteropServices.Marshal.SizeOf<SharedHeader>();
    private static readonly int RenderNodeSize = System.Runtime.InteropServices.Marshal.SizeOf<RenderNode>();

    private readonly CancellationTokenSource _shutdownCts = new();
    private volatile bool _running = true;

    // Simulation state - determines if real simulation is active
    private volatile bool _simulationActive = false;
    private volatile SimulationStatus _currentStatus = SimulationStatus.Stopped;

    // Multi-GPU infrastructure
    private ComputeCluster? _cluster;
    private AsyncAnalysisOrchestrator? _orchestrator;
    private RQGraph? _graph;
    private OptimizedGpuSimulationEngine? _physicsEngine;
    private long _currentTick;
    private int _snapshotInterval = 100;

    private ServerModeSettingsDto _settings = ServerModeSettingsDto.Default;
    private bool _useFallbackSimulation;

    // Latest results cache
    private double _latestSpectralDim = double.NaN;
    private double _latestMcmcEnergy = double.NaN;

    // Cache render nodes to avoid per-tick allocations
    private RenderNode[]? _renderNodesBuffer;

    private static readonly JsonSerializerOptions PipeJsonOptions = new(JsonSerializerDefaults.General)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private bool _physicsInitAttempted;

    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownCts.Token);

        // Initialize Multi-GPU cluster
        InitializeMultiGpuCluster();

        using MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(
            SharedMemoryMapName,
            SharedMemoryCapacityBytes,
            MemoryMappedFileAccess.ReadWrite);

        using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, SharedMemoryCapacityBytes, MemoryMappedFileAccess.ReadWrite);

        var publishTask = Task.Run(() => PublishLoop(accessor, linkedCts.Token), linkedCts.Token);
        var pipeTask = Task.Run(() => PipeLoopAsync(linkedCts.Token), linkedCts.Token);

        try
        {
            await Task.WhenAll(publishTask, pipeTask).ConfigureAwait(false);
            return 0;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        finally
        {
            DisposeMultiGpuCluster();
        }
    }

    private void InitializeMultiGpuCluster()
    {
        try
        {
            _cluster = new ComputeCluster();
            _cluster.Initialize();

            if (_cluster.IsMultiGpuAvailable)
            {
                _orchestrator = new AsyncAnalysisOrchestrator(_cluster);
                _orchestrator.Initialize(100_000);

                // Subscribe to results
                _orchestrator.SpectralCompleted += OnSpectralCompleted;
                _orchestrator.McmcCompleted += OnMcmcCompleted;

                Console.WriteLine($"[ServerMode] Multi-GPU cluster initialized: {_cluster.TotalGpuCount} GPUs");
                Console.WriteLine($"[ServerMode] Physics device: {_cluster.PhysicsDevice?.Name}");
                Console.WriteLine($"[ServerMode] Spectral workers: {_cluster.SpectralWorkers.Length}");
                Console.WriteLine($"[ServerMode] MCMC workers: {_cluster.McmcWorkers.Length}");
            }
            else
            {
                Console.WriteLine($"[ServerMode] Single GPU mode: {_cluster.PhysicsDevice?.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ServerMode] GPU init warning: {ex.Message}");
            _cluster?.Dispose();
            _cluster = null;
        }
    }

    private void DisposeMultiGpuCluster()
    {
        if (_orchestrator != null)
        {
            _orchestrator.SpectralCompleted -= OnSpectralCompleted;
            _orchestrator.McmcCompleted -= OnMcmcCompleted;
            _orchestrator.Dispose();
            _orchestrator = null;
        }

        _physicsEngine?.Dispose();
        _physicsEngine = null;

        _cluster?.Dispose();
        _cluster = null;
    }

    private void OnSpectralCompleted(object? sender, SpectralResultEventArgs e)
    {
        if (e.Result.IsValid)
        {
            _latestSpectralDim = e.Result.SpectralDimension;
            Console.WriteLine($"[ServerMode] Spectral d_s={e.Result.SpectralDimension:F4} (worker {e.Result.WorkerId})");
        }
    }

    private void OnMcmcCompleted(object? sender, McmcResultEventArgs e)
    {
        _latestMcmcEnergy = e.Result.MeanEnergy;
        Console.WriteLine($"[ServerMode] MCMC E={e.Result.MeanEnergy:F4} (worker {e.Result.WorkerId}, T={e.Result.Temperature:F2})");
    }

    private async Task PipeLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Use NamedPipeServerStream.MaxAllowedServerInstances to allow reconnections
                await using NamedPipeServerStream server = new(
                    pipeName: ControlPipeName,
                    direction: PipeDirection.In,
                    maxNumberOfServerInstances: NamedPipeServerStream.MaxAllowedServerInstances,
                    transmissionMode: PipeTransmissionMode.Byte, // Changed from Message for compatibility
                    options: PipeOptions.Asynchronous);

                Console.WriteLine("[ServerMode] Waiting for client connection...");
                await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine("[ServerMode] Client connected!");

                using StreamReader reader = new(server);

                while (server.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                    if (line is null)
                        break;

                    SimCommand? cmd;
                    try
                    {
                        cmd = JsonSerializer.Deserialize<SimCommand>(line, PipeJsonOptions);
                    }
                    catch (JsonException)
                    {
                        continue;
                    }

                    if (cmd is null)
                        continue;

                    switch (cmd.Type)
                    {
                        case SimCommandType.Shutdown:
                            _running = false;
                            _shutdownCts.Cancel();
                            return;

                        case SimCommandType.Handshake:
                            // Report current status - do NOT auto-start simulation
                            // But make sure data plane has something for UI to attach to.
                            // EnsureSimulationInitialized(); // REMOVED: Prevent auto-start of default graph
                            Console.WriteLine($"[ServerMode] Handshake received. Current status: {_currentStatus}");
                            break;

                        case SimCommandType.Start:
                            // Only start simulation on explicit command
                            EnsureSimulationInitialized();

                            // If GPU engine failed, we still allow a lightweight "fallback" run mode so UI iteration updates work.
                            if (_physicsEngine is null)
                            {
                                _useFallbackSimulation = true;
                                if (_currentStatus == SimulationStatus.Faulted)
                                {
                                    Console.WriteLine("[ServerMode] GPU physics unavailable; switching to fallback simulation mode.");
                                    _currentStatus = SimulationStatus.Stopped;
                                }
                            }
 
                             if (!_simulationActive)
                             {
                                 Console.WriteLine("[ServerMode] >>> START command received <<<");
                                 _simulationActive = true;
                                 _currentStatus = SimulationStatus.Running;
                                 Console.WriteLine("[ServerMode] Simulation STARTED by UI command");
                             }
                             else
                             {
                                 // Already active - resume from pause
                                 _currentStatus = SimulationStatus.Running;
                                 Console.WriteLine("[ServerMode] Simulation RESUMED by UI command");
                             }
                             break;

                        case SimCommandType.Stop:
                            _simulationActive = false;
                            _currentStatus = SimulationStatus.Stopped;
                            _useFallbackSimulation = false;
                            Console.WriteLine("[ServerMode] Simulation STOPPED");
                            break;

                        case SimCommandType.UpdateSettings:
                            if (!string.IsNullOrEmpty(cmd.PayloadJson))
                            {
                                if (TryApplySettings(cmd.PayloadJson))
                                {
                                    Console.WriteLine($"[ServerMode] Settings applied: Nodes={_settings.NodeCount} Seed={_settings.Seed} Degree={_settings.TargetDegree}");
                                }
                                else
                                {
                                    Console.WriteLine("[ServerMode] Settings update rejected (invalid payload)");
                                }
                            }
                            break;

                        case SimCommandType.GetMultiGpuStatus:
                            Console.WriteLine("[ServerMode] Multi-GPU status requested");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ServerMode] Pipe loop exception: {ex.Message}");
            }
        }
    }

    private bool TryApplySettings(string payloadJson)
    {
        try
        {
            var settings = JsonSerializer.Deserialize<ServerModeSettingsDto>(payloadJson, PipeJsonOptions);
            if (settings is null)
                return false;

            // Clamp for safety
            int nodeCount = settings.NodeCount <= 0 ? ServerModeSettingsDto.Default.NodeCount : settings.NodeCount;
            if (nodeCount > 2_000_000)
                nodeCount = 2_000_000;

            int targetDegree = settings.TargetDegree <= 0 ? ServerModeSettingsDto.Default.TargetDegree : settings.TargetDegree;
            if (targetDegree > 256)
                targetDegree = 256;

            var newSettings = new ServerModeSettingsDto
            {
                NodeCount = nodeCount,
                TargetDegree = targetDegree,
                Seed = settings.Seed,
                Temperature = settings.Temperature <= 0 ? ServerModeSettingsDto.Default.Temperature : settings.Temperature
            };

            // Check if settings actually changed
            bool nodeCountChanged = _settings.NodeCount != newSettings.NodeCount;
            bool seedChanged = _settings.Seed != newSettings.Seed;
            bool degreeChanged = _settings.TargetDegree != newSettings.TargetDegree;
            
            _settings = newSettings;

            // Reinitialize graph if node count, seed, or degree changed
            // Do this regardless of simulation status - the graph needs to match settings
            if (nodeCountChanged || seedChanged || degreeChanged)
            {
                Console.WriteLine($"[ServerMode] Settings changed: Nodes={nodeCount} (was {(nodeCountChanged ? "different" : "same")}), reinitializing graph...");
                ReinitializeSimulation();
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private void ReinitializeSimulation()
    {
        _simulationActive = false;
        _currentStatus = SimulationStatus.Stopped;
        _currentTick = 0;

        _physicsEngine?.Dispose();
        _physicsEngine = null;
        _physicsInitAttempted = false;
        _useFallbackSimulation = false;

        _graph = null;
        _renderNodesBuffer = null;

        EnsureSimulationInitialized();
    }

    private void EnsureSimulationInitialized()
    {
        // Initialize a basic graph + engine as soon as UI connects, so shared memory contains drawable data.
        if (_graph is null)
        {
            Console.WriteLine("[ServerMode] Initializing simulation graph...");
            try
            {
                // RQGraph ctor: RQGraph(int, double, double, int, double, double, double, double, int)
                const double minWeight = 0.05;
                const double maxWeight = 1.0;
                const double topologyMutationRate = 0.01;
                const double fieldDt = 0.01;
                const double gravityDt = 0.01;

                int n = Math.Max(1, _settings.NodeCount);
                int seed = _settings.Seed;
                double temperature = _settings.Temperature;
                int targetDegree = Math.Max(1, _settings.TargetDegree);

                _graph = new RQGraph(
                    n,
                    minWeight,
                    maxWeight,
                    seed,
                    temperature,
                    topologyMutationRate,
                    fieldDt,
                    gravityDt,
                    targetDegree);

                Console.WriteLine($"[ServerMode] Graph initialized: {_graph.N} nodes, {_graph.FlatEdgesFrom?.Length ?? 0} edges");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ServerMode] ERROR initializing graph: {ex.Message}");
                _currentStatus = SimulationStatus.Faulted;
                return;
            }
        }

        // Only attempt to create the physics engine once per process lifetime.
        // If it fails (e.g., missing GPU capability), we still keep the server attachable and publish shared memory.
        if (_physicsEngine is null && _graph != null && !_physicsInitAttempted)
        {
            _physicsInitAttempted = true;

            Console.WriteLine("[ServerMode] Initializing physics engine...");
            try
            {
                _physicsEngine = new OptimizedGpuSimulationEngine(_graph);
                _physicsEngine.Initialize();
                _physicsEngine.UploadState();
                Console.WriteLine("[ServerMode] Physics engine initialized.");
            }
            catch (Exception ex)
            {
                ConsoleExceptionLogger.Log("[ServerMode] ERROR initializing physics engine:", ex);
                _physicsEngine?.Dispose();
                _physicsEngine = null;

                // Keep graph alive for UI visualization and mark faulted so UI can reflect it.
                _currentStatus = SimulationStatus.Faulted;
            }
        }

        if (_currentStatus == SimulationStatus.Unknown)
            _currentStatus = SimulationStatus.Stopped;
    }

    private void EnsureRenderBuffer(int nodeCount)
    {
        if (_renderNodesBuffer is null || _renderNodesBuffer.Length < nodeCount)
        {
            _renderNodesBuffer = new RenderNode[nodeCount];
        }
    }

    private static void FillRenderNodes(RQGraph graph, RenderNode[] buffer)
    {
        // NOTE: We don't have a stable "position" API exposed from RQGraph here. For now, we generate a deterministic layout
        // based on node id so UI has visible output and can validate IPC + reconnection + running state.
        // This should be replaced with real positions once the physics engine exposes them.
        int n = graph.N;
        for (int i = 0; i < n; i++)
        {
            float angle = (float)(i * (Math.Tau / Math.Max(1, n)));
            buffer[i] = new RenderNode
            {
                X = MathF.Cos(angle) * 10f,
                Y = MathF.Sin(angle) * 10f,
                Z = 0f,
                R = 0.2f,
                G = 0.8f,
                B = 1.0f,
                Id = i
            };
        }
    }

    private static void FillRenderNodes(RQGraph graph, RenderNode[] buffer, long iteration)
    {
        int n = graph.N;
        
        // Check if we have valid coordinate sources
        // We need to verify arrays exist AND have valid (non-zero, non-NaN) data
        bool hasSpectral = graph.SpectralX is not null && 
                           graph.SpectralX.Length == n &&
                           graph.SpectralY is not null &&
                           graph.SpectralZ is not null;
        
        // Quick check if spectral data looks valid (check first non-zero or varied data)
        if (hasSpectral)
        {
            bool anyNonZero = false;
            for (int check = 0; check < Math.Min(10, n); check++)
            {
                if (graph.SpectralX![check] != 0 || graph.SpectralY![check] != 0)
                {
                    anyNonZero = true;
                    break;
                }
            }
            if (!anyNonZero) hasSpectral = false;
        }
        
#pragma warning disable CS0618 // Coordinates is obsolete but needed for visualization
        bool hasCoords = graph.Coordinates is not null && 
                         graph.Coordinates.Length == n;
        
        if (hasCoords)
        {
            bool anyNonZero = false;
            for (int check = 0; check < Math.Min(10, n); check++)
            {
                if (graph.Coordinates[check].X != 0 || graph.Coordinates[check].Y != 0)
                {
                    anyNonZero = true;
                    break;
                }
            }
            if (!anyNonZero) hasCoords = false;
        }
#pragma warning restore CS0618
        
        for (int i = 0; i < n; i++)
        {
            float x, y, z;
            
            if (hasSpectral && !double.IsNaN(graph.SpectralX![i]))
            {
                // Use spectral coordinates (3D)
                x = (float)graph.SpectralX![i];
                y = (float)graph.SpectralY![i];
                z = (float)graph.SpectralZ![i];
            }
            else if (hasCoords)
            {
#pragma warning disable CS0618
                x = (float)graph.Coordinates[i].X;
                y = (float)graph.Coordinates[i].Y;
                z = 0f;
#pragma warning restore CS0618
            }
            else
            {
                // Fallback: generate circular layout with animation
                float t = (float)(iteration * 0.01);
                float angle = (float)(i * (Math.Tau / Math.Max(1, n)));
                x = MathF.Cos(angle + t) * 10f;
                y = MathF.Sin(angle + t) * 10f;
                z = 0f;
            }
            
            // Color based on node state
            float r, g, b;
            var state = graph.State[i];
            switch (state)
            {
                case NodeState.Excited:
                    r = 1.0f; g = 0.3f; b = 0.1f; // Orange/red for excited
                    break;
                case NodeState.Refractory:
                    r = 0.3f; g = 0.3f; b = 0.8f; // Blue for refractory
                    break;
                default: // Rest
                    r = 0.2f; g = 0.8f; b = 0.2f; // Green for rest
                    break;
            }
            
            buffer[i] = new RenderNode
            {
                X = x,
                Y = y,
                Z = z,
                R = r,
                G = g,
                B = b,
                Id = i
            };
        }
    }

    public void Dispose()
    {
        DisposeMultiGpuCluster();
        _shutdownCts.Dispose();
    }
}

// Simulation status enum for shared memory
public enum SimulationStatus
{
    Unknown = 0,
    Running = 1,
    Paused = 2,
    Stopped = 3,
    Faulted = 4
}
