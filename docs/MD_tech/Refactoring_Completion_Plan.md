# Refactoring Completion Plan

This plan covers the final steps of the migration to Vortice.Windows (DirectX 12) and the restructuring of the RqSimulator solution.

## Remaining Tasks

### 1. Testing and CI Regression Protection (Step 18)
- [ ] **Create Test Project**: Create `RqSimRenderingEngine.Tests` project.
- [ ] **Backend Selection Tests**:
    - Verify `RenderBackendKind.Auto` selects DX12 on Windows.
    - Verify `RenderBackendKind.Dx12` creates `Dx12RenderHost`.
    - Verify `RenderBackendKind.Veldrid` creates `VeldridRenderHost`.
    - Test `RenderBackendPreferenceStore` persistence.
- [ ] **Fallback Tests**:
    - Simulate DX12 initialization failure and verify fallback to Veldrid (or error reporting).
    - Verify diagnostic messages when adapter is missing.
- [ ] **Build Verification**:
    - Ensure `RqSimRenderingEngine` builds cleanly (already verified).
    - Add a test that explicitly checks for platform compatibility warnings (optional, or via analyzer config).

### 2. Documentation (Step 19)
- [ ] **Update README**:
    - Describe the new architecture (Engine vs UI vs Rendering).
    - Add instructions for switching backends.
    - Document requirements for DX12.

### 3. Performance Validation (Step 20)
- [ ] **Benchmarks**:
    - Create a benchmark suite (e.g., using BenchmarkDotNet or a custom frame-time logger).
    - Compare FPS/CPU/GPU usage between Veldrid and DX12 backends for:
        - Large graph rendering (nodes + edges).
        - Compute shader performance (if applicable).

## Execution Strategy

1.  **Initialize Test Infrastructure**: Create the missing test project and add necessary dependencies.
2.  **Implement Smoke Tests**: Write the unit/integration tests for backend selection and fallback logic.
3.  **Run Tests**: Verify everything passes.
4.  **Documentation**: Update the documentation files.
5.  **Benchmarks**: Set up and run benchmarks (if time permits/requested).
