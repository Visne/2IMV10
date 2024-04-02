using SharpGLTF.Schema2;

namespace Common;

public static class Extensions
{
    public static List<Triangle> GetTriangles(this MeshPrimitive primitive)
    {
        var triangleIndices = primitive.GetTriangleIndices().ToList();

        var positions = primitive.VertexAccessors.Single(a => a.Key == "POSITION").Value.AsVector3Array();
        var normals = primitive.VertexAccessors.Single(a => a.Key == "NORMAL").Value.AsVector3Array();
        
        var triangles = triangleIndices.Select(indices =>
        {
            var (a, b, c) = indices;
            return new Triangle(positions[a], positions[b], positions[c], normals[a]); // TODO: Is this correct?
        }).ToList();

        return triangles;
    }
}
