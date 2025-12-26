# Render Integration Notes (UI)

This document describes where to plug the `ProcessesDispatcher` data stream into rendering without breaking existing in-process simulation.

## Current state
- `LifeCycleManager` ensures `RqSimConsole` (server-mode) is started/attached on UI startup and can be gracefully shut down.
- Shared-data transport for rendering exists (`IPC/DataReader.cs`), but existing render code still draws from in-process graph.

## Recommended integration points
1. Create a `DataReader` instance owned by the render backend (or `Form_Main`) with explicit `Dispose()` when backend is stopped.
2. When render loop needs data:
   - if `DataReader.IsConnected == false` -> `TryConnect()`.
   - `TryReadHeader(out header)` -> read `NodeCount`.
   - `ReadNodesArray(header.NodeCount)` -> build render buffers.
3. If `SimState.IsStale(maxAge)` -> show "paused/stalled" indicator.

## Non-breaking strategy
- Keep the existing in-process rendering path as default.
- Add a feature toggle (checkbox/menu) later to choose source:
  - `InProcessGraph` (current)
  - `ExternalProcessStream` (DataReader)

## Backend requirements
- Simulation backend must create shared memory map and write:
  - `SharedHeader` at offset 0
  - `RenderNode[]` immediately after header
- Backend must update `LastUpdateTimestampUtcTicks` (UTC ticks) every data publish.
