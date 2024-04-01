using System.Numerics;
using System.Runtime.CompilerServices;

namespace Common;

public static class Mathematics
{
    private const double Tolerance = 0.000001; // TODO: Make configurable

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CloseTo(this Vector2 me, Vector2 other)
    {
        var distance = Math.Sqrt(Math.Pow(me.X - other.X, 2) + Math.Pow(me.Y - other.Y, 2));
        return distance <= Tolerance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3? PlaneLineIntersection(ref Line3D line, double height)
    {
        if ((line.A.Y >= height && line.B.Y >= height) || (line.A.Y <= height && line.B.Y <= height))
            return null;

        var lerp = (height - line.A.Y) / (line.B.Y - line.A.Y);

        return Vector3.Lerp(line.A, line.B, (float)lerp);
    }
}

public record MultiLine
{
    public readonly List<Line> Parts;

    public MultiLine(Line start)
    {
        Parts = [ start ];
    }

    public bool TryAdd(Line line)
    {
        var start = Parts.First().A;
        var end = Parts.Last().B;

        if (end.CloseTo(line.A))
        {
            Parts.Add(line);
        }
        else if (end.CloseTo(line.B))
        {
            Parts.Add(new Line(line.B, line.A));
        }
        else if (start.CloseTo(line.B))
        {
            Parts.Insert(0, line);
        }
        else if (start.CloseTo(line.A))
        {
            Parts.Insert(0, new Line(line.B, line.A));
        }
        else
        {
            return false;
        }

        return true;
    }
}

public readonly record struct Model(List<Triangle> Triangles)
{
    public (float bottom, float top) VerticalBounds()
    {
        var vertices = Triangles.SelectMany(t => new[] { t.A.Y, t.B.Y, t.C.Y }).ToList();

        return (vertices.Min(), vertices.Max());
    }
}

public readonly record struct Triangle(Vector3 A, Vector3 B, Vector3 C)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Line? PlaneIntersection(double height)
    {
        var points = Disintegrate()
            .Select(l => Mathematics.PlaneLineIntersection(ref l, height))
            .OfType<Vector3>()
            .Select(v => new Vector2(v.X, v.Z))
            .ToList();

        if (points.Count != 2)
            return null;

        return new Line(points[0], points[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Line3D> Disintegrate()
    {
        return
        [
            new(A, B),
            new(B, C),
            new(C, A),
        ];
    }
}

public record struct Line3D(Vector3 A, Vector3 B);

public readonly record struct Line(Vector2 A, Vector2 B)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Line Translate(Vector2 translation)
    {
        return new Line(A + translation, B + translation);
    }
}
