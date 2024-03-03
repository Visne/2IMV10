using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using SurfaceVisualizer.Shaders;

namespace SurfaceVisualizer;

public class VertexArrayObject
{
    private readonly int _handle = GL.GenVertexArray();
    private BufferObject? _indexBuffer;
    private IList<uint>? _indices;

    public void DrawTriangles(int count)
    {
        Bind();
        GL.DrawArrays(PrimitiveType.Triangles, 0, count);
    }

    public void DrawElements()
    {
        if (_indices is not { } indices)
            throw new Exception($"Indices not set! Call {nameof(SetIndices)} first.");

        Bind();
        GL.DrawElements(BeginMode.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
    }

    public void SetIndices(IList<uint> indices, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
    {
        Bind();
        _indexBuffer ??= new BufferObject(BufferTarget.ElementArrayBuffer);
        _indexBuffer.SetData(indices, usageHint);
        _indices = indices;
    }

    public void SetAttributePointer<T>(ShaderProgram shaderProgram,
        string attribute,
        int size,
        int stride,
        int offset,
        bool normalized = false)
    {
        var mapping = new Dictionary<Type, VertexAttribPointerType>
        {
            { typeof(sbyte),  VertexAttribPointerType.Byte },
            { typeof(byte),   VertexAttribPointerType.UnsignedByte },
            { typeof(short),  VertexAttribPointerType.Short },
            { typeof(ushort), VertexAttribPointerType.UnsignedShort },
            { typeof(int),    VertexAttribPointerType.Int },
            { typeof(uint),   VertexAttribPointerType.UnsignedInt },
            { typeof(float),  VertexAttribPointerType.Float },
            { typeof(double), VertexAttribPointerType.Double },
        };

        if (!mapping.TryGetValue(typeof(T), out var pointerType))
        {
            throw new NotImplementedException($"{nameof(SetAttributePointer)} does not support type {nameof(T)}.");
        }

        Bind();
        var index = shaderProgram.GetAttribLocation(attribute);
        var byteSize = Marshal.SizeOf<T>();
        GL.VertexAttribPointer(index, size, pointerType, normalized, stride * byteSize, offset * byteSize);
        GL.EnableVertexAttribArray(index);
    }

    private void Bind() => GL.BindVertexArray(_handle);
}
