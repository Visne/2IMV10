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
    public Color LightColor { get; set; } = Colors.White;
    public Color ObjectColor { get; set; } = Color.Parse("#ccc");
    public string Model { get; set; } = "/home/vince/Desktop/Models/boys_surface_fixed.glb";
    public bool IsWireframe { get; set; }
    public bool ShowCuttingPlanes { get; set; }
}
