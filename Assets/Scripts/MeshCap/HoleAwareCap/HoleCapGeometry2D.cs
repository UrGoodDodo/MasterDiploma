using UnityEngine;

public struct HoleCapPoint2
{
    public float x;
    public float y;

    public HoleCapPoint2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

public static class HoleCapGeometry2D
{
    public static HoleCapPoint2 Project(Vector3 p, Vector3 axisX, Vector3 axisY)
    {
        return new HoleCapPoint2(Vector3.Dot(p, axisX), Vector3.Dot(p, axisY));
    }

    public static float Orient(HoleCapPoint2 a, HoleCapPoint2 b, HoleCapPoint2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    public static bool OnSegment(HoleCapPoint2 a, HoleCapPoint2 p, HoleCapPoint2 b)
    {
        const float eps = 1e-6f;
        return p.x >= Mathf.Min(a.x, b.x) - eps && p.x <= Mathf.Max(a.x, b.x) + eps && p.y >= Mathf.Min(a.y, b.y) - eps && p.y <= Mathf.Max(a.y, b.y) + eps;
    }

    public static bool SegmentsIntersect(HoleCapPoint2 a, HoleCapPoint2 b, HoleCapPoint2 c, HoleCapPoint2 d)
    {
        float o1 = Orient(a, b, c);
        float o2 = Orient(a, b, d);
        float o3 = Orient(c, d, a);
        float o4 = Orient(c, d, b);

        const float eps = 1e-6f;

        if (Mathf.Abs(o1) < eps && OnSegment(a, c, b)) return true;
        if (Mathf.Abs(o2) < eps && OnSegment(a, d, b)) return true;
        if (Mathf.Abs(o3) < eps && OnSegment(c, a, d)) return true;
        if (Mathf.Abs(o4) < eps && OnSegment(c, b, d)) return true;

        return (o1 > 0f) != (o2 > 0f) && (o3 > 0f) != (o4 > 0f);
    }

    public static float DistanceSqr(HoleCapPoint2 a, HoleCapPoint2 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    public static void BuildPlaneBasis(Vector3 normal, out Vector3 axisX, out Vector3 axisY)
    {
        Vector3 helper = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up;
        axisX = Vector3.Cross(helper, normal).normalized;
        axisY = Vector3.Cross(normal, axisX).normalized;
    }
}