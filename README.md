

# RqSim: Relational Quantum Simulation Platform

**Next-Gen Graph-Based Emergent Physics Engine**

![Status](https://img.shields.io/badge/Status-Active_Development-green)
![Platform](https://img.shields.io/badge/Platform-.NET_10_|_Windows-blue)
![Compute](https://img.shields.io/badge/Compute-Multi--GPU_HPC-purple)
![Rendering](https://img.shields.io/badge/Rendering-DirectX_12-red)

**RqSim** is a computational simulation environment designed to model physical systems without presupposing a fundamental spacetime background. It is a specialized software framework written in C# that leverages modern parallel computing techniques to test hypotheses where spacetime geometry is an emergent property rather than a pre-existing container.

The **Relational-Quantum (RQ) hypothesis** is utilized here as the primary **Reference Use Case**. This model is selected for its maximal reductionism: it eliminates the "background" entirely, reducing physics to a pure interaction graph. This characteristic makes it the optimal candidate for testing the *ab initio* procedural generation of physical reality.

Below is a detailed technical analysis of the RqSim architecture and engineering solutions.

---

## 1. Core Architecture: Graph as Fundamental Ontology

The foundational design principle of RqSim is the rejection of Cartesian coordinate systems and global time synchronization within the core physics engine.

### RQGraph: Topology-First Data Structure
The central data structure is the `RQGraph` class, segmented into `partial` classes for modularity. In contrast to standard physics engines (e.g., Unity, Unreal) where entities possess `Vector3(x, y, z)` properties, RqSim employs a strictly relational ontology:

* **Nodes:** Represent abstract quantum states (Hilbert spaces), not spatial points.
* **Edges:** Represent quantum correlations, entanglement, or adjacency. The edge weight ($w_{ij}$) quantifies interaction strength.
* **Metric:** Distance is topological, derived from the graph structure (e.g., Heat Kernel, Spectral Distance, or Ollivier-Ricci curvature) rather than Euclidean displacement.

### Engineering Solution: Coordinate Isolation
A strict architectural boundary is enforced regarding spatial coordinates. In the codebase, any `Coordinates` property within `RQGraph` is marked with the `[Obsolete]` attribute or isolated in UI-specific namespaces. These coordinates are exclusively accessible by the rendering subsystem (`RqSimRenderingEngine`) for Visualization purposes. The physics core (`Physics/*`, `GPUOptimized/*`) is agnostic to embedding dimensions; any attempt by the physics engine to access explicit coordinates is treated as an architectural violation.

---

## 2. Event-Based & Time-Free Model: Asynchronous Causality

A significant architectural requirement of RqSim (specifically in the `NonPhysicsEngines` module) is the implementation of an **Event-Driven Engine** that challenges the concept of a global simulation tick.

### The Global Time Problem
Standard simulations typically employ a synchronous `Update(dt)` loop. This approach contradicts the principle of relativity, which posits the absence of a global "now."

### Solution: Local Proper Time ($\tau$)
RqSim implements a model where every node tracks its own **Proper Time**.
* **Priority Queue Scheduling:** Instead of iterative loops, the engine utilizes a priority queue to process events based on local time thresholds.
* **Local Causality:** A node updates its state only when sufficient proper time has accumulated or upon receiving a signal from a neighbor, ensuring causal consistency.
* **Parallelization via Graph Coloring:** To maximize throughput without race conditions, the `ParallelEventEngine` employs a **Greedy Graph Coloring** algorithm. Nodes assigned to the same "color" are topologically disjoint (share no direct edges) and can be updated in parallel across CPU threads.

---

## 3. GPU Acceleration (ComputeSharp): High-Performance Mathematics

To process graph topologies exceeding $N > 10^5$ nodes with complex quantum evolution, RqSim integrates GPGPU via the **ComputeSharp** library. This allows for the direct translation of C# logic into HLSL shaders without context switching penalties.

### Key GPU Modules (CSR Engine)
The modern core of RqSim utilizes a **Compressed Sparse Row (CSR)** format for maximum memory efficiency on the GPU.

* **CSR Engine:** Optimized for large sparse graphs. It manages topology in monolithic GPU buffers (`RowOffsets`, `ColumnIndices`, `EdgeWeights`).
* **BiCGStab Solver:** Implements the Biconjugate Gradient Stabilized method on GPU for solving linear systems (e.g., Cayley evolution matrices).
* **Double Precision:** Uses `double` and `double2` (Complex) arithmetic in HLSL to minimize rounding errors in unitary evolution.
* **Zero-Copy Rendering:** Data resides on the GPU. The rendering engine reads physics buffers directly via `SharedGpuBuffer` interop, eliminating CPU-GPU bandwidth bottlenecks.



LIMITATION: Topology evolution is restricted to the pre-allocated super-graph edges. True dynamical triangulation (adding nodes/edges beyond capacity) requires GPU Stream Compaction



### HPC Stack Integration (Vortice.Windows)
The project has migrated to a high-performance computing stack:
* **Arch ECS:** Data-oriented design for entity management (used in experimental branches).
* **DirectX 12 / Vortice:** Low-level graphics API for the `RqSimRenderingEngine`.
* **ImGui.NET:** Immediate mode GUI for real-time debug overlays and metric visualization.

---

## 4. Infrastructure: Thermodynamics of Computation

### Strict Energy Ledger
The `EnergyLedger` class enforces a rigorous conservation law within the simulation.
* **Vacuum Pool:** The system maintains a "vacuum reservoir." Any topological modification (e.g., edge creation) or particle generation requires an energy expenditure extracted from this pool.
* **Thermodynamic Cost (Landauer's Principle):** The implementation acknowledges that information processing is not thermodynamically free. Operations involving entropy generation (randomness) or structural reorganization incur a virtual energy cost, preventing "perpetual motion" anomalies in the simulation logic.

### Modular Pipeline (`IPhysicsModule`)
The infrastructure supports the injection of arbitrary physical laws via the `PhysicsPipeline`. This allows seamless switching between:
1.  **Stochastic Models (MCMC):** Metropolis-Hastings algorithms for finding ground states.
2.  **Unitary Evolution:** Deterministic quantum evolution using Hamiltonian constraints ($H|\Psi\rangle = 0$).

---

## 5. Technical Implementation Details

### The Physics Pipeline
The simulation loop is orchestrated by a customizable pipeline. A typical setup for a **Quantum Gravity** experiment looks like this:

```csharp
var pipeline = new PhysicsPipeline()
    .AddModule(new HamiltonianEvalShader())       // 1. Calculate Geometry Constraint (H)
    .AddModule(new LapseFieldShader())            // 2. Compute Local Time Dilation (N)
    .AddModule(new RicciFlowLapseShader())        // 3. Evolve Topology (Gravity)
    .AddModule(new QuantumPhaseEvolutionShader()) // 4. Rotate Quantum Phases
    .AddModule(new InformationCurrentShader());   // 5. Verify Unitarity


### Critical Shaders (HLSL)

The physics is defined by compute shaders located in `GPUOptimized/`:

| Shader Class | Physical Function |
| --- | --- |
| `HamiltonianConstraintKernel` | Calculates . Represents the "error" in spacetime geometry. |
| `LapseFieldShader` | Implements $N(x) \approx \frac{1}{1+ |
| `RicciFlowLapseShader` | Smoothes the graph metric: . Emergent Gravity. |
| `QuantumPhaseEvolutionShader` | Applies the unitary operator . |

### Multi-GPU Support

Large-scale simulations utilize `IMultiGpuOrchestrator` to partition the graph across multiple physical GPUs, synchronizing boundary nodes via PCIe bus transfers to maintain causal continuity across the cluster.

---

## 6. Experiments & Hypotheses

The platform includes built-in definitions (`Experiments/Definitions`) to test specific emergent phenomena:

* **Vacuum Genesis:** Initialization from a null graph. Tests if stable geometry can nucleate from quantum fluctuations.
* **Black Hole Evaporation:** Models the dissipation of dense sub-graphs via Hawking radiation analogues (weight decay).
* **Wormhole Stability:** Tests the longevity of non-local connections under Ricci Flow.
* **Spectral Dimension Analysis:** Tools to verify if the graph dimension evolves from  (UV) to  (IR).

---

## 7. Build & Run

**Prerequisites:**

* Windows 10/11 (DirectX 12 capable)
* Visual Studio 2022
* .NET 10.0 SDK
* Dedicated GPU (NVIDIA/AMD) with Shader Model 6.0+ support.

**Execution:**

1. Open `RqSim.sln`.
2. Set `RqSimUI` as the startup project for the GUI dashboard.
3. Set `RqSimConsole` for headless/server mode (HPC clusters).
4. Ensure `Release` mode is selected for AVX2 and GPU optimizations.
5...
6...
custom added
---


=================== more Legacy =================





# RqSim: A GPU-Accelerated Framework for Graph-Based Emergent Physics

**RqSim** is a computational simulation environment designed to model physical systems without presupposing a fundamental spacetime background. It is a specialized software framework written in C# that leverages modern parallel computing techniques to test hypotheses where spacetime geometry is an emergent property rather than a pre-existing container.

The **Relational-Quantum (RQ) hypothesis** is utilized here as the primary **Reference Use Case**. This model is selected for its maximal reductionism: it eliminates the "background" entirely, reducing physics to a pure interaction graph. This characteristic makes it the optimal candidate for testing the *ab initio* procedural generation of physical reality.

Below is a detailed technical analysis of the RqSim architecture and engineering solutions.

---

## 1. Core Architecture: Graph as Fundamental Ontology

The foundational design principle of RqSim is the rejection of Cartesian coordinate systems and global time synchronization in the physics engine.

### `RQGraph`: Topology-First Data Structure
The central data structure is the `RQGraph` class, segmented into `partial` classes for modularity. In contrast to standard physics engines (e.g., Unity, Unreal) where entities possess `Vector3(x, y, z)` properties, RqSim employs a strictly relational ontology:

* **Nodes**: Represent abstract quantum states (Hilbert spaces), not spatial points.
* **Edges**: Represent quantum correlations or entanglement. The edge weight ($w_{ij}$) quantifies interaction strength.
* **Metric**: Distance is topological, derived from the graph structure (e.g., shortest path weighted by $-\ln(w)$ or spectral distance) rather than Euclidean displacement.

### Engineering Solution: Coordinate Isolation
A strict architectural boundary is enforced regarding spatial coordinates. In the codebase, the `Coordinates` property within `RQGraph` is marked with the `[Obsolete]` attribute to prevent accidental usage in physics calculations. These coordinates are exclusively accessible by the rendering subsystem (`PartialForm3D.cs`) for User Interface (UI) visualization purposes. The physics core (`Physics/*`, `Core/*`) is agnostic to embedding dimensions; any attempt by the physics engine to access explicit coordinates is treated as an architectural violation.

---

## 2. Event-Based & Time-Free Model: Asynchronous Causality

A significant architectural requirement of RqSim is the implementation of an **Event-Driven Engine** that eliminates the global simulation tick.

### The Global Time Problem
Standard simulations typically employ a synchronous `Update(dt)` loop, advancing the state of the entire system simultaneously. This approach contradicts the principle of relativity, which posits the absence of a global "now."

### Solution: Local Proper Time ($\tau$)
RqSim implements a model where every node tracks its own **Proper Time**.
1.  **Priority Queue Scheduling**: Instead of iterative loops, the engine utilizes a priority queue to process events based on local time thresholds.
2.  **Local Causality**: A node updates its state only when sufficient proper time has accumulated or upon receiving a signal from a neighbor, ensuring causal consistency.
3.  **Parallelization via Graph Coloring**:
    * To maximize throughput without race conditions, the `ParallelEventEngine` employs a **Greedy Graph Coloring** algorithm.
    * Nodes assigned to the same "color" are topologically disjoint (share no direct edges). Consequently, they can be updated in parallel across CPU threads or GPU cores without the need for complex locking mechanisms, as no shared state is modified concurrently.

This architecture embeds relativistic effects, such as time dilation in high-curvature regions, directly into the task scheduler logic.

---

## 3. GPU Acceleration (ComputeSharp): High-Performance Mathematics

To process graph topologies exceeding $N > 10^4$ nodes, RqSim integrates GPGPU (General-Purpose computing on Graphics Processing Units) via the **ComputeSharp** library. This allows for the direct translation of C# logic into HLSL shaders.

### Key GPU Modules:

1.  **CSR Engine (Compressed Sparse Row)**:
    *   Optimized for large sparse graphs ($N > 10^5$).
    *   Implements **BiCGStab** solver for Cayley evolution on GPU.
    *   Uses double-precision complex arithmetic in HLSL.
    *   Zero-copy data transfer for rendering.

2.  **Original Dense Engine**:
    *   Optimized for smaller, denser graphs.
    *   Direct buffer-based matrix storage.

3.  **Spectral Analysis**:
    *   Computes eigenvalues/eigenvectors for spectral dimension analysis.
    *   Uses power iteration methods on GPU.

### HPC Stack Integration (Veldrid + Arch ECS)

The project has migrated to a high-performance computing stack:
*   **Arch ECS**: Data-oriented design for entity management.
*   **Veldrid**: Cross-platform low-level graphics API (DX11/Vulkan/Metal).
*   **ImGui.NET**: Immediate mode GUI for debug overlays.

This architecture allows for real-time visualization of massive graphs with millions of edges.

---

## 4. Infrastructure: Thermodynamics of Computation

### Strict Energy Ledger
The `EnergyLedger` class enforces a rigorous conservation law within the simulation.
* **Vacuum Pool**: The system maintains a "vacuum reservoir." Any topological modification (e.g., edge creation) or particle generation requires an energy expenditure extracted from this pool.
* **Thermodynamic Cost (Maxwell's Demon)**: The implementation acknowledges that information processing is not thermodynamically free. Operations such as random number generation (entropic fluctuations) or structural reorganization incur an energy cost. This mechanism prevents the emergence of "perpetual motion" anomalies within the simulation logic, functioning as a computational analogue to Maxwell's Demon constraints.

### Modularity and Experimentation (`IExperiment`)
The infrastructure supports the injection of arbitrary physical laws and initial conditions via the `IExperiment` interface.
Implemented scenarios include:
* **`VacuumGenesisExperiment`**: Initialization from a null or minimal graph state at high "temperature" (simulating a Big Bang scenario).
* **`BlackHoleEvaporationExperiment`**: Investigation of the dynamics of dense cluster dissipation.
* **`BioFoldingExperiment`**: Application of spatial folding physics to biological chain models.

---

## 5. Reference Use Case: The Relational-Quantum Hypothesis

The RQ (Relational-Quantum) hypothesis serves as the primary validation model for this engine due to its **maximal reductionism**.

1.  **Background Independence**: The RqSim environment is not a container. Removing all nodes results in the elimination of the coordinate system itself, adhering to the strict architectural constraints regarding `[Obsolete]` coordinates.
2.  **Emergence**:
    * **Gravity**: The simulation does not implement the inverse-square law explicitly. Gravity emerges as a result of edge weight evolution driven by curvature flows (Ricci Flow).
    * **Matter**: There is no fundamental `Particle` class. Matter is represented as stable, topologically protected "knots" or dense clusters within the graph structure.
3.  **Spectral Dimension Analysis**: RqSim provides the tooling to verify the hypothesis that the universe exhibits 2D characteristics at Planck scales (UV limit, $d_S \approx 2$) and evolves into 4D at macroscopic scales (IR limit, $d_S \approx 4$). The `SpectralWalkEngine` is specifically optimized for this analysis.

---

## 6. Conclusion

**RqSim** represents a specialized intersection of high-performance computing and theoretical physics.

* **Engineering Perspective**: It serves as a rigorous implementation of high-performance C# techniques, utilizing Structs, `Span<T>`, GPU/ComputeSharp integration, and lock-free algorithms to model complex systems with asynchronous local time.
* **Physics Perspective**: It provides a controlled computational environment for *ab initio* cosmology, allowing researchers to define interaction rules and observe whether 4D geometry and gravitational dynamics emerge from a pre-geometric substrate.

The project demonstrates that simulating fundamental reality requires topological data structures and strict information-theoretic conservation laws rather than pre-baked 3D engines.
### 2.1 ✅ Correctly Implemented (Align with RQ)


#### A. Relational Time (RQGraph.RelationalTime.cs)
- ✅ Clock nodes selected by connectivity (hub nodes)
- ✅ Fubini-Study metric for time increment
- ✅ No external time parameter in `ComputeRelationalDtExtended()`
- ✅ Time derived from quantum state change

**Physics**: Correct implementation of Page-Wootters mechanism.

#### B. Network Gravity (RQGraph.NetworkGravity.cs)
- ✅ Forman-Ricci curvature from graph topology
- ✅ Geometry evolution from curvature + stress-energy
- ✅ No coordinate-based metric
- ✅ Einstein equations analog: `ΔWeight ∝ (Mass - Curvature)`

**Physics**: Proper background-independent gravity.

#### C. Relational Dirac (RQGraph.DiracRelational.cs)
- ✅ No coordinate references
- ✅ Staggered fermion approach
- ✅ Gauge parallel transport via edge phases
- ✅ Chirality from sublattices

**Physics**: Consistent discrete Dirac operator on graph.

#### D. Unified Energy (RQGraph.UnifiedEnergy.cs)
- ✅ Single functional combining all fields
- ✅ Metropolis acceptance criterion
- ✅ Binding energy from triangles

**Physics**: Correct total energy computation.

#### E. Probabilistic Quantum (RQGraph.ProbabilisticQuantum.cs)
- ✅ Vacuum fluctuations ∝ curvature
- ✅ Hawking radiation ∝ T⁴
- ✅ Stochastic emission replaces manual events

