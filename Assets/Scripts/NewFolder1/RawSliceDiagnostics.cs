using System.Collections.Generic;
using UnityEngine;

public static class RawSliceDiagnostics
{
    public static bool Enabled = true;

    public static void LogAfterTriangles(RawSliceBuildData top, RawSliceBuildData bottom, RawSliceContext ctx)
    {
        if (!Enabled) return;

        Debug.Log($"[Slice] after triangles: topV={top.Vertices.Count}, topT={top.Triangles.Count / 3}, topUV={top.UVs.Count}, topTypes={top.TriangleSurfaceTypes.Count}, topEdges={ctx.TopContourEdges.Count}, botV={bottom.Vertices.Count}, botT={bottom.Triangles.Count / 3}, botUV={bottom.UVs.Count}, botTypes={bottom.TriangleSurfaceTypes.Count}, botEdges={ctx.BottomContourEdges.Count}");
    }

    public static void LogAfterRemap(HashSet<EdgeKey> mergedTopEdges, HashSet<EdgeKey> mergedBottomEdges)
    {
        if (!Enabled) return;

        Debug.Log($"[Slice] after remap: topEdges={mergedTopEdges.Count}, botEdges={mergedBottomEdges.Count}");
    }

    public static void LogContours(string label, ContourExtractionResult contours)
    {
        if (!Enabled) return;

        Debug.Log($"[Contours {label}] closed={contours.ClosedLoops.Count}, open={contours.OpenChains.Count}, branch={contours.BranchingVertices.Count}, warnings={contours.Warnings.Count}");

        int count = Mathf.Min(5, contours.Warnings.Count);

        for (int i = 0; i < count; i++) Debug.Log($"[Contours {label} warning] {contours.Warnings[i]}");
    }

    public static void LogBeforeCaps(string label, RawSliceBuildData buildData, List<List<int>> loops)
    {
        if (!Enabled) return;

        Debug.Log($"[AddCaps {label}] loops={loops?.Count ?? 0}, beforeV={buildData.Vertices.Count}, beforeT={buildData.Triangles.Count / 3}, beforeUV={buildData.UVs.Count}, beforeTypes={buildData.TriangleSurfaceTypes.Count}");
    }

    public static void LogCapResult(string label, bool success, List<Vector3> capVertices, List<int> capTriangles, List<SurfaceType> capTriangleTypes)
    {
        if (!Enabled) return;

        Debug.Log($"[AddCaps {label}] success={success}, capV={capVertices.Count}, capT={capTriangles.Count / 3}, capTypes={capTriangleTypes.Count}");
    }

    public static void LogAfterCaps(string label, RawSliceBuildData buildData)
    {
        if (!Enabled) return;

        Debug.Log($"[AddCaps {label}] afterV={buildData.Vertices.Count}, afterT={buildData.Triangles.Count / 3}, afterUV={buildData.UVs.Count}, afterTypes={buildData.TriangleSurfaceTypes.Count}");
    }

    public static void LogAfterAllCaps(RawSliceBuildData top, RawSliceBuildData bottom)
    {
        if (!Enabled) return;

        Debug.Log($"[Slice] after caps: topV={top.Vertices.Count}, topT={top.Triangles.Count / 3}, topUV={top.UVs.Count}, topTypes={top.TriangleSurfaceTypes.Count}, botV={bottom.Vertices.Count}, botT={bottom.Triangles.Count / 3}, botUV={bottom.UVs.Count}, botTypes={bottom.TriangleSurfaceTypes.Count}");
    }

    public static void LogFinal(RawMeshData topMesh, RawMeshData bottomMesh)
    {
        if (!Enabled) return;

        Debug.Log($"[Final] topValid={topMesh.IsValid}, topV={topMesh.Vertices.Length}, topT={topMesh.Triangles.Length / 3}, topUV={topMesh.UVs.Length}, topTypes={topMesh.TriangleSurfaceTypes.Length}");
        Debug.Log($"[Final] botValid={bottomMesh.IsValid}, botV={bottomMesh.Vertices.Length}, botT={bottomMesh.Triangles.Length / 3}, botUV={bottomMesh.UVs.Length}, botTypes={bottomMesh.TriangleSurfaceTypes.Length}");
    }
}