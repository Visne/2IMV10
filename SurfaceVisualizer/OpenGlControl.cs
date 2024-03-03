using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace SurfaceVisualizer;

public abstract class OpenGlControl : OpenGlControlBase, ICustomHitTest
{
    protected bool IsDragging;
    protected Point MousePosition;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    protected virtual void Render(TimeSpan deltaTime)
    {
    }

    protected virtual void Init()
    {
    }

    protected sealed override void OnOpenGlInit(GlInterface gl)
    {
        GL.LoadBindings(new AvaloniaTkContext(gl));
        Init();
    }

    protected sealed override void OnOpenGlRender(GlInterface gl, int fb)
    {
        GL.Viewport(0, 0, (int) Bounds.Width, (int) Bounds.Height);
        Render(_stopwatch.Elapsed);
        _stopwatch.Restart();

        RequestNextFrameRendering();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) => IsDragging = true;
    protected override void OnPointerReleased(PointerReleasedEventArgs e) => IsDragging = false;
    protected override void OnPointerMoved(PointerEventArgs e) => MousePosition = e.GetPosition(this);

    public bool HitTest(Point point)
    {
        point += new Point(Bounds.X, Bounds.Y);
        return Bounds.Contains(point);
    }

    private class AvaloniaTkContext(GlInterface glInterface) : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName) => glInterface.GetProcAddress(procName);
    }
}
