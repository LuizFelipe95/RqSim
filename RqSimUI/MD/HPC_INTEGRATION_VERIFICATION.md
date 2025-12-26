# HPC Stack Integration - Verification Report

**Project:** RqSimulator (RqSim + RqSimGraphEngine)  
**Target:** .NET 10  
**Last updated:** 2025-12  

This document verifies integration of the HPC stack used for large-scale simulation and visualization:
- ComputeSharp (GPU compute)
- CSR sparse data layout
- Arch ECS (data oriented layer)
- Veldrid (GPU rendering)

---

## 1. Checklist status summary

| Section | Status | Notes |
|---------|--------|------|
| NuGet dependencies | ? | Veldrid + Arch in UI, ComputeSharp in engine |
| Arch ECS data layer | ? | ECS components and world sync systems exist |
| Veldrid render core | ? | Host + render systems exist |
| WinForms integration | ? | UI hosts Veldrid and compute pipelines |
| CSR data layout (physics) | ? | CSR topology + weight-only updates implemented |
| Hamiltonian kernel / BiCGStab | ? | CSR Cayley solver present |
| CSR Unified Stage 6 pipeline | ? | Unified CSR engine wired into UI loop |

---

## 2. Dependencies (informational)

### RqSimUI
Uses:
- Veldrid
- Veldrid.SPIRV
- Veldrid.ImGui
- Arch

### RqSimGraphEngine
Uses:
- ComputeSharp
- Arch (shared ECS types)

---

## 3. CSR physics stack (ComputeSharp)

**Implemented (current paths):**
- `RqSimGraphEngine/RQSimulation/GPUCompressedSparseRow/Data/CsrTopology.cs`
  - CSR layout: row offsets, column indices, edge weights
  - **weight-only update**: `UpdateEdgeWeightsFromDense(...)` (replaces only the GPU weight buffer)

- `RqSimGraphEngine/RQSimulation/GPUCompressedSparseRow/Shaders/*`
  - CSR kernels used by solver and unified engine

- `RqSimGraphEngine/RQSimulation/GPUCompressedSparseRow/Solvers/*`
  - BiCGStab solver components

- `RqSimGraphEngine/RQSimulation/GPUCompressedSparseRow/Unified/CsrUnifiedEngine.cs`
  - Stage 6 unified coordinator
  - supports weight ownership sync:
    - `SyncWeightsFromGraph(...)`
    - `CopyWeightsToGraph(...)`

---

## 4. UI/Simulation integration

### 4.1 Engine selection + runtime switching

- `RqSim/FormSimAPI/FormSimAPI_GPU.cs`
  - engine selection (`GpuEngineType`)
  - runtime switching (`SetGpuEngineType`)

### 4.2 Event-loop wiring

- `RqSim/FormSimAPI/FormSimAPI_SimulationLoops.cs`
  - executes CSR unified physics step when CSR mode is active
  - in CSR weight ownership mode:
    1) pull CSR weights ? graph
    2) CPU gravity step operates on graph view
    3) push weights ? CSR using weight-only update

---

## 5. Rendering stack (Arch ECS + Veldrid)

This repository contains an ECS-driven GPU renderer intended for large graphs:
- ECS holds per-node visual data
- renderer batches uploads to GPU buffers

(Exact file list is intentionally omitted here to keep this verification report stable; refer to the `RqSim/Rendering/*` folder.)

---

**Verification result:** HPC stack components are present and integrated. CSR Unified Stage 6 is wired into the simulation loop and supports efficient weight-only sync.
