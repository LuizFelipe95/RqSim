# Hard Science Mode - Режим с Ограничениями

## Статус: ? Q1-COMPLIANT (v3.2 Final)

Дата ревью: 2025  
Версия: v3.2 (Q1 Compliance Release + Saturating Arithmetic)

---

## Сводная таблица

| Компонент | Статус | Комментарий |
|---------|--------|-------------|
| Фиксированная точка арифметики | ? **Q1-COMPLIANT** | int32 fixed-point с saturating atomic add |
| Conservation Kernels | ? **Q1-COMPLIANT** | int32 fixed-point + CPU aggregation |
| Energy Audit 64-bit | ? **Q1-COMPLIANT** | GPU Hi/Lo emulation + CPU 64-bit |
| Integrity Flags | ? **Q1-COMPLIANT** | GPU?CPU overflow/underflow detection |
| SinkhornAdaptive kernel | ? **Q1-COMPLIANT** | TDR protection + Jaccard fallback |
| SimulationHealthMonitor | ? **Реализован** | CPU-side NaN/Inf detection |
| SimulationParameters | ? **Q1-COMPLIANT** | Правильное выравнивание полей |
| StrongScienceSimulationConfig | ? **Интегрирован** | Профильная система |
| GpuIntegrityViolationException | ? **НОВОЕ v3.2** | Исключение для GPU integrity failures |

---

## HLSL/ComputeSharp Ограничения

### Критические ограничения
ComputeSharp/HLSL **не поддерживает**:
1. `long` (int64) в GPU шейдерах
2. Передача `ReadWriteBuffer<T>` как параметра метода
3. 64-bit atomic операции

```
CMPS0050: uses the invalid type long 
(only int, uint, float, double are supported)
```

### Решения для Q1 Compliance
1. **int32 fixed-point** (scale = 2^24, ~7 decimal digits)
2. **CPU-side aggregation** для 64-bit сумм
3. **Насыщающая арифметика** с флагами переполнения
4. **Hi/Lo int pairs** для GPU 64-bit эмуляции

---

## 1. Fixed-Point Limits (v3.2 NEW)

### Статус: ? **Q1-COMPLIANT**

**Файл:** `PhysicsConstants.Simulation.cs`

```csharp
public static class FixedPointLimits
{
    /// <summary>Fixed-point scale: 2^24 = 16,777,216</summary>
    public const int ENERGY_SCALE = 16777216;
    
    /// <summary>Safe max for int32: ~95% of Int32.MaxValue</summary>
    public const int MAX_SAFE_ACCUMULATOR = 2000000000;
    
    /// <summary>Max physical energy before saturation: ~119.2 units</summary>
    public const double MAX_PHYSICAL_ENERGY = (double)MAX_SAFE_ACCUMULATOR / ENERGY_SCALE;
}
```

### Integrity Flags

```csharp
public static class IntegrityFlags
{
    public const int FLAG_OK = 0;
    public const int FLAG_OVERFLOW_DETECTED = 1;
    public const int FLAG_UNDERFLOW_DETECTED = 2;
    public const int FLAG_TDR_TRUNCATION = 4;
    public const int FLAG_CONSERVATION_VIOLATION = 8;
    public const int FLAG_NAN_DETECTED = 16;
    public const int FLAG_64BIT_OVERFLOW = 32;
}
```

---

## 2. Conservation Law Implementation

### Статус: ? **Q1-COMPLIANT**

**Файлы:**
- `InterlockedDoubleHelper.cs`
- `ConservationShaders.cs`

### Saturating Atomic Add (v3.2)

```csharp
// SafeAtomicAdd - saturating arithmetic with overflow detection
private void SafeAtomicAdd(ref int accumulator, int delta, ref int flags)
{
    const int MAX_SAFE = 2000000000;
    const int MIN_SAFE = -2000000000;
    const int FLAG_OVERFLOW = 1;
    const int FLAG_UNDERFLOW = 2;
    
    int oldVal, newVal;
    do
    {
        oldVal = accumulator;
        
        // Overflow detection
        if (delta > 0 && oldVal > MAX_SAFE - delta)
        {
            Hlsl.InterlockedOr(ref flags, FLAG_OVERFLOW);
            return; // Saturate - don't add
        }
        if (delta < 0 && oldVal < MIN_SAFE - delta)
        {
            Hlsl.InterlockedOr(ref flags, FLAG_UNDERFLOW);
            return; // Saturate - don't add
        }
        
        newVal = oldVal + delta;
    }
    while (Hlsl.InterlockedCompareExchange(ref accumulator, newVal, oldVal) != oldVal);
}
```

### CPU Aggregation for 64-bit

```csharp
public static double AggregateToDouble(int[] scaledValues, int scale)
{
    long total = 0;  // 64-bit accumulator on CPU
    foreach (int val in scaledValues)
        total += val;
    return (double)total / scale;
}
```

---

## 3. 64-bit GPU Emulation (v3.2 NEW)

### Статус: ? **Q1-COMPLIANT**

**Файл:** `ConservationShaders.cs`

```csharp
// ScientificEnergyAuditKernel64 - Hi/Lo int pair emulation
private void AddToNodeMass64(int value)
{
    int signExtension = (value < 0) ? -1 : 0;
    uint lowPart = (uint)value;
    
    // Step 1: Atomic add to low part
    int oldLow;
    Hlsl.InterlockedAdd(ref TotalNodeMassLo[0], value, out oldLow);
    
    uint oldLowU = (uint)oldLow;
    uint newLowU = oldLowU + lowPart;
    
    // Step 2: Detect carry/borrow
    int carry = 0;
    if (value >= 0 && newLowU < oldLowU) carry = 1;
    else if (value < 0 && newLowU > oldLowU) carry = -1;
    
    // Step 3: Add sign extension + carry to high part
    int highDelta = signExtension + carry;
    if (highDelta != 0)
        Hlsl.InterlockedAdd(ref TotalNodeMassHi[0], highDelta, out _);
}
```

**ВАЖНО:** Метод не принимает `ReadWriteBuffer<T>` как параметр - это ограничение HLSL.

---

## 4. EnergyLedger Validation (v3.2 NEW)

### Статус: ? **Q1-COMPLIANT**

**Файл:** `EnergyLedger.cs`

```csharp
public void ValidateConservation(int integrityFlags)
{
    if (integrityFlags == PhysicsConstants.IntegrityFlags.FLAG_OK)
        return;
    
    // Build detailed error report
    var issues = new StringBuilder();
    
    if ((integrityFlags & FLAG_OVERFLOW_DETECTED) != 0)
        issues.AppendLine("? OVERFLOW: Fixed-point overflow detected.");
        
    if ((integrityFlags & FLAG_64BIT_OVERFLOW) != 0)
        issues.AppendLine("? 64BIT_OVERFLOW: Global sum exceeded capacity.");
    
    throw new GpuIntegrityViolationException(issues.ToString(), integrityFlags);
}
```

---

## 5. Memory Alignment Fix

### Статус: ? **Q1-COMPLIANT**

**Файл:** `SimulationParameters.cs`

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public partial struct SimulationParameters
{
    // ...
    public int SinkhornIterations;     // Offset 216, Size 4
    private int _paddingSinkhorn;      // ALIGNMENT FIX
    public double SinkhornEpsilon;     // Offset 224, Size 8 (aligned)
    // ...
}
```

---

## 6. TDR Protection

### Статус: ? **Q1-COMPLIANT**

**Файл:** `GravityShadersDouble.cs`

```csharp
// SinkhornOllivierRicciKernelAdaptive
int effectiveMaxNeighbors = Math.Min(maxNeighbors, 128);
int effectiveIterations = Math.Min(sinkhornIterations, 30);

int estimatedOps = degU * degV * effectiveIterations;
if (estimatedOps > maxOperationsPerThread)
{
    curvatures[e] = ComputeJaccardCurvatureFallback(...);
    truncationFlags[e] = 2;  // TDR fallback
    return;
}
```

### Safe Limits

| Mode | maxNeighbors | iterations | TDR Risk |
|------|--------------|------------|----------|
| Visual | 32 | 10 | ? None |
| Scientific | 64 | 20 | ? Low |
| Maximum | 128 | 30 | ?? Medium |

---

## 7. Exception Hierarchy (v3.2 NEW)

| Exception | Namespace | Purpose |
|-----------|-----------|---------|
| `ScientificMalpracticeException` | `RQSimulation.Core.StrongScience` | Научная интегритет нарушена |
| `GpuIntegrityViolationException` | `RQSimulation` | GPU fixed-point overflow |
| `EnergyConservationException` | `RQSimulation` | Нарушение сохранения энергии |
| `CausalityViolationException` | `RQSimulation` | Нарушение причинности |

---

## Q1 Publication Methodology Text

> **Numerical Precision:**
> Conservation laws are implemented using int32 fixed-point arithmetic
> with a scale factor of 2^24, providing ~7 significant decimal digits.
> Energy transfers use saturating atomic arithmetic with explicit overflow
> detection flags. For 64-bit accumulation, both GPU-side Hi/Lo emulation
> and CPU-side 64-bit aggregation are employed.

> **GPU Atomics:**
> HLSL does not support 64-bit atomic operations or passing buffer references
> as method parameters. We use native int32 InterlockedAdd operations with
> Compare-And-Swap loops for saturating behavior. The split-uint CAS pattern
> was explicitly rejected due to torn-write race conditions.

> **Integrity Monitoring:**
> GPU kernels set integrity flags (overflow, underflow, NaN, TDR truncation)
> that are read by CPU-side validators. Any non-zero flag triggers detailed
> diagnostic output and optionally throws `GpuIntegrityViolationException`.

> **Timeout Protection:**
> Sinkhorn iterations are capped at 30 with maxNeighbors ? 128 to prevent
> Windows TDR from killing GPU operations. High-degree nodes fall back
> to O(n) Jaccard approximation with explicit audit flags.

---

## Deprecated Kernels

| Kernel | Issue | Replacement |
|--------|-------|-------------|
| `InterlockedAddDoubleKernel_Legacy` | Race conditions | `ScientificConservationKernel` |
| `DoubleConservationKernel` | Split-uint CAS | `ScientificConservationKernel` |
| `EnergyAuditKernel` | High/Low разделение | `ScientificEnergyAuditKernel64` |

---

## Changelog

### v3.2 (2025-01-XX)
- ? Added `FixedPointLimits` constants
- ? Added `IntegrityFlags` for GPU?CPU communication
- ? Implemented saturating atomic add with overflow detection
- ? Added `ScientificEnergyAuditKernel64` with Hi/Lo emulation
- ? Added `ValidateConservation` method to EnergyLedger
- ? Renamed `ScientificMalpracticeException` in EnergyLedger to `GpuIntegrityViolationException`
- ? Fixed HLSL compilation error (cannot pass ReadWriteBuffer as method parameter)

### v3.1 (2024-XX-XX)
- Исходная версия Q1-COMPLIANT
