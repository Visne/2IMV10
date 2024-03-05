using System.Numerics;
using SharpGLTF.Schema2;

var model = ModelRoot.Load("/home/vince/Desktop/Models/cube.glb");
var mesh = model.DefaultScene.VisualChildren.Single().Mesh;
var primitive = mesh.Primitives.Single();

for (var height = -2f; height < 2f; height += 0.1f)
{
    foreach (var (a, b, c) in primitive.GetTriangles())
    {
        var aBelow = a.Y < height;
        var bBelow = b.Y < height;
        var cBelow = c.Y < height;

        if (!(aBelow == bBelow && bBelow == cBelow))
        {
            // Triangle intersects the plane
            Console.WriteLine($"intersects {height:0.000}");
            goto ContinueOuter;
        }
    }
    
    Console.WriteLine($"does not intersect {height:0.000}");
    
    ContinueOuter: ;
}

internal static class MeshPrimitiveExtensions
{
    public static IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> GetTriangles(this MeshPrimitive primitive)
    {
        var triangleIndices = primitive.GetTriangleIndices();
        var accessor = primitive.VertexAccessors.Single(a => a.Key == "POSITION").Value;

        var positions = accessor.AsVector3Array();

        foreach (var (a, b, c) in triangleIndices)
        {
            yield return (positions[a], positions[b], positions[c]);
        }
    }
}
