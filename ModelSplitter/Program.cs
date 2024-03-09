using System.Numerics;
using SharpGLTF.Schema2;

var model = ModelRoot.Load("/Users/joris/Documents/Models/cube.glb");
var mesh = model.DefaultScene.VisualChildren.Single().Mesh;
var primitive = mesh.Primitives.Single();

Console.WriteLine("Begin of ModelSplitter project");

float[] splitHeights = [float.MinValue, -1f, 0f, 1f, float.MaxValue];

List<List<(Vector3, Vector3, Vector3, String)>> models = new List<List<(Vector3, Vector3, Vector3, String)>>();

for (int i = 1; i < splitHeights.Length; i++ )
{
    var low = splitHeights[i - 1];
    var high = splitHeights[i];
    List<(Vector3, Vector3, Vector3, String)> splitTriangles = new List<(Vector3, Vector3, Vector3, String)>();

    Console.WriteLine("now at height" + i);



    foreach (var (a1, b1, c1) in primitive.GetTriangles())
    {
        Console.WriteLine("in the tiangles");
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
        Console.WriteLine("aftersort");

        if (c.Y <= low || a.Y >= high)
        {
            //Outside of current split
            Console.WriteLine("outside split");
        }
        else
        if (a.Y >= low & c.Y <= high)
        {
            //Fully inside current split
            Console.WriteLine("within split");
            splitTriangles.Add((a, b, c, "within"));
        }
        //Cases where the triangles intersect the line
        else
        if (a.Y < low)
        {
            Console.WriteLine("intersect bottom");
            if (b.Y > low)
            {
                //One vertex below bottom
                Console.WriteLine("pointing down");
                splitTriangles.Add((getIntersection(a, c, low), b, c, "bottomDown1"));
                splitTriangles.Add((getIntersection(a, c, low), getIntersection(a, b, low), b, "BottomDown2"));
            }
            else
            {
                //Two vertices below bottom
                Console.WriteLine("pointing up");
                splitTriangles.Add((getIntersection(a, c, low), getIntersection(b, c, low), c, "BottomUp"));
            }
        }
        else
        if (c.Y > high)
        {
            Console.WriteLine("Intersect top");
            if(b.Y < high)
            {
                //One vertex Above top
                Console.WriteLine("pointing up");
                splitTriangles.Add((getIntersection(a, c, high), a, b, "topUp1"));
                splitTriangles.Add((getIntersection(a, c, high), getIntersection(b, c, high), b, "TopUp2"));
            }
            else
            {
                //Two vertices Above top
                Console.WriteLine("pointing down");
                Console.WriteLine("values , "+ getIntersection(a, c, high).ToString()+ " , " + a + " , " + b + " , " + c + " , " + a1 + " , " + b1 +" , "+c1); 
                splitTriangles.Add((getIntersection(a, c, high), getIntersection(a, b, high), a, "TopDown"));
            }
        }
        Console.WriteLine("what edgecase did i forget?");
    }
    Console.WriteLine("afterTriangles");
    models.Add(splitTriangles);
}
Console.WriteLine("total finish");

printModels(models);


static void printModels(List<List<(Vector3 A, Vector3 B, Vector3 C, String)>> themodel)
{
    var modelArray = themodel.ToArray();
    Console.WriteLine(modelArray.Length);
    int i = 0;
    foreach (var model in themodel)
    {
        i++;
        int j = 0;
        foreach (var triangle in model)
        {
            j++;
            Console.WriteLine("model: " + i + " triangle: " + j+ " values: " +triangle.ToString());
        }
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
