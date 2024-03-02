using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using SharpGLTF.Schema2;

namespace SurfaceVisualizer;

public class BufferObject
{
    private readonly int _handle = GL.GenBuffer();
    private readonly BufferTarget _target;

    // ReSharper disable once ConvertToPrimaryConstructor
    public BufferObject(BufferTarget target)
    {
        _target = target;
    }

    public void SetData<T>(IList<T> data, BufferUsageHint usageHint = BufferUsageHint.DynamicDraw) where T : struct
    {
        Bind();
        GL.BufferData(_target, data.Count * Marshal.SizeOf<T>(), data.ToArray(), usageHint);
    }

    public void SetData(Accessor data, BufferUsageHint usageHint = BufferUsageHint.DynamicDraw)
        => SetData(data.SourceBufferView.Content, usageHint);

    private void Bind() => GL.BindBuffer(_target, _handle);
}
