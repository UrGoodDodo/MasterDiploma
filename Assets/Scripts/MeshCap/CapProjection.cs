using System.Collections.Generic;
using UnityEngine;

public class ProjectedLoop
{
    public int SourceLoopIndex;
    public List<int> Indices = new List<int>();
    public List<Vector2> Points = new List<Vector2>();
    public float SignedArea;
    public float AbsArea => Mathf.Abs(SignedArea);
}

public class CapProjection
{
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector3 Bitangent;

    public CapProjection(Vector3 normal)
    {
        Normal = normal.normalized;
        Tangent = Vector3.Cross(Normal, Vector3.up);

        if (Tangent.sqrMagnitude < 0.0001f) Tangent = Vector3.Cross(Normal, Vector3.right);

        Tangent.Normalize();
        Bitangent = Vector3.Cross(Normal, Tangent).normalized;
    }

    public Vector2 Project(Vector3 point)
    {
        return new Vector2(Vector3.Dot(point, Tangent), Vector3.Dot(point, Bitangent));
    }

    public static ProjectedLoop ProjectLoopTo2D(List<int> loop, List<Vector3> vertices, CapProjection projection)
    {
        ProjectedLoop projected = new ProjectedLoop();

        if (loop == null || vertices == null || projection == null) return projected;

        for (int i = 0; i < loop.Count; i++)
        {
            int index = loop[i];

            if (index < 0 || index >= vertices.Count) continue;

            projected.Indices.Add(index);
            projected.Points.Add(projection.Project(vertices[index]));
        }

        projected.SignedArea = EstimateSignedArea2D(projected.Points);

        return projected;
    }

    public static List<ProjectedLoop> ProjectLoopsTo2D(List<List<int>> loops, List<Vector3> vertices, Vector3 normal)
    {
        List<ProjectedLoop> projectedLoops = new List<ProjectedLoop>();

        if (loops == null || vertices == null) return projectedLoops;

        CapProjection projection = new CapProjection(normal);

        for (int i = 0; i < loops.Count; i++)
        {
            ProjectedLoop projected = ProjectLoopTo2D(loops[i], vertices, projection);
            projected.SourceLoopIndex = i;

            if (projected.Points.Count >= 3) projectedLoops.Add(projected);
        }

        return projectedLoops;
    }

    private static float EstimateSignedArea2D(List<Vector2> points)
    {
        if (points == null || points.Count < 3) return 0f;

        float area = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Count];

            area += a.x * b.y - b.x * a.y;
        }

        return area * 0.5f;
    }

    public static bool PointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        if (polygon == null || polygon.Count < 3) return false;

        bool inside = false;

        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];

            bool intersects = ((a.y > point.y) != (b.y > point.y)) && (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x);

            if (intersects) inside = !inside;
        }

        return inside;
    }
}