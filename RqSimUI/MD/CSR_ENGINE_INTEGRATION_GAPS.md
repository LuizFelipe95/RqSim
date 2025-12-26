# CSR Engine Integration Status Report

**Project:** RqSimulator  
**Target:** .NET 10  
**Last updated:** 2025-12  

This document tracks CSR-mode integration of GPU compute and rendering paths.

> Historical note: this file originally documented integration gaps. As of Dec 2025 the CSR path is integrated end-to-end, so this is now a status report.

---

## 1. Summary

| Area | Status | Notes |
|------|--------|------|
| Engine selection (`GpuEngineType`) | ? | `Auto`, `Original`, `Csr`, `CpuOnly` |
| Runtime switching | ? | `FormSimAPI.SetGpuEngineType(...)` reinitializes engines as needed |
| CSR Cayley solver | ? | `GpuCayleyEvolutionEngineCsr` (double precision) |
| CSR Unified Stage 6 | ? | `GPUCompressedSparseRow/Unified/CsrUnifiedEngine` is wired into UI event-loop |
| CSR weight ownership mode | ? | CSR can be authoritative for weights; sync to `RQGraph` is weight-only (no CSR rebuild) when adjacency unchanged |
| CSR topology rebuild detection | ? | signature mismatch fallback triggers `UpdateTopology(graph)` |
| CSR 3D visualization (Veldrid) | ? | ECS + Veldrid pipeline present and used for large graphs |

---

## 2. Key integration points

### 2.1 Engine selection and initialization

- `RqSim/FormSimAPI/FormSimAPI_GPU.cs`
  - `GpuEngineType`
  - `SetGpuEngineType(...)`
  - `InitializeGpuEnginesWithType(...)`
  - `ActiveEngineType` resolved from `Auto`

### 2.2 Event-loop routing

- `RqSim/FormSimAPI/FormSimAPI_SimulationLoops.cs`
  - CSR Unified is executed in the event-loop when `ActiveEngineType == Csr`
  - In CSR weight-ownership mode, the loop:
    1) pulls CSR weights ? graph
    2) applies CPU-side gravity step (Forman)
    3) pushes updated weights ? CSR without topology rebuild

### 2.3 Weight-only CSR updates

- `RqSimGraphEngine/RQSimulation/GPUCompressedSparseRow/Data/CsrTopology.cs`
  - `UpdateEdgeWeightsFromDense(double[,] weights)`
  - replaces only the GPU weight buffer while keeping row/col buffers intact

- `RqSimGraphEngine/RQSimulation/GPUCompressedSparseRow/Unified/CsrUnifiedEngine.cs`
  - `SyncWeightsFromGraph(...)`
  - `CopyWeightsToGraph(...)`

---

## 3. CSR Veldrid visualization pipeline (high level)

CSR visualization is optimized for large sparse graphs and is separate from compute kernels.

Physics ? Graphics flow:

- ComputeSharp engines produce readback-able buffers
- `PhysicsSyncSystem` converts to ECS `NodeVisualData`
- Veldrid render systems draw nodes/edges

---

## 4. Removed gaps (for historical reference)

Previously open gaps that are now closed:
- CSR Unified engine not wired into event-loop: **closed**
- CSR weights required full topology rebuild: **closed** via weight-only buffer replacement

---
