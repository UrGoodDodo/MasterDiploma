using System.Collections.Generic;
using UnityEngine;

public class CapRegion
{
    public ProjectedLoop Outer;
    public List<ProjectedLoop> Holes = new List<ProjectedLoop>();
}

public static class CapTopologyBuilder
{
    public static List<CapRegion> BuildRegions(List<ProjectedLoop> loops)
    {
        List<CapRegion> regions = new List<CapRegion>();

        if (loops == null || loops.Count == 0) return regions;

        int count = loops.Count;

        int[] depth = new int[count];

        for (int i = 0; i < count; i++)
        {
            depth[i] = 0;

            for (int j = 0; j < count; j++)
            {
                if (i == j) continue;

                Vector2 testPoint = FindContainmentTestPoint(loops[i].Points, loops[j].Points);

                if (PointInPolygon(testPoint, loops[j].Points)) depth[i]++;
            }
        }

        for (int i = 0; i < count; i++)
        {
            if (depth[i] % 2 == 0)
            {
                CapRegion region = new CapRegion();
                region.Outer = loops[i];

                for (int j = 0; j < count; j++)
                {
                    if (depth[j] == depth[i] + 1)
                    {
                        Vector2 testPoint = FindContainmentTestPoint(loops[j].Points, loops[i].Points);

                        if (PointInPolygon(testPoint, loops[i].Points)) region.Holes.Add(loops[j]);
                    }
                }

                regions.Add(region);
            }
        }

        return regions;
    }

    public static Vector2 FindContainmentTestPoint(List<Vector2> sourceLoop, List<Vector2> targetPolygon)
    {
        if (sourceLoop == null || sourceLoop.Count == 0) return Vector2.zero;

        for (int i = 0; i < sourceLoop.Count; i++)
        {
            Vector2 p = sourceLoop[i];

            if (!PointOnPolygonBoundary(p, targetPolygon, 0.00001f)) return p;
        }

        return sourceLoop[0];
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

    public static bool PointOnPolygonBoundary(Vector2 point, List<Vector2> polygon, float epsilon)
    {
        if (polygon == null || polygon.Count < 2) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];

            if (PointOnSegment(point, a, b, epsilon)) return true;
        }

        return false;
    }

    public static bool PointOnSegment(Vector2 point, Vector2 a, Vector2 b, float epsilon)
    {
        Vector2 ab = b - a;
        Vector2 ap = point - a;

        float abLengthSqr = ab.sqrMagnitude;

        if (abLengthSqr < epsilon * epsilon) return Vector2.Distance(point, a) <= epsilon;

        float t = Vector2.Dot(ap, ab) / abLengthSqr;

        if (t < -epsilon || t > 1f + epsilon) return false;

        Vector2 closest = a + ab * Mathf.Clamp01(t);

        return Vector2.Distance(point, closest) <= epsilon;
    }
}