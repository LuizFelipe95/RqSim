# Deep Analysis of RQ-Hypothesis Implementation in RQSim

## Executive Summary

This document is the **implementation-facing** deep analysis of RQSim (Dec 2025) against the Relational‑Quantum (RQ) hypothesis.

Key changes since early 2025:
- GPU modernization is implemented through **Stage 6 (CSR Unified Engine)**.
- CSR Unified is integrated into the UI event-loop and can operate in **weight ownership mode** (CSR weights are the authoritative state; `RQGraph` is kept in sync).

---

# PART I: IMPLEMENTATION STATUS OVERVIEW

## 0. Current Implementation Status (Dec 2025)

### ✅ GPU modernization (ComputeSharp) – status matrix

| Capability | Dense GPU (`GPUOptimized`) | CSR GPU (`GPUCompressedSparseRow`) |
|-----------|-----------------------------|------------------------------------|
| Wheeler–DeWitt constraint (double) | ✅ | ✅ (unified kernel) |
| Spectral action (double) | ✅ | ✅ (unified kernel) |
| Quantum edges (double) | ✅ | ✅ (unified kernel) |
| MCMC support | ✅ | ✅ |
| Internal observer (Stage 5) | ✅ | ✅ |
| Unified coordinator (Stage 6) | n/a | ✅ `Unified/CsrUnifiedEngine` |

### ✅ Runtime selection and switching

User-controllable selection is supported:

- `CpuOnly`
- `Original` (Dense GPU)
- `Csr` (Sparse GPU)
- `Auto` (chooses CSR vs Dense based on N/nnz heuristics)

Implementation:
- `RqSim/FormSimAPI/FormSimAPI_GPU.cs` (`GpuEngineType`, init, switching)
- `RqSim/FormSimAPI/FormSimAPI_SimulationLoops.cs` (event-loop branching)

### ✅ CSR Unified integration details (Stage 6 end-to-end)

CSR Unified integration is designed to preserve invariants of the existing CPU pipeline:

**Ownership contract (current):**
- `RQGraph` is the primary container for simulation state (nodes, adjacency, UI metrics)
- CSR Unified maintains a derived CSR topology and GPU buffers
- In CSR mode, CSR Unified can be configured as **weight source-of-truth**

**No-rebuild weights update:**
- CSR adjacency (row offsets / column indices) remains stable unless topology changes
- Weight updates use only:
  - `CsrTopology.UpdateEdgeWeightsFromDense(...)`
  - replace GPU weight buffer without rebuilding CSR structure

**Topology change detection:**
- CSR Unified tracks a lightweight signature based on adjacency.
- If adjacency changes (edges added/removed), CSR Unified falls back to `UpdateTopology(graph)`.

---

# PART II: THEORY → IMPLEMENTATION MAPPING (WHAT IS PHYSICALLY PARALLELIZABLE)

## 1. Constraint (Wheeler–DeWitt)

Parallelizable by nodes:
- `H_i = H_geom(i) - κ·H_matter(i)`
- `violation_i = H_i^2`
- reduction: sum/average

CSR unified kernel: `CsrUnifiedConstraintKernelDouble`.

## 2. Spectral Action

Parallelizable:
- per-edge: volume contribution
- per-node: curvature and variance for Weyl²

CSR unified kernel: `CsrUnifiedSpectralActionKernelDouble`.

## 3. Quantum edges

Parallelizable by edges:
- unitary phase rotation of complex amplitudes (Double2)
- collapse uses per-edge independent RNG samples

CSR unified kernel: `CsrUnifiedQuantumEdgeKernelDouble`.

---

# PART III: CRITICAL IMPLEMENTATION SEMANTICS

## Correlation mass

Current mass definition is derived from weights:

- `m_i = sqrt(sum_j w_ij^2)`

Accessors:
- `CorrelationMass` returns the cache
- `GetNodeMass(i)` uses the same cache and returns `0` if not computed

CSR unified explicitly calls `EnsureCorrelationMassComputed()` before uploading masses.

---

# PART IV: REFERENCES

- `RqSimGraphEngine/MODERNIZATION_GPU.md`
- `RqSimGraphEngine/RQSimulation/GPUOptimized/GPU_IMPLEMENTATION.md`
- `RqSimGraphEngine/README.md`

---

*Last updated: 2025-12*
