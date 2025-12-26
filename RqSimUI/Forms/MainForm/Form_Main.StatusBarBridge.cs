namespace RqSimForms;

partial class Form_Main
{
    private void InitializeMainFormStatusBar()
    {
        // Reuse existing renderer status bar implementation from 3DVisual partial.
        InitializeRendererStatusBar();
        UpdateRendererStatusBar();
    }

    private void UpdateMainFormStatusBar()
    {
        UpdateRendererStatusBar();
    }
}
