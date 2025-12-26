# RQ-Hypothesis Physics Algorithms

This document describes the mathematical foundations **and the current implementation mapping** of the physics algorithms in RqSim (Dec 2025).

Scope:
- Core RQ-Hypothesis algorithms implemented in `RqSimGraphEngine/RQSimulation/*`
- GPU execution backends (`GPUOptimized` dense and `GPUCompressedSparseRow` CSR)
- Unified CSR pipeline (`GPUCompressedSparseRow/Unified/CsrUnifiedEngine`)

---

## 1. Relational Quantum Mechanics foundation

### 1.1 Core principles (implementation constraints)

1. **No background spacetime**: coordinates are visualization only; physics uses only topology + weights
2. **Relational time**: simulation supports event-based time (per-node proper time)
3. **Graph ontology**: state is stored on nodes/edges
4. **Emergent gravity**: curvature-driven weight evolution

### 1.2 Mathematical objects (as implemented)

| Object | Symbol | Representation |
|--------|--------|----------------|
| Node quantum state | ? | Complex vector per node (various representations) |
| Edge weight | w_ij | Correlation strength, stored in `RQGraph.Weights[i,j]` |
| CSR topology | (row,col,w) | `CsrTopology` row offsets / col indices / weights |
| Correlation mass | m_i | `m_i = sqrt(sum_j w_ij^2)` (cached in `_correlationMass`) |
| Gauge phase (U(1) etc.) | U_ij | Phase/link variables on edges in respective modules |

---

## 2. Relational time (Page–Wootters / event-based approximation)

### 2.1 Wheeler–DeWitt constraint (conceptual)

The RQ model treats the universe as globally constrained:

- `H_total |?? = 0`

In code, this appears as a **constraint violation metric** (CPU and GPU variants) rather than enforcing an exact constraint at every step.

### 2.2 Practical time model used by the UI pipeline

The WinForms simulation loop uses an **event-based model**:
- each node has its own proper time
- the loop processes “events” (node updates) rather than a global synchronized tick

This matches the operational requirement “no global now”, while still providing a UI-friendly “sweep counter”.

---

## 3. Network gravity (graph curvature flow)

### 3.1 Discrete action (graph analogue)

Continuum:

- `S_EH = (1/16?G) ? R ?(-g) d^4x`

Graph analogue (informal):

- `S_graph = ?_(i,j?E) w_ij · R_ij`

### 3.2 Forman?Ricci curvature (fast / local)

Forman-style curvature is used for performance-friendly geometry evolution.

Implementation is split across:
- CPU gravity evolution (used as fallback)
- GPU implementations (dense and CSR)

### 3.3 Ollivier?Ricci curvature (approx / optional)

Ollivier curvature is conceptually closer to optimal transport but expensive.
A Jaccard-style approximation is used in performance paths.

---

## 4. Constraint (Wheeler–DeWitt violation)

### 4.1 Node-local constraint

Per node:
- `H_i = H_geom(i) ? ?·H_matter(i)`
- `violation_i = H_i^2`

Reduction:
- `total_violation = (1/N) ?_i violation_i`

### 4.2 GPU implementation mapping

- Dense: `RQSimulation/GPUOptimized/Constraint/*` (double precision)
- CSR Unified: `RQSimulation/GPUCompressedSparseRow/Unified/*` (`CsrUnifiedConstraintKernelDouble`)

---

## 5. Spectral action (Chamseddine–Connes)

Spectral action is used to stabilize emergent dimension around the target (typically 4).

Decomposition:
- Volume term (edge-parallel)
- Einstein term (sum curvature)
- Weyl? proxy (curvature variance)
- Dimension potential term

GPU mapping:
- Dense: `RQSimulation/GPUOptimized/SpectralAction/*`
- CSR Unified: `CsrUnifiedSpectralActionKernelDouble`

---

## 6. Quantum edges (Quantum Graphity)

Edges carry complex amplitudes (existence superposition).

Per edge:
- unitary evolution (phase rotation)
- existence probability `P = |?|^2`
- collapse via RNG sampling

GPU mapping:
- Dense: `RQSimulation/GPUOptimized/QuantumEdges/*`
- CSR Unified: `CsrUnifiedQuantumEdgeKernelDouble`

---

## 7. CSR Unified pipeline (Stage 6)

### 7.1 Unified step

The CSR unified coordinator computes, in a physically consistent order:

1) constraint
2) spectral action
3) quantum edge evolution

API:
- `CsrUnifiedEngine.PhysicsStepGpu(double dt)`

### 7.2 Weight ownership + sync semantics

In CSR mode, CSR unified can act as the **source of truth for weights**.

Mechanism:
- weight-only updates (no CSR rebuild) via `CsrTopology.UpdateEdgeWeightsFromDense(...)`
- topology rebuild only when adjacency changes (signature mismatch)

---

## 8. Numerical considerations

### 8.1 Double precision

Physics-critical GPU paths use double precision when supported.

### 8.2 HLSL intrinsic limitation

ComputeSharp HLSL intrinsics accept `float`.
For double pipelines:
- transcendentals use float intrinsics + cast
- accumulation remains double

---

*Last updated: 2025-12*
