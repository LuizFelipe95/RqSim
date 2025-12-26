# DX12 GPU 3D Rendering Issue - RESOLVED

## Problem Summary
In the standalone 3D visualization window (`RqSim3DForm`), when switching to **GPU 3D (DX12)** render mode, the visualization initially showed black screen, then horizontal stripes, then oversized spheres.

## Current Status: ? FULLY RESOLVED

### All Issues Fixed
| Issue | Status | Fix Applied |
|-------|--------|-------------|
| Coordinate System Mismatch | ? FIXED | `CreateLookAtLeftHanded` |
| Reverse-Z Projection Matrix | ? FIXED | Corrected M33 formula: `-n/(f-n)` |
| Orbit Camera Z Direction | ? FIXED | Negative Z for LH system |
| HLSL Matrix Convention | ? FIXED | Added `row_major` to HLSL cbuffers |
| **Sphere Size Too Large** | ? FIXED | Scale radius relative to graph size |

### ?? Known Non-Critical Issues
| Issue | Status | Impact |
|-------|--------|--------|
| GPU Culling Shader Error | ? FAILING | Non-critical - falls back to direct draw |
| Occlusion Culling Shader Error | ? FAILING | Non-critical - falls back to no occlusion |

---

## Root Cause Analysis

### Issue #1: Coordinate System (Fixed)
Use `CreateLookAtLeftHanded` instead of `CreateLookAt` for DirectX left-handed system.

### Issue #2: Reverse-Z Projection (Fixed)
**Correct formula**: M33 = `-n/(f-n)` (NOT `-f/(f-n)`)

### Issue #3: HLSL Matrix Convention (Fixed)
```hlsl
cbuffer Camera : register(b0)
{
    row_major float4x4 View;       // Matches C# row-major layout
    row_major float4x4 Projection;
};
```

### Issue #4: Sphere Radius Scaling (Fixed)
**Problem**: `_nodeRadius` was used as absolute world units (0.5 units = 25% of graph size!)

**Fix**: Scale radius relative to graph size:
```csharp
// Scale radius relative to graph size
float baseRadiusFraction = 0.02f;  // 2% of graph at slider minimum
float worldRadius = _graphRadius * baseRadiusFraction * _nodeRadius;

// Clamp to reasonable range
worldRadius = MathF.Max(worldRadius, 0.01f);
worldRadius = MathF.Min(worldRadius, _graphRadius * 0.3f);
```

---

## Files Modified

| File | Changes |
|------|---------|
| `CameraMatrixHelper.cs` | Fixed Reverse-Z formula, LH view matrices |
| `Dx12Shaders.cs` | Added `row_major` to NodeVs, LineVs cbuffer matrices |
| `Form_Rsim3DForm.GPU3D.cs` | Scale sphere radius relative to graph size |
| `SphereRenderer.cs` | Added diagnostic logging |
| `LineRenderer.cs` | Added diagnostic logging |

---

## Test Verification

Test suite in `Tests\RqSimRenderingEngine.Dx12Tests\`:

| Test File | Tests | Status |
|-----------|-------|--------|
| CameraMatrixHelperTests.cs | 21 | ? All Pass |
| ReverseZProjectionTests.cs | 17 | ? All Pass |
| CoordinateSystemTests.cs | 17 | ? All Pass |
| ShaderMatrixConventionTests.cs | 15 | ? All Pass |
| SphereMeshTests.cs | 9 | ? All Pass |
| InstanceBufferTests.cs | 5 | ? All Pass |
| **Total** | **84** | ? **All Pass** |

---

## Technical Reference

### Matrix Memory Layout
| System | Layout | Element Order |
|--------|--------|---------------|
| C# `Matrix4x4` | Row-major | M11, M12, M13, M14, M21, ... |
| HLSL default | Column-major | M11, M21, M31, M41, M12, ... |
| HLSL `row_major` | Row-major | M11, M12, M13, M14, M21, ... |

### Reverse-Z Projection (Row-Major, Left-Handed)
```
| xScale   0        0           0     |
| 0        yScale   0           0     |
| 0        0        -n/(f-n)    1     |  <- M33, M34
| 0        0        n*f/(f-n)   0     |  <- M43, M44
```

### Sphere Radius Scaling
- ImGui 2D: radius in **screen pixels** (1-3 px typical)
- GPU 3D: radius in **world units** (must scale to graph size)
- Formula: `worldRadius = graphRadius ? 0.02 ? sliderValue`

---

## Resolution Summary

1. ? **Black Screen** ? Fixed with `row_major` in HLSL shaders
2. ? **Wrong Depth** ? Fixed Reverse-Z formula (M33 = -n/(f-n))
3. ? **Oversized Spheres** ? Scale radius relative to graph bounds
4. ? **All 84 unit tests pass**
