# GPU Implementation Guide

Multi GPU 

Про MCMC: "MCMC на GPU 11-20 должен работать в режиме Independent Chains. То есть, мы загружаем топологию один раз, и каждый из 10 GPU запускает свою цепочку Метрополиса с разным Random Seed и разной температурой (Parallel Tempering). Результаты (энергии) собираются на CPU для анализа фазовых переходов."

Про Память: "При реализации SpectralWalkEngine на вторичных GPU убедись, что буферы не пересоздаются (Allocation) каждый раз. Используй паттерн EnsureCapacity или создай буферы один раз с запасом, а при UploadTopology просто обновляй данные через CopyFrom."

Про ComputeSharp: "Помни, что объекты ReadWriteBuffer привязаны к тому GraphicsDevice, который их создал. Нельзя передать буфер, созданный на Device[0], в кернел, запущенный на Device[1]. Данные нужно перегонять через ToArray() (CPU массив)."

This document describes the GPU-accelerated physics engines in RqSim, with focus on double-precision support, ComputeSharp integration, **runtime selection between CPU / Dense-GPU / CSR-GPU**, **Multi-GPU cluster architecture**, and **Plugin-based pipeline**.

It complements `RqSimGraphEngine/MODERNIZATION_GPU.md` (RQ-Hypothesis checklist plan).

---

## 0. Implementation Checklist Status (2025-01)

### ? COMPLETED GPU Algorithms

| Item | Status | Location |
|------|--------|----------|
| **Wilson Loop Kernel** | ? Implemented | `GPUOptimized/Topology/TopologicalInvariantsShader.cs` |
| **Triangle Detection (CSR)** | ? Implemented | `WilsonFluxPerNodeShader`, `TriangleCountShader` |
| **Edge Gauge Protection** | ? Implemented | `EdgeGaugeProtectionShader` - prevents removal of edges with non-trivial flux |
| **Gauge Flux Reduction** | ? Implemented | `GaugeFluxReductionShader` - block-level reduction for monitoring |
| **Render Data Mapper** | ? Implemented | `GPUOptimized/Rendering/RenderMapperShader.cs` - GPU physics?vertex conversion |
| **Gauss Law Violation Monitor** | ? Implemented | `GaussLawViolationShader`, `GaussLawViolationReductionShader`, `DivergenceEShader` |
| **Async Analysis Orchestrator** | ? Integrated | `Core/Scheduler/AsyncAnalysisOrchestrator.cs` with fire-and-forget dispatch |
| **Potential Link Activator** | ? Implemented | `GPUOptimized/Topology/PotentialLinkActivator.cs` - sleeping edge activation |
| **Ollivier-Ricci Curvature** | ? Implemented | `GPUOptimized/TopologicalModels/OllivierRicciCurvature.cs` (CPU with Jaccard approx) |

### ? COMPLETED GPU Plugins (NEW)

| Plugin | Category | Stage | Group |
|--------|----------|-------|-------|
| `TopologicalInvariantsGpuModule` | Topology | Preparation | TopologyProtection |
| `PotentialLinkActivatorGpuModule` | Topology | Integration | TopologyEvolution |
| `GaussLawMonitorGpuModule` | Gauge | PostProcess | GaugeConstraints |
| `RenderDataMapperGpuModule` | Rendering | PostProcess | Visualization |
| `GravityShaderFormanModule` | Gravity | Forces | - |
| `GpuMCMCEngineModule` | Sampling | Forces | - |

### Key Features

1. **Wilson Loop GPU Computation**
   - `WilsonFluxPerNodeShader`: Parallel triangle detection using CSR two-pointer intersection
   - Double precision for phase accumulation (FP64 critical for gauge invariance)
   - Binary search for edge lookup in sorted CSR

2. **Render Pipeline Optimization**
   - `RenderMapperShader` runs on GPU before frame rendering
   - Converts double physics state ? float vertex colors without CPU roundtrip
   - Color modes: Phase (HSV rainbow), Energy (brightness), Mass (red gradient)

3. **Gauge Violation Monitoring**
   - Block-level reduction avoids per-frame PCIe bottleneck
   - Single value readback every ~100 frames for monitoring
   - `GaussLawViolationShader`: per-node (?·E - ?)? computation

4. **Async Multi-GPU Analysis**
   - Fire-and-forget snapshot dispatch from physics loop
   - Temperature ladder for Parallel Tempering MCMC
   - Thread-safe result collection via `ConcurrentBag`

5. **Plugin Group Execution (NEW)**
   - Modules can specify `ModuleGroup` for atomic execution
   - `GroupExecutionMode.Sequential` or `GroupExecutionMode.Parallel`
   - All modules in group complete before next stage/group starts
   - Example: TopologyProtection group runs Wilson loop + gauge check atomically

---

## 0.1 Plugin Architecture (NEW)

### Plugin Categories

```
IncludedPlugins/
??? CPU/
?   ??? EnergyLedgerCpuModule
?   ??? HamiltonianCpuModule
?   ??? ComplexEdgeCpuModule
?   ??? GaugeAwareTopologyCpuModule
?   ??? InternalObserverCpuModule
?   ??? MCMCSamplerCpuModule
??? GPUOptimizedCSR/
    ??? GravityShaderFormanModule
    ??? GpuMCMCEngineModule
    ??? TopologicalInvariantsGpuModule (NEW)
    ??? PotentialLinkActivatorGpuModule (NEW)
    ??? GaussLawMonitorGpuModule (NEW)
    ??? RenderDataMapperGpuModule (NEW)
```

### Group Execution

```csharp
// Modules in same group execute atomically
public string? ModuleGroup => "TopologyProtection";
public GroupExecutionMode GroupMode => GroupExecutionMode.Parallel;

// Pipeline execution order:
// 1. Preparation stage
//    ??? TopologyProtection group (parallel)
//        ??? TopologicalInvariantsGpuModule
//        ??? (other topology constraint modules)
// 2. Forces stage
//    ??? Individual modules by priority
// 3. Integration stage
//    ??? TopologyEvolution group
//        ??? PotentialLinkActivatorGpuModule
// 4. PostProcess stage
//    ??? GaugeConstraints group
//    ?   ??? GaussLawMonitorGpuModule
//    ??? Visualization group
//        ??? RenderDataMapperGpuModule
```

### Registry Usage

```csharp
// Register all default plugins
IncludedPluginsRegistry.RegisterAllDefaultPlugins(pipeline);

// Or register by category
IncludedPluginsRegistry.RegisterDefaultGpuPlugins(pipeline);

// Get specific plugin types
var topologyPlugins = IncludedPluginsRegistry.TopologyPluginTypes;
var gaugePlugins = IncludedPluginsRegistry.GaugePluginTypes;
```

---

## 1. Engine Modes and Runtime Switching

RqSim supports three execution backends:

| Mode | Where it is selected | Purpose |
|------|-----------------------|---------|
| CPU | UI (GPU disabled) | Full compatibility, easiest debugging |
| GPU Dense ("Original") | UI engine selector / Auto | Best for smaller or denser graphs |
| GPU CSR | UI engine selector / Auto | Best for large sparse graphs |

### 1.1 Runtime switching path (UI)

In the WinForms UI, engine mode can be changed at runtime.

The pipeline entrypoint is:

- `RqSim/FormSimAPI/FormSimAPI_GPU.cs`
  - `SetGpuEngineType(GpuEngineType engineType)`
  - `InitializeGpuEnginesWithType(GpuEngineType requestedType)`

Key properties:

- `CurrentEngineType`: user selection (`Auto`, `Original`, `Csr`, `CpuOnly`)
- `ActiveEngineType`: resolved backend actually in use (when `Auto` is selected)

### 1.2 Current integration status

| Subsystem | CPU | GPU Dense | GPU CSR |
|----------:|:---:|:---------:|:-------:|
| Gravity / curvature flow used by event-loops | ? | ? (`OptimizedGpuSimulationEngine` + `graph.InitGpuGravity()`) | ? (CSR Unified can own weights; CPU gravity updates are pushed to CSR without topology rebuild) |
| Spectral dimension (random walk / heat kernel) | ? | ? (`SpectralWalkEngine`, `GpuSpectralEngine`) | ? (topology in CSR form) |
| Cayley unitary evolution | ? | ? (double when available) | ? (`GpuCayleyEvolutionEngineCsr`) |
| Internal Observer (Stage 5) | ? (fallback via graph APIs) | ? (`GPUOptimized/Observer`) | ? (`GPUCompressedSparseRow/Observer`) |
| CSR Unified Engine (Stage 6) | n/a | n/a | ? (wired into UI event-loop; can run physics step and act as weights source-of-truth) |
| **Multi-GPU Cluster** | n/a | ? (Physics GPU 0) | ? (Workers) |

Notes:
- CSR mode is integrated for the **Cayley solver** via `GpuCayleyEvolutionEngineCsr`.
- Stage 6 CSR Unified is wired into the event-loop and supports **weight-only synchronization** (no CSR rebuild) when only weights change.

---

## 2. GPU Engine Architecture

### 2.1 Core engines in use

| Engine | Purpose | Precision |
|--------|---------|-----------|
| `OptimizedGpuSimulationEngine` | Batched dense GPU step and sync back to `RQGraph` | Mixed (float-heavy) |
| `GpuGravityEngine` | Curvature + weight evolution | Float (and some double variants exist in other modules) |
| `SpectralWalkEngine` | Random-walk spectral dimension estimator | Float |
| `GpuSpectralEngine` | Heat-kernel / Laplacian spectral dimension estimator | Float |
| `StatisticsEngine` | GPU reductions / aggregates | Float |

### 2.2 Double-precision engines (physics-critical)

| Engine | Focus | Files |
|--------|-------|-------|
| `GpuCayleyEvolutionEngineDouble` | Unitarity-critical evolution | `GPUOptimized/CayleyEvolution/*Double.cs` |
| `GpuRelationalTimeEngineDouble` | Lapse / local time step (double accumulation) | `GPUOptimized/RelationalTime/*Double.cs` |
| `GpuKleinGordonEngineDouble` | Klein-Gordon field (Verlet integrator) | `GPUOptimized/KleinGordon/*Double.cs` |

### 2.3 RQ Hypothesis Stage 5 (Internal Observer)

Two GPU backends exist:

- Dense: `RQSimulation/GPUOptimized/Observer/*`
  - `GpuObserverEngine`
  - `ObserverShadersDouble.cs`

- CSR: `RQSimulation/GPUCompressedSparseRow/Observer/*`
  - `CsrObserverEngine`
  - `CsrObserverShaders.cs`

Parallelization is physically admissible because operations are:
- phase shifts: per observer node (independent)
- probability density: per node (independent)
- CSR correlations: per observer node iterating neighbors (independent across observer nodes)

### 2.4 RQ Hypothesis Stage 6 (CSR Unified Engine)

`RQSimulation/GPUCompressedSparseRow/Unified/CsrUnifiedEngine` implements a unified CSR pipeline.

#### Weight ownership mode

In UI CSR mode, `FormSimAPI` can treat CSR Unified as the **source of truth for weights**:

- Graph -> CSR (fast): `CsrTopology.UpdateEdgeWeightsFromDense(...)`
- CSR -> Graph (export): `CsrUnifiedEngine.CopyWeightsToGraph(...)`

A lightweight topology signature is used to detect adjacency changes; if topology changes, CSR unified falls back to full `UpdateTopology(graph)`.

It shares one CSR topology to minimize bandwidth.

**Mass semantics note**: CSR Unified now explicitly calls `graph.EnsureCorrelationMassComputed()` and uploads `graph.CorrelationMass` (with `GetNodeMass(i)` as fallback) to guarantee consistency.

---

## 3. Multi-GPU Cluster Architecture (NEW)

### 3.1 Overview

RqSim supports a heterogeneous Multi-GPU cluster for parallel analysis:

```
???????????????????????????????????????????????????????????????????
?                    Multi-GPU Cluster (1 + N Workers)            ?
???????????????????????????????????????????????????????????????????
?  GPU 0 (Physics)     ?  GPU 1-K (Spectral)  ?  GPU K+1-N (MCMC) ?
?  ?????????????????   ?  ??????????????????  ?  ???????????????? ?
?  OptimizedGpuEngine  ?  SpectralWalkEngine  ?  GpuMCMCEngine    ?
?  Ground Truth State  ?  d_s computation     ?  Parallel Temper  ?
?  Weight Evolution    ?  Random walks        ?  Vacuum sampling  ?
???????????????????????????????????????????????????????????????????
           ?                      ?                     ?
           ?    GraphSnapshot     ?    GraphSnapshot    ?
           ?    (CPU RAM)         ?    (CPU RAM)        ?
           ?                      ?                     ?
    ????????????????????????????????????????????????????????????
    ?              AsyncAnalysisOrchestrator                   ?
    ?  - Fire-and-forget dispatch                              ?
    ?  - Result collection via events                          ?
    ?  - Temperature ladder for Parallel Tempering             ?
    ????????????????????????????????????????????????????????????
```

### 3.2 Architecture Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `ComputeCluster` | `Core/Infrastructure/ComputeCluster.cs` | Hardware manager - enumerates GPUs, assigns roles |
| `GraphSnapshot` | `Core/Infrastructure/GraphSnapshot.cs` | DTO for topology transfer (CSR format) |
| `SpectralWorker` | `Core/Scheduler/SpectralWorker.cs` | Worker wrapper for spectral dimension |
| `McmcWorker` | `Core/Scheduler/McmcWorker.cs` | Worker wrapper for MCMC with temperature |
| `AsyncAnalysisOrchestrator` | `Core/Scheduler/AsyncAnalysisOrchestrator.cs` | Task scheduler and result collector |

### 3.3 Data Flow

1. **Physics GPU (Device 0)** computes evolution steps via `OptimizedGpuSimulationEngine`
2. Every N steps (configurable), `DownloadSnapshot()` copies VRAM ? CPU RAM
3. `AsyncAnalysisOrchestrator` dispatches snapshot to free workers (fire-and-forget)
4. Workers upload snapshot to their GPU and compute independently
5. Results reported via events, collected in thread-safe containers

**PCIe Bottleneck Bypass**: Snapshot transfer is infrequent (~100 steps), and computations on each GPU are heavy and independent. No per-step synchronization required.

### 3.4 Device Binding Rules

**CRITICAL**: `ReadWriteBuffer<T>` objects are bound to the `GraphicsDevice` that created them.

```csharp
// WRONG: Cannot pass buffer from Device[0] to kernel on Device[1]
var buffer = device0.AllocateReadWriteBuffer<float>(1000);
device1.For(1000, shaderUsingBuffer);  // ERROR!

// CORRECT: Transfer via CPU array
float[] cpuArray = buffer.ToArray();  // VRAM ? RAM
var buffer1 = device1.AllocateReadWriteBuffer(cpuArray);  // RAM ? VRAM
```

### 3.5 Parallel Tempering (MCMC)

MCMC workers run at different temperatures for efficient vacuum search:

```
Worker 0: ? = 2.0   (T = 0.5)  - Low temp, selective sampling
Worker 1: ? = 1.0   (T = 1.0)  - Standard temperature
Worker 2: ? = 0.5   (T = 2.0)  - High temp, exploratory
...
Worker N: ? = 0.1   (T = 10.0) - Very high temp, rapid mixing
```

Temperature ladder uses geometric spacing: `T_i = T_min * (T_max/T_min)^(i/(n-1))`

### 3.6 Integration Points

**FormSimAPI (EngineAPI)**:
- `FormSimAPI_MultiGpu.cs`: Cluster lifecycle, snapshot dispatch, result handling
- `InitializeMultiGpuCluster()`: Initialize cluster with config
- `TryDispatchSnapshot(tick)`: Called from physics loop
- `GetMultiGpuSpectralDimension()`: Latest d_s from workers

**RqSimConsole (ServerMode)**:
- `ServerModeHost.cs`: Cluster init, publish loop integration
- `ServerContracts.cs`: SharedHeader with Multi-GPU status fields

**Configuration**:
- `ConsoleConfig.MultiGpuSettings`: Enable, worker counts, intervals
- `MultiGpuConfig` class in FormSimAPI

### 3.7 Usage Example

```csharp
// Initialize cluster
var cluster = new ComputeCluster();
cluster.Initialize(new ClusterConfiguration
{
    SpectralWorkerCount = 0,  // Auto-distribute
    McmcWorkerCount = 0,
    MaxGraphSize = 100_000
});

// Create orchestrator
var orchestrator = new AsyncAnalysisOrchestrator(cluster);
orchestrator.Initialize(100_000);

// In physics loop
if (currentTick % 100 == 0)
{
    var snapshot = physicsEngine.DownloadSnapshot(currentTick);
    orchestrator.OnPhysicsStepCompleted(snapshot);  // Fire-and-forget
}

// Get results
var latestDs = orchestrator.GetLatestSpectralResult();
var mcmcResults = orchestrator.GetLatestMcmcResults();
```

---

## 4. Mass semantics: `CorrelationMass` vs `GetNodeMass(i)`

### 4.1 Source of truth

- `RQGraph` stores mass in a cache: `_correlationMass`
- `CorrelationMass` returns the cache array
- `GetNodeMass(i)` returns `_correlationMass[i]` (or `0.0` if cache not initialized)

Therefore:
- They are consistent **only if** `EnsureCorrelationMassComputed()` was called and weights are up to date.

### 4.2 How it is computed

In `RQGraph.Physics.cs`:

- per node: `m_i = sqrt( sum_j w_ij^2 )`

This matches the intended "correlation strength mass" model.

---

## 5. Double precision support

### 5.1 GPU requirements

Shader Model 6.0+ is required for native double precision. Detection:

```csharp
var device = GraphicsDevice.GetDefault();
bool supportsDouble = device.IsDoublePrecisionSupportAvailable();
```

### 5.2 ComputeSharp attributes

All double-precision shaders use:

```csharp
[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[GeneratedComputeShaderDescriptor]
[RequiresDoublePrecisionSupport]
public readonly partial struct MyKernelDouble : IComputeShader
{
    public readonly ReadWriteBuffer<double> data;
}
```

### 5.3 HLSL intrinsic limitation

ComputeSharp HLSL intrinsics (`Sin`, `Cos`, `Exp`, `Log`, `Sqrt`, ...) accept `float` only.
For double pipelines, the project uses:

- float intrinsics for transcendentals
- double accumulation for sums / inner products

---

## 6. Orchestration

`RQUnifiedGpuEngine` is a dense GPU orchestrator for multiple modules (relational time, Klein-Gordon, spinors, Yang-Mills, Cayley, Hawking).

The UI event-loop (`FormSimAPI_SimulationLoops.cs`) currently uses:
- `OptimizedGpuSimulationEngine.StepGpuBatch(...)` for gravity + scalar updates
- sync back via `OptimizedGpuEngine.SyncAllStatesToGraph()`
- Multi-GPU snapshot dispatch via `TryDispatchSnapshot(tick)`

CSR Unified Engine integration into this loop is pending (Stage 6 wiring).

---

*Last updated: 2025-01* 
