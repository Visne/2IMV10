using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SurfaceVisualizer;

public class Window(string title, int width, int height) : GameWindow(GameWindowSettings.Default,
    new NativeWindowSettings
    {
        APIVersion = new Version(4, 5),
        Title = title,
        ClientSize = new Vector2i(width, height),
    })
{
    private ShaderProgram _shaderProgram = null!;
    private int _vertexBufferHandle;
    private int _colorBufferHandle;
    private int _vaoHandle;

    protected override void OnLoad()
    {
        base.OnLoad();

        var vertexSource = Utilities.GetEmbeddedResource("SurfaceVisualizer.Shaders.vertex.glsl");
        var fragmentSource = Utilities.GetEmbeddedResource("SurfaceVisualizer.Shaders.fragment.glsl");
        _shaderProgram = new ShaderProgram(vertexSource, fragmentSource);

        _vertexBufferHandle = GL.GenBuffer();
        _colorBufferHandle = GL.GenBuffer();
        _vaoHandle = GL.GenVertexArray();

        GL.Enable(EnableCap.FramebufferSrgb); 
        GL.ClearColor(0.05f, 0.05f, 0.05f, 1f);

        GL.BindVertexArray(_vaoHandle);

        float[] positions =
        [
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f,  0.5f, 0.0f,
        ];

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);
        GL.BufferData(BufferTarget.ArrayBuffer, positions.Length * sizeof(float), positions, BufferUsageHint.StaticDraw);

        var positionIndex = _shaderProgram.GetAttribLocation("position");
        GL.VertexAttribPointer(positionIndex, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(positionIndex);

        float[] colors =
        [
            1f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f,
        ];

        GL.BindBuffer(BufferTarget.ArrayBuffer, _colorBufferHandle);
        GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.StaticDraw);

        var colorIndex = _shaderProgram.GetAttribLocation("color");
        GL.VertexAttribPointer(colorIndex, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(colorIndex);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _shaderProgram.Use();
        GL.BindVertexArray(_vaoHandle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
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
}
