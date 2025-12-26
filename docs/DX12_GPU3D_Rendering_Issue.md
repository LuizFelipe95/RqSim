# DX12 GPU 3D Rendering Issue - Stripes/Lines Instead of Spheres

## Problem Summary
In the standalone 3D visualization window (`RqSim3DForm`), when switching to **GPU 3D (DX12)** render mode, the visualization shows horizontal stripes or diagonal lines instead of the expected 3D sphere with nodes and edges.

## Root Causes Identified

### Issue #1: Coordinate System Mismatch (FIXED)
**Symptom**: Geometry projected to incorrect locations.

**Root Cause**: The code was using `Matrix4x4.CreateLookAt` which creates a **right-handed** view matrix, but DirectX/DX12 expects a **left-handed** coordinate system.

**Fix Applied**: Changed to use `Matrix4x4.CreateLookAtLeftHanded`:
```csharp
// CameraMatrixHelper.cs
return Matrix4x4.CreateLookAtLeftHanded(cameraPosition, target, Vector3.UnitY);
```

### Issue #2: Reverse-Z Projection Matrix Incorrect (FIXED)
**Symptom**: Horizontal stripes filling the screen (all geometry projecting to thin bands).

**Root Cause**: The Reverse-Z projection matrix M33 and M43 values were calculated incorrectly for left-handed coordinate system.

**Wrong formula**:
```csharp
M33 = nearPlane / range       // n/(f-n) - WRONG for Reverse-Z
M43 = -nearPlane * farPlane / range
```

**Correct formula for Reverse-Z Left-Handed**:
```csharp
// For Reverse-Z LH (near->1, far->0):
M33 = -farPlane / range       // -f/(f-n) 
M43 = nearPlane * farPlane / range  // n*f/(f-n) - positive!
M34 = 1                       // perspective divide term
```

**Derivation**:
- Standard LH projection maps: near?0, far?1
- Reverse-Z LH maps: near?1, far?0
- The depth formula becomes: `z_ndc = f*(z-n) / (z*(f-n))`
- Rearranging: M33 = -f/(f-n), M43 = n*f/(f-n)

### Issue #3: Orbit Camera Z Direction
**Symptom**: Camera facing wrong direction.

**Root Cause**: In left-handed system, +Z goes into the screen. Camera position calculation needed adjustment.

**Fix Applied**:
```csharp
Vector3 cameraPosition = target + new Vector3(
    distance * cosPitch * sinYaw,
    distance * sinPitch,
    -distance * cosPitch * cosYaw  // Negative Z for LH
);
```

## Diagnostic Logging

The following logging was added to verify matrix values:
```
[SphereRenderer] View M41-M44: 0.000, 0.000, 3.000, 1.000
[SphereRenderer] Proj M33=-0.999, M34=1.000, M43=0.010, M44=0.000
```

Expected values after fix:
- M34 = 1.0 (left-handed perspective divide)
- M33 ? -f/(f-n) ? -1.0 for large f
- M43 = n*f/(f-n) > 0

## Depth Configuration (Correct)

| Component | Setting | Purpose |
|-----------|---------|---------|
| DepthFunc | `Greater` | Reverse-Z comparison |
| Clear Depth | `0.0` | Far plane initialization |
| Near Plane | `0.01` | Close viewing support |
| Far Plane | `cameraDistance * 20` | Dynamic based on view |

## Files Modified

| File | Changes |
|------|---------|
| `CameraMatrixHelper.cs` | Fixed Reverse-Z projection matrix, LH view matrices |
| `SphereRenderer.cs` | Added diagnostic logging |
| `LineRenderer.cs` | Added diagnostic logging |

## Technical Reference

### Left-Handed Coordinate System (DirectX)
- +X: Right
- +Y: Up
- +Z: **Into screen** (away from viewer)

### Matrix Conventions
- `System.Numerics.Matrix4x4`: Row-major storage
- HLSL with `mul(vector, matrix)`: Row-vector convention (matches C#)
- Vortice/DX12: Expects row-major matrices

### Projection Matrix Layout (Row-Major)
```
| xScale   0        0           0     |
| 0        yScale   0           0     |
| 0        0        M33         M34   |
| 0        0        M43         0     |
```

For Reverse-Z Left-Handed:
- M33 = -f/(f-n)
- M34 = 1
- M43 = n*f/(f-n)
