using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Linq.Expressions;

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
        _vm.ModelChanged = true;
    }

    private async void UseDefaultModels(object? _1, RoutedEventArgs _2)
    {

        // TODO: Validate the model and display an error if invalid
        switch(_vm.ModelIndex)
        {
            case 0:
                _vm.Model = Path.Combine("Resources", "Models", "boys_surface.glb");
                break;
            case 1:
                _vm.Model = Path.Combine("Resources", "Models", "klein_bottle.glb");
                break;
            case 2:
                _vm.Model = Path.Combine("Resources", "Models", "roman_surface.glb");
                break;
            case 3:
                _vm.Model = Path.Combine("Resources", "Models", "triaxial_hexatorus.glb");
                break;
            case 4:
                _vm.Model = Path.Combine("Resources", "Models", "monkey.gltf");
                break;
        }
        _vm.ModelChanged = true;
    }

    private void ReloadButtonClicked(object? _1, RoutedEventArgs _2)
    {
        _vm.ModelChanged = true;
    }
}
