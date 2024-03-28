using System.Numerics;
using Common;
using SharpGLTF.Schema2;
using Triangle = Common.Triangle;

var model = ModelRoot.Load("/Users/joris/Documents/Models/cube.glb");
var mesh = model.LogicalMeshes[0];
var primitive = mesh.Primitives[0];

List<double> splitHeights = [0f];

ModelSplitter.SplitModel(primitive.GetTriangles(), splitHeights, true);

public static class ModelSplitter
{
    
    static public List<List<Triangle>> 
        SplitModel(IList<Triangle> model,
        List<double> planes, bool print = false)
    {

        List<List<Triangle>> models = [];

        (float bottom, float top) = helperFunctions.getSize(model);
        planes.Insert(0, bottom);
        planes.Insert(planes.Count, top);

        
        for (int i = 1; i < planes.Count; i++)
        {
            double low = planes[i - 1];
            double high = planes[i];

            double step = -1*low + ((high - low) / 2);

            List<Triangle> splitTriangles = new List<Triangle>();

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
                    splitTriangles.Add(moveTriangle(new Triangle(a, b, c), step));
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
                        splitTriangles.Add(moveTriangle(new Triangle(helperFunctions.getIntersection(a, c, low), b, c), step));
                        splitTriangles.Add(moveTriangle(new Triangle(helperFunctions.getIntersection(a, c, low), helperFunctions.getIntersection(a, b, low), b), step));
                    }
                    else
                    {
                        //Two vertices below bottom
                        splitTriangles.Add(moveTriangle(new Triangle(helperFunctions.getIntersection(a, c, low), helperFunctions.getIntersection(b, c, low), c), step));
                    }
                }
                else
                if (c.Y > high)
                {
                    if (b.Y < high)
                    {
                        //One vertex Above top
                        splitTriangles.Add(moveTriangle(new Triangle(helperFunctions.getIntersection(a, c, high), a, b), step));
                        splitTriangles.Add(moveTriangle(new Triangle(helperFunctions.getIntersection(a, c, high), helperFunctions.getIntersection(b, c, high), b), step));
                    }
                    else
                    {
                        //Two vertices Above top
                        splitTriangles.Add(moveTriangle(new Triangle(helperFunctions.getIntersection(a, c, high), helperFunctions.getIntersection(a, b, high), a), step));
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

    static Triangle moveTriangle(Triangle triangle, double  stepinput)
    {
        float step = (float)(stepinput);
        Vector3 A = new Vector3(triangle.A.X, triangle.A.Y - step, triangle.A.Z);
        Vector3 B = new Vector3(triangle.B.X, triangle.B.Y - step, triangle.B.Z);
        Vector3 C = new Vector3(triangle.C.X, triangle.C.Y - step, triangle.C.Z);
        return new Triangle(A, B, C);
    }

}

class helperFunctions
{
    static public Vector3 getIntersection(Vector3 A, Vector3 B, double HeightInput)
    {
        float Height = (float)(HeightInput);
        float totalLength = Vector3.Distance(A, B);
        float t = (Height - A.Y) / (B.Y - A.Y);

        Vector3 result = Vector3.Lerp(A, B, t);

        return result;
    }

    static public (float bottom, float top) getSize(IList<Triangle> model)
    {
        float bottom = float.MaxValue;
        float top = float.MinValue;

        foreach (var triangle in model)
        {
            var a = triangle.A; var b = triangle.B; var c = triangle.C;

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

    static public void printModels(List<List<Triangle>> themodel)
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
    public static IList<Triangle> GetTriangles(this MeshPrimitive primitive)
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
