using System.Collections.Generic;
using UnityEngine;

public static class AngularRingHoleCap
{
    private class RingPoint
    {
        public int OriginalLoopIndex;
        public int CapIndex;
        public float Angle;
    }

    public static bool Triangulate(List<int> outerLoop, List<int> innerLoop, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal, bool enableDiagnostics)
    {
        if (outerLoop == null || innerLoop == null) return false;
        if (outerLoop.Count < 3 || innerLoop.Count < 3) return false;

        Vector3 n = normal.normalized;
        HoleCapGeometry2D.BuildPlaneBasis(n, out Vector3 axisX, out Vector3 axisY);

        Vector3 center = Vector3.zero;
        int total = outerLoop.Count + innerLoop.Count;

        for (int i = 0; i < outerLoop.Count; i++) center += mainVertices[outerLoop[i]];
        for (int i = 0; i < innerLoop.Count; i++) center += mainVertices[innerLoop[i]];

        center /= total;

        int outerOffset = capVertices.Count;
        for (int i = 0; i < outerLoop.Count; i++) capVertices.Add(mainVertices[outerLoop[i]]);

        int innerOffset = capVertices.Count;
        for (int i = 0; i < innerLoop.Count; i++) capVertices.Add(mainVertices[innerLoop[i]]);

        List<RingPoint> outer = BuildSortedRingPoints(outerLoop, mainVertices, center, axisX, axisY, outerOffset);
        List<RingPoint> inner = BuildSortedRingPoints(innerLoop, mainVertices, center, axisX, axisY, innerOffset);

        int oi = 0;
        int ii = 0;

        int guard = 0;
        int maxGuard = outer.Count + inner.Count + 8;

        while (guard++ < maxGuard)
        {
            int oiNext = (oi + 1) % outer.Count;
            int iiNext = (ii + 1) % inner.Count;

            float outerNextAngle = NormalizeAngleForward(outer[oiNext].Angle, outer[oi].Angle);
            float innerNextAngle = NormalizeAngleForward(inner[iiNext].Angle, outer[oi].Angle);

            bool advanceOuter = outerNextAngle <= innerNextAngle;

            if (advanceOuter)
            {
                AddOrientedCapTriangle(capVertices, outTriangles, outTriangleTypes, outer[oi].CapIndex, outer[oiNext].CapIndex, inner[ii].CapIndex, n);
                oi = oiNext;
            }
            else
            {
                AddOrientedCapTriangle(capVertices, outTriangles, outTriangleTypes, outer[oi].CapIndex, inner[iiNext].CapIndex, inner[ii].CapIndex, n);
                ii = iiNext;
            }

            if (oi == 0 && ii == 0) break;
        }

        if (enableDiagnostics) ValidateRingCapAttachment(outer, inner, outTriangles, outTriangleTypes);

        return outTriangles.Count > 0;
    }

    private static List<RingPoint> BuildSortedRingPoints(List<int> loop, List<Vector3> mainVertices, Vector3 center, Vector3 axisX, Vector3 axisY, int capOffset)
    {
        List<RingPoint> result = new List<RingPoint>();

        for (int i = 0; i < loop.Count; i++)
        {
            Vector3 p = mainVertices[loop[i]] - center;

            float x = Vector3.Dot(p, axisX);
            float y = Vector3.Dot(p, axisY);
            float angle = Mathf.Atan2(y, x);

            if (angle < 0f) angle += Mathf.PI * 2f;

            result.Add(new RingPoint { OriginalLoopIndex = i, CapIndex = capOffset + i, Angle = angle });
        }

        result.Sort((a, b) => a.Angle.CompareTo(b.Angle));

        return result;
    }

    private static float NormalizeAngleForward(float angle, float reference)
    {
        while (angle < reference) angle += Mathf.PI * 2f;
        return angle;
    }

    private static void AddOrientedCapTriangle(List<Vector3> vertices, List<int> triangles, List<SurfaceType> triangleTypes, int a, int b, int c, Vector3 expectedNormal)
    {
        Vector3 p0 = vertices[a];
        Vector3 p1 = vertices[b];
        Vector3 p2 = vertices[c];

        Vector3 actualNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;

        if (Vector3.Dot(actualNormal, expectedNormal) < 0f)
        {
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(b);
        }
        else
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        triangleTypes.Add(SurfaceType.Cap);
    }

    private static void ValidateRingCapAttachment(List<RingPoint> outer, List<RingPoint> inner, List<int> outTriangles, List<SurfaceType> outTriangleTypes)
    {
        HashSet<int> used = new HashSet<int>();

        for (int i = 0; i < outTriangles.Count; i++) used.Add(outTriangles[i]);

        int missingOuter = 0;
        int missingInner = 0;

        for (int i = 0; i < outer.Count; i++) if (!used.Contains(outer[i].CapIndex)) missingOuter++;
        for (int i = 0; i < inner.Count; i++) if (!used.Contains(inner[i].CapIndex)) missingInner++;

        Debug.Log($"AngularRingHoleCap Diagnostics: Outer={outer.Count}, MissingOuter={missingOuter}, Inner={inner.Count}, MissingInner={missingInner}, Triangles={outTriangles.Count / 3}, TriangleTypes={outTriangleTypes.Count}");
    }
}