using Avalonia;
using Avalonia.Input;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpGLTF.Schema2;
using SurfaceVisualizer.Shaders;
using Common;
using static PlaneCutter.PlaneCutter;
using static ModelSplitter.ModelSplitter;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;

namespace SurfaceVisualizer;

public class ModelView : OpenGlControl
{
    private const double MouseSensitivity = 0.3;
    private const double UpDownSensitivity = 0.005;
    private const double ZoomSensitivity = 0.1;
    private Point _lastMousePos;
    private double _yaw;
    private double _pitch;
    private double _height = 0.8f;
    private double _zoom = 1f;
    private ShaderProgram _shaderProgram = null!;
    private ShaderProgram _basic = null!;
    private ShaderProgram _handDrawn = null!;
    private readonly List<(VertexArrayObject VAO, Model Model)> _modelVaos = [];
    private MainWindowViewModel _vm = null!;
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
        var fragmentSource = Utilities.GetEmbeddedResource("SurfaceVisualizer.EmbeddedResources.Shaders.fragment_phong.glsl");
        _basic = new ShaderProgram(vertexSource, fragmentSource);
        
        fragmentSource = Utilities.GetEmbeddedResource("SurfaceVisualizer.EmbeddedResources.Shaders.fragment.glsl");
        _handDrawn = new ShaderProgram(vertexSource, fragmentSource);

        _shaderProgram = _vm.Shader switch
        {
            0 => _basic,
            1 => _basic,
            2 => _handDrawn,
            _ => throw new NotImplementedException(),
        };
        
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
        Dictionary<double, (int, int)> PolygonAspectCount = [];
        foreach (var (height, segments) in planes)
        {
            var segments2 = segments;
            var intersections = GetIntersections(segments2);
            var polygons = ToPolygons(segments);
            

            //replace the ui selection criteria here
            var polyCount = _vm.UsePolygons ? polygons.Count : 0;
            var intersectionCount = _vm.UseIntersections ? intersections.Count : 0;
            //Maybe we can add other aspects than the distinct objecss here, like intersectoins.
            PolygonAspectCount.Add(height, (polyCount, intersectionCount));
        }

        
        PolygonAspectCount.Add(PolygonAspectCount.Keys.Min() - 0.001, (0,0));
        //PolygonAspectCount.Add(PolygonAspectCount.Keys.Max() + 0.001, (0,0));

        var heights = PolygonAspectCount.Keys.ToList();
        heights.Sort();
        heights.Reverse();

        List<double> changePoints = [];
        (int?, int?) prev = (null, null);
        foreach (var height in heights)
        {
            if (prev == (null, null))
            {
                changePoints.Add(height);
                prev = PolygonAspectCount[height];
            }
            else if (prev != PolygonAspectCount[height])
            {
                changePoints.Add(height);
                prev = PolygonAspectCount[height];
            }
        }

        _cuttingPlanes.Clear();
        for (var i = 1; i < changePoints.Count; i++)
        {
            _cuttingPlanes.Add((changePoints[i - 1] + changePoints[i]) / 2);
        }

        if (_vm.CustomPlanes.Length > 0)
        {
            var (bottom, top) = model.VerticalBounds();

            foreach (var number in _vm.CustomPlanes.Split(";"))
            {
                var corrected = number.Trim().Replace(',', '.');
                if (double.TryParse(corrected, CultureInfo.InvariantCulture, out var addHeight))
                {
                    _cuttingPlanes.Add(bottom + (top - bottom) * addHeight);
                }
                else
                {
                    Console.WriteLine($"Failed to parse: {corrected}");
                }
            }
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

            _modelVaos.Add((vao, m));
        });
    }

    protected override void Render(TimeSpan deltaTime)
    {
        base.Render(deltaTime);

        // TODO: Refactor this
        if (_vm.ModelChanged)
        {
            LoadModel(_vm.Model);
            _vm.ModelChanged = false;
        }

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        (_shaderProgram, var polygonMode) = _vm.Shader switch
        {
            0 => (_basic, PolygonMode.Line),
            1 => (_basic, PolygonMode.Fill),
            2 => (_handDrawn, PolygonMode.Fill),
            _ => throw new NotImplementedException(),
        };

        GL.PolygonMode(MaterialFace.FrontAndBack, polygonMode);

        _shaderProgram.Use();

        var modelTransform = Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_yaw))
            * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_pitch));
        var viewTranfsform = Matrix4.CreateTranslation(0f, -(float)_height, -3f * (float)_zoom);
        var projectionTransform = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),
            (float)Math.Max(Bounds.Width / Bounds.Height, 0.01), 0.01f, 100.0f);

        _shaderProgram.SetMatrix4("model", ref modelTransform);
        _shaderProgram.SetMatrix4("view", ref viewTranfsform);
        _shaderProgram.SetMatrix4("projection", ref projectionTransform);
        
        _shaderProgram.SetFloat("ambientStrength", _vm.AmbientStrength);
        _shaderProgram.SetFloat("diffuseStrength", _vm.DiffuseStrength);

        _shaderProgram.SetVec3("lightColor", _vm.LightColor.Vector());
        _shaderProgram.SetVec3("objectColor", _vm.ObjectColor.Vector());
        _shaderProgram.SetVec3("lightPos", new Vector3(0, 10, 5));

        var height = 0f;
        for (var i = 0; i < _modelVaos.Count; i++)
        {
            var vao = _modelVaos[i].VAO;
            var model = _modelVaos[i].Model;
            var newModelTransform = modelTransform;

            var translation = Matrix4.CreateTranslation(0, _vm.GapSize * i + height, 0);

            newModelTransform = _vm.PartRotationMode switch
            {
                0 => translation * newModelTransform,
                1 => newModelTransform * translation,
                _ => throw new NotImplementedException(),
            };

            _shaderProgram.SetMatrix4("model", ref newModelTransform);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            _shaderProgram.SetInt("drawingFront", 0);
            vao.DrawElements();

            GL.CullFace(CullFaceMode.Back);
            _shaderProgram.SetInt("drawingFront", 1);
            vao.DrawElements();
            GL.Disable(EnableCap.CullFace);

            var bounds = model.VerticalBounds();
            height += bounds.Top - bounds.Bottom;
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
        // TODO: e.Delta.Y changes even if the user scrolls outside of the window, causing a sudden jump
        _zoom -= e.Delta.Y * ZoomSensitivity;
    }
}
