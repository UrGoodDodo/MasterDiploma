using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ContourEdgeDebugger
{
    public static void ValidateContourEdges(string label, HashSet<EdgeKey> contourEdges, List<Vector3> vertices)
    {
        if (contourEdges == null || contourEdges.Count == 0)
        {
            Debug.LogWarning($"[{label}] ContourEdges is empty.");
            return;
        }

        var adjacency = new Dictionary<int, HashSet<int>>();

        // Собираем граф смежности
        foreach (var edge in contourEdges)
        {
            if (edge.A == edge.B)
            {
                Debug.LogWarning($"[{label}] Degenerate edge detected: {edge.A} -> {edge.B}");
                continue;
            }

            if (!adjacency.ContainsKey(edge.A))
                adjacency[edge.A] = new HashSet<int>();

            if (!adjacency.ContainsKey(edge.B))
                adjacency[edge.B] = new HashSet<int>();

            adjacency[edge.A].Add(edge.B);
            adjacency[edge.B].Add(edge.A);
        }

        int degree2 = 0;
        int degree1 = 0;
        int degree3Plus = 0;
        int degree0 = 0;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"===== CONTOUR VALIDATION [{label}] =====");
        sb.AppendLine($"Edges count: {contourEdges.Count}");
        sb.AppendLine($"Unique contour vertices: {adjacency.Count}");

        foreach (var kvp in adjacency)
        {
            int vertexIndex = kvp.Key;
            int degree = kvp.Value.Count;

            if (degree == 2) degree2++;
            else if (degree == 1) degree1++;
            else if (degree == 0) degree0++;
            else if (degree >= 3) degree3Plus++;

            if (degree != 2)
            {
                string posInfo = "";
                if (vertices != null && vertexIndex >= 0 && vertexIndex < vertices.Count)
                {
                    posInfo = $" pos={vertices[vertexIndex]}";
                }

                sb.AppendLine(
                    $"[BAD DEGREE] vertex={vertexIndex}, degree={degree},{posInfo} neighbors=[{string.Join(", ", kvp.Value)}]"
                );
            }
        }

        int components = CountConnectedComponents(adjacency);

        sb.AppendLine("----- DEGREE SUMMARY -----");
        sb.AppendLine($"degree == 2 : {degree2}");
        sb.AppendLine($"degree == 1 : {degree1}");
        sb.AppendLine($"degree >= 3 : {degree3Plus}");
        sb.AppendLine($"degree == 0 : {degree0}");

        sb.AppendLine("----- GRAPH SUMMARY -----");
        sb.AppendLine($"Connected components: {components}");

        bool validForLoopPipeline = (degree1 == 0 && degree3Plus == 0 && degree0 == 0);

        if (validForLoopPipeline)
        {
            sb.AppendLine("RESULT: contourEdges LOOKS VALID for loop extraction + cap triangulation.");
        }
        else
        {
            sb.AppendLine("RESULT: contourEdges is NOT valid for simple loop extraction. Fix contour generation first.");
        }

        Debug.Log(sb.ToString());
    }

    private static int CountConnectedComponents(Dictionary<int, HashSet<int>> adjacency)
    {
        var visited = new HashSet<int>();
        int count = 0;

        foreach (var start in adjacency.Keys)
        {
            if (visited.Contains(start))
                continue;

            count++;
            FloodFill(start, adjacency, visited);
        }

        return count;
    }

    private static void FloodFill(int start, Dictionary<int, HashSet<int>> adjacency, HashSet<int> visited)
    {
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            foreach (int neighbor in adjacency[current])
            {
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
}