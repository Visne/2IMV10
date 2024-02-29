using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace SurfaceVisualizer;

public class VertexBufferObject
{
    private readonly int _handle = GL.GenBuffer();
    private readonly BufferTarget _target;

    // ReSharper disable once ConvertToPrimaryConstructor
    public VertexBufferObject(BufferTarget target)
    {
        _target = target;
    }

    public void SetData<T>(T[] data, BufferUsageHint usageHint = BufferUsageHint.DynamicDraw) where T : struct
    {
        GL.BindBuffer(_target, _handle);
        GL.BufferData(_target, data.Length * Marshal.SizeOf(typeof(T)), data, usageHint);
    }
}
