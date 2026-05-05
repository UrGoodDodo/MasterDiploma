using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public static class RawVoronoiDiagnostics
{
    public static bool Enabled = true;
    public static bool LogOnlyFailures = true;

    private static readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

    public static void Clear()
    {
        while (messages.TryDequeue(out _)) { }
    }

    public static void Flush()
    {
        if (!Enabled) return;

        while (messages.TryDequeue(out string message)) Debug.Log(message);
    }

    public static void LogCellStart(int cellIndex, Vector3 seed, int clipCount)
    {
        if (!Enabled) return;

        messages.Enqueue($"[Cell {cellIndex}] START seed={seed}, clipCount={clipCount}");
    }

    public static void LogCellEnd(int cellIndex, RawMeshData cellMesh)
    {
        if (!Enabled) return;

        bool valid = cellMesh != null && cellMesh.IsValid;

        if (LogOnlyFailures && valid) return;

        messages.Enqueue($"[Cell {cellIndex}] END valid={valid}, V={cellMesh?.Vertices?.Length ?? 0}, T={(cellMesh?.Triangles?.Length ?? 0) / 3}, UV={cellMesh?.UVs?.Length ?? 0}, types={cellMesh?.TriangleSurfaceTypes?.Length ?? 0}");
    }

    public static void LogClipStart(int cellIndex, int clipIndex, RawMeshData current, Plane plane, bool keepPositiveSide)
    {
        if (!Enabled) return;

        messages.Enqueue($"[Cell {cellIndex} Clip {clipIndex}] START currentValid={current != null && current.IsValid}, V={current?.Vertices?.Length ?? 0}, T={(current?.Triangles?.Length ?? 0) / 3}, keepPositive={keepPositiveSide}, planeN={plane.normal}, planeD={plane.distance}");
    }

    public static void LogClipEnd(int cellIndex, int clipIndex, bool ok, RawMeshClipResult clipResult)
    {
        if (!Enabled) return;

        bool hasMesh = clipResult != null && clipResult.HasMesh;
        RawMeshData mesh = clipResult?.MeshData;

        if (LogOnlyFailures && ok && hasMesh) return;

        messages.Enqueue($"[Cell {cellIndex} Clip {clipIndex}] END ok={ok}, hasMesh={hasMesh}, empty={clipResult?.IsEmpty}, valid={mesh != null && mesh.IsValid}, V={mesh?.Vertices?.Length ?? 0}, T={(mesh?.Triangles?.Length ?? 0) / 3}, UV={mesh?.UVs?.Length ?? 0}, types={mesh?.TriangleSurfaceTypes?.Length ?? 0}");
    }

    public static void LogAfterTriangles(int cellIndex, int clipIndex, RawSliceBuildData top, RawSliceBuildData bottom, RawSliceContext ctx)
    {
        if (!Enabled) return;

        messages.Enqueue($"[Cell {cellIndex} Clip {clipIndex}] after triangles: topV={top.Vertices.Count}, topT={top.Triangles.Count / 3}, topUV={top.UVs.Count}, topTypes={top.TriangleSurfaceTypes.Count}, topEdges={ctx.TopContourEdges.Count}, botV={bottom.Vertices.Count}, botT={bottom.Triangles.Count / 3}, botUV={bottom.UVs.Count}, botTypes={bottom.TriangleSurfaceTypes.Count}, botEdges={ctx.BottomContourEdges.Count}");
    }

    public static void LogContours(int cellIndex, int clipIndex, string side, ContourExtractionResult contours)
    {
        if (!Enabled) return;

        messages.Enqueue($"[Cell {cellIndex} Clip {clipIndex} {side}] contours: closed={contours.ClosedLoops.Count}, open={contours.OpenChains.Count}, branch={contours.BranchingVertices.Count}, warnings={contours.Warnings.Count}");

        for (int i = 0; i < Mathf.Min(3, contours.Warnings.Count); i++) messages.Enqueue($"[Cell {cellIndex} Clip {clipIndex} {side}] warning: {contours.Warnings[i]}");
    }

    public static void LogCapResult(int cellIndex, int clipIndex, string side, int loopCount, bool success, List<Vector3> capVertices, List<int> capTriangles, List<SurfaceType> capTriangleTypes)
    {
        if (!Enabled) return;

        messages.Enqueue($"[Cell {cellIndex} Clip {clipIndex} {side}] cap: loops={loopCount}, success={success}, capV={capVertices.Count}, capT={capTriangles.Count / 3}, capTypes={capTriangleTypes.Count}");
    }

    public static void LogFinalSlice(int cellIndex, int clipIndex, RawMeshData topMesh, RawMeshData bottomMesh)
    {
        if (!Enabled) return;

        messages.Enqueue($"[Cell {cellIndex} Clip {clipIndex}] final slice: topValid={topMesh.IsValid}, topV={topMesh.Vertices.Length}, topT={topMesh.Triangles.Length / 3}, topUV={topMesh.UVs.Length}, topTypes={topMesh.TriangleSurfaceTypes.Length}, botValid={bottomMesh.IsValid}, botV={bottomMesh.Vertices.Length}, botT={bottomMesh.Triangles.Length / 3}, botUV={bottomMesh.UVs.Length}, botTypes={bottomMesh.TriangleSurfaceTypes.Length}");
    }
}