using System.Numerics;
using SharpGLTF.Schema2;

var model = ModelRoot.Load("/Users/joris/Documents/Models/cube.glb");
var mesh = model.DefaultScene.VisualChildren.Single().Mesh;
var primitive = mesh.Primitives.Single();

float[] splitHeights = [0f];

ModelSplitter.SplitModel(primitive.GetTriangles(), splitHeights, true);

class ModelSplitter
{

    static public List<List<(Vector3, Vector3, Vector3)>> SplitModel(IEnumerable<(Vector3, Vector3, Vector3)> model,
        IEnumerable<float> planes, bool print = false)
    {
        List<List<(Vector3, Vector3, Vector3)>> models = new List<List<(Vector3, Vector3, Vector3)>>();
        (float bottom, float top) = helperFunctions.getSize(model);
        planes = planes.Prepend(bottom).Append(top);
        float[] splits = planes.ToArray();

        for (int i = 1; i < splits.Length; i++)
        {
            var low = splits[i - 1];
            var high = splits[i];

            var step = -1*low + ((high - low) / 2);

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

                if (a.Y >= low & c.Y <= high)
                {
                    //Fully inside current split
                    splitTriangles.Add(moveTriangle((a, b, c), step));
                }
                else
                if (c.Y <= low || a.Y >= high)
                {
                    //Outside of current split
                }
                //Cases where the triangles intersect the line
                else
                if (a.Y < low)
                {
                    if (b.Y > low)
                    {
                        //One vertex below bottom
                        splitTriangles.Add(moveTriangle((helperFunctions.getIntersection(a, c, low), b, c), step));
                        splitTriangles.Add(moveTriangle((helperFunctions.getIntersection(a, c, low), helperFunctions.getIntersection(a, b, low), b), step));
                    }
                    else
                    {
                        //Two vertices below bottom
                        splitTriangles.Add(moveTriangle((helperFunctions.getIntersection(a, c, low), helperFunctions.getIntersection(b, c, low), c), step));
                    }
                }
                else
                if (c.Y > high)
                {
                    if (b.Y < high)
                    {
                        //One vertex Above top
                        splitTriangles.Add(moveTriangle((helperFunctions.getIntersection(a, c, high), a, b), step));
                        splitTriangles.Add(moveTriangle((helperFunctions.getIntersection(a, c, high), helperFunctions.getIntersection(b, c, high), b), step));
                    }
                    else
                    {
                        //Two vertices Above top
                        splitTriangles.Add(moveTriangle((helperFunctions.getIntersection(a, c, high), helperFunctions.getIntersection(a, b, high), a), step));
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

    static (Vector3, Vector3, Vector3) moveTriangle((Vector3 a, Vector3 b, Vector3 c) triangle, float  step)
    {
        triangle.a.Y -= step;
        triangle.b.Y -= step;
        triangle.c.Y -= step;
        return(triangle.a, triangle.b, triangle.c);
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

    static public (float bottom, float top) getSize(IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> model)
    {
        float bottom = float.MaxValue;
        float top = float.MinValue;

        foreach (var (a, b, c) in model)
        {
            if (a.Y < bottom)
            {
                bottom = a.Y;
            }
            if (b.Y < bottom)
            {
                bottom = b.Y;
            }
            if (c.Y < bottom)
            {
                bottom = c.Y;
            }

            if (a.Y > top)
            {
                top = a.Y;
            }
            if (b.Y > top)
            {
                top = b.Y;
            }
            if (c.Y > top)
            {
                top = c.Y;
            }
        }
        return (bottom, top);
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
