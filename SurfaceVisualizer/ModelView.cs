using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpGLTF.Schema2;
using SurfaceVisualizer.Shaders;
using Common;
using static PlaneCutter.PlaneCutter;
using static ModelSplitter.ModelSplitter;

namespace SurfaceVisualizer;

public class ModelView : OpenGlControl
{
    private const double MouseSensitivity = 0.3;
    private const double UpDownSensitivity = 0.005;
    private const double ZoomSensitivity = 0.1;
    private Point _lastMousePos;
    private double _yaw;
    private double _pitch;
    private double _height;
    private double _zoom = 1f;
    private ShaderProgram _shaderProgram = null!;
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
        var modelRoot = ModelRoot.Load(path);
        var mesh = modelRoot.LogicalMeshes[0];
        var primitive = mesh.Primitives[0];
        var model = new Model(primitive);

        var updatedTriangles = new List<Triangle>();
        foreach (var triangle in model.Triangles)
        {
            var updatedTriangle = triangle.Rotate(_vm.RotX, _vm.RotY, _vm.RotZ);
            updatedTriangles.Add(updatedTriangle);
        }
        model.Triangles = updatedTriangles;

        var planes = GetCuttingPlanes(model);
        
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

        var models = SplitModel(model, _cuttingPlanes);

        _modelVaos.Clear();
        models.ForEach(m =>
        {
            var data = m.GetRenderData();
            // TODO handle the model
            var vao = new VertexArrayObject();
            vao.SetIndices(data.Indices);

            var vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);
            vertexBuffer.SetData(data.Vertices.Select(v => v.Position).ToArray(), BufferUsageHint.StaticDraw);
            vao.SetAttributePointer<float>(_shaderProgram, "position", 3, 3, 0);
            
            var normalBuffer = new BufferObject(BufferTarget.ArrayBuffer);
            normalBuffer.SetData(data.Vertices.Select(v => v.Normal).ToArray(), BufferUsageHint.StaticDraw);
            vao.SetAttributePointer<float>(_shaderProgram, "normal", 3, 3, 0);

            _modelVaos.Add(vao);
        });

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
        var view = Matrix4.CreateTranslation(0f, -(float)_height, -3f * (float)_zoom);
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

        for (var i = 0; i < _modelVaos.Count; i++)
        {
            var vao = _modelVaos[i];
            var newModel = model;
            newModel = Matrix4.CreateTranslation(0, 0.8f * i, 0) * newModel; // TODO: Configurable/correct gap
            _shaderProgram.SetMatrix4("model", ref newModel);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            _shaderProgram.SetInt("drawingFront", 0);
            vao.DrawElements();

            GL.CullFace(CullFaceMode.Back);
            _shaderProgram.SetInt("drawingFront", 1);
            vao.DrawElements();
            GL.Disable(EnableCap.CullFace);
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
        
        if (e.GetCurrentPoint(null).Properties.IsMiddleButtonPressed || (e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            _height += delta.Y * UpDownSensitivity;
        }
        else
        {
            _pitch = Math.Clamp(_pitch + delta.Y * MouseSensitivity, -89.9f, 89.9f);
        }

        _yaw += delta.X * MouseSensitivity;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        // TODO: Clamp
        // TODO: Make non-linear?
        _zoom -= e.Delta.Y * ZoomSensitivity;
    }
}
