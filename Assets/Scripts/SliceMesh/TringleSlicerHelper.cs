using System.Collections.Generic;
using UnityEngine;

public static class TringleSlicerHelper
{
    public static void ProcessAllTop(int i0, int i1, int i2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //+++
    {
        int t0 = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[i0], ctx.MainUVs[i0]);
        int t1 = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[i1], ctx.MainUVs[i1]);
        int t2 = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[i2], ctx.MainUVs[i2]);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, ctx.TopTriangleTypes, t0, t1, t2, origNormal, surfaceType);
    }

    public static void ProcessAllBottom(int i0, int i1, int i2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //---
    {
        int b0 = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[i0], ctx.MainUVs[i0]);
        int b1 = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[i1], ctx.MainUVs[i1]);
        int b2 = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[i2], ctx.MainUVs[i2]);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, ctx.BotTriangleTypes, b0, b1, b2, origNormal, surfaceType);
    }

    public static void ProcessTwoAboveOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //++0
    {
        ProcessAllTop(i0, i1, i2, origNormal, surfaceType, ref ctx);
        int onIdx = (s0 == 0) ? i0 : (s1 == 0 ? i1 : i2);
        GetOrCreateSharedVertex(onIdx, ctx.MainVertices, ctx.MainUVs, ctx.TopVertices, ctx.TopUVs, ctx.BotVertices, ctx.BotUVs, ctx.OnPlaneVertexCache, out _, out _);
    }

    public static void ProcessTwoBelowOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //--0
    {
        ProcessAllBottom(i0, i1, i2, origNormal, surfaceType, ref ctx);
        int onIdx = (s0 == 0) ? i0 : (s1 == 0 ? i1 : i2);
        GetOrCreateSharedVertex(onIdx, ctx.MainVertices, ctx.MainUVs, ctx.TopVertices, ctx.TopUVs, ctx.BotVertices, ctx.BotUVs, ctx.OnPlaneVertexCache, out _, out _);
    }

    public static void ProcessOneAboveTwoOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //+00
    {
        ProcessAllTop(i0, i1, i2, origNormal, surfaceType, ref ctx);
        List<int> onOriginal = new List<int>(2);
        if (s0 == 0) onOriginal.Add(i0);
        if (s1 == 0) onOriginal.Add(i1);
        if (s2 == 0) onOriginal.Add(i2);
        GetOrCreateSharedVertex(onOriginal[0], ctx.MainVertices, ctx.MainUVs, ctx.TopVertices, ctx.TopUVs, ctx.BotVertices, ctx.BotUVs, ctx.OnPlaneVertexCache, out int topOn0, out int botOn0);
        GetOrCreateSharedVertex(onOriginal[1], ctx.MainVertices, ctx.MainUVs, ctx.TopVertices, ctx.TopUVs, ctx.BotVertices, ctx.BotUVs, ctx.OnPlaneVertexCache, out int topOn1, out int botOn1);
        ctx.TopContourEdges.Add(new EdgeKey(topOn0, topOn1));
        ctx.BotContourEdges.Add(new EdgeKey(botOn0, botOn1));
    }

    public static void ProcessOneBelowTwoOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //-00
    {
        ProcessAllBottom(i0, i1, i2, origNormal, surfaceType, ref ctx);
        List<int> onOriginal = new List<int>(2);
        if (s0 == 0) onOriginal.Add(i0);
        if (s1 == 0) onOriginal.Add(i1);
        if (s2 == 0) onOriginal.Add(i2);
        GetOrCreateSharedVertex(onOriginal[0], ctx.MainVertices, ctx.MainUVs, ctx.TopVertices, ctx.TopUVs, ctx.BotVertices, ctx.BotUVs, ctx.OnPlaneVertexCache, out int topOn0, out int botOn0);
        GetOrCreateSharedVertex(onOriginal[1], ctx.MainVertices, ctx.MainUVs, ctx.TopVertices, ctx.TopUVs, ctx.BotVertices, ctx.BotUVs, ctx.OnPlaneVertexCache, out int topOn1, out int botOn1);
        ctx.TopContourEdges.Add(new EdgeKey(topOn0, topOn1));
        ctx.BotContourEdges.Add(new EdgeKey(botOn0, botOn1));
    }

    public static void ProcessOneAboveOneBelowOneOnPlane(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //+0-
    {
        int aboveIdx = (s0 > 0) ? i0 : (s1 > 0 ? i1 : i2);
        int belowIdx = (s0 < 0) ? i0 : (s1 < 0 ? i1 : i2);
        int onIdx = (s0 == 0) ? i0 : (s1 == 0 ? i1 : i2);
        GetOrCreateIntersection(aboveIdx, belowIdx, ref ctx, out int topIntersection, out int botIntersection);
        GetOrCreateSharedVertex(onIdx, ctx.MainVertices, ctx.MainUVs, ctx.TopVertices, ctx.TopUVs, ctx.BotVertices, ctx.BotUVs, ctx.OnPlaneVertexCache, out int topOn, out int botOn);

        ctx.TopContourEdges.Add(new EdgeKey(topOn, topIntersection));
        ctx.BotContourEdges.Add(new EdgeKey(botOn, botIntersection));

        int topAbove = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[aboveIdx], ctx.MainUVs[aboveIdx]);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, ctx.TopTriangleTypes, topAbove, topOn, topIntersection, origNormal, surfaceType);

        int botBelow = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[belowIdx], ctx.MainUVs[belowIdx]);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, ctx.BotTriangleTypes, botBelow, botIntersection, botOn, origNormal, surfaceType);
    }

    public static void ProcessTwoAboveOneBelow(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //++-
    {
        int aboveIdx1, aboveIdx2, belowIdx;

        if (s0 > 0 && s1 > 0) { aboveIdx1 = i0; aboveIdx2 = i1; belowIdx = i2; }
        else if (s0 > 0 && s2 > 0) { aboveIdx1 = i0; aboveIdx2 = i2; belowIdx = i1; }
        else { aboveIdx1 = i1; aboveIdx2 = i2; belowIdx = i0; }

        GetOrCreateIntersection(aboveIdx1, belowIdx, ref ctx, out int topEdge1, out int botEdge1);
        GetOrCreateIntersection(aboveIdx2, belowIdx, ref ctx, out int topEdge2, out int botEdge2);

        ctx.TopContourEdges.Add(new EdgeKey(topEdge1, topEdge2));
        ctx.BotContourEdges.Add(new EdgeKey(botEdge1, botEdge2));

        int topA1 = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[aboveIdx1], ctx.MainUVs[aboveIdx1]);
        int topA2 = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[aboveIdx2], ctx.MainUVs[aboveIdx2]);

        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, ctx.TopTriangleTypes, topA1, topA2, topEdge1, origNormal, surfaceType);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, ctx.TopTriangleTypes, topA2, topEdge2, topEdge1, origNormal, surfaceType);

        int botB = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[belowIdx], ctx.MainUVs[belowIdx]);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, ctx.BotTriangleTypes, botB, botEdge1, botEdge2, origNormal, surfaceType);
    }

    public static void ProcessOneAboveTwoBelow(int i0, int i1, int i2, int s0, int s1, int s2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //+--
    {
        int aboveIdx, belowIdx1, belowIdx2;

        if (s0 > 0) { aboveIdx = i0; belowIdx1 = i1; belowIdx2 = i2; }
        else if (s1 > 0) { aboveIdx = i1; belowIdx1 = i0; belowIdx2 = i2; }
        else { aboveIdx = i2; belowIdx1 = i0; belowIdx2 = i1; }

        GetOrCreateIntersection(aboveIdx, belowIdx1, ref ctx, out int topEdge1, out int botEdge1);
        GetOrCreateIntersection(aboveIdx, belowIdx2, ref ctx, out int topEdge2, out int botEdge2);

        ctx.TopContourEdges.Add(new EdgeKey(topEdge1, topEdge2));
        ctx.BotContourEdges.Add(new EdgeKey(botEdge1, botEdge2));

        int topA = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[aboveIdx], ctx.MainUVs[aboveIdx]);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, ctx.TopTriangleTypes, topA, topEdge1, topEdge2, origNormal, surfaceType);

        int botB1 = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[belowIdx1], ctx.MainUVs[belowIdx1]);
        int botB2 = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[belowIdx2], ctx.MainUVs[belowIdx2]);

        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, ctx.BotTriangleTypes, botB1, botB2, botEdge1, origNormal, surfaceType);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, ctx.BotTriangleTypes, botB2, botEdge2, botEdge1, origNormal, surfaceType);
    }

    public static void ProcessAllOnPlane(int i0, int i1, int i2, Vector3 origNormal, SurfaceType surfaceType, ref SliceContext ctx) //000
    {
        int t0 = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[i0], ctx.MainUVs[i0]);
        int t1 = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[i1], ctx.MainUVs[i1]);
        int t2 = AddVertex(ctx.TopVertices, ctx.TopUVs, ctx.MainVertices[i2], ctx.MainUVs[i2]);
        AddOrientedTriangle(ctx.TopVertices, ctx.TopTriangles, ctx.TopTriangleTypes, t0, t1, t2, origNormal, surfaceType);

        int b0 = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[i0], ctx.MainUVs[i0]);
        int b1 = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[i1], ctx.MainUVs[i1]);
        int b2 = AddVertex(ctx.BotVertices, ctx.BotUVs, ctx.MainVertices[i2], ctx.MainUVs[i2]);
        AddOrientedTriangle(ctx.BotVertices, ctx.BotTriangles, ctx.BotTriangleTypes, b0, b1, b2, origNormal, surfaceType);
    }

    //-------------------------------------------------------------------------------------------------

    static int AddVertex(List<Vector3> vertices, List<Vector2> uvs, Vector3 point, Vector2 uv)
    {
        int index = vertices.Count;
        vertices.Add(point);
        uvs.Add(uv);
        return index;
    }
    static int AddVertex(List<Vector3> vertices, Vector3 point)
    {
        int index = vertices.Count;
        vertices.Add(point);
        return index;
    }

    static void AddOrientedTriangle(List<Vector3> vertices, List<int> triangles, List<SurfaceType> triangleTypes, int idx1, int idx2, int idx3, Vector3 originalNormal, SurfaceType surfaceType)
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

        triangleTypes.Add(surfaceType);
    }

    static void GetOrCreateSharedVertex( int originalIdx, Vector3[] mainVertices, Vector2[] mainUVs, List<Vector3> topVertices, List<Vector2> topUVs, List<Vector3> botVertices, List<Vector2> botUVs, Dictionary<int, (int top, int bottom)> cache, out int topIndex, out int botIndex)
    {
        if (cache.TryGetValue(originalIdx, out var existing))
        {
            topIndex = existing.top;
            botIndex = existing.bottom;
            return;
        }

        topIndex = AddVertex(topVertices, topUVs, mainVertices[originalIdx], mainUVs[originalIdx]);
        botIndex = AddVertex(botVertices, botUVs, mainVertices[originalIdx], mainUVs[originalIdx]);

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

        Vector3 intersectionLocal = GetIntersectionPointLocal(
            idxA,
            idxB,
            ctx.MainVertices,
            ctx.Transform,
            ctx.Plane
        );

        Vector2 intersectionUV = GetIntersectionUV(
            idxA,
            idxB,
            ctx.MainVertices,
            ctx.MainUVs,
            ctx.Transform,
            ctx.Plane
        );

        topIndex = AddVertex(ctx.TopVertices, ctx.TopUVs, intersectionLocal, intersectionUV);
        botIndex = AddVertex(ctx.BotVertices, ctx.BotUVs, intersectionLocal, intersectionUV);

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

    static Vector2 GetIntersectionUV(int idxA, int idxB, Vector3[] localVerts, Vector2[] mainUVs, Transform objTransform, Plane worldPlane)
    {
        Vector3 worldA = objTransform.TransformPoint(localVerts[idxA]);
        Vector3 worldB = objTransform.TransformPoint(localVerts[idxB]);

        float distA = worldPlane.GetDistanceToPoint(worldA);
        float distB = worldPlane.GetDistanceToPoint(worldB);

        float t = -distA / (distB - distA);
        t = Mathf.Clamp01(t);

        return Vector2.Lerp(mainUVs[idxA], mainUVs[idxB], t);
    }
}
