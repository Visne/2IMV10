using System.Reflection;

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
