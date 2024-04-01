using System.Numerics;
using Common;
using Triangle = Common.Triangle;

//var model = ModelRoot.Load("/Users/joris/Documents/Models/cube.glb");
//var mesh = model.LogicalMeshes[0];
//var primitive = mesh.Primitives[0];

//List<double> splitHeights = [0f];

//ModelSplitter.SplitModel(primitive.GetTriangles(), splitHeights, true);

namespace ModelSplitter;

public static class ModelSplitter
{
    public static List<Model> SplitModel(Model model, List<double> planes, bool print = false)
    {
        List<Model> models = [];

        var (bottom, top) = model.VerticalBounds();
        planes.Insert(0, bottom);
        planes.Add(top);

        for (var i = 1; i < planes.Count; i++)
        {
            var low = (float)planes[i - 1];
            var high = (float)planes[i];

            var step = i * .5f;
            //double step = -1*low + ((high - low) / 2);

            Model subModel = new([]);

            foreach (var triangle in model.Triangles)
            {
                var (a, b, c) = triangle;
                if (a.Y > b.Y)
                {
                    (a, b) = (b, a);
                }

                if (b.Y > c.Y)
                {
                    (b, c) = (c, b);
                }

                if (a.Y > b.Y)
                {
                    // TODO: Don't think this is correct?
                    var z = a;
                    b = a;
                    a = z;
                }
                // Sorted the values, now a is smallest and c is highest

                if (a.Y >= low & c.Y <= high)
                {
                    // Fully inside current split
                    subModel.Triangles.Add(MoveTriangle(new Triangle(a, b, c), step));
                }
                else if (c.Y <= low || a.Y >= high)
                {
                    // Outside of current split
                }
                // Cases where the triangles intersect the line
                else if (a.Y < low)
                {
                    if (b.Y > low)
                    {
                        // One vertex below bottom
                        subModel.Triangles.Add(MoveTriangle(new Triangle(HelperFunctions.GetIntersection(a, c, low), b, c), step));
                        subModel.Triangles.Add(MoveTriangle(new Triangle(HelperFunctions.GetIntersection(a, c, low), HelperFunctions.GetIntersection(a, b, low), b), step));
                    }
                    else
                    {
                        // Two vertices below bottom
                        subModel.Triangles.Add(MoveTriangle(new Triangle(HelperFunctions.GetIntersection(a, c, low), HelperFunctions.GetIntersection(b, c, low), c), step));
                    }
                }
                else if (c.Y > high)
                {
                    if (b.Y < high)
                    {
                        // One vertex Above top
                        subModel.Triangles.Add(MoveTriangle(new Triangle(HelperFunctions.GetIntersection(a, c, high), a, b), step));
                        subModel.Triangles.Add(MoveTriangle(new Triangle(HelperFunctions.GetIntersection(a, c, high), HelperFunctions.GetIntersection(b, c, high), b), step));
                    }
                    else
                    {
                        // Two vertices above top
                        subModel.Triangles.Add(MoveTriangle(new Triangle(HelperFunctions.GetIntersection(a, c, high), HelperFunctions.GetIntersection(a, b, high), a), step));
                    }
                }
            }

            models.Add(subModel);
        }

        if (print)
        {
            HelperFunctions.PrintModels(models);
        }

        return models;
    }

    private static Triangle MoveTriangle(Triangle triangle, float step)
    {
        var a = triangle.A with { Y = triangle.A.Y - step };
        var b = triangle.B with { Y = triangle.B.Y - step };
        var c = triangle.C with { Y = triangle.C.Y - step };
        return new Triangle(a, b, c);
    }
}

internal static class HelperFunctions
{
    public static Vector3 GetIntersection(Vector3 a, Vector3 b, float height)
    {
        var t = (height - a.Y) / (b.Y - a.Y);

        return Vector3.Lerp(a, b, t);
    }

    public static void PrintModels(List<Model> models)
    {
        Console.WriteLine(models.Count);
        for (var i = 0; i < models.Count; i++)
        {
            var triangles = models[i].Triangles;
            for (var j = 0; j < triangles.Count; j++)
            {
                var triangle = triangles[j];
                Console.WriteLine($"model: {i} triangle: {j} values: {triangle}");
            }
        }
    }
}
