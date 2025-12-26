using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;
using RQSimulation;

namespace RqSimConsole.ServerMode;

internal sealed partial class ServerModeHost
{
    private const int FallbackTickIntervalMs = 50;

    private void PublishLoop(MemoryMappedViewAccessor accessor, CancellationToken cancellationToken)
    {
        long iteration = 0;
        long lastLoggedIteration = -1;
        long lastLogUtcTicks = 0;
        long lastSimStepUtcTicks = 0;

        while (_running && !cancellationToken.IsCancellationRequested)
        {
            if (_simulationActive && _currentStatus == SimulationStatus.Running)
            {
                if (!_useFallbackSimulation)
                {
                    iteration++;
                }
                else
                {
                    var now = DateTimeOffset.UtcNow.UtcTicks;
                    if (lastSimStepUtcTicks == 0)
                        lastSimStepUtcTicks = now;

                    if (now - lastSimStepUtcTicks >= TimeSpan.FromMilliseconds(FallbackTickIntervalMs).Ticks)
                    {
                        iteration++;
                        lastSimStepUtcTicks = now;
                    }
                }

                if (_physicsEngine != null)
                {
                    try
                    {
                        _physicsEngine.StepGpuBatch(
                            batchSize: 10,
                            dt: (float)PhysicsConstants.BaseTimestep,
                            G: (float)PhysicsConstants.GravitationalCoupling,
                            lambda: (float)PhysicsConstants.CosmologicalConstant,
                            degreePenalty: (float)PhysicsConstants.DegreePenaltyFactor,
                            diffusionRate: (float)PhysicsConstants.FieldDiffusionRate,
                            higgsLambda: (float)PhysicsConstants.HiggsLambda,
                            higgsMuSquared: (float)PhysicsConstants.HiggsMuSquared);
                    }
                    catch (Exception ex)
                    {
                        ConsoleExceptionLogger.Log("[ServerMode] Physics step error:", ex);
                        _currentStatus = SimulationStatus.Faulted;
                        _simulationActive = false;
                    }
                }
                else if (_useFallbackSimulation)
                {
                    // No physics engine available; keep iteration advancing so UI sees progress.
                    // Graph topology is static; rendering can still animate via time-based layout.
                }
            }

            _currentTick = iteration;

            var nowTicks = DateTimeOffset.UtcNow.UtcTicks;

            var status = _orchestrator?.GetStatus();

            int nodeCount = _graph?.N ?? 0;
            int edgeCount = _graph?.FlatEdgesFrom?.Length ?? 0;
            
            // Compute real metrics from graph
            double systemEnergy = 0.0;
            double spectralDim = 0.0;
            int excitedCount = 0;
            double heavyMass = 0.0;
            int largestCluster = 0;
            int strongEdgeCount = 0;
            double qNorm = 0.0;
            double entanglement = 0.0;
            double correlation = 0.0;
            double networkTemp = 0.0;
            double effectiveG = 0.0;
            
            if (_graph is not null)
            {
                // Count excited nodes
                for (int i = 0; i < _graph.N; i++)
                {
                    if (_graph.State[i] == NodeState.Excited)
                        excitedCount++;
                }
                
                // Get spectral dimension if available (protect against NaN)
                spectralDim = _graph.SmoothedSpectralDimension;
                if (double.IsNaN(spectralDim) || double.IsInfinity(spectralDim))
                    spectralDim = 0.0;
                
                // Compute energy metric (sum of weights)
                if (_graph.FlatEdgesFrom is not null && _graph.Weights is not null)
                {
                    int edgeLen = _graph.FlatEdgesFrom.Length;
                    for (int e = 0; e < edgeLen; e++)
                    {
                        int i = _graph.FlatEdgesFrom[e];
                        int j = _graph.FlatEdgesTo[e];
                        double w = _graph.Weights[i, j];
                        if (!double.IsNaN(w))
                        {
                            systemEnergy += w;
                            if (w > 0.7) strongEdgeCount++;
                        }
                    }
                }
                
                // Get heavy mass from correlation mass
                var correlationMass = _graph.CorrelationMass;
                if (correlationMass is not null)
                {
                    for (int i = 0; i < Math.Min(_graph.N, correlationMass.Length); i++)
                    {
                        if (!double.IsNaN(correlationMass[i]))
                            heavyMass += correlationMass[i];
                    }
                }
                
                // Get largest cluster (with exception protection)
                try
                {
                    var threshold = _graph.GetAdaptiveHeavyThreshold();
                    if (!double.IsNaN(threshold) && threshold > 0)
                    {
                        var clusters = _graph.GetStrongCorrelationClusters(threshold);
                        if (clusters.Count > 0)
                        {
                            largestCluster = clusters.Max(c => c.Count);
                        }
                    }
                }
                catch
                {
                    // Cluster computation can fail on malformed graphs
                    largestCluster = 0;
                }
                
                // Get quantum metrics if available (with NaN protection)
                try
                {
                    qNorm = _graph.ComputeAvgPairCorrelation();
                    if (double.IsNaN(qNorm) || double.IsInfinity(qNorm))
                        qNorm = 0.0;
                    entanglement = qNorm; // Same metric
                    correlation = qNorm;
                }
                catch
                {
                    qNorm = entanglement = correlation = 0.0;
                }
                
                // Network temperature
                networkTemp = _graph.NetworkTemperature;
                if (double.IsNaN(networkTemp) || double.IsInfinity(networkTemp))
                    networkTemp = 1.0;
            }

            int maxNodesThatFit = (int)Math.Max(0, (SharedMemoryCapacityBytes - HeaderSize) / Math.Max(1, RenderNodeSize));
            if (nodeCount > maxNodesThatFit)
                nodeCount = maxNodesThatFit;

            SharedHeader header = new()
            {
                Iteration = iteration,
                NodeCount = nodeCount,
                EdgeCount = edgeCount,
                SystemEnergy = systemEnergy,
                StateCode = (int)_currentStatus,
                LastUpdateTimestampUtcTicks = nowTicks,

                GpuClusterSize = _cluster?.TotalGpuCount ?? 1,
                BusySpectralWorkers = status?.BusySpectralWorkers ?? 0,
                BusyMcmcWorkers = status?.BusyMcmcWorkers ?? 0,
                LatestSpectralDimension = spectralDim > 0 ? spectralDim : _latestSpectralDim,
                LatestMcmcEnergy = _latestMcmcEnergy,
                TotalSpectralResults = status?.TotalSpectralResults ?? 0,
                TotalMcmcResults = status?.TotalMcmcResults ?? 0,
                
                // Extended metrics
                ExcitedCount = excitedCount,
                HeavyMass = heavyMass,
                LargestCluster = largestCluster,
                StrongEdgeCount = strongEdgeCount,
                QNorm = qNorm,
                Entanglement = entanglement,
                Correlation = correlation,
                NetworkTemperature = networkTemp,
                EffectiveG = effectiveG
            };

            accessor.Write(0, ref header);

            if (_graph != null && nodeCount > 0)
            {
                EnsureRenderBuffer(nodeCount);
                FillRenderNodes(_graph, _renderNodesBuffer!, iteration);
                accessor.WriteArray(HeaderSize, _renderNodesBuffer!, 0, nodeCount);
            }
            
            // Diagnostic logging (throttled to every 2 seconds)
            if (iteration != lastLoggedIteration && nowTicks - lastLogUtcTicks >= TimeSpan.FromSeconds(2).Ticks)
            {
                string coordSource = "None";
                if (_graph is not null)
                {
                    bool hasSpectral = _graph.SpectralX is not null && _graph.SpectralX.Length == _graph.N;
#pragma warning disable CS0618
                    bool hasCoords = _graph.Coordinates is not null && _graph.Coordinates.Length == _graph.N;
#pragma warning restore CS0618
                    coordSource = hasSpectral ? "Spectral3D" : (hasCoords ? "Coords2D" : "Fallback");
                }
                
                Console.WriteLine($"[ServerMode] Tick={iteration} Status={_currentStatus} Nodes={nodeCount} Coords={coordSource} Excited={excitedCount} StrongEdges={strongEdgeCount} SpectralDim={spectralDim:F3}");
                lastLoggedIteration = iteration;
                lastLogUtcTicks = nowTicks;
            }

            if (_simulationActive && _orchestrator != null && _physicsEngine != null && iteration % _snapshotInterval == 0)
            {
                try
                {
                    var snapshot = _physicsEngine.DownloadSnapshot(iteration);
                    _orchestrator.OnPhysicsStepCompleted(snapshot);
                }
                catch
                {
                }
            }

            try
            {
                Task.Delay(50, cancellationToken).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
