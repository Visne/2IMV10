using Avalonia;
using Avalonia.Input;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpGLTF.Schema2;
using SurfaceVisualizer.Shaders;

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
    private MainWindowViewModel _vm = null!;
    private string _currentModel = null!;

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

        _vao = new VertexArrayObject();
        _vao.SetIndices(primitive.GetIndices());

        var vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        vertexBuffer.SetData(primitive.VertexAccessors["POSITION"], BufferUsageHint.StaticDraw);
        _vao.SetAttributePointer<float>(_shaderProgram, "position", 3, 3, 0);

        var normalBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        normalBuffer.SetData(primitive.VertexAccessors["NORMAL"], BufferUsageHint.StaticDraw);
        _vao.SetAttributePointer<float>(_shaderProgram, "normal", 3, 3, 0);

        _currentModel = path;
    }

    protected override void Render(TimeSpan deltaTime)
    {
        base.Render(deltaTime);

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

        _vao.DrawElements();
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

        _zoom -= e.Delta.Y * ZoomSensitivity;
    }
}
