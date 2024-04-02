using System.Numerics;
using Common;
using Triangle = Common.Triangle;

namespace ModelSplitter;

public static class ModelSplitter
{
    // If this method doesn't return submodels from bottom to top stuff goes wrong
    public static List<Model> SplitModel(Model model, List<double> planes, bool print = false)
    {
        List<Model> subModels = [];

        var (bottom, top) = model.VerticalBounds();
        planes = [..planes]; // Copy list
        planes.Insert(0, bottom);
        planes.Add(top);
        planes.Sort();

        for (var i = 0; i < planes.Count - 1; i++)
        {
            var low = planes[i];
            var high = planes[i + 1];

            Model subModel = new();

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
                        var intersectAC = new Line3D(a, c).PlaneIntersection(low)!.Value;
                        var intersectAB = new Line3D(a, b).PlaneIntersection(low)!.Value;
                        subModel.Add(new Triangle(intersectAC, b, c, triangle.Normal, triangle.WindingCorrect));
                        subModel.Add(new Triangle(intersectAC, intersectAB, b, triangle.Normal, triangle.WindingCorrect));
                    }
                    else
                    {
                        // Two vertices below bottom
                        var intersectAC = new Line3D(a, c).PlaneIntersection(low)!.Value;
                        var intersectBC = new Line3D(b, c).PlaneIntersection(low)!.Value;
                        subModel.Add(new Triangle(intersectAC, intersectBC, c, triangle.Normal, triangle.WindingCorrect));
                    }
                }
                else if (c.Y > high)
                {
                    if (b.Y < high)
                    {
                        // One vertex above top
                        var intersectAC = new Line3D(a, c).PlaneIntersection(high)!.Value;
                        var intersectBC = new Line3D(b, c).PlaneIntersection(high)!.Value;
                        subModel.Add(new Triangle(intersectAC, a, b, triangle.Normal, triangle.WindingCorrect));
                        subModel.Add(new Triangle(intersectBC, intersectAC, b, triangle.Normal, triangle.WindingCorrect));
                    }
                    else
                    {
                        // Two vertices above top
                        var intersectAC = new Line3D(a, c).PlaneIntersection(high)!.Value;
                        var intersectAB = new Line3D(a, b).PlaneIntersection(high)!.Value;
                        subModel.Add(new Triangle(intersectAB, intersectAC, a, triangle.Normal, triangle.WindingCorrect));
                    }
                }
            }

            var step = (float)planes[i];
            subModel.Triangles = subModel.Triangles.Select(t => t.Translate(new Vector3(0, -step, 0))).ToList();

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
