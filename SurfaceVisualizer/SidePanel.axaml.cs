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
        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
        {
            // TODO: Implement roper error message
            throw new Exception($"No {nameof(IStorageProvider)}");
        }

        var file = (await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open model",
            FileTypeFilter =
            [
                new FilePickerFileType("glTF model (.gltf, .glb)") { Patterns = [ "*.gltf", "*.glb" ] },
                new FilePickerFileType("All files") { Patterns = [ "*" ] },
            ],
            AllowMultiple = false,
            SuggestedStartLocation = _lastFolder,
        })).SingleOrDefault();

        if (file == null)
            return;

        _lastFolder = await file.GetParentAsync();
        
        // TODO: Validate the model and display an error if invalid
        _vm.Model = file.Path.AbsolutePath;
    }
}
