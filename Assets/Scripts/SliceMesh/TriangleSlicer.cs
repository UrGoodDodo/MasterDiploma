using System;
using System.Collections.Generic;
using UnityEngine;

public static class TriangleSlicer
{

    public static void ClassifyTriangles(GameObject gameObject, int[] classified_vertices, Plane plane, out List<Vector3> topVertices, out List<int> topTriangles, out List<Vector3> botVertices, out List<int> botTriangles, out HashSet<EdgeKey> topContourEdges, out HashSet<EdgeKey> botContourEdges)
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] mainVertices = mesh.vertices;
        int[] mainTriangles = mesh.triangles;

        topVertices = new List<Vector3>();
        topTriangles = new List<int>();
        botVertices = new List<Vector3>();
        botTriangles = new List<int>();

        Dictionary<EdgeKey, (int topIndex, int bottomIndex)> edgeCache = new Dictionary<EdgeKey, (int, int)>();

        topContourEdges = new HashSet<EdgeKey>();
        botContourEdges = new HashSet<EdgeKey>();

        var ctx = new SliceContext
        {
            Transform = gameObject.transform,
            Plane = plane,
            MainVertices = mainVertices,

            TopVertices = topVertices,
            TopTriangles = topTriangles,

            BotVertices = botVertices,
            BotTriangles = botTriangles,

            TopContourEdges = topContourEdges,
            BotContourEdges = botContourEdges,

            EdgeCache = new Dictionary<EdgeKey, (int top, int bottom)>(),
            OnPlaneVertexCache = new Dictionary<int, (int top, int bottom)>()
        };

        for (int i = 0; i < mainTriangles.Length; i += 3)
        {
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
                TringleSlicerHelper.ProcessAllTop(triangle_ind_1, triangle_ind_2, triangle_ind_3, origNormal, ref ctx);
            }
            else if (belowCount == 3)
            {
                TringleSlicerHelper.ProcessAllBottom(triangle_ind_1, triangle_ind_2, triangle_ind_3, origNormal, ref ctx);
            }
            else if (aboveCount == 2 && onCount == 1)
            {
                TringleSlicerHelper.ProcessTwoAboveOneOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, ref ctx);
            }
            else if (belowCount == 2 && onCount == 1)
            {
                TringleSlicerHelper.ProcessTwoBelowOneOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, ref ctx);
            }
            else if (aboveCount == 1 && onCount == 2)
            {
                TringleSlicerHelper.ProcessOneAboveTwoOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, ref ctx);
            }
            else if (belowCount == 1 && onCount == 2)
            {
                TringleSlicerHelper.ProcessOneBelowTwoOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, ref ctx);
            }
            else if (aboveCount == 2 && belowCount == 1)
            {
                TringleSlicerHelper.ProcessTwoAboveOneBelow(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, ref ctx);
            }
            else if (aboveCount == 1 && belowCount == 2)
            {
                TringleSlicerHelper.ProcessOneAboveTwoBelow(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, ref ctx);
            }
            else if (aboveCount == 1 && belowCount == 1 && onCount == 1)
            {
                TringleSlicerHelper.ProcessOneAboveOneBelowOneOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, vert_sign_1, vert_sign_2, vert_sign_3, origNormal, ref ctx);
            }
            else if (onCount == 3)
            {
                TringleSlicerHelper.ProcessAllOnPlane(triangle_ind_1, triangle_ind_2, triangle_ind_3, origNormal, ref ctx);
            }
        }

        topVertices = ctx.TopVertices;
        topTriangles = ctx.TopTriangles;
        botVertices = ctx.BotVertices;
        botTriangles = ctx.BotTriangles;
        topContourEdges = ctx.TopContourEdges;
        botContourEdges = ctx.BotContourEdges;
    }
    public static void MergeDuplicateVertices( ref List<Vector3> vertices, ref HashSet<EdgeKey> contourEdges,float epsilon = 0.001f)
    {
        var positionToIndex = new Dictionary<Vector3, int>();
        var oldToNew = new Dictionary<int, int>();

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 pos = vertices[i];
            bool found = false;
            foreach (var kvp in positionToIndex)
            {
                if (Vector3.Distance(kvp.Key, pos) < epsilon)
                {
                    oldToNew[i] = kvp.Value;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                positionToIndex[pos] = i;
                oldToNew[i] = i;
            }
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
        contourEdges = newEdges;
    }
}
