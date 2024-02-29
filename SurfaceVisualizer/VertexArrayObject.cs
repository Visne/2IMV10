using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using SurfaceVisualizer.Shaders;

namespace SurfaceVisualizer;

public class VertexArrayObject
{
    private readonly int _handle = GL.GenVertexArray();

    public void DrawTriangles(int count)
    {
        Bind();
        GL.DrawArrays(PrimitiveType.Triangles, 0, count);
    }

    public void SetAttributePointer<T>(ShaderProgram shaderProgram, string attribute, int size, int stride, int offset, bool normalized = false)
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
            throw new NotImplementedException();
        }

        Bind();
        var index = shaderProgram.GetAttribLocation(attribute);
        var byteSize = Marshal.SizeOf<T>();
        GL.VertexAttribPointer(index, size, pointerType, normalized, stride * byteSize, offset * byteSize);
        GL.EnableVertexAttribArray(index);
    }

    private void Bind() => GL.BindVertexArray(_handle);
}
