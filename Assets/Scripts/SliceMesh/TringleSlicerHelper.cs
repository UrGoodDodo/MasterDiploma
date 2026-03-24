using System.Collections.Generic;
using UnityEngine;

public static class TringleSlicerHelper
{
    public static void ProcessAllTop(int i0, int i1, int i2, Vector3 origNormal, ref SliceContext ctx) //+++
    {
        int t0 = AddVertex(ctx.TopVertices, ctx.MainVertices[i0]);
        int t1 = AddVertex(ctx.TopVertices, ctx.MainVertices[i1]);
        int t2 = AddVertex(ctx.TopVertices, ctx.MainVertices[i2]);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, t0, t1, t2, origNormal);
    }

    public static void ProcessAllBottom(int i0, int i1, int i2, Vector3 origNormal, ref SliceContext ctx) //---
    {
        int b0 = AddVertex(ctx.BotVertices, ctx.MainVertices[i0]);
        int b1 = AddVertex(ctx.BotVertices, ctx.MainVertices[i1]);
        int b2 = AddVertex(ctx.BotVertices, ctx.MainVertices[i2]);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, b0, b1, b2, origNormal);
    }

    public static void ProcessTwoAboveOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, ref SliceContext ctx) //++0
    {
        ProcessAllTop(i0, i1, i2, origNormal, ref ctx);
        int onIdx = (s0 == 0) ? i0 : (s1 == 0 ? i1 : i2);
        GetOrCreateSharedVertex(onIdx, ctx.MainVertices, ctx.TopVertices, ctx.BotVertices, ctx.OnPlaneVertexCache, out _, out _);
    }

    public static void ProcessTwoBelowOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, ref SliceContext ctx) //--0
    {
        ProcessAllBottom(i0, i1, i2, origNormal, ref ctx);
        int onIdx = (s0 == 0) ? i0 : (s1 == 0 ? i1 : i2);
        GetOrCreateSharedVertex(onIdx, ctx.MainVertices, ctx.TopVertices, ctx.BotVertices, ctx.OnPlaneVertexCache, out _, out _);
    }

    public static void ProcessOneAboveTwoOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, ref SliceContext ctx) //+00
    {
        ProcessAllTop(i0, i1, i2, origNormal, ref ctx);
        List<int> onOriginal = new List<int>(2);
        if (s0 == 0) onOriginal.Add(i0);
        if (s1 == 0) onOriginal.Add(i1);
        if (s2 == 0) onOriginal.Add(i2);
        GetOrCreateSharedVertex(onOriginal[0], ctx.MainVertices, ctx.TopVertices, ctx.BotVertices, ctx.OnPlaneVertexCache, out int topOn0, out int botOn0);
        GetOrCreateSharedVertex(onOriginal[1], ctx.MainVertices, ctx.TopVertices, ctx.BotVertices, ctx.OnPlaneVertexCache, out int topOn1, out int botOn1);
        ctx.TopContourEdges.Add(new EdgeKey(topOn0, topOn1));
        ctx.BotContourEdges.Add(new EdgeKey(botOn0, botOn1));
    }

    public static void ProcessOneBelowTwoOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, ref SliceContext ctx) //-00
    {
        ProcessAllBottom(i0, i1, i2, origNormal, ref ctx);
        List<int> onOriginal = new List<int>(2);
        if (s0 == 0) onOriginal.Add(i0);
        if (s1 == 0) onOriginal.Add(i1);
        if (s2 == 0) onOriginal.Add(i2);
        GetOrCreateSharedVertex(onOriginal[0], ctx.MainVertices, ctx.TopVertices, ctx.BotVertices, ctx.OnPlaneVertexCache, out int topOn0, out int botOn0);
        GetOrCreateSharedVertex(onOriginal[1], ctx.MainVertices, ctx.TopVertices, ctx.BotVertices, ctx.OnPlaneVertexCache, out int topOn1, out int botOn1);
        ctx.TopContourEdges.Add(new EdgeKey(topOn0, topOn1));
        ctx.BotContourEdges.Add(new EdgeKey(botOn0, botOn1));
    }

    public static void ProcessOneAboveOneBelowOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, ref SliceContext ctx) //+0-
    {
        int aboveIdx = (s0 > 0) ? i0 : (s1 > 0 ? i1 : i2);
        int belowIdx = (s0 < 0) ? i0 : (s1 < 0 ? i1 : i2);
        int onIdx = (s0 == 0) ? i0 : (s1 == 0 ? i1 : i2);
        GetOrCreateIntersection(aboveIdx, belowIdx, ref ctx, out int topIntersection, out int botIntersection);
        GetOrCreateSharedVertex(onIdx, ctx.MainVertices, ctx.TopVertices, ctx.BotVertices, ctx.OnPlaneVertexCache, out int topOn, out int botOn);

        ctx.TopContourEdges.Add(new EdgeKey(topOn, topIntersection));
        ctx.BotContourEdges.Add(new EdgeKey(botOn, botIntersection));

        int topAbove = AddVertex(ctx.TopVertices, ctx.MainVertices[aboveIdx]);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, topAbove, topOn, topIntersection, origNormal);

        int botBelow = AddVertex(ctx.BotVertices, ctx.MainVertices[belowIdx]);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, botBelow, botIntersection, botOn, origNormal);
    }

    public static void ProcessTwoAboveOneBelow(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, ref SliceContext ctx) //++-
    {
        int aboveIdx1, aboveIdx2, belowIdx;

        if (s0 > 0 && s1 > 0) { aboveIdx1 = i0; aboveIdx2 = i1; belowIdx = i2; }
        else if (s0 > 0 && s2 > 0) { aboveIdx1 = i0; aboveIdx2 = i2; belowIdx = i1; }
        else { aboveIdx1 = i1; aboveIdx2 = i2; belowIdx = i0; }

        GetOrCreateIntersection(aboveIdx1, belowIdx, ref ctx, out int topEdge1, out int botEdge1);
        GetOrCreateIntersection(aboveIdx2, belowIdx, ref ctx, out int topEdge2, out int botEdge2);

        ctx.TopContourEdges.Add(new EdgeKey(topEdge1, topEdge2));
        ctx.BotContourEdges.Add(new EdgeKey(botEdge1, botEdge2));

        int topA1 = AddVertex(ctx.TopVertices, ctx.MainVertices[aboveIdx1]);
        int topA2 = AddVertex(ctx.TopVertices, ctx.MainVertices[aboveIdx2]);

        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, topA1, topA2, topEdge1, origNormal);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, topA2, topEdge2, topEdge1, origNormal);

        int botB = AddVertex(ctx.BotVertices, ctx.MainVertices[belowIdx]);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, botB, botEdge1, botEdge2, origNormal);
    }

    public static void ProcessOneAboveTwoBelow(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, ref SliceContext ctx) //+--
    {
        int aboveIdx, belowIdx1, belowIdx2;

        if (s0 > 0) { aboveIdx = i0; belowIdx1 = i1; belowIdx2 = i2; }
        else if (s1 > 0) { aboveIdx = i1; belowIdx1 = i0; belowIdx2 = i2; }
        else { aboveIdx = i2; belowIdx1 = i0; belowIdx2 = i1; }

        GetOrCreateIntersection(aboveIdx, belowIdx1, ref ctx, out int topEdge1, out int botEdge1);
        GetOrCreateIntersection(aboveIdx, belowIdx2, ref ctx, out int topEdge2, out int botEdge2);

        ctx.TopContourEdges.Add(new EdgeKey(topEdge1, topEdge2));
        ctx.BotContourEdges.Add(new EdgeKey(botEdge1, botEdge2));

        int topA = AddVertex(ctx.TopVertices, ctx.MainVertices[aboveIdx]);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, topA, topEdge1, topEdge2, origNormal);

        int botB1 = AddVertex(ctx.BotVertices, ctx.MainVertices[belowIdx1]);
        int botB2 = AddVertex(ctx.BotVertices, ctx.MainVertices[belowIdx2]);

        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, botB1, botB2, botEdge1, origNormal);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, botB2, botEdge2, botEdge1, origNormal);
    }

    public static void ProcessAllOnPlane(int i0, int i1, int i2, Vector3 origNormal, ref SliceContext ctx) //000
    {
        int t0 = AddVertex(ctx.TopVertices, ctx.MainVertices[i0]);
        int t1 = AddVertex(ctx.TopVertices, ctx.MainVertices[i1]);
        int t2 = AddVertex(ctx.TopVertices, ctx.MainVertices[i2]);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, t0, t1, t2, origNormal);

        int b0 = AddVertex(ctx.BotVertices, ctx.MainVertices[i0]);
        int b1 = AddVertex(ctx.BotVertices, ctx.MainVertices[i1]);
        int b2 = AddVertex(ctx.BotVertices, ctx.MainVertices[i2]);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, b0, b1, b2, origNormal);
    }

    //-------------------------------------------------------------------------------------------------

    static int AddVertex(List<Vector3> vertices, Vector3 point)
    {
        int index = vertices.Count;
        vertices.Add(point);
        return index;
    }

    static void AddOrientedTriangle(List<Vector3> vertices, List<int> triangles, int idx1, int idx2, int idx3, Vector3 originalNormal)
    {
        Vector3 p1 = vertices[idx1];
        Vector3 p2 = vertices[idx2];
        Vector3 p3 = vertices[idx3];
        Vector3 newNormal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

        if (Vector3.Dot(originalNormal, newNormal) < 0)
        {
            triangles.Add(idx1);
            triangles.Add(idx3);
            triangles.Add(idx2);
        }
        else
        {
            triangles.Add(idx1);
            triangles.Add(idx2);
            triangles.Add(idx3);
        }
    }

    static void GetOrCreateSharedVertex( int originalIdx, Vector3[] mainVertices, List<Vector3> topVertices, List<Vector3> botVertices, Dictionary<int, (int top, int bottom)> cache, out int topIndex, out int botIndex)
    {
        if (cache.TryGetValue(originalIdx, out var existing))
        {
            topIndex = existing.top;
            botIndex = existing.bottom;
            return;
        }

        topIndex = AddVertex(topVertices, mainVertices[originalIdx]);
        botIndex = AddVertex(botVertices, mainVertices[originalIdx]);

        cache.Add(originalIdx, (topIndex, botIndex));
    }

    static void GetOrCreateIntersection(int idxA, int idxB, ref SliceContext ctx, out int topIndex, out int botIndex)
    {
        EdgeKey key = new EdgeKey(idxA, idxB);
        if (ctx.EdgeCache.TryGetValue(key, out var existing))
        {
            topIndex = existing.top;
            botIndex = existing.bottom;
            return;
        }
        Vector3 intersectionLocal = GetIntersectionPointLocal(idxA, idxB, ctx.MainVertices, ctx.Transform, ctx.Plane);
        topIndex = AddVertex(ctx.TopVertices, intersectionLocal);
        botIndex = AddVertex(ctx.BotVertices, intersectionLocal);
        ctx.EdgeCache.Add(key, (topIndex, botIndex));
    }

    static Vector3 GetIntersectionPointLocal(int idxA, int idxB, Vector3[] localVerts, Transform objTransform, Plane worldPlane)
    {
        Vector3 worldA = objTransform.TransformPoint(localVerts[idxA]);
        Vector3 worldB = objTransform.TransformPoint(localVerts[idxB]);
        float distA = worldPlane.GetDistanceToPoint(worldA);
        float distB = worldPlane.GetDistanceToPoint(worldB);
        float t = -distA / (distB - distA); // t от 0 до 1
        Vector3 worldPoint = worldA + t * (worldB - worldA);
        return objTransform.InverseTransformPoint(worldPoint);
    }
}
