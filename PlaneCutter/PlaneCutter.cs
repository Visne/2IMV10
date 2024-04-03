using Common;
using JetBrains.Annotations;
using System.Drawing;
using System.Collections.Generic;
using System.Numerics;

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


    public static List<Vector2> GetIntersections (List<Line> lines)
    {
        List<Vector2> intersections = new List<Vector2>();


        /*
        // Flatten the lines into events (start and end points)
        List<Vector2> events = lines.SelectMany(line => new[] { line.A, line.B }).Distinct().ToList();

        // Sort events by x-coordinate
        events.Sort((a, b) => a.X.CompareTo(b.X));

        // Active lines
        var activeLines = new List<Line>();

        foreach (var evt in events)
        {
            // Add lines that start at the event
            activeLines.AddRange(lines.Where(line => line.A == evt));

            // Remove lines that end at the event
            activeLines.RemoveAll(line => line.B == evt);

            // Check for intersections between active lines
            for (int i = 0; i < activeLines.Count - 1; i++)
            {
                int neighbours = 0;
                for (int j = i + 1; j < activeLines.Count; j++)
                {
                    if (activeLines[i].Intersects(activeLines[j], out Vector2 intersectionPoint))
                    {
                        bool currentNeighbour = false;
                        if (activeLines[i].A.CloseTo(activeLines[j].A) 
                            || activeLines[i].A.CloseTo(activeLines[j].B)
                            || activeLines[i].B.CloseTo(activeLines[j].A)
                            || activeLines[i].B.CloseTo(activeLines[j].B)
                            )
                        {
                            //two folowing lines
                            neighbours++;
                            currentNeighbour = true;
                        }
                        if (!currentNeighbour || (currentNeighbour && neighbours > 2))
                        {
                            //such that the current intersection is not a neighboor. Or it is a neighbour > 2 thus splitting point
                            intersections.Add(intersectionPoint);
                        }
                        
                    }
                }
            }
        }
        */

        
        for (int i = 0; i < lines.Count -1; i++)
        {
            for (int j = i+1; j < lines.Count; j++)
            {
                int neighbours = 0;                   
                if (lines[i].Intersects(lines[j], out Vector2 intersectionPoint))
                {
                    bool currentNeighbour = false;
                    if (lines[i].A.CloseTo(lines[j].A)
                        || lines[i].A.CloseTo(lines[j].B)
                        || lines[i].B.CloseTo(lines[j].A)
                        || lines[i].B.CloseTo(lines[j].B)
                        )
                    {
                    //two folowing lines
                    neighbours++;
                        currentNeighbour = true;
                    }
                    if (!currentNeighbour || (currentNeighbour && neighbours > 2))
                    {
                        //such that the current intersection is not a neighboor. Or it is a neighbour > 2 thus splitting point
                        intersections.Add(intersectionPoint);
                    }

                }

            }
        }
        

        return intersections;
    }
}
