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

public class Model(List<Triangle> triangles)
{
    public List<Triangle> Triangles { get; set; } = triangles;

    public (float bottom, float top) VerticalBounds()
    {
        var vertices = Triangles.SelectMany(t => new[] { t.A.Y, t.B.Y, t.C.Y }).ToList();

        return (vertices.Min(), vertices.Max());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Triangle triangle) => Triangles.Add(triangle);
}

public readonly record struct Triangle(Vector3 A, Vector3 B, Vector3 C)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Line? PlaneIntersection(double height)
    {
        var points = Disintegrate()
            .Select(l => l.PlaneIntersection(height))
            .OfType<Vector3>()
            .Select(v => new Vector2(v.X, v.Z))
            .ToList();

        if (points.Count != 2)
            return null;

        return new Line(points[0], points[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Triangle Translate(Vector3 translation) => new(A + translation, B + translation, C + translation);

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

    public Triangle SortHeight()
    {
        var (a, b, c) = (A, B, C);

        if (a.Y > c.Y)
            (a, c) = (c, a);

        if (a.Y > b.Y)
            (a, b) = (b, a);

        if (b.Y > c.Y)
            (b, c) = (c, b);

        return new Triangle(a, b, c);
    }
}

public readonly record struct Line3D(Vector3 A, Vector3 B)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3? PlaneIntersection(double height)
    {
        if ((A.Y >= height && B.Y >= height) || (A.Y <= height && B.Y <= height))
            return null;

        var lerp = (height - A.Y) / (B.Y - A.Y);

        return Vector3.Lerp(A, B, (float)lerp);
    }
}

public readonly record struct Line(Vector2 A, Vector2 B)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Line Translate(Vector2 translation)
    {
        return new Line(A + translation, B + translation);
    }
}
