using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace SurfaceVisualizer;

public partial class SidePanel : UserControl
{
    private MainWindowViewModel _vm = null!;

    public SidePanel() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _vm = (MainWindowViewModel)DataContext!;
    }

    private IStorageFolder? _lastFolder;

    private async void OpenFileButtonClicked(object? _1, RoutedEventArgs _2)
    {
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider!;
        var file = (await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open model",
            FileTypeFilter =
            [
                new FilePickerFileType("glTF model (.gltf, .glb)") { Patterns = [ "*.gltf", "*.glb" ] },
            ],
            AllowMultiple = false,
            SuggestedStartLocation = _lastFolder,
        })).SingleOrDefault();

        if (file == null)
            return;

        _lastFolder = await file.GetParentAsync();
        _vm.Model = file.Path.AbsolutePath;
    }
}
