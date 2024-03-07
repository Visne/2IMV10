using GeometRi;
using JetBrains.Annotations;

namespace PlaneCutter;

public static class PlaneCutter
{
    [MustUseReturnValue]
    public static Dictionary<double, List<Segment3d>> GetCuttingPlanes(IList<Triangle> triangles)
    {
        Dictionary<double, List<Segment3d>> planes = new();

        for (var height = -2.05; height < 2.0; height += 0.01)
        {
            // Horizontal plane at y = height
            Plane3d plane = new(new Point3d(0, height, 0), new Vector3d(0, 1, 0));
            List<Segment3d> segments = [];

            foreach (var triangle in triangles)
            {
                switch (triangle.IntersectionWith(plane))
                {
                    case Segment3d line:
                        segments.Add(line);
                        break;
                    case Triangle t:
                        segments.Add(new Segment3d(t.A, t.B));
                        segments.Add(new Segment3d(t.B, t.C));
                        segments.Add(new Segment3d(t.C, t.A));
                        break;
                    case null or Point3d:
                        // Ignore
                        break;
                }
            }

            if (segments.Count == 0)
                continue;

            planes.Add(height, segments);
        }

        return planes;
    }
    
    [MustUseReturnValue]
    public static List<List<Point3d>> ToPolygons(List<Segment3d> segments)
    {
        List<List<Point3d>> polygons = [];

        foreach (var segment in segments)
        {
            foreach (var polygon in polygons.Where(p => p.Count > 0))
            {
                if (polygon.First() == segment.P1)
                {
                    polygon.Insert(0, segment.P2);
                }
                else if (polygon.Last() == segment.P1)
                {
                    polygon.Add(segment.P2);
                }
                else if (polygon.First() == segment.P2)
                {
                    polygon.Insert(0, segment.P1);
                }
                else if (polygon.Last() == segment.P2)
                {
                    polygon.Add(segment.P1);
                }
                else
                {
                    continue;
                }
                
                goto NextSegment;
            }

            List<Point3d> newPolygon = [ segment.P1, segment.P2 ];
            polygons.Add(newPolygon);

            NextSegment: ;
        }

        polygons = polygons.Where(p => p.Count > 1).ToList();

        polygons = MergePolygons(polygons);

        return polygons;
    }

    [MustUseReturnValue]
    private static List<List<Point3d>> MergePolygons(List<List<Point3d>> polygons)
    {
        // Whether we made a merge in the previous iteration
        var didMerge = true;

        // If we didn't merge in the previous iteration, all polygons are attached and we can return
        while (didMerge)
        {
            didMerge = false;

            foreach (var polygon in polygons)
            {
                var start = polygon.First();
                var end = polygon.Last();

                if (polygons.Find(p => p != polygon && start == p.Last()) is { } before)
                {
                    before.AddRange(polygon);
                    polygons.Remove(polygon);
                    didMerge = true;
                }
                else if (polygons.Find(p => p != polygon && end == p.First()) is { } after)
                {
                    polygon.AddRange(after);
                    polygons.Remove(after);
                    didMerge = true;
                }
                else if (polygons.Find(p => p != polygon && start == p.First()) is { } beforeFlipped)
                {
                    beforeFlipped.Reverse();
                
                    polygon.InsertRange(0, beforeFlipped);
                    polygons.Remove(beforeFlipped);
                    didMerge = true;
                }
                else if (polygons.Find(p => p != polygon && end == p.Last()) is { } afterFlipped)
                {
                    afterFlipped.Reverse();

                    polygon.AddRange(afterFlipped);
                    polygons.Remove(afterFlipped);
                    didMerge = true;
                }

                if (didMerge)
                {
                    // We removed items from the list which breaks the iteration, so restart it from scratch
                    break;
                }
            }
        }

        return polygons;
    }
}
