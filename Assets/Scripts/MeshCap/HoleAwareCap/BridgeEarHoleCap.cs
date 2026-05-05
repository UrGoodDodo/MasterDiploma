using System.Collections.Generic;
using UnityEngine;

public static class BridgeEarHoleCap
{
    public static bool Triangulate(CapLoopSet loopSet, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal, bool enableDiagnostics)
    {
        if (loopSet == null || !loopSet.IsValid) return false;

        Vector3 n = normal.normalized;
        HoleCapGeometry2D.BuildPlaneBasis(n, out Vector3 axisX, out Vector3 axisY);

        List<int> mergedLoop = new List<int>(loopSet.OuterLoop);

        for (int i = 0; i < loopSet.Holes.Count; i++) mergedLoop = BridgeHole2D(mergedLoop, loopSet.Holes[i], mainVertices, axisX, axisY);

        mergedLoop = CleanLoopForTriangulation(mergedLoop, mainVertices, axisX, axisY);

        if (mergedLoop == null || mergedLoop.Count < 3) return false;

        int vertexOffset = capVertices.Count;

        for (int i = 0; i < mergedLoop.Count; i++) capVertices.Add(mainVertices[mergedLoop[i]]);

        List<int> localTriangles = TriangulateEarClipping2D(mergedLoop, mainVertices, axisX, axisY);

        for (int i = 0; i < localTriangles.Count; i++) outTriangles.Add(localTriangles[i] + vertexOffset);
        for (int i = 0; i < localTriangles.Count / 3; i++) outTriangleTypes.Add(SurfaceType.Cap);

        if (enableDiagnostics) Debug.Log($"BridgeEarHoleCap result: CapVertices={mergedLoop.Count}, Holes={loopSet.Holes.Count}, Triangles={localTriangles.Count / 3}");

        return localTriangles.Count > 0;
    }

    private static List<int> TriangulateEarClipping2D(List<int> loop, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        List<int> triangles = new List<int>();

        if (loop == null || loop.Count < 3) return triangles;

        List<int> polygon = new List<int>();

        for (int i = 0; i < loop.Count; i++) polygon.Add(i);

        float area = SignedArea2DByLoop(loop, vertices, axisX, axisY);
        bool ccw = area > 0f;

        int safety = 0;
        int maxSafety = polygon.Count * polygon.Count;

        while (polygon.Count > 3 && safety++ < maxSafety)
        {
            bool earFound = false;

            for (int i = 0; i < polygon.Count; i++)
            {
                int prevIndex = polygon[(i - 1 + polygon.Count) % polygon.Count];
                int currIndex = polygon[i];
                int nextIndex = polygon[(i + 1) % polygon.Count];

                HoleCapPoint2 prev = HoleCapGeometry2D.Project(vertices[loop[prevIndex]], axisX, axisY);
                HoleCapPoint2 curr = HoleCapGeometry2D.Project(vertices[loop[currIndex]], axisX, axisY);
                HoleCapPoint2 next = HoleCapGeometry2D.Project(vertices[loop[nextIndex]], axisX, axisY);

                if (!IsConvex(prev, curr, next, ccw)) continue;
                if (ContainsAnyPointInTriangle(prev, curr, next, polygon, prevIndex, currIndex, nextIndex, loop, vertices, axisX, axisY)) continue;

                if (ccw)
                {
                    triangles.Add(prevIndex);
                    triangles.Add(currIndex);
                    triangles.Add(nextIndex);
                }
                else
                {
                    triangles.Add(prevIndex);
                    triangles.Add(nextIndex);
                    triangles.Add(currIndex);
                }

                polygon.RemoveAt(i);
                earFound = true;
                break;
            }

            if (!earFound)
            {
                Debug.LogWarning($"BridgeEarHoleCap: ear clipping stopped early. Remaining vertices={polygon.Count}");
                break;
            }
        }

        if (polygon.Count == 3)
        {
            if (ccw)
            {
                triangles.Add(polygon[0]);
                triangles.Add(polygon[1]);
                triangles.Add(polygon[2]);
            }
            else
            {
                triangles.Add(polygon[0]);
                triangles.Add(polygon[2]);
                triangles.Add(polygon[1]);
            }
        }

        return triangles;
    }

    private static bool IsConvex(HoleCapPoint2 a, HoleCapPoint2 b, HoleCapPoint2 c, bool ccw)
    {
        float cross = HoleCapGeometry2D.Orient(a, b, c);
        const float eps = 1e-7f;
        return ccw ? cross > eps : cross < -eps;
    }

    private static bool ContainsAnyPointInTriangle(HoleCapPoint2 a, HoleCapPoint2 b, HoleCapPoint2 c, List<int> polygon, int ia, int ib, int ic, List<int> loop, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            int pIndex = polygon[i];

            if (pIndex == ia || pIndex == ib || pIndex == ic) continue;

            HoleCapPoint2 p = HoleCapGeometry2D.Project(vertices[loop[pIndex]], axisX, axisY);

            if (PointInTriangle(p, a, b, c)) return true;
        }

        return false;
    }

    private static bool PointInTriangle(HoleCapPoint2 p, HoleCapPoint2 a, HoleCapPoint2 b, HoleCapPoint2 c)
    {
        const float eps = 1e-7f;

        float o1 = HoleCapGeometry2D.Orient(a, b, p);
        float o2 = HoleCapGeometry2D.Orient(b, c, p);
        float o3 = HoleCapGeometry2D.Orient(c, a, p);

        bool hasNeg = o1 < -eps || o2 < -eps || o3 < -eps;
        bool hasPos = o1 > eps || o2 > eps || o3 > eps;

        return !(hasNeg && hasPos);
    }

    private static List<int> BridgeHole2D(List<int> outer, List<int> hole, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        List<int> workingHole = new List<int>(hole);

        float outerArea = SignedArea2DByLoop(outer, vertices, axisX, axisY);
        float holeArea = SignedArea2DByLoop(workingHole, vertices, axisX, axisY);

        if (Mathf.Sign(outerArea) == Mathf.Sign(holeArea)) workingHole.Reverse();

        int holeLocalIndex = FindRightmostVertex2D(workingHole, vertices, axisX, axisY);
        int outerLocalIndex = FindVisibleOuterVertex2D(workingHole[holeLocalIndex], outer, workingHole, vertices, axisX, axisY);

        return MergeOuterAndHoleByBridge(outer, workingHole, outerLocalIndex, holeLocalIndex);
    }

    private static List<int> MergeOuterAndHoleByBridge(List<int> outer, List<int> hole, int outerIndex, int holeIndex)
    {
        List<int> merged = new List<int>();

        for (int i = 0; i <= outerIndex; i++) merged.Add(outer[i]);

        for (int i = 0; i < hole.Count; i++)
        {
            int index = (holeIndex + i) % hole.Count;
            merged.Add(hole[index]);
        }

        merged.Add(hole[holeIndex]);
        merged.Add(outer[outerIndex]);

        for (int i = outerIndex + 1; i < outer.Count; i++) merged.Add(outer[i]);

        return merged;
    }

    private static int FindRightmostVertex2D(List<int> loop, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        int best = 0;
        HoleCapPoint2 bestPoint = HoleCapGeometry2D.Project(vertices[loop[0]], axisX, axisY);

        for (int i = 1; i < loop.Count; i++)
        {
            HoleCapPoint2 p = HoleCapGeometry2D.Project(vertices[loop[i]], axisX, axisY);

            if (p.x > bestPoint.x || Mathf.Approximately(p.x, bestPoint.x) && p.y < bestPoint.y)
            {
                best = i;
                bestPoint = p;
            }
        }

        return best;
    }

    private static int FindVisibleOuterVertex2D(int holeVertexIndex, List<int> outer, List<int> hole, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        HoleCapPoint2 holePoint = HoleCapGeometry2D.Project(vertices[holeVertexIndex], axisX, axisY);

        int bestIndex = -1;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < outer.Count; i++)
        {
            int outerVertexIndex = outer[i];
            HoleCapPoint2 outerPoint = HoleCapGeometry2D.Project(vertices[outerVertexIndex], axisX, axisY);

            if (!IsBridgeVisible(holeVertexIndex, outerVertexIndex, holePoint, outerPoint, outer, hole, vertices, axisX, axisY)) continue;

            float d = HoleCapGeometry2D.DistanceSqr(holePoint, outerPoint);

            if (d < bestDistance)
            {
                bestDistance = d;
                bestIndex = i;
            }
        }

        if (bestIndex != -1) return bestIndex;

        Debug.LogWarning("BridgeEarHoleCap: visible bridge not found, using nearest fallback.");

        int fallback = 0;
        float fallbackDistance = float.MaxValue;

        for (int i = 0; i < outer.Count; i++)
        {
            HoleCapPoint2 p = HoleCapGeometry2D.Project(vertices[outer[i]], axisX, axisY);
            float d = HoleCapGeometry2D.DistanceSqr(holePoint, p);

            if (d < fallbackDistance)
            {
                fallbackDistance = d;
                fallback = i;
            }
        }

        return fallback;
    }

    private static bool IsBridgeVisible(int holeVertexIndex, int outerVertexIndex, HoleCapPoint2 bridgeA, HoleCapPoint2 bridgeB, List<int> outer, List<int> hole, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        return !SegmentIntersectsLoopEdges(bridgeA, bridgeB, holeVertexIndex, outerVertexIndex, outer, vertices, axisX, axisY) && !SegmentIntersectsLoopEdges(bridgeA, bridgeB, holeVertexIndex, outerVertexIndex, hole, vertices, axisX, axisY);
    }

    private static bool SegmentIntersectsLoopEdges(HoleCapPoint2 a, HoleCapPoint2 b, int allowedIndexA, int allowedIndexB, List<int> loop, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        for (int i = 0; i < loop.Count; i++)
        {
            int i0 = loop[i];
            int i1 = loop[(i + 1) % loop.Count];

            if (i0 == allowedIndexA || i1 == allowedIndexA || i0 == allowedIndexB || i1 == allowedIndexB) continue;

            HoleCapPoint2 c = HoleCapGeometry2D.Project(vertices[i0], axisX, axisY);
            HoleCapPoint2 d = HoleCapGeometry2D.Project(vertices[i1], axisX, axisY);

            if (HoleCapGeometry2D.SegmentsIntersect(a, b, c, d)) return true;
        }

        return false;
    }

    private static List<int> CleanLoopForTriangulation(List<int> loop, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        List<int> cleaned = new List<int>();

        if (loop == null) return cleaned;

        for (int i = 0; i < loop.Count; i++)
        {
            int index = loop[i];

            if (cleaned.Count == 0 || cleaned[cleaned.Count - 1] != index) cleaned.Add(index);
        }

        if (cleaned.Count > 1 && cleaned[0] == cleaned[cleaned.Count - 1]) cleaned.RemoveAt(cleaned.Count - 1);

        bool changed = true;

        while (changed && cleaned.Count >= 3)
        {
            changed = false;

            for (int i = 0; i < cleaned.Count; i++)
            {
                int prev = cleaned[(i - 1 + cleaned.Count) % cleaned.Count];
                int curr = cleaned[i];
                int next = cleaned[(i + 1) % cleaned.Count];

                HoleCapPoint2 a = HoleCapGeometry2D.Project(vertices[prev], axisX, axisY);
                HoleCapPoint2 b = HoleCapGeometry2D.Project(vertices[curr], axisX, axisY);
                HoleCapPoint2 c = HoleCapGeometry2D.Project(vertices[next], axisX, axisY);

                if (Mathf.Abs(HoleCapGeometry2D.Orient(a, b, c)) < 1e-8f)
                {
                    cleaned.RemoveAt(i);
                    changed = true;
                    break;
                }
            }
        }

        return cleaned;
    }

    private static float SignedArea2DByLoop(List<int> loop, List<Vector3> vertices, Vector3 axisX, Vector3 axisY)
    {
        float area = 0f;

        for (int i = 0; i < loop.Count; i++)
        {
            HoleCapPoint2 a = HoleCapGeometry2D.Project(vertices[loop[i]], axisX, axisY);
            HoleCapPoint2 b = HoleCapGeometry2D.Project(vertices[loop[(i + 1) % loop.Count]], axisX, axisY);

            area += a.x * b.y - b.x * a.y;
        }

        return area * 0.5f;
    }
}