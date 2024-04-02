using Common;
using SharpGLTF.Schema2;

namespace SurfaceVisualizer;

public static class Extensions
{
    public static List<Triangle> GetTriangles(this MeshPrimitive primitive)
    {
        var triangleIndices = primitive.GetTriangleIndices();
        var accessor = primitive.VertexAccessors.Single(a => a.Key == "POSITION").Value;

        var positions = accessor.AsVector3Array();

        return triangleIndices.Select(indices =>
        {
            var (a, b, c) = indices;
            return new Triangle(positions[a], positions[b], positions[c]);
        }).ToList();
    }
}
