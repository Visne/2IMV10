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
        List<Model> subModels = [];

        var (bottom, top) = model.VerticalBounds();
        planes = [..planes]; // Copy list
        planes.Insert(0, bottom);
        planes.Add(top);
        planes.Sort();

        for (var i = 1; i < planes.Count; i++)
        {
            var low = planes[i - 1];
            var high = planes[i];

            var step = (float)i;
            //var step = -1 * low + (high - low) / 2;

            Model subModel = new([]);

            foreach (var triangle in model.Triangles.Select(t => t.SortHeight()))
            {
                var (a, b, c) = triangle;

                if (a.Y >= low & c.Y <= high)
                {
                    // Fully inside current split
                    subModel.Add(triangle);
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
                        subModel.Add(new Triangle(new Line3D(a, c).PlaneIntersection(low)!.Value, b, c));
                        subModel.Add(new Triangle(new Line3D(a, c).PlaneIntersection(low)!.Value, new Line3D(a, b).PlaneIntersection(low)!.Value, b));
                    }
                    else
                    {
                        // Two vertices below bottom
                        subModel.Add(new Triangle(new Line3D(a, c).PlaneIntersection(low)!.Value, new Line3D(b, c).PlaneIntersection(low)!.Value, c));
                    }
                }
                else if (c.Y > high)
                {
                    if (b.Y < high)
                    {
                        // One vertex above top
                        subModel.Add(new Triangle(new Line3D(a, c).PlaneIntersection(high)!.Value, a, b));
                        subModel.Add(new Triangle(new Line3D(a, c).PlaneIntersection(high)!.Value, new Line3D(b, c).PlaneIntersection(high)!.Value, b));
                    }
                    else
                    {
                        // Two vertices above top
                        subModel.Add(new Triangle(new Line3D(a, c).PlaneIntersection(high)!.Value, new Line3D(a, b).PlaneIntersection(high)!.Value, a));
                    }
                }
            }

            // TODO: Translate such that submodel has y=0 at the bottom or middle
            subModel.Triangles = subModel.Triangles.Select(t => t.Translate(new Vector3(0, step, 0))).ToList();

            if (subModel.Triangles.Count > 0)
            {
                subModels.Add(subModel);
            }
        }

        if (print)
        {
            HelperFunctions.PrintModels(subModels);
        }

        return subModels;
    }
}

internal static class HelperFunctions
{
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
