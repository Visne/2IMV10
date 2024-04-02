using Avalonia.Controls;
using Avalonia.Media;

namespace SurfaceVisualizer;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
}

public class MainWindowViewModel
{
    public float AmbientStrength { get; set; } = 0.8f;
    public float DiffuseStrength { get; set; } = 0.5f;
    public int RotX { get; set; } = 0;
    public int RotY { get; set; } = 0;
    public int RotZ { get; set; } = 0;
    public Color LightColor { get; set; } = Colors.White;
    public Color ObjectColor { get; set; } = Color.Parse("#ccc");
    public string Model { get; set; } = Path.Combine("Resources", "Models", "boys_surface.glb");
    public bool IsWireframe { get; set; }
    public bool ShowCuttingPlanes { get; set; }
}
