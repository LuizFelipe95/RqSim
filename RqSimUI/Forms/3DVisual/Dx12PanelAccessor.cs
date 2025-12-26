using System.Windows.Forms;

namespace RqSimForms;

public partial class Form_Main
{
    // DX12 host is attached to this panel (currently the existing 3D panel).
    // This accessor centralizes the panel choice so other partials don't depend on older Veldrid fields.
    private Panel? _dx12Panel => _panel3D;
}