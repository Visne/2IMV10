using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Common;
using SharpGLTF.Schema2;
using static PlaneCutter.PlaneCutter;

namespace VisualDebugger;

public partial class MainWindow : Window
{
    private MainWindowViewModel _vm = null!;
    private readonly List<(double, List<Line>)> _planes;
    private readonly Random _rand = new();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _vm = (MainWindowViewModel)DataContext!;
    }

    public MainWindow()
    {
        InitializeComponent();

        // TODO: Fix hardcoded path
        var model = ModelRoot.Load(Path.Combine("Resources", "Models", "boys_surface.glb"));
        var mesh = model.DefaultScene.VisualChildren.Single().Mesh;
        var primitive = mesh.Primitives.Single();
        _planes = GetCuttingPlanes(new Model(primitive));

        SidePanel.Height.ValueChanged += (_, _) => Redraw();
        Redraw();
    }

    private void Redraw()
    {
        Canvas.Children.Clear();

        if (_vm.Height >= _planes.Count)
            return;

        if (((double, List<Line>)?)_planes.ElementAtOrDefault(_vm.Height) is not var (height, poly))
            return;

        var segments = poly.Select(s => s.Translate(new Vector2(0.8f, 0.8f))).Distinct().ToList();
        var segments2 = poly.Select(s => s.Translate(new Vector2(0.8f, 0.8f))).Distinct().ToList();
        

        var polygons = ToPolygons(segments);
        var intersections = GetIntersections(segments2);

        Console.WriteLine("polygons: " + polygons.Count.ToString());
        Console.WriteLine("intersections: " + intersections.Count.ToString());

        List<Avalonia.Controls.Shapes.Line> lines = [];
        List<Avalonia.Controls.Shapes.Line> intersects = [];
        foreach (var polygon in polygons)
        {
            var bytes = new byte[3];
            _rand.NextBytes(bytes);
            var randomColor = new Color(255, bytes[0], bytes[1], bytes[2]);

            foreach (var part in polygon.Parts)
            {
                lines.Add(new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Point(part.A.X * 500, part.A.Y * 500),
                    EndPoint = new Point(part.B.X * 500, part.B.Y * 500),
                    Stroke = new SolidColorBrush(randomColor),
                });
            }

            // // Close polygon
            // if (polygon.Count <= 1)
            //     continue;
            //
            // var end = polygon.Last();
            // var start = polygon[0];
            //
            // lines.Add(new Line
            // {
            //     StartPoint = new Point(end.X * 500, end.Z * 500),
            //     EndPoint = new Point(start.X * 500, start.Z * 500),
            //     Stroke = new SolidColorBrush(randomColor),
            // });
        }

        // lines = segments.Select(s => new Line
        // {
        //     StartPoint = new Point(s.P1.X * 500, s.P1.Z * 500),
        //     EndPoint = new Point(s.P2.X * 500, s.P2.Z * 500),
        //     Stroke = Brushes.White,
        // }).ToList();

        foreach (var intersect in intersections)
        {
            var bytes = new byte[3];
            _rand.NextBytes(bytes);
            var randomColor = new Color(255, bytes[0], bytes[1], bytes[2]);

            intersects.Add(new Avalonia.Controls.Shapes.Line
            {
                StartPoint = new Point(intersect.X * 500, intersect.Y * 500),
                EndPoint = new Point(intersect.X * 500 + 50, intersect.Y * 500),
                Stroke = new SolidColorBrush(randomColor)
            });
        }

        foreach (var line in lines)
        {
            Canvas.Children.Add(line);
        }
        foreach (var line in intersects)
        {
            Canvas.Children.Add(line);
        }
    }
}

public class MainWindowViewModel
{
    public int Height { get; set; }
}
