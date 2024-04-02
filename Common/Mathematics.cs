using System.Numerics;
using System.Runtime.CompilerServices;
using SharpGLTF.Schema2;

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

public class Model
{
    public Model()
    {
        Triangles = [];
    }

    public Model(MeshPrimitive primitive)
    {
        Triangles = primitive.GetTriangles();
    }
    
    public List<Triangle> Triangles;

    public (float bottom, float top) VerticalBounds()
    {
        var vertices = Triangles.SelectMany(t => new[] { t.A.Y, t.B.Y, t.C.Y }).ToList();

        return (vertices.Min(), vertices.Max());
    }
    
    public (List<(Vector3 Position, Vector3 Normal)> Vertices, List<uint> Indices) GetRenderData()
    {
        List<(Vector3, Vector3)> vertices = [];
        List<uint> indices = [];

        for (uint i = 0; i < Triangles.Count; i++)
        {
            var triangle = Triangles[(int)i];

            if (!triangle.WindingCorrect)
            {
                triangle = new Triangle(triangle.A, triangle.C, triangle.B, -triangle.Normal);
            }

            // Add the vertices of the triangle to the vertices list
            vertices.Add((triangle.A, triangle.Normal));
            vertices.Add((triangle.B, triangle.Normal));
            vertices.Add((triangle.C, triangle.Normal));

            // Add the indices of the triangle to the indices list
            indices.Add(i * 3); // Index of triangle.A
            indices.Add(i * 3 + 1); // Index of triangle.B
            indices.Add(i * 3 + 2); // Index of triangle.C
        }

        return (vertices, indices);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Triangle triangle) => Triangles.Add(triangle);
}

public readonly record struct Triangle(Vector3 A, Vector3 B, Vector3 C, Vector3 Normal, bool WindingCorrect = true)
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
    public Triangle Translate(Vector3 translation) => new(A + translation, B + translation, C + translation, Normal, WindingCorrect);

    public Triangle Rotate(int rotX, int rotY, int rotZ)
    {
        var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll((float)(Math.PI / 180) * rotY, (float)(Math.PI / 180) * rotX, (float)(Math.PI / 180) * rotZ);
        return new Triangle(Vector3.Transform(A, rotationMatrix), Vector3.Transform(B, rotationMatrix), Vector3.Transform(C, rotationMatrix), Vector3.TransformNormal(Normal, rotationMatrix), WindingCorrect);
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

    public Triangle SortHeight()
    {
        var (a, b, c) = (A, B, C);

        while (a.Y > b.Y || a.Y > c.Y)
        {
            // Doesn't change winding order
            (a, b, c) = (b, c, a);
        }

        // A is lowest now
        if (b.Y > c.Y)
        {
            // Changes winding order
            return new Triangle(a, c, b, -Normal, false);
        }

        return new Triangle(a, b, c, Normal);
    }

    public void Deconstruct(out Vector3 a, out Vector3 b, out Vector3 c) => (a, b, c) = (A, B, C);
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
