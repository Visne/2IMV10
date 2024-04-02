using Common;
using JetBrains.Annotations;

namespace PlaneCutter;

public static class PlaneCutter
{
    [MustUseReturnValue]
    public static List<(double, List<Line>)> GetCuttingPlanes(Model model)
    {
        List<(double, List<Line>)> planes = [];

        for (var height = -2.05; height < 2.0; height += 0.01)
        {
            // Horizontal plane at y = height
            List<Line> segments = [];

            foreach (var triangle in model.Triangles)
            {
                if (triangle.PlaneIntersection(height) is { } line)
                {
                    segments.Add(line);
                }
            }

            if (segments.Count == 0)
                continue;

            planes.Add((height, segments));
        }

        return planes;
    }
    
    [MustUseReturnValue]
    public static List<MultiLine> ToPolygons(List<Line> lines)
    {
        List<MultiLine> multilines = [];

        while (lines.Count > 0)
        {
            var multi = new MultiLine(lines.Last());
            lines.RemoveAt(lines.Count - 1);
            multilines.Add(multi);
            
            while (lines.Count > 0)
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];

                    if (multi.TryAdd(line))
                    {
                        lines.RemoveAt(i);
                        break;
                    }

                    if (i == lines.Count - 1)
                    {
                        goto PolygonDone;
                    }
                }
            }
            PolygonDone: ;
        }

        return multilines;
    }
}
