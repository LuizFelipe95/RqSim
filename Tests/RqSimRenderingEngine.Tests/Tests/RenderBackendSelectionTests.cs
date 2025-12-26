using RqSimRenderingEngine.Abstractions;

namespace RqSimRenderingEngine.Tests;

/// <summary>
/// Smoke tests for render backend selection and fallback behavior.
/// NOTE: RenderHostFactory has been moved to RqSimUI.Rendering.Plugins for plugin architecture.
/// These tests are temporarily disabled until moved to appropriate test project.
/// </summary>
[TestClass]
[Ignore("RenderHostFactory moved to RqSimUI.Rendering.Plugins - tests need migration")]
public class RenderBackendSelectionTests
{
    [TestMethod]
    public void Placeholder_Test()
    {
        Assert.Inconclusive("RenderBackendSelection tests need to be migrated to RqSimUI.Tests project");
    }
}
