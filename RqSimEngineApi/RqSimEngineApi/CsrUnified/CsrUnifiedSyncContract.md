# CSR Unified Engine UI Integration Contract

## Goal
Integrate `RQSimulation.GPUCompressedSparseRow.Unified.CsrUnifiedEngine` into the WinForms event-loop without breaking existing CPU/Dense GPU behavior and while allowing runtime switching.

## Ownership model
### Owned by `RQGraph` (source of truth)
- Dense topology (`Edges`, `Weights`)
- Node state (`State`, proper time, etc.)
- Any CPU-side physics modules

### Owned by `CsrUnifiedEngine` (GPU-side working set)
- CSR topology buffers derived from `RQGraph` (row offsets, col indices, weights)
- Node masses buffer (correlation mass)
- CSR Unified caches:
  - `LastConstraintViolation`
  - `LastSpectralAction`
  - Curvatures/violations arrays internal to CSR engine
- Quantum edge amplitudes managed internally (not mapped 1:1 to `RQGraph` yet)

## Sync points
### Graph -> CSR Unified
- Always before running CSR unified step (or when graph changed):
  - ensure `graph.EnsureCorrelationMassComputed()`
  - if topology changed (edges toggled) -> `UpdateTopology(graph)`
  - if only weights changed -> refresh CSR weights via `UpdateTopology(graph)` (current API rebuilds topology)

### CSR Unified -> Graph
- For Stage 6 integration in UI loop, CSR unified is treated as a **metrics/physics sub-pipeline**.
- It does not mutate `RQGraph` state directly (to avoid desync of other modules).
- Metrics (constraint violation, spectral action) can be surfaced via FormSimAPI and/or console.

## Runtime switching safety rules
- Switching `GpuEngineType` reinitializes engines.
- If switching to `Csr`:
  - create CSR engines (Cayley CSR + CSR Unified)
- If switching away from `Csr`:
  - dispose CSR engines
- Avoid mixing CSR unified outputs into `RQGraph.Weights` in this phase.

