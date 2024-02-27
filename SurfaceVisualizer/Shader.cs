using OpenTK.Graphics.OpenGL4;

namespace SurfaceVisualizer;

public abstract class Shader
{
    private readonly int _handle;

    protected Shader(ShaderType type, string shaderSource)
    {
        _handle = GL.CreateShader(type);
        GL.ShaderSource(_handle, shaderSource);
        GL.CompileShader(_handle);

        // Check if shader compiled correctly
        GL.GetShader(_handle, ShaderParameter.CompileStatus, out var success);
        if (success == 1)
            return;

        throw new Exception($"Shader failed to compile:\n{GL.GetShaderInfoLog(_handle)}");
    }

    public void Delete()
    {
        GL.DeleteShader(_handle);
    }

    public static implicit operator int(Shader s) => s._handle;
}

public class VertexShader(string shaderSource) : Shader(ShaderType.VertexShader, shaderSource);
public class FragmentShader(string shaderSource) : Shader(ShaderType.FragmentShader, shaderSource);
