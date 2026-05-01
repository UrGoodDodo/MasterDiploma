using System.Collections.Generic;
using UnityEngine;

public static class RawTriangleSlicerHelper
{
    public static void ProcessAllTop(int i0, int i1, int i2, Vector3 origNormal, SurfaceType surfaceType, ref RawSliceContext ctx)
    {
        int t0 = AddVertex(ctx.Top, ctx.MainVertices[i0], ctx.MainUVs[i0]);
        int t1 = AddVertex(ctx.Top, ctx.MainVertices[i1], ctx.MainUVs[i1]);
        int t2 = AddVertex(ctx.Top, ctx.MainVertices[i2], ctx.MainUVs[i2]);

        AddOrientedTriangle(ctx.Top, t0, t1, t2, origNormal, surfaceType);
    }

    public static void ProcessAllBottom(int i0, int i1, int i2, Vector3 origNormal, SurfaceType surfaceType,ref RawSliceContext ctx)
    {
        int b0 = AddVertex(ctx.Bottom, ctx.MainVertices[i0], ctx.MainUVs[i0]);
        int b1 = AddVertex(ctx.Bottom, ctx.MainVertices[i1], ctx.MainUVs[i1]);
        int b2 = AddVertex(ctx.Bottom, ctx.MainVertices[i2], ctx.MainUVs[i2]);

        AddOrientedTriangle(ctx.Bottom, b0, b1, b2, origNormal, surfaceType);
    }

    public static void ProcessTwoAboveOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref RawSliceContext ctx)
    {
        ProcessAllTop(i0, i1, i2, origNormal, surfaceType, ref ctx);

        int onIdx = s0 == 0 ? i0 : s1 == 0 ? i1 : i2;

        GetOrCreateSharedVertex(onIdx, ref ctx, out _, out _);
    }

    public static void ProcessTwoBelowOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref RawSliceContext ctx)
    {
        ProcessAllBottom(i0, i1, i2, origNormal, surfaceType, ref ctx);

        int onIdx = s0 == 0 ? i0 : s1 == 0 ? i1 : i2;

        GetOrCreateSharedVertex(onIdx, ref ctx, out _, out _);
    }

    public static void ProcessOneAboveTwoOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref RawSliceContext ctx)
    {
        ProcessAllTop(i0, i1, i2, origNormal, surfaceType, ref ctx);

        List<int> onOriginal = new List<int>(2);

        if (s0 == 0) onOriginal.Add(i0);
        if (s1 == 0) onOriginal.Add(i1);
        if (s2 == 0) onOriginal.Add(i2);

        GetOrCreateSharedVertex(onOriginal[0], ref ctx, out int topOn0, out int botOn0);
        GetOrCreateSharedVertex(onOriginal[1], ref ctx, out int topOn1, out int botOn1);

        ctx.TopContourEdges.Add(new EdgeKey(topOn0, topOn1));
        ctx.BottomContourEdges.Add(new EdgeKey(botOn0, botOn1));
    }

    public static void ProcessOneBelowTwoOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref RawSliceContext ctx)
    {
        ProcessAllBottom(i0, i1, i2, origNormal, surfaceType, ref ctx);

        List<int> onOriginal = new List<int>(2);

        if (s0 == 0) onOriginal.Add(i0);
        if (s1 == 0) onOriginal.Add(i1);
        if (s2 == 0) onOriginal.Add(i2);

        GetOrCreateSharedVertex(onOriginal[0], ref ctx, out int topOn0, out int botOn0);
        GetOrCreateSharedVertex(onOriginal[1], ref ctx, out int topOn1, out int botOn1);

        ctx.TopContourEdges.Add(new EdgeKey(topOn0, topOn1));
        ctx.BottomContourEdges.Add(new EdgeKey(botOn0, botOn1));
    }

    public static void ProcessOneAboveOneBelowOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType,ref RawSliceContext ctx)
    {
        int aboveIdx = s0 > 0 ? i0 : s1 > 0 ? i1 : i2;
        int belowIdx = s0 < 0 ? i0 : s1 < 0 ? i1 : i2;
        int onIdx = s0 == 0 ? i0 : s1 == 0 ? i1 : i2;

        GetOrCreateIntersection(aboveIdx, belowIdx, ref ctx, out int topIntersection, out int botIntersection);

        GetOrCreateSharedVertex(onIdx, ref ctx, out int topOn, out int botOn);

        ctx.TopContourEdges.Add(new EdgeKey(topOn, topIntersection));
        ctx.BottomContourEdges.Add(new EdgeKey(botOn, botIntersection));

        int topAbove = AddVertex(ctx.Top, ctx.MainVertices[aboveIdx], ctx.MainUVs[aboveIdx]);

        AddOrientedTriangle(ctx.Top, topAbove, topOn, topIntersection, origNormal, surfaceType);

        int botBelow = AddVertex(ctx.Bottom, ctx.MainVertices[belowIdx], ctx.MainUVs[belowIdx]);

        AddOrientedTriangle(ctx.Bottom, botBelow, botIntersection, botOn, origNormal, surfaceType);
    }

    public static void ProcessTwoAboveOneBelow(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref RawSliceContext ctx)
    {
        int aboveIdx1;
        int aboveIdx2;
        int belowIdx;

        if (s0 > 0 && s1 > 0)
        {
            aboveIdx1 = i0;
            aboveIdx2 = i1;
            belowIdx = i2;
        }
        else if (s0 > 0 && s2 > 0)
        {
            aboveIdx1 = i0;
            aboveIdx2 = i2;
            belowIdx = i1;
        }
        else
        {
            aboveIdx1 = i1;
            aboveIdx2 = i2;
            belowIdx = i0;
        }

        GetOrCreateIntersection(aboveIdx1, belowIdx, ref ctx, out int topEdge1, out int botEdge1);

        GetOrCreateIntersection(aboveIdx2, belowIdx, ref ctx, out int topEdge2, out int botEdge2);

        ctx.TopContourEdges.Add(new EdgeKey(topEdge1, topEdge2));
        ctx.BottomContourEdges.Add(new EdgeKey(botEdge1, botEdge2));

        int topA1 = AddVertex(ctx.Top, ctx.MainVertices[aboveIdx1], ctx.MainUVs[aboveIdx1]);

        int topA2 = AddVertex(ctx.Top, ctx.MainVertices[aboveIdx2], ctx.MainUVs[aboveIdx2]);

        AddOrientedTriangle(ctx.Top, topA1, topA2, topEdge1, origNormal, surfaceType);

        AddOrientedTriangle(ctx.Top, topA2, topEdge2, topEdge1, origNormal, surfaceType);

        int botB = AddVertex(ctx.Bottom, ctx.MainVertices[belowIdx], ctx.MainUVs[belowIdx]);

        AddOrientedTriangle(ctx.Bottom, botB, botEdge1, botEdge2, origNormal, surfaceType);
    }

    public static void ProcessOneAboveTwoBelow(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref RawSliceContext ctx)
    {
        int aboveIdx;
        int belowIdx1;
        int belowIdx2;

        if (s0 > 0)
        {
            aboveIdx = i0;
            belowIdx1 = i1;
            belowIdx2 = i2;
        }
        else if (s1 > 0)
        {
            aboveIdx = i1;
            belowIdx1 = i0;
            belowIdx2 = i2;
        }
        else
        {
            aboveIdx = i2;
            belowIdx1 = i0;
            belowIdx2 = i1;
        }

        GetOrCreateIntersection(aboveIdx, belowIdx1, ref ctx, out int topEdge1, out int botEdge1);

        GetOrCreateIntersection(aboveIdx, belowIdx2, ref ctx, out int topEdge2, out int botEdge2);

        ctx.TopContourEdges.Add(new EdgeKey(topEdge1, topEdge2));
        ctx.BottomContourEdges.Add(new EdgeKey(botEdge1, botEdge2));

        int topA = AddVertex(ctx.Top, ctx.MainVertices[aboveIdx], ctx.MainUVs[aboveIdx]);

        AddOrientedTriangle(ctx.Top, topA, topEdge1, topEdge2, origNormal, surfaceType);

        int botB1 = AddVertex(ctx.Bottom, ctx.MainVertices[belowIdx1], ctx.MainUVs[belowIdx1]);

        int botB2 = AddVertex(ctx.Bottom, ctx.MainVertices[belowIdx2], ctx.MainUVs[belowIdx2]);

        AddOrientedTriangle(ctx.Bottom, botB1, botB2, botEdge1, origNormal, surfaceType);

        AddOrientedTriangle(ctx.Bottom, botB2, botEdge2, botEdge1, origNormal, surfaceType);
    }

    public static void ProcessAllOnPlane(int i0, int i1, int i2, Vector3 origNormal, SurfaceType surfaceType, ref RawSliceContext ctx)
    {
        int t0 = AddVertex(ctx.Top, ctx.MainVertices[i0], ctx.MainUVs[i0]);
        int t1 = AddVertex(ctx.Top, ctx.MainVertices[i1], ctx.MainUVs[i1]);
        int t2 = AddVertex(ctx.Top, ctx.MainVertices[i2], ctx.MainUVs[i2]);

        AddOrientedTriangle(ctx.Top, t0, t1, t2, origNormal, surfaceType);

        int b0 = AddVertex(ctx.Bottom, ctx.MainVertices[i0], ctx.MainUVs[i0]);
        int b1 = AddVertex(ctx.Bottom, ctx.MainVertices[i1], ctx.MainUVs[i1]);
        int b2 = AddVertex(ctx.Bottom, ctx.MainVertices[i2], ctx.MainUVs[i2]);

        AddOrientedTriangle(ctx.Bottom, b0, b1, b2, origNormal, surfaceType);
    }

    private static int AddVertex(RawSliceBuildData buildData, Vector3 point, Vector2 uv)
    {
        return buildData.AddVertex(point, uv);
    }

    private static void AddOrientedTriangle(RawSliceBuildData buildData, int idx1, int idx2, int idx3, Vector3 originalNormal, SurfaceType surfaceType)
    {
        Vector3 p1 = buildData.Vertices[idx1];
        Vector3 p2 = buildData.Vertices[idx2];
        Vector3 p3 = buildData.Vertices[idx3];

        Vector3 newNormal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

        if (Vector3.Dot(originalNormal, newNormal) < 0f)
        {
            buildData.AddTriangle(idx1, idx3, idx2, surfaceType);
        }
        else
        {
            buildData.AddTriangle(idx1, idx2, idx3, surfaceType);
        }
    }

    private static void GetOrCreateSharedVertex(int originalIdx, ref RawSliceContext ctx, out int topIndex, out int bottomIndex)
    {
        if (ctx.OnPlaneVertexCache.TryGetValue(originalIdx, out var existing))
        {
            topIndex = existing.top;
            bottomIndex = existing.bottom;
            return;
        }

        topIndex = AddVertex(ctx.Top, ctx.MainVertices[originalIdx], ctx.MainUVs[originalIdx]);

        bottomIndex = AddVertex(ctx.Bottom, ctx.MainVertices[originalIdx],ctx.MainUVs[originalIdx]);

        ctx.OnPlaneVertexCache.Add(originalIdx, (topIndex, bottomIndex));
    }

    private static void GetOrCreateIntersection(int idxA, int idxB, ref RawSliceContext ctx, out int topIndex, out int bottomIndex)
    {
        EdgeKey key = new EdgeKey(idxA, idxB);

        if (ctx.EdgeCache.TryGetValue(key, out var existing))
        {
            topIndex = existing.top;
            bottomIndex = existing.bottom;
            return;
        }

        Vector3 intersectionLocal = GetIntersectionPointLocal(idxA, idxB, ctx.MainVertices, ctx.LocalPlane);

        Vector2 intersectionUV = GetIntersectionUV(idxA, idxB, ctx.MainVertices, ctx.MainUVs, ctx.LocalPlane);

        topIndex = AddVertex(ctx.Top, intersectionLocal, intersectionUV);
        bottomIndex = AddVertex(ctx.Bottom, intersectionLocal, intersectionUV);

        ctx.EdgeCache.Add(key, (topIndex, bottomIndex));
    }

    private static Vector3 GetIntersectionPointLocal(int idxA, int idxB, Vector3[] localVertices, Plane localPlane)
    {
        Vector3 a = localVertices[idxA];
        Vector3 b = localVertices[idxB];

        float distA = localPlane.GetDistanceToPoint(a);
        float distB = localPlane.GetDistanceToPoint(b);

        float denominator = distB - distA;

        if (Mathf.Abs(denominator) < 1e-6f)
            return a;

        float t = -distA / denominator;
        t = Mathf.Clamp01(t);

        return a + t * (b - a);
    }

    private static Vector2 GetIntersectionUV(int idxA, int idxB, Vector3[] localVertices, Vector2[] mainUVs, Plane localPlane)
    {
        Vector3 a = localVertices[idxA];
        Vector3 b = localVertices[idxB];

        float distA = localPlane.GetDistanceToPoint(a);
        float distB = localPlane.GetDistanceToPoint(b);

        float denominator = distB - distA;

        if (Mathf.Abs(denominator) < 1e-6f)
            return mainUVs[idxA];

        float t = -distA / denominator;
        t = Mathf.Clamp01(t);

        return Vector2.Lerp(mainUVs[idxA], mainUVs[idxB], t);
    }
}