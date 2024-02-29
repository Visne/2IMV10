using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SurfaceVisualizer.Shaders;

namespace SurfaceVisualizer;

public class Window(string title, int width, int height) : GameWindow(
    GameWindowSettings.Default,
    new NativeWindowSettings
    {
        APIVersion = new Version(4, 5),
        Title = title,
        ClientSize = new Vector2i(width, height),
        NumberOfSamples = 8, // MSAA
    })
{
    private ShaderProgram _shaderProgram = null!;
    private VertexBufferObject _vertexBuffer = null!;
    private VertexArrayObject _vao = null!;
    private Vector2i _windowSize;
    private double _time;

    protected override void OnLoad()
    {
        base.OnLoad();

        var vertexSource = Utilities.GetEmbeddedResource("SurfaceVisualizer.Shaders.vertex.glsl");
        var fragmentSource = Utilities.GetEmbeddedResource("SurfaceVisualizer.Shaders.fragment.glsl");
        _shaderProgram = new ShaderProgram(vertexSource, fragmentSource);

        _vertexBuffer = new VertexBufferObject(BufferTarget.ArrayBuffer);
        _vao = new VertexArrayObject();

        GL.Enable(EnableCap.FramebufferSrgb);
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.05f, 0.05f, 0.05f, 1f);

        float[] vertices =
        [
            // Positions         // Colors
            -0.5f, -0.5f, 0f,    1f, 0f, 0f,
             0.5f, -0.5f, 0f,    0f, 1f, 0f,
             0.0f,  0.5f, 0f,    0f, 0f, 1f,
        ];

        _vertexBuffer.SetData(vertices, BufferUsageHint.StaticDraw);
        _vao.SetAttributePointer<float>(_shaderProgram, "position", 3, 6, 0);
        _vao.SetAttributePointer<float>(_shaderProgram, "color", 3, 6, 3);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        _time += e.Time;

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shaderProgram.Use();

        var model = Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_time * 100f));
        var view = Matrix4.CreateTranslation(0.0f, 0.0f, -3f);
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),
            _windowSize.X / (float) _windowSize.Y, 0.01f, 100.0f);

        _shaderProgram.SetMatrix4("model", ref model);
        _shaderProgram.SetMatrix4("view", ref view);
        _shaderProgram.SetMatrix4("projection", ref projection);

        _vao.DrawTriangles(3);

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        _windowSize = e.Size;
        InvalidateViewport();
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (e.Key)
        {
            case Keys.Escape:
                Close();
                break;
        }
    }

    private void InvalidateViewport()
    {
        GL.Viewport(0, 0, _windowSize.X, _windowSize.Y);
    }
}
