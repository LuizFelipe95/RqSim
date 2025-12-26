# Simulation Process Dispatcher Architecture

## Goals
- Keep simulation (backend) alive independently of UI/render crashes.
- Allow UI to reattach to a running simulation process and continue rendering.
- Support controlled shutdown: UI requests graceful stop; force-terminate only if backend does not exit.

## Processes & Responsibilities
- **UI/Render (RqSimUI)**: Visualizes state, issues control commands via named pipes, reads graph data via shared memory.
- **Simulation Backend (RqSimConsole --server-mode)**: Runs physics loop headless, exposes control pipe, publishes shared memory, keeps running when UI exits unexpectedly.

## Folder & File Map (`RqSimUI/ProcessesDispatcher`)
- `DispatcherConfig.cs` – Paths, pipe names, map names, timeouts.
- `Contracts/SimCommand.cs` – Command DTO + enum.
- `Contracts/SimState.cs` – Status/heartbeat for UI logic.
- `Contracts/SharedMemoryLayout.cs` – Blittable structs for shared memory header and render nodes.
- `IPC/IpcController.cs` – Named pipe client to send control commands (start/pause/step/settings/shutdown/handshake).
- `IPC/DataReader.cs` – Memory-mapped file reader for header + render nodes; detects stale data via timestamps.
- `Managers/SimProcessDispatcher.cs` – Starts/attaches to simulation process; detach/kill helpers; exposed status.
- `Managers/LifeCycleManager.cs` – Wires UI lifecycle (Load/Closing), handshake on attach, graceful shutdown prompt, detach logic for crash tolerance.

## Control & Data Channels
- **Named Pipe**: `DispatcherConfig.ControlPipeName` (default `RqSim_Control_Pipe`). JSON messages of `SimCommand`.
- **Shared Memory**: `DispatcherConfig.SharedMemoryName` (default `RqSim_Shared_Memory`).
  - Header: `SharedHeader` (Iteration, NodeCount, EdgeCount, SystemEnergy, StateCode, LastUpdateTimestampUtcTicks).
  - Data: contiguous array of `RenderNode` structs after header.

## Lifecycle Flow
1. **Form Load**: LifeCycleManager ensures simulation is running (attach or spawn). Sends `Handshake` command. Connects `DataReader` on demand by render backends.
2. **UI Crash/Render Crash**: Simulation continues; SimProcessDispatcher keeps no UI dependency. Render layers can dispose their reader safely; reattach later.
3. **Form Reopen**: Ensure running ? handshake; render resumes by reusing shared memory data.
4. **Planned UI Close**: Prompt user: stop simulation vs leave running. `Shutdown` command + timeout; if still alive, force kill. If leaving running, dispatcher detaches only.

## Backend (RqSimConsole) Expectations (to be implemented there)
- Start in `--server-mode` creates named pipe server + shared memory (`CreateOrOpen`).
- Main loop writes header + node array + timestamp regularly.
- Handles commands: Handshake, Start/Pause, Step, UpdateSettings(payload JSON), Shutdown.
- Keeps running if UI disconnects; accepts reconnections.

## Integration Points in UI
- Wire `LifeCycleManager` into `Form_Main` Load / FormClosing events.
- Render backends can consume `DataReader` to fetch header + nodes for drawing instead of in-process graph instance.

## Safety & Monitoring
- `SimProcessDispatcher.IsConnected` guards UI operations.
- `DataReader` exposes stale detection via timestamp; callers can show “stream paused” if data older than timeout.
- All IPC operations use timeouts and exception-safe disposal.
