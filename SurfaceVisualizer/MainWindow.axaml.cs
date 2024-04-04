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
    public int RotX { get; set; }
    public int RotY { get; set; }
    public int RotZ { get; set; }
    public float GapSize { get; set; } = 0.2f;
    public Color LightColor { get; set; } = Colors.White;
    public Color ObjectColor { get; set; } = Color.Parse("#ccc");
    public string Model { get; set; } = Path.Combine("Resources", "Models", "boys_surface.glb");
    public bool ModelChanged { get; set; }
    public int PartRotationMode { get; set; }
    public int Shader { get; set; } = 1;
    public bool UsePolygons {  get; set; } = true;
    public bool UseIntersections { get; set; } = true;
}
