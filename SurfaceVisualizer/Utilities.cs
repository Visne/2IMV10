using System.Reflection;
using Avalonia.Media;
using OpenTK.Mathematics;

namespace SurfaceVisualizer;

public static class Utilities
{
    public static string GetEmbeddedResource(string resource)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resource);

        if (stream is null)
        {
            throw new Exception($"No resource {resource}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

public static class ColorExtensions
{
    public static Vector3 Vector(this Color c) => new (c.R / 255f, c.G / 255f, c.B / 255f);
}
