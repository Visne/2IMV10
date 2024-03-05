using System.Numerics;
using SharpGLTF.Schema2;

var model = ModelRoot.Load("/home/vince/Desktop/Models/cube.glb");
var mesh = model.DefaultScene.VisualChildren.Single().Mesh;
var primitive = mesh.Primitives.Single();

float[] splitHeights = { -1f, 0f, 1f };

for (int i = 1; i < splitHeights.Length; i++ )
{
    var low = splitHeights[i - 1];
    var high = splitHeights[i];
    (Vector3 A, Vector3 B, Vector3 C)[] splitTriangles = [];



    foreach (var (a1, b1, c1) in primitive.GetTriangles())
    {
        var a = a1;
        var b = b1;
        var c = c1;
        if (a.Y > b.Y) {
            var z = a;
            a = b;
            b = z;
        }
        if (b.Y > c.Y) {
            var z = b;
            b = c;
            c = z;
        }
        if (a.Y > b.Y) {
            var z = a;
            b = a;
            a = z;
        }
        //Sorted the values, now a is smallest and c is highest


        if (a.Y < low || c.Y >= high)
        {
            //Outside of current split
            return;
        }
        if (a.Y >= low & c.Y < high)
        {
            //Fully inside current split
            splitTriangles.Append((a, b, c));
        }
        //Cases where the triangles intersect the line
        if (a.Y < low)
        {
            if (b.Y > low)
            {
                //One vertex below bottom
                splitTriangles.Append((getIntersection(a, c, low), b, c));
                splitTriangles.Append((getIntersection(a, c, low), getIntersection(a, b, low), b));
                return;
            }
            else
            {
                //Two vertices below bottom
                splitTriangles.Append((getIntersection(a, c, low), getIntersection(b, c, low), c));
                return;
            }
        }

        if (c.Y >= high)
        {
            if(b.Y < high)
            {
                //One vertex Above top
                splitTriangles.Append((getIntersection(a, c, low), a, b));
                splitTriangles.Append((getIntersection(a, c, low), getIntersection(b, c, low), b));
                return;
            }
            else
            {
                //Two vertices Above top
                splitTriangles.Append((getIntersection(a, c, low), getIntersection(b, c, low), a));
                return;
            }
        }
        Console.WriteLine("what edgecase did i forget?");
    }


}
static Vector3 getIntersection(Vector3 A, Vector3 B, float Height)
{
    float totalLength = Vector3.Distance(A, B);
    float t = (Height - A.Y) / (B.Y - A.Y);

    Vector3 result = Vector3.Lerp(A, B, t);

    return result;
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
