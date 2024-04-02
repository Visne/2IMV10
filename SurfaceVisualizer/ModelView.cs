using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpGLTF.Schema2;
using SurfaceVisualizer.Shaders;
using VisualDebugger;
using Common;
using static PlaneCutter.PlaneCutter;
using static ModelSplitter.ModelSplitter;

namespace SurfaceVisualizer;

public class ModelView : OpenGlControl
{
    private const double MouseSensitivity = 0.3;
    private const double ZoomSensitivity = 0.1f;
    private Point _lastMousePos;
    private double _yaw;
    private double _pitch;
    private double _zoom = 1f;
    private ShaderProgram _shaderProgram = null!;
    private VertexArrayObject _vao = null!;
    private readonly List<VertexArrayObject> _modelVaos = [];
    private MainWindowViewModel _vm = null!;
    private string _currentModel = null!;
    private VertexArrayObject _planeVao = null!;
    private readonly List<double> _cuttingPlanes = [];

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _vm = (MainWindowViewModel)DataContext!;
    }

    protected override void Init()
    {
        base.Init();

        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);

        var vertexSource = Utilities.GetEmbeddedResource("SurfaceVisualizer.EmbeddedResources.Shaders.vertex.glsl");
        var fragmentSource = Utilities.GetEmbeddedResource("SurfaceVisualizer.EmbeddedResources.Shaders.fragment.glsl");
        _shaderProgram = new ShaderProgram(vertexSource, fragmentSource);

        LoadModel(_vm.Model);
    }

    private void LoadModel(string path)
    {
        var model = ModelRoot.Load(path);
        var mesh = model.LogicalMeshes[0];
        var primitive = mesh.Primitives[0];

        var planes = GetCuttingPlanes(new Model(primitive.GetTriangles()));
        
        // TODO this should be moved into the planecutter class.
        Dictionary<double, int> polygonCounts = [];
        foreach (var (height, segments) in planes)
        {
            var polygons = ToPolygons(segments);
            polygonCounts.Add(height, polygons.Count);
        }
        
        polygonCounts.Add(polygonCounts.Keys.Min() - 0.001, 0);
        //polygonCounts.Add(polygonCounts.Keys.Max() + 0.001, 0);

        var heights = polygonCounts.Keys.ToList();
        heights.Sort();
        heights.Reverse();

        List<double> changePoints = [];
        double? prev = null;
        foreach (var height in heights)
        {
            if (prev == null)
            {
                changePoints.Add(height);
                prev = polygonCounts[height];
            }
            else if (prev != polygonCounts[height])
            {
                changePoints.Add(height);
                prev = polygonCounts[height];
            }
        }

        _cuttingPlanes.Clear();
        for (var i = 1; i < changePoints.Count; i++)
        {
            _cuttingPlanes.Add((changePoints[i - 1] + changePoints[i]) / 2);
        }

        var models = SplitModel(new Model(primitive.GetTriangles()), _cuttingPlanes);

        _modelVaos.Clear();
        models.ForEach(m =>
        {
            var (vertices, indices) = GetVerticesAndIndicesFromTriangles(m.Triangles);
            // TODO handle the model
            var vao = new VertexArrayObject();
            vao.SetIndices(indices);

            var vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);
            vertexBuffer.SetData(vertices, BufferUsageHint.StaticDraw);
            vao.SetAttributePointer<float>(_shaderProgram, "position", 3, 3, 0);

            _modelVaos.Add(vao);
        });

        _vao = new VertexArrayObject();
        _vao.SetIndices(primitive.GetIndices());

        // TODO: Display error message if NORMAL or POSITION is missing
        var vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        vertexBuffer.SetData(primitive.VertexAccessors["POSITION"], BufferUsageHint.StaticDraw);
        _vao.SetAttributePointer<float>(_shaderProgram, "position", 3, 3, 0);

        var normalBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        normalBuffer.SetData(primitive.VertexAccessors["NORMAL"], BufferUsageHint.StaticDraw);
        _vao.SetAttributePointer<float>(_shaderProgram, "normal", 3, 3, 0);

        var planeModel = ModelRoot.Load(Path.Combine("Resources", "Models", "plane.glb"));
        var planePrimitive = planeModel.LogicalMeshes[0].Primitives[0];
        
        _planeVao = new VertexArrayObject();
        _planeVao.SetIndices(planePrimitive.GetIndices());
        
        var planeVertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        planeVertexBuffer.SetData(planePrimitive.VertexAccessors["POSITION"], BufferUsageHint.StaticDraw);
        _planeVao.SetAttributePointer<float>(_shaderProgram, "position", 3, 3, 0);

        var planeNormalBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        planeNormalBuffer.SetData(planePrimitive.VertexAccessors["NORMAL"], BufferUsageHint.StaticDraw);
        _planeVao.SetAttributePointer<float>(_shaderProgram, "normal", 3, 3, 0);

        _currentModel = path;
    }

    protected override void Render(TimeSpan deltaTime)
    {
        base.Render(deltaTime);

        // TODO: Refactor this
        if (_vm.Model != _currentModel)
        {
            LoadModel(_vm.Model);
        }

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shaderProgram.Use();

        var model = Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_yaw))
            * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_pitch));
        var view = Matrix4.CreateTranslation(0f, 0f, -3f * (float)_zoom);
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),
            (float)Math.Max(Bounds.Width / Bounds.Height, 0.01), 0.01f, 100.0f);

        _shaderProgram.SetMatrix4("model", ref model);
        _shaderProgram.SetMatrix4("view", ref view);
        _shaderProgram.SetMatrix4("projection", ref projection);
        
        _shaderProgram.SetFloat("ambientStrength", _vm.AmbientStrength);
        _shaderProgram.SetFloat("diffuseStrength", _vm.DiffuseStrength);

        _shaderProgram.SetVec3("lightColor", _vm.LightColor.Vector());
        _shaderProgram.SetVec3("objectColor", _vm.ObjectColor.Vector());
        _shaderProgram.SetVec3("lightPos", new Vector3(0, 10, 5));

        GL.PolygonMode(MaterialFace.FrontAndBack, _vm.IsWireframe ? PolygonMode.Line : PolygonMode.Fill);
        _vao.DrawElements();

        foreach (var vao in _modelVaos)
        {
            vao.DrawElements();
        }

        if (_vm.ShowCuttingPlanes)
        {
            foreach (var cuttingPlane in _cuttingPlanes)
            {
                var newModel = Matrix4.CreateTranslation(0, (float)cuttingPlane, 0) * model;
                _shaderProgram.SetMatrix4("model", ref newModel);
                _shaderProgram.SetVec3("lightColor", Colors.White.Vector());
                _shaderProgram.SetVec3("objectColor", Color.Parse("#ccc").Vector());

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                _planeVao.DrawElements();
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _lastMousePos = MousePosition;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        
        if (!IsDragging)
            return;

        var delta = MousePosition - _lastMousePos;
        _lastMousePos = MousePosition;

        _yaw += delta.X * MouseSensitivity;
        _pitch = Math.Clamp(_pitch + delta.Y * MouseSensitivity, -89.9f, 89.9f);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        // TODO: Clamp
        // TODO: Make non-linear?
        _zoom -= e.Delta.Y * ZoomSensitivity;
    }

    public static (IList<System.Numerics.Vector3> vertices, IList<uint> indices) GetVerticesAndIndicesFromTriangles(IEnumerable<Triangle> triangles)
    {
        List<System.Numerics.Vector3> vertices = [];
        List<uint> indices = [];

        foreach (var triangle in triangles)
        {
            // Add the vertices of the triangle to the vertices list
            vertices.Add(triangle.A);
            vertices.Add(triangle.B);
            vertices.Add(triangle.C);

            // Add the indices of the triangle to the indices list
            // The index of a vertex is its position in the vertices list
            indices.Add((uint)(vertices.Count - 3)); // Index of triangle.A
            indices.Add((uint)(vertices.Count - 2)); // Index of triangle.B
            indices.Add((uint)(vertices.Count - 1)); // Index of triangle.C
        }

        return (vertices, indices);
    }
}
