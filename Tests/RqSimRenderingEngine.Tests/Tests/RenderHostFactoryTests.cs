using Microsoft.VisualStudio.TestTools.UnitTesting;
using RqSimRenderingEngine.Abstractions;

namespace RqSimRenderingEngine.Tests;

/// <summary>
/// Tests for RenderHostFactory core functionality.
/// NOTE: RenderHostFactory has been moved to RqSimUI.Rendering.Plugins for plugin architecture.
/// These tests are temporarily disabled until moved to appropriate test project.
/// </summary>
[TestClass]
[Ignore("RenderHostFactory moved to RqSimUI.Rendering.Plugins - tests need migration")]
public sealed class RenderHostFactoryTests
{
    [TestMethod]
    public void Placeholder_Test()
    {
        // Tests disabled - RenderHostFactory moved to RqSimUI
        Assert.Inconclusive("RenderHostFactory tests need to be migrated to RqSimUI.Tests project");
    }
}
