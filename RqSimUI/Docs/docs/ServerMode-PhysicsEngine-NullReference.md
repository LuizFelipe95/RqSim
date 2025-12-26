# ServerMode: Physics Engine and Visualization Integration

## Summary
This document describes the ServerMode architecture for running `RqSimConsole` as a backend process with `RqSimUI` as a visualization frontend connected via shared memory.

## Current Status: ?? In Progress
- Physics engine initializes successfully
- Graph data is computed and published to shared memory
- UI connects and receives iteration/node counts
- **Known Issue**: Coordinates show as all zeros - fallback circular layout should display instead

## Known Issues

### Issue: All coordinates = 0, single point displayed
**Symptom**: `[CSR External] Loaded 400 nodes from shared memory. X=[0.00,0.00], Y=[0.00,0.00]`

**Root Cause**: The graph does not have `SpectralX/Y/Z` or `Coordinates` computed. The server-mode creates a fresh `RQGraph` but doesn't run the spectral embedding or force-directed layout that would populate these arrays.

**Fix Applied**: `FillRenderNodes()` now:
1. Validates coordinate arrays by checking if first 10 nodes have non-zero data
2. Falls back to circular animated layout if no valid coordinates exist
3. Coordinates: `X = cos(angle + t) * 10`, `Y = sin(angle + t) * 10`

**After rebuild**: Console should use fallback circular layout when coordinates are all zeros.

### Issue: Metrics show zeros or NaN
**Symptom**: SpectralDim=NaN, Excited=400, Strong Edges=0

**Root Cause**: Metrics depend on graph state that isn't being updated during simulation, or computed values return NaN.

**Fix Applied**: Added NaN/Infinity protection for all metrics in `PublishLoop`:
- `spectralDim`: Returns 0 if NaN
- `qNorm`, `entanglement`, `correlation`: Try/catch with 0 fallback
- `networkTemp`: Returns 1.0 if NaN
- `systemEnergy`, `strongEdgeCount`: Skip NaN weights

## Architecture

### IPC via Shared Memory
- **SharedHeader**: Contains iteration count, node/edge counts, and ALL extended metrics
- **RenderNode[]**: Array of node positions (X, Y, Z) and colors (R, G, B) for visualization
- **Named Pipe**: Control commands (Start, Pause, Stop, UpdateSettings)

### SharedHeader Extended Metrics
The SharedHeader includes:
- `ExcitedCount` - number of excited nodes
- `HeavyMass` - total correlation mass
- `LargestCluster` - size of largest strong correlation cluster
- `StrongEdgeCount` - edges with weight > 0.7
- `QNorm`, `Entanglement`, `Correlation` - quantum metrics
- `LatestSpectralDimension` - graph spectral dimension
- `NetworkTemperature` - network temperature
- `EffectiveG` - effective gravitational coupling

### Data Flow
1. Console initializes `RQGraph` and `OptimizedGpuSimulationEngine`
2. Physics engine runs GPU-accelerated simulation steps
3. Console computes metrics and publishes to shared memory:
   - Coordinates: SpectralX/Y/Z ? Coordinates ? **Fallback circular layout**
   - Node state coloring (Excited=orange, Refractory=blue, Rest=green)
   - Extended metrics (with NaN protection)
4. UI reads shared memory and renders

## Files Changed

### Console Side
- `RqSimConsole/ServerMode/ServerModeHost.cs`
  - `FillRenderNodes()`: Validates coordinate arrays, uses circular fallback
  
- `RqSimConsole/ServerMode/ServerModeHost.PublishLoop.cs`
  - NaN/Infinity protection for all metrics
  - Diagnostic logging with coordinate source

- `RqSimConsole/ServerMode/ServerContracts.cs`
  - Extended SharedHeader with metrics fields

### UI Side
- `RqSimUI/ProcessesDispatcher/Contracts/SharedMemoryLayout.cs` - Extended SharedHeader
- `RqSimUI/ProcessesDispatcher/Contracts/SimState.cs` - Extended with metrics
- `RqSimUI/ProcessesDispatcher/IPC/DataReader.cs` - Reads extended metrics
- `RqSimUI/Forms/PartialForms/Form_Main_Dashboard.cs` - Uses extended metrics
- `RqSimUI/Forms/3DVisual/Dx12Vulkan/PartialForm3D_CSR_Rendering.cs` - External node rendering

### Engine Side
- `RqSimGraphEngine/RQSimulation/GPUOptimized/Compute/OptimizedGpuSimulationEngine.cs`
  - Null-guard for `ScalarField` in `UploadState()`

## Debugging

### Verify Console is using new code
Look for this log format (after rebuild):
```
[ServerMode] Tick=N Status=Running Nodes=400 Coords=Fallback Excited=X StrongEdges=Y SpectralDim=Z.ZZZ
```

If you see old format (`Tick=N Status=Running Nodes=400`), the old console binary is still running.

### Verify coordinates are being published
UI log should show coordinate range:
```
[CSR External] Loaded 400 nodes from shared memory. X=[-10.00,10.00], Y=[-10.00,10.00]
```

If `X=[0.00,0.00]`, the fallback isn't working - rebuild both projects.

## Troubleshooting

### Symptoms: Only one point visible
**Check**: Rebuild `RqSimConsole` project and restart console process
**Check**: Look for `Coords=Fallback` in console logs

### Symptoms: Metrics all zeros
**Check**: SharedHeader struct matches in all 3 projects
**Check**: Rebuild all projects after struct changes

### Symptoms: Cross-thread operation error
**Observed**: `Cross-thread operation not valid: Control 'button_RunModernSim'`
**Impact**: UI control update from wrong thread - doesn't affect data
**Workaround**: Use `Invoke` for UI updates (existing issue, not critical)

## Notes for Maintainers
- SharedHeader struct MUST be identical in Console, UI, and Dx12WinForm projects
- Node colors: Orange (Excited), Blue (Refractory), Green (Rest)
- Fallback circular layout: radius=10, animates with `iteration * 0.01` rotation
- Always rebuild Console project when changing `ServerModeHost.cs`
