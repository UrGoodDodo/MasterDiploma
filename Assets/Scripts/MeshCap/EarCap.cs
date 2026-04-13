using System.Collections.Generic;
using UnityEngine;

public class EarCap : ICapStrategy
{
    public void TriangulateCap(List<int> loop, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal)
    {
        if (loop.Count < 3) return;

        List<Vector2> loop2D = ProjectLoopTo2D(loop, mainVertices, normal);

        EnsureCounterClockwise(loop, loop2D);

        List<int> capLoopIndices = new List<int>();
        foreach (int idx in loop)
        {
            capLoopIndices.Add(capVertices.Count);
            capVertices.Add(mainVertices[idx]);
        }

        List<int> indices = new List<int>();
        for (int i = 0; i < loop2D.Count; i++) indices.Add(i);

        int safety = 0;
        while (indices.Count > 3 && safety++ < 1000)
        {
            bool earFound = false;

            for (int i = 0; i < indices.Count; i++)
            {
                int i0 = indices[(i - 1 + indices.Count) % indices.Count];
                int i1 = indices[i];
                int i2 = indices[(i + 1) % indices.Count];

                Vector2 a = loop2D[i0];
                Vector2 b = loop2D[i1];
                Vector2 c = loop2D[i2];

                if (!IsConvex(a, b, c)) continue;

                bool hasPointInside = false;
                for (int j = 0; j < indices.Count; j++)
                {
                    int currentIdx = indices[j];
                    if (currentIdx == i0 || currentIdx == i1 || currentIdx == i2) continue;

                    if (PointInTriangle(loop2D[currentIdx], a, b, c))
                    {
                        hasPointInside = true;
                        break;
                    }
                }

                //Debug.Log($"Triangle ({i0},{i1},{i2}): Convex={IsConvex(a, b, c)}, PointsInside={hasPointInside}");

                if (hasPointInside) continue;

                outTriangles.Add(capLoopIndices[i0]);
                outTriangles.Add(capLoopIndices[i1]);
                outTriangles.Add(capLoopIndices[i2]);
                outTriangleTypes.Add(SurfaceType.Cap);

                indices.RemoveAt(i);
                earFound = true;
                break;
            }

            if (!earFound)
            {
                //Debug.LogWarning("asdf.");
                break;
            }
        }
        if (indices.Count == 3)
        {
            outTriangles.Add(capLoopIndices[indices[0]]);
            outTriangles.Add(capLoopIndices[indices[1]]);
            outTriangles.Add(capLoopIndices[indices[2]]);
            outTriangleTypes.Add(SurfaceType.Cap);
        }
    }

    // ------------------------------------------------------------

    public static List<Vector2> ProjectLoopTo2D(List<int> loop, List<Vector3> vertices, Vector3 normal)
    {
        List<Vector2> loop2D = new List<Vector2>(loop.Count);
        GetPlaneBasis(normal, out Vector3 axisX, out Vector3 axisY);

        Vector3 referencePoint = vertices[loop[0]];

        for (int i = 0; i < loop.Count; i++)
        {
            Vector3 p = vertices[loop[i]] - referencePoint; // Смещаем точку
            Vector2 projected = new Vector2(
                Vector3.Dot(p, axisX),
                Vector3.Dot(p, axisY)
            );
            loop2D.Add(projected);
        }
        return loop2D;
    }

    private static void GetPlaneBasis(Vector3 normal, out Vector3 axisX, out Vector3 axisY)
    {
        normal.Normalize();

        axisX = Vector3.Cross(normal, Vector3.up);

        if (axisX.sqrMagnitude < 0.000001f)
        {
            axisX = Vector3.Cross(normal, Vector3.right);
        }

        axisX.Normalize();
        axisY = Vector3.Cross(normal, axisX).normalized;
    }

    public static float SignedArea2D(List<Vector2> polygon)
    {
        float area = 0f;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];

            area += (a.x * b.y - b.x * a.y);
        }

        return area * 0.5f;
    }

    public static void EnsureCounterClockwise(List<int> loop, List<Vector2> loop2D)
    {
        float area = SignedArea2D(loop2D);

        if (area < 0f)
        {
            loop.Reverse();
            loop2D.Reverse();
        }
    }

    private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c, float epsilon = 1e-6f)
    {
        float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        return cross > epsilon;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float det = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);

        if (Mathf.Abs(det) < 1e-10f) return false;

        float l1 = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / det;
        float l2 = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / det;
        float l3 = 1f - l1 - l2;

        return l1 > -1e-6f && l2 > -1e-6f && l3 > -1e-6f;
    }
}