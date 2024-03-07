using Avalonia.Controls;

namespace VisualDebugger;

public partial class SidePanel : UserControl
{
    private MainWindowViewModel _vm = null!;

    public SidePanel() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _vm = (MainWindowViewModel)DataContext!;
    }
}
