using System.Numerics;
using SharpGLTF.Schema2;

var model = ModelRoot.Load("/Users/joris/Documents/Models/cube.glb");
var mesh = model.DefaultScene.VisualChildren.Single().Mesh;
var primitive = mesh.Primitives.Single();

float[] splitHeights = [float.MinValue, 0f, float.MaxValue];

ModelSplitter.SplitModel(primitive.GetTriangles(), splitHeights, true);

class ModelSplitter
{

static public List<List<(Vector3, Vector3, Vector3)>> SplitModel (IEnumerable<(Vector3, Vector3, Vector3)> model, float[] planes, bool print = false)
    {
        List<List<(Vector3, Vector3, Vector3)>> models = new List<List<(Vector3, Vector3, Vector3)>>();

        for (int i = 1; i < planes.Length; i++ )
{
    var low = planes[i - 1];
    var high = planes[i];
    List<(Vector3, Vector3, Vector3)> splitTriangles = new List<(Vector3, Vector3, Vector3)>();

    foreach (var (a1, b1, c1) in model)
    {
        var a = a1;
        var b = b1;
        var c = c1;
        if (a.Y > b.Y) 
        {
            var z = a;
            a = b;
            b = z;
        }
        if (b.Y > c.Y) 
        {
            var z = b;
            b = c;
            c = z;
        }
        if (a.Y > b.Y) 
        {
            var z = a;
            b = a;
            a = z;
        }
        //Sorted the values, now a is smallest and c is highest

        if (c.Y <= low || a.Y >= high)
        {
            //Outside of current split
        }
        else
        if (a.Y >= low & c.Y <= high)
        {
            //Fully inside current split
            splitTriangles.Add((a, b, c));
        }
        //Cases where the triangles intersect the line
        else
        if (a.Y < low)
        {
            if (b.Y > low)
            {
                //One vertex below bottom
                splitTriangles.Add((helperFunctions.getIntersection(a, c, low), b, c));
                splitTriangles.Add((helperFunctions.getIntersection(a, c, low), helperFunctions.getIntersection(a, b, low), b));
            }
            else
            {
                //Two vertices below bottom
                splitTriangles.Add((helperFunctions.getIntersection(a, c, low), helperFunctions.getIntersection(b, c, low), c));
            }
        }
        else
        if (c.Y > high)
        {
            if(b.Y < high)
            {
                //One vertex Above top
                splitTriangles.Add((helperFunctions.getIntersection(a, c, high), a, b));
                splitTriangles.Add((helperFunctions.getIntersection(a, c, high), helperFunctions.getIntersection(b, c, high), b));
            }
            else
            {
                //Two vertices Above top
                splitTriangles.Add((helperFunctions.getIntersection(a, c, high), helperFunctions.getIntersection(a, b, high), a));
            }
        }
    }
    models.Add(splitTriangles);
}

        if (print)
        {
            helperFunctions.printModels(models);
        }
        return models;

    }
}

class helperFunctions
{
    static public Vector3 getIntersection(Vector3 A, Vector3 B, float Height)
    {
        float totalLength = Vector3.Distance(A, B);
        float t = (Height - A.Y) / (B.Y - A.Y);

        Vector3 result = Vector3.Lerp(A, B, t);

        return result;
    }
    static public void printModels(List<List<(Vector3 A, Vector3 B, Vector3 C)>> themodel)
    {
        var modelArray = themodel.ToArray();
        Console.WriteLine(modelArray.Length);
        int i = 0;
        foreach (var model in themodel)
        {
            int j = 0;
            foreach (var triangle in model)
            {
                Console.WriteLine("model: " + i + " triangle: " + j + " values: " + triangle.ToString());
                j++;
            }
            i++;
        }
    }
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
