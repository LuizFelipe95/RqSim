using RqSimRenderingEngine.Abstractions;

namespace RqSimRenderingEngine.Tests;

/// <summary>
/// Tests for DX12 fallback behavior and error handling.
/// NOTE: RenderHostFactory has been moved to RqSimUI.Rendering.Plugins for plugin architecture.
/// These tests are temporarily disabled until moved to appropriate test project.
/// </summary>
[TestClass]
[Ignore("RenderHostFactory moved to RqSimUI.Rendering.Plugins - tests need migration")]
public class RenderBackendFallbackTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Placeholder_Test()
    {
        Assert.Inconclusive("RenderBackendFallback tests need to be migrated to RqSimUI.Tests project");
    }
}
