using System.Collections.Generic;
using UnityEngine;

public enum OpenChainMode
{
    Ignore,
    Reject,
    CloseSmallGaps
}

public static class ContourExtractor 
{
    public static ContourExtractionResult ExtractContoursFromEdges(HashSet<EdgeKey> contourEdges, List<Vector3> vertices = null, OpenChainMode openChainMode = OpenChainMode.Ignore, float closeGapEpsilon = 0.001f)
    {
        ContourExtractionResult result = new ContourExtractionResult();

        if (contourEdges == null || contourEdges.Count == 0) return result;

        Dictionary<int, List<int>> adjacency = BuildAdjacency(contourEdges);

        foreach (var pair in adjacency)
        {
            int degree = pair.Value.Count;

            if (degree > 2)
            {
                result.BranchingVertices.Add(pair.Key);
                result.BranchingNeighbors[pair.Key] = new List<int>(pair.Value);
                result.Warnings.Add($"Branching vertex {pair.Key}: degree={degree}");
            }
        }

        HashSet<EdgeKey> unusedEdges = new HashSet<EdgeKey>(contourEdges);

        while (unusedEdges.Count > 0)
        {
            EdgeKey startEdge = GetFirstEdge(unusedEdges);
            List<int> path = TryBuildPathFromEdge(startEdge, adjacency, unusedEdges);

            if (path == null || path.Count < 2) continue;

            path = CleanPath(path);

            if (path.Count >= 3 && IsClosedPath(path, contourEdges))
            {
                if (path.Count > 1 && path[0] == path[path.Count - 1]) path.RemoveAt(path.Count - 1);
                result.ClosedLoops.Add(path);
            }
            else
            {
                HandleOpenChain(path, result, vertices, openChainMode, closeGapEpsilon);
            }
        }

        return result;
    }

    private static List<int> TryBuildPathFromEdge(EdgeKey startEdge, Dictionary<int, List<int>> adjacency, HashSet<EdgeKey> unusedEdges)
    {
        List<int> path = new List<int>();

        int start = startEdge.A;
        int previous = -1;
        int current = start;

        int safety = 0;
        int maxSafety = unusedEdges.Count + 100;

        while (safety++ < maxSafety)
        {
            path.Add(current);

            if (!adjacency.TryGetValue(current, out List<int> neighbors)) return path;

            int next = FindNextUnusedNeighbor(current, previous, neighbors, unusedEdges);

            if (next == -1) return path;

            EdgeKey edge = new EdgeKey(current, next);
            unusedEdges.Remove(edge);

            if (next == start) return path;

            previous = current;
            current = next;
        }

        return path;
    }

    private static bool IsClosedPath(List<int> path, HashSet<EdgeKey> originalEdges)
    {
        if (path == null || path.Count < 3) return false;

        int first = path[0];
        int last = path[path.Count - 1];

        if (first == last) return true;

        return originalEdges.Contains(new EdgeKey(first, last));
    }

    private static List<int> CleanPath(List<int> path)
    {
        List<int> cleaned = new List<int>();

        if (path == null) return cleaned;

        for (int i = 0; i < path.Count; i++)
        {
            int index = path[i];

            if (cleaned.Count == 0 || cleaned[cleaned.Count - 1] != index) cleaned.Add(index);
        }

        return cleaned;
    }

    private static Dictionary<int, List<int>> BuildAdjacency(HashSet<EdgeKey> edges)
    {
        Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();

        foreach (EdgeKey edge in edges)
        {
            if (!adjacency.ContainsKey(edge.A))
                adjacency[edge.A] = new List<int>();

            if (!adjacency.ContainsKey(edge.B))
                adjacency[edge.B] = new List<int>();

            if (!adjacency[edge.A].Contains(edge.B))
                adjacency[edge.A].Add(edge.B);

            if (!adjacency[edge.B].Contains(edge.A))
                adjacency[edge.B].Add(edge.A);
        }

        return adjacency;
    }

    private static EdgeKey GetFirstEdge(HashSet<EdgeKey> edges)
    {
        foreach (EdgeKey edge in edges)
            return edge;

        return default;
    }

    private static int FindNextUnusedNeighbor(int current, int previous, List<int> neighbors, HashSet<EdgeKey> unusedEdges)
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            int candidate = neighbors[i];

            if (candidate == previous)
                continue;

            EdgeKey edge = new EdgeKey(current, candidate);

            if (unusedEdges.Contains(edge))
                return candidate;
        }

        for (int i = 0; i < neighbors.Count; i++)
        {
            int candidate = neighbors[i];
            EdgeKey edge = new EdgeKey(current, candidate);

            if (unusedEdges.Contains(edge))
                return candidate;
        }

        return -1;
    }

    private static void HandleOpenChain(List<int> path, ContourExtractionResult result, List<Vector3> vertices, OpenChainMode mode, float closeGapEpsilon)
    {
        if (path == null || path.Count < 2) return;

        if (mode == OpenChainMode.CloseSmallGaps && TryCloseSmallGap(path, vertices, closeGapEpsilon))
        {
            result.ClosedLoops.Add(path);
            result.Warnings.Add($"Open chain repaired by closing small gap: vertices={path.Count}");
            return;
        }

        result.OpenChains.Add(path);
        result.Warnings.Add($"Open chain detected: vertices={path.Count}");

        if (mode == OpenChainMode.Reject) result.Warnings.Add("Open chain reject mode active: cap should be considered invalid");
    }

    private static bool TryCloseSmallGap(List<int> path, List<Vector3> vertices, float epsilon)
    {
        if (path == null || path.Count < 3) return false;
        if (vertices == null || vertices.Count == 0) return false;

        int first = path[0];
        int last = path[path.Count - 1];

        if (first < 0 || first >= vertices.Count) return false;
        if (last < 0 || last >= vertices.Count) return false;

        float distance = Vector3.Distance(vertices[first], vertices[last]);

        if (distance > epsilon) return false;

        if (path[0] == path[path.Count - 1]) path.RemoveAt(path.Count - 1);

        return true;
    }
}