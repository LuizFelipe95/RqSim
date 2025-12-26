# DX12 Migration Error Report

## Issue Description
The application fails to switch to the DirectX 12 rendering backend. When "Auto" or "DirectX 12" is selected in the UI, the active backend remains "Vulkan" (Veldrid default). The expected diagnostic log file `render_backend.log` is not created, indicating that the backend selection logic in `RenderHostFactory` is not being executed.

## Diagnosis
Analysis of the Visual Studio "Output" window during debugging revealed a critical exception occurring immediately after Veldrid assemblies are loaded.

### Critical Exception
```text
Exception thrown: 'System.MissingFieldException' in Veldrid.dll
```

### Root Cause Analysis
The `System.MissingFieldException` typically occurs when there is a binary incompatibility between referenced assemblies. In this case, it indicates a **NuGet package version mismatch** within the Veldrid ecosystem.

Current `RqSimUI.csproj` configuration:
- `Veldrid` (Version 4.9.0)
- `Veldrid.StartupUtilities` (Version 4.9.0)
- `Veldrid.ImGui` (Version 5.89.2-ga121087cad)
- `Veldrid.SPIRV` (Version 1.0.15)

The `Veldrid.ImGui` package version `5.89.2-ga121087cad` appears to be a build from a specific commit that may rely on a different version of the core `Veldrid` library than the stable `4.9.0` installed. This mismatch causes the runtime to fail when trying to access fields or methods that do not exist in the loaded assembly.

### Impact
1.  **Runtime Instability:** The graphics subsystem fails to initialize correctly.
2.  **Execution Flow Interruption:** The exception likely occurs during the initialization of the `VeldridHost` or `ImGuiController`, causing the application to abort the backend switching process before it reaches the `RenderHostFactory.Create` call. This explains the absence of logs.

## Recommended Fix
Align all Veldrid-related packages to compatible versions.
1.  Update `Veldrid`, `Veldrid.StartupUtilities`, `Veldrid.SDL2`, `Veldrid.SPIRV` to the latest stable versions (e.g., 4.9.1 if available, or ensure strict 4.9.0 alignment).
2.  Replace `Veldrid.ImGui` with a version explicitly compatible with Veldrid 4.9.0, or use a local source/fork if the official package is outdated.
3.  Perform a `Rebuild Solution` to ensure clean DLLs in the output directory.
