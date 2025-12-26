# Physical GPU Engine Architecture: Dense GPU vs CSR GPU (ComputeSharp / Veldrid)

*Project:* `RqSimGraphEngine` (TFM: `net10.0`)

This document explains the **physics-oriented execution model** of the two GPU backends used in RQSim:

- **Dense GPU** (`RQSimulation/GPUOptimized/`) — optimized for *small/medium* graphs with contiguous buffers.
- **CSR GPU** (`RQSimulation/GPUCompressedSparseRow/`) — optimized for *large sparse* graphs using CSR (Compressed Sparse Row).

It also clarifies:

- where and why **double precision (64?bit)** is required,
- what ComputeSharp can (and cannot) do with doubles and HLSL intrinsics,
- how Veldrid and ImGui fit into the overall solution (render/UX),
- how CPU/ECS/SIMD components interoperate with GPU pipelines.

---

## 1) High-level goal: physics correctness first, then performance

The simulation targets physically meaningful invariants (constraint violation, spectral action, unitarity, etc.). This implies:

- **Numerical stability** is more important than raw throughput for core observables.
- **Determinism and reproducibility** are preferred where feasible.
- **Double precision** is mandatory in "physics-critical" paths (energy-like scalars, constraints, accumulated reductions).

GPU acceleration is used to:

- parallelize per-node / per-edge computations,
- reduce bottlenecks in curvature-like metrics and action evaluations,
- support large sparse graphs without `O(N^2)` memory.

---

## 2) Packages and their roles

### 2.1 ComputeSharp (`ComputeSharp`, `ComputeSharp.D2D1`)

- Primary compute backend for physics kernels.
- Generates HLSL shaders from C# kernels (`IComputeShader`).
- Used for both Dense and CSR engines.

**Double support:**
- ComputeSharp supports `double` on hardware/drivers that expose `double` support.
- Kernels that use doubles should apply ComputeSharp’s explicit marker:
  - `[RequiresDoublePrecisionSupport]`

**Intrinsic limitation (important):**
- Many HLSL intrinsics exposed by ComputeSharp operate on `float` only.
- Common workaround: cast `double -> float` for transcendental calls.

Example pattern used across kernels:

```csharp
// exp(-i H dt) uses cos/sin
// Hlsl.Cos / Hlsl.Sin take float ? return float ? promote to double
double phase = H * dt;
double cosP = Hlsl.Cos((float)phase);
double sinP = Hlsl.Sin((float)phase);
```

This is acceptable when:

- the phase is within a range where float trig is stable for your tolerance,
- you maintain critical accumulations in double.

### 2.2 Veldrid (`Veldrid`, `Veldrid.SPIRV`, `Veldrid.ImGui`)

- Cross?API rendering abstraction (Direct3D11/12, Vulkan, Metal via MoltenVK, OpenGL).
- Used for visualization and UI integration rather than physics compute.
- `Veldrid.SPIRV` enables compiling shaders from SPIR?V.
- `Veldrid.ImGui` integrates Dear ImGui into the rendering loop.

Typical responsibility split:

- ComputeSharp: physics compute on GPU.
- Veldrid: rendering of graph, overlays, diagnostics, ImGui controls.

> Note: In this repo, physics kernels live under `RQSimulation/*`. Veldrid is typically consumed by UI/renderer projects or modules.

### 2.3 Arch (`Arch` ECS)

- ECS framework used for high?performance CPU data layout and iteration.
- Common pattern: ECS owns CPU-side components; GPU engines consume *packed arrays/buffers* derived from ECS state.

### 2.4 SIMD (`System.Numerics.Vectors`)

- CPU fallback and/or helper computations where GPU is not active.
- Often used for vectorized reductions, edge scanning, or dense math.

---

## 3) Two GPU engines: what differs and why

### 3.1 Dense GPU engine (`RQSimulation/GPUOptimized/`)

**Idea:** store graph in "dense-friendly" contiguous buffers and run kernels that assume reasonably uniform memory access.

Typical characteristics:

- Best when `N` is small/medium (rule-of-thumb: `N < 10k`, but depends on density).
- Easier to implement kernels.
- Reductions and per-node calculations can be expressed in straightforward GPU loops.

Data layout tends to be:

- adjacency described by CSR-like offsets + neighbors even in “dense” mode (to avoid `N^2` array), but buffers are optimized for smaller graphs and frequent updates,
- per-edge weights in contiguous arrays,
- per-node arrays for mass, curvature, state, etc.

Dense GPU is preferred when:

- the algorithm needs frequent random access and the graph is not extremely sparse,
- topology changes often and rebuild overhead is acceptable,
- development simplicity matters.

### 3.2 CSR GPU engine (`RQSimulation/GPUCompressedSparseRow/`)

**Idea:** optimize for *large sparse* graphs with strict `O(N + E)` storage.

CSR representation:

- `rowOffsets` length `N + 1`
- `colIndices` length `E`
- `weights` length `E`

Key benefits:

- Memory scales linearly with edges instead of quadratically.
- SpMV (Sparse Matrix–Vector multiply) and neighbor scans become bandwidth efficient.

CSR GPU is preferred when:

- `N >= 10k` and the graph is sparse,
- algorithms are mostly neighbor-scan / SpMV / reductions,
- topology does not change every tick or can be updated incrementally.

---

## 4) Physics semantics and why double precision matters

### 4.1 Physics-critical computations

These operations are sensitive to accumulation error, cancellation, and long?running drift:

- Wheeler–DeWitt style **constraint violation**
- Spectral Action (energy-like scalar, curvature statistics)
- Quantum amplitude evolution (unitarity / norm preservation)
- Euclidean action terms for MCMC acceptance

For these, **double precision** should be used at least for:

- main state fields (weights, masses, curvature),
- accumulations/reductions (sums/averages/variances),
- acceptance probability calculations.

### 4.2 Non-critical computations (can be float)

Suitable for `float`:

- random-walk statistics used only as indicators,
- visualization attributes,
- UI-only helpers.

The repo already documents this split (see `MODERNIZATION_GPU.md`).

---

## 5) Core invariants (shared between Dense and CSR)

### 5.1 Mass semantics (Correlation Mass)

**Definition used in the implementation:**

- `m_i = sqrt( sum_j w_ij^2 )`

**Source of truth:** cached mass array in the graph.

Operationally:

- Engines must ensure correlation mass exists before kernels needing `mass[i]`.
- CSR Unified mode typically uploads mass as a per-node buffer.

### 5.2 Topology vs weights

Many physics steps depend on two separate concerns:

- **Topology** (adjacency): nodes + edges, neighbor lists.
- **Weights**: physical coupling values per edge.

CSR is ideal when topology is stable but weights change often, because it can update **weights only** without rebuilding CSR structure.

---

## 6) Engine responsibilities and execution pipeline

Both engines follow the same conceptual pipeline:

1. **Prepare topology buffers** (if needed).
2. **Prepare scalar/vector buffers** (weights, mass, curvature, amplitudes, etc.).
3. Run a sequence of GPU kernels:
   - per-node kernels
   - per-edge kernels
   - reductions (sum/mean/variance)
4. Read back minimal results:
   - observables (constraint violation, action components)
   - optional debug arrays (curvature field)

### 6.1 Dense engine implementation pattern

- Multiple specialized engines under `GPUOptimized/*`.
- Each subsystem has:
  - GPU buffers,
  - upload/download helpers,
  - compute entry points returning scalars.

Common design points:

- Per-node kernels read neighbor segments via offsets.
- Per-edge kernels operate on contiguous edge arrays.

### 6.2 CSR engine implementation pattern

- CSR data is shared across multiple subsystems.
- The **Unified** engine coordinates sub-engines to avoid duplicated uploads.

Key design points:

- `CsrTopology` is the shared dependency.
- Weight updates are lightweight when adjacency does not change.
- Many operations can be expressed as SpMV-like passes + reductions.

---

## 7) Weight ownership mode (CSR Unified integration pattern)

A frequent integration problem is deciding the authoritative set of edge weights:

- CPU graph (`RQGraph`) modifies weights
- GPU CSR buffers modify weights

**Weight ownership mode** solves this by treating CSR as the source-of-truth for weights while allowing CPU modules to run.

Typical loop (when CSR owns weights):

1. **Pull** weights from CSR into the CPU graph view.
2. Run CPU modules that read/update weights.
3. **Push** weights back to CSR buffers *without rebuilding topology*.

Safety mechanism:

- A topology signature check ensures adjacency mismatch triggers a full `UpdateTopology`.

---

## 8) Practical guidance: choosing Dense vs CSR

Use Dense GPU when:

- `N` is small/medium,
- topology changes frequently,
- you need quick iteration on new kernels.

Use CSR GPU when:

- `N` is large (10k+),
- graph is sparse,
- you want stable memory footprint and good bandwidth behavior.

A good dynamic strategy:

- pick CSR beyond a threshold on `N`/`E`,
- keep a single high-level interface in the UI loop that selects the engine type.

---

## 9) Debugging and validation recommendations

### 9.1 Correctness tests (GPU vs CPU)

For each physics observable:

- compare GPU result with CPU reference under fixed seed,
- assert absolute/relative tolerance:
  - constraint: `1e-10`
  - action: `1e-8` to `1e-10` depending on reductions

### 9.2 Determinism notes

GPU reductions are not always bitwise deterministic due to parallel reduction order.

If strict reproducibility is needed:

- use deterministic reduction strategy,
- or accept tolerance-based comparisons.

---

## 10) Where to look in the code

GPU code locations:

- Dense GPU: `RQSimulation/GPUOptimized/`
- CSR GPU: `RQSimulation/GPUCompressedSparseRow/`

Project roadmap/details:

- `MODERNIZATION_GPU.md`
- `README.md` (pipeline integration notes)

---

## 11) Notes on future extension

When adding a new physics kernel:

- start with Dense version for clarity,
- then port to CSR if the operation is neighbor-scan/SpMV friendly,
- keep the physical invariants and tolerances documented alongside the kernel.

If the kernel uses:

- trig/exp/log ? expect `float` intrinsics; test error tolerance carefully.
- long reductions ? prefer partial sums in `double`.

---

*Last updated: 2025-12 (generated for current workspace state)*
