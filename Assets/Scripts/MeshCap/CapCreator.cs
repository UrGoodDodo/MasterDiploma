using System.Collections.Generic;
using UnityEngine;

public static class CapCreator
{
    public enum CapType
    {
        Fan,
        EarClipping
    }

    public static void TriangulateCapByType(CapType capType , List<int> loop, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, Vector3 normal)
    {
        ICapStrategy capStrategy;

        switch (capType)
        {
            case CapType.Fan:
                capStrategy = new FanCap();
                break;
            case CapType.EarClipping:
                capStrategy = new EarCap();
                break;
            default:
                capStrategy = new FanCap();
                break;
        }

        capStrategy.TriangulateCap(loop, mainVertices, capVertices, outTriangles, normal);
    }

    //-----------------------------------------------------------------------------------------


    public static List<List<int>> ExtractLoopsFromEdges(HashSet<EdgeKey> contourEdges)
    {
        var adjacency = new Dictionary<int, List<int>>();

        foreach (var edge in contourEdges)
        {
            if (!adjacency.ContainsKey(edge.A))
                adjacency[edge.A] = new List<int>();

            if (!adjacency.ContainsKey(edge.B))
                adjacency[edge.B] = new List<int>();

            adjacency[edge.A].Add(edge.B);
            adjacency[edge.B].Add(edge.A);
        }

        var loops = new List<List<int>>();
        var visitedVertices = new HashSet<int>();

        foreach (var start in adjacency.Keys)
        {
            if (visitedVertices.Contains(start))
                continue;

            if (adjacency[start].Count < 2)
                continue;

            List<int> loop = BuildSingleLoop(start, adjacency);

            if (loop != null && loop.Count >= 3)
            {
                foreach (int v in loop)
                    visitedVertices.Add(v);

                loops.Add(loop);
            }
        }

        return loops;
    }

    private static List<int> BuildSingleLoop(int start, Dictionary<int, List<int>> adjacency)
    {
        List<int> loop = new List<int>();

        int prev = -1;
        int current = start;

        int safety = 0;

        while (safety++ < 10000)
        {
            loop.Add(current);

            List<int> neighbors = adjacency[current];

            int next = -1;

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i] != prev)
                {
                    next = neighbors[i];
                    break;
                }
            }

            if (next == -1)
                return null;

            if (next == start)
            {
                return loop;
            }

            prev = current;
            current = next;

            if (loop.Contains(current))
            {
                return null;
            }
        }
        return null;
    }
}
