using System;
using System.Collections.Generic;
using UnityEngine;

public static class TriangleSlicer
{

    public static void ClassifyTriangles(GameObject gameObject, int[] classified_vertices, Plane plane, List<SurfaceType> sourceTriangleTypes, out List<Vector3> topVertices, out List<Vector2> topUVs, out List<int> topTriangles, out List<SurfaceType> topTriangleTypes, out List<Vector3> botVertices, out List<Vector2> botUVs, out List<int> botTriangles, out List<SurfaceType> botTriangleTypes, out HashSet<EdgeKey> topContourEdges, out HashSet<EdgeKey> botContourEdges)
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] mainVertices = mesh.vertices;
        int[] mainTriangles = mesh.triangles;
        
        Vector2[] mainUVs = mesh.uv;
        if (mainUVs == null || mainUVs.Length != mainVertices.Length)
        {
            mainUVs = new Vector2[mainVertices.Length];
        }

        topVertices = new List<Vector3>();
        topUVs = new List<Vector2>();
        topTriangles = new List<int>();
        topTriangleTypes = new List<SurfaceType>();
        
        botVertices = new List<Vector3>();
        botUVs = new List<Vector2>();
        botTriangles = new List<int>();
        botTriangleTypes = new List<SurfaceType>();

        Dictionary<EdgeKey, (int topIndex, int bottomIndex)> edgeCache = new Dictionary<EdgeKey, (int, int)>();

        topContourEdges = new HashSet<EdgeKey>();
        botContourEdges = new HashSet<EdgeKey>();

        var ctx = new SliceContext
        {
            Transform = gameObject.transform,
            Plane = plane,
            MainVertices = mainVertices,
            MainUVs = mainUVs,

            TopVertices = topVertices,
            TopUVs = topUVs,
            TopTriangles = topTriangles,
            TopTriangleTypes = topTriangleTypes,

            BotVertices = botVertices,
            BotUVs = botUVs,
            BotTriangles = botTriangles,
            BotTriangleTypes = botTriangleTypes,

            TopContourEdges = topContourEdges,
            BotContourEdges = botContourEdges,

            EdgeCache = new Dictionary<EdgeKey, (int top, int bottom)>(),
            OnPlaneVertexCache = new Dictionary<int, (int top, int bottom)>()
        };

        for (int i = 0; i < mainTriangles.Length; i += 3)
        {

            int triangleIndex = i / 3;

            SurfaceType surfaceType = SurfaceType.Main;
            if (sourceTriangleTypes != null && triangleIndex < sourceTriangleTypes.Count)
            {
                surfaceType = sourceTriangleTypes[triangleIndex];
            }

            int triangle_ind_1 = mainTriangles[i];
            int triangle_ind_2 = mainTriangles[i + 1];
            int triangle_ind_3 = mainTriangles[i + 2];

            int vert_sign_1 = classified_vertices[triangle_ind_1];
            int vert_sign_2 = classified_vertices[triangle_ind_2];
            int vert_sign_3 = classified_vertices[triangle_ind_3];

            int aboveCount = (vert_sign_1 > 0 ? 1 : 0) + (vert_sign_2 > 0 ? 1 : 0) + (vert_sign_3 > 0 ? 1 : 0);
            int belowCount = (vert_sign_1 < 0 ? 1 : 0) + (vert_sign_2 < 0 ? 1 : 0) + (vert_sign_3 < 0 ? 1 : 0);
            int onCount = (vert_sign_1 == 0 ? 1 : 0) + (vert_sign_2 == 0 ? 1 : 0) + (vert_sign_3 == 0 ? 1 : 0);

            Vector3 origNormal = Vector3.Cross(mainVertices[triangle_ind_2] - mainVertices[triangle_ind_1], mainVertices[triangle_ind_3] - mainVertices[triangle_ind_1]).normalized;


            if (aboveCount == 3)
            {
                TringleSlicerHelper.ProcessAllTop(triangle_ind_1, triangle_ind_2, triangle_ind_3, origNormal, surfaceType, ref ctx);
            }
            else if (belowCount == 3)
            {
                TringleSlicerHelper.ProcessAllBottom(triangle_ind_1, triangle_ind_2, triangle_ind_3, origNormal, surfaceType, ref ctx);
            }
            else if (aboveCount == 2 && onCount == 1)
            {
                TringleSlicerHelper.ProcessTwoAboveOneOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, surfaceType, ref ctx);
            }
            else if (belowCount == 2 && onCount == 1)
            {
                TringleSlicerHelper.ProcessTwoBelowOneOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, surfaceType, ref ctx);
            }
            else if (aboveCount == 1 && onCount == 2)
            {
                TringleSlicerHelper.ProcessOneAboveTwoOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, surfaceType, ref ctx);
            }
            else if (belowCount == 1 && onCount == 2)
            {
                TringleSlicerHelper.ProcessOneBelowTwoOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, surfaceType, ref ctx);
            }
            else if (aboveCount == 2 && belowCount == 1)
            {
                TringleSlicerHelper.ProcessTwoAboveOneBelow(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, surfaceType, ref ctx);
            }
            else if (aboveCount == 1 && belowCount == 2)
            {
                TringleSlicerHelper.ProcessOneAboveTwoBelow(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, surfaceType, ref ctx);
            }
            else if (aboveCount == 1 && belowCount == 1 && onCount == 1)
            {
                TringleSlicerHelper.ProcessOneAboveOneBelowOneOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, surfaceType, ref ctx);
            }
            else if (onCount == 3)
            {
                TringleSlicerHelper.ProcessAllOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, origNormal, surfaceType, ref ctx);
            }
        }

        topVertices = ctx.TopVertices;
        topUVs = ctx.TopUVs;
        topTriangles = ctx.TopTriangles;
        topTriangleTypes = ctx.TopTriangleTypes;

        botVertices = ctx.BotVertices;
        botUVs = ctx.BotUVs;
        botTriangles = ctx.BotTriangles;
        botTriangleTypes = ctx.BotTriangleTypes;

        topContourEdges = ctx.TopContourEdges;
        botContourEdges = ctx.BotContourEdges;
    }

    public static void MergeDuplicateVertices(ref List<Vector3> vertices, ref List<Vector2> uvs, ref List<int> triangles, ref HashSet<EdgeKey> contourEdges, float epsilon = 0.001f)
    {
        var newVertices = new List<Vector3>();
        var newUVs = new List<Vector2>();
        var oldToNew = new int[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 pos = vertices[i];
            Vector2 uv = (uvs != null && i < uvs.Count) ? uvs[i] : Vector2.zero;

            int foundIndex = -1;

            for (int j = 0; j < newVertices.Count; j++)
            {
                if (Vector3.Distance(newVertices[j], pos) < epsilon)
                {
                    foundIndex = j;
                    break;
                }
            }

            if (foundIndex >= 0)
            {
                oldToNew[i] = foundIndex;
            }
            else
            {
                int newIndex = newVertices.Count;
                newVertices.Add(pos);
                newUVs.Add(uv);
                oldToNew[i] = newIndex;
            }
        }

        for (int i = 0; i < triangles.Count; i++)
        {
            triangles[i] = oldToNew[triangles[i]];
        }

        var newEdges = new HashSet<EdgeKey>();
        foreach (var edge in contourEdges)
        {
            int newA = oldToNew[edge.A];
            int newB = oldToNew[edge.B];

            if (newA != newB)
            {
                newEdges.Add(new EdgeKey(newA, newB));
            }
        }

        vertices = newVertices;
        uvs = newUVs;
        contourEdges = newEdges;
    }

    public static HashSet<EdgeKey> RemapContourEdgesByPosition( List<Vector3> vertices, HashSet<EdgeKey> contourEdges, float epsilon = 0.001f)
    {
        var oldToRepresentative = new int[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            oldToRepresentative[i] = i;

            for (int j = 0; j < i; j++)
            {
                if (Vector3.Distance(vertices[i], vertices[j]) < epsilon)
                {
                    oldToRepresentative[i] = oldToRepresentative[j];
                    break;
                }
            }
        }

        var remappedEdges = new HashSet<EdgeKey>();

        foreach (var edge in contourEdges)
        {
            int a = oldToRepresentative[edge.A];
            int b = oldToRepresentative[edge.B];

            if (a != b)
            {
                remappedEdges.Add(new EdgeKey(a, b));
            }
        }

        return remappedEdges;
    }
}
