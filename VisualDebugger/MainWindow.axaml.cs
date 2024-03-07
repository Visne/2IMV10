using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using GeometRi;
using SharpGLTF.Schema2;
using static PlaneCutter.PlaneCutter;

namespace VisualDebugger;

public partial class MainWindow : Window
{
    private MainWindowViewModel _vm = null!;
    private readonly List<KeyValuePair<double, List<Segment3d>>> _planes;
    private readonly Random _rand = new();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _vm = (MainWindowViewModel)DataContext!;
    }

    public MainWindow()
    {
        InitializeComponent();

        var model = ModelRoot.Load("/home/vince/Desktop/Models/boyssurface.glb");
        var mesh = model.DefaultScene.VisualChildren.Single().Mesh;
        var primitive = mesh.Primitives.Single();
        _planes = GetCuttingPlanes(primitive.GetTriangles()).ToList();

        SidePanel.Height.ValueChanged += (_, _) => Redraw();
        Redraw();
    }

    private void Redraw()
    {
        Canvas.Children.Clear();

        if (_vm.Height >= _planes.Count)
            return;

        var segments = _planes[_vm.Height].Value.Select(s => s.Translate(new Vector3d(0.8, 0, 0.8))).Distinct().ToList();

        var polygons = ToPolygons(segments);
        Console.WriteLine($"polygons: {polygons.Count}");

        List<Line> lines = [];
        foreach (var polygon in polygons)
        {
            var bytes = new byte[3];
            _rand.NextBytes(bytes);
            var randomColor = new Color(255, bytes[0], bytes[1], bytes[2]);

            for (var i = 1; i < polygon.Count; i++)
            {
                var p1 = polygon[i - 1];
                var p2 = polygon[i];

                lines.Add(new Line
                {
                    StartPoint = new Point(p1.X * 500, p1.Z * 500),
                    EndPoint = new Point(p2.X * 500, p2.Z * 500),
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

        foreach (var line in lines)
        {
            Canvas.Children.Add(line);
        }
    }
}

public class MainWindowViewModel
{
    public int Height { get; set; }
}

public static class Extensions
{
    public static IList<Triangle> GetTriangles(this MeshPrimitive primitive)
    {
        var triangleIndices = primitive.GetTriangleIndices();
        var accessor = primitive.VertexAccessors.Single(a => a.Key == "POSITION").Value;

        var positions = accessor.AsVector3Array();

        IList<Triangle> triangles = [];

        foreach (var (a, b, c) in triangleIndices)
        {
            var (pa, pb, pc) = (positions[a].Point(), positions[b].Point(), positions[c].Point());

            if (!Point3d.CollinearPoints(pa, pb, pc))
            {
                triangles.Add(new Triangle(pa, pb, pc));
            }
            else
            {
                // TODO: What to do with this? Ignore?
            }
        }

        return triangles;
    }

    public static Point3d Point(this Vector3 vector)
    {
        return new Point3d(vector.X, vector.Y, vector.Z);
    }
}
