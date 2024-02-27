using OpenTK.Graphics.OpenGL4;

namespace SurfaceVisualizer;

public class ShaderProgram
{
    private readonly int _handle;

    public ShaderProgram(string vertexSource, string fragmentSource)
    {
        VertexShader vertexShader = new(vertexSource);
        FragmentShader fragmentShader = new(fragmentSource);

        _handle = GL.CreateProgram();

        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);

        GL.LinkProgram(_handle);

        // Check if linking was successful
        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out var success);
        if (success != 1)
        {
            throw new Exception($"Shader program failed to compile:\n{GL.GetProgramInfoLog(_handle)}");
        }

        // It was successful, so clean up shaders
        GL.DetachShader(_handle, vertexShader);
        vertexShader.Delete();
        GL.DetachShader(_handle, fragmentShader);
        fragmentShader.Delete();
    }

    public void Use() => GL.UseProgram(_handle);
    public int GetAttribLocation(string attribName) => GL.GetAttribLocation(_handle, attribName);
    public static implicit operator int(ShaderProgram p) => p._handle;
}
