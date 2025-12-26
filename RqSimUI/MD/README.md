# Deep Analysis of RQ-Hypothesis Implementation in RQSim

## Executive Summary

This document is the **current (Dec 2025)** deep analysis of RQSim against the Relational‑Quantum (RQ) hypothesis. It summarizes what is implemented, what is approximated, and how the simulation pipeline maps to the theory.

Key 2025 update:
- The GPU roadmap in `MODERNIZATION_GPU.md` is now effectively implemented through **Stage 6**.
- `GPUCompressedSparseRow/Unified/CsrUnifiedEngine` is integrated into the **WinForms event-loop** and can operate in **weight-ownership mode** (CSR is the source of truth for weights, with efficient sync back to `RQGraph`).

---

# PART I: IMPLEMENTATION STATUS OVERVIEW

## 0. Current Implementation Status (Dec 2025)

### ✅ Completed GPU Modernization Stages (ComputeSharp)

| Stage | Subsystem | Status | Primary locations |
|------:|----------|--------|-------------------|
| 1 | Wheeler–DeWitt constraint (double precision) | ✅ | `RQSimulation/GPUOptimized/Constraint/*`, `RQSimulation/GPUCompressedSparseRow/Unified/*` |
| 2 | Spectral Action (double precision) | ✅ | `RQSimulation/GPUOptimized/SpectralAction/*`, `RQSimulation/GPUCompressedSparseRow/Unified/*` |
| 3 | Quantum Edges (double precision) | ✅ | `RQSimulation/GPUOptimized/QuantumEdges/*`, `RQSimulation/GPUCompressedSparseRow/Unified/*` |
| 4 | GPU MCMC (double precision) | ✅ | `RQSimulation/GPUOptimized/MCMC/*`, `RQSimulation/GPUCompressedSparseRow/MCMC/*` |
| 5 | GPU Internal Observer | ✅ | `RQSimulation/GPUOptimized/Observer/*`, `RQSimulation/GPUCompressedSparseRow/Observer/*` |
| 6 | CSR Unified Engine (shared topology pipeline) | ✅ | `RQSimulation/GPUCompressedSparseRow/Unified/CsrUnifiedEngine.cs` |

### ✅ Runtime mode integration (CPU / Dense GPU / CSR GPU)

Mode switching is implemented in the UI pipeline:

- Selection and runtime switching:
  - `RqSim/FormSimAPI/FormSimAPI_GPU.cs` (`GpuEngineType`, `SetGpuEngineType`, `InitializeGpuEnginesWithType`)
- Event-loop execution:
  - `RqSim/FormSimAPI/FormSimAPI_SimulationLoops.cs` (`RunParallelEventBasedLoop`)

CSR Stage 6 integration details:
- CSR Unified is wired into the event-loop and executed each cycle (throttled sync)
- CSR Unified can operate in **weight ownership** mode:
  - CSR is treated as source of truth for weights
  - Graph weights are pulled from CSR before CPU-side operations and pushed back after updates
  - Weight sync is **weight-only** (no CSR rebuild) when adjacency is unchanged

Related helpers:
- `RqSim/FormSimAPI/FormSimAPI_CsrUnified.cs`
- `RQSimulation/GPUCompressedSparseRow/Data/CsrTopology.cs` (`UpdateEdgeWeightsFromDense`, weight buffer replacement)

### ✅ Completed RQ-Hypothesis features (high level)

| Area | Implemented status | Notes |
|------|--------------------|------|
| Background independence | ✅ | Coordinates are visualization-only; physics uses topology + weights |
| Relational/async time model | ✅ | Event-based model: per-node proper time, no global “now” |
| Quantum graphity topology change | ✅ | Quantum edges + collapse + evolution; topology changes supported |
| Wheeler–DeWitt style constraint | ✅ | Constraint violation metric is computed (CPU + GPU variants) |
| Spectral dimension monitoring | ✅ | Random-walk + alternative GPU heat/metrics pipelines |
| Internal observer (no external observer) | ✅ | Internal subsystem measurements, CPU + GPU |

---

# PART II: KEY ALGORITHMS AND INVARIANTS

## 1. Mass semantics (Correlation Mass)

**Source of truth:** `_correlationMass` cache in `RQGraph`.

- `RQGraph.CorrelationMass` returns `_correlationMass`
- `RQGraph.GetNodeMass(i)` returns `_correlationMass[i]` (or `0.0` if not computed)
- `RQGraph.EnsureCorrelationMassComputed()` recomputes from weights when needed

**Definition (current implementation):**

- `m_i = sqrt( sum_j w_ij^2 )`

This mass model is used consistently across CPU and GPU paths. CSR Unified explicitly ensures the mass cache exists before uploading masses.

## 2. CSR Unified weight ownership in the UI loop

When `GpuEngineType.Csr` is active and `CsrUnifiedOwnsWeights == true`:

1) `PullWeightsFromCsrUnified(graph)` brings `graph.Weights` into sync with CSR.
2) CPU gravity / other CPU modules operate on the graph view.
3) `PushWeightsToCsrUnified(graph)` updates CSR weights **without rebuilding** CSR structure.

A lightweight topology signature prevents silent mismatch: if adjacency changes, CSR Unified falls back to full `UpdateTopology(graph)`.

---

# PART III: DOCUMENTS AND REFERENCES

- GPU modernization plan: `MODERNIZATION_GPU.md`
- Modernization order (legacy checklist): `MODERNIZATION_ORDER.md`
- GPU architecture overview (updated): `RQSimulation/GPUOptimized/GPU_IMPLEMENTATION.md`

---

*Last updated: 2025-12*
