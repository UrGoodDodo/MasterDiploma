using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public static class VoronoiFractureGenerator
{
    public static List<MeshClipResult> GenerateFragments(Mesh sourceMesh, Transform sourceTransform, List<SurfaceType> sourceTriangleTypes, List<Vector3> seedsWorld)
    {
        List<MeshClipResult> fragments = new List<MeshClipResult>();

        if (sourceMesh == null || sourceTransform == null)
            return fragments;

        if (seedsWorld == null || seedsWorld.Count == 0)
            return fragments;

        foreach (Vector3 seed in seedsWorld)
        {
            bool success = VoronoiCellBuilder.TryBuildCell(sourceMesh, sourceTransform, sourceTriangleTypes, seed, seedsWorld, out MeshClipResult cellResult);

            if (!success)
                continue;

            if (cellResult == null || cellResult.IsEmpty)
                continue;

            fragments.Add(cellResult);
        }

        return fragments;
    }

    public static List<MeshClipResult> GenerateFragmentsParallel(Mesh sourceMesh, Transform sourceTransform, List<SurfaceType> sourceTriangleTypes, List<Vector3> seedsWorld)
    {
        List<MeshClipResult> fragments = new List<MeshClipResult>();

        if (sourceMesh == null || sourceTransform == null)
            return fragments;

        if (seedsWorld == null || seedsWorld.Count == 0)
            return fragments;

        RawMeshData rawSource = RawMeshConverter.FromMesh(sourceMesh, sourceTriangleTypes);

        RawTransformData rawTransform = new RawTransformData(sourceTransform);

        RawMeshData[] rawCells = new RawMeshData[seedsWorld.Count];

        Parallel.For(0, seedsWorld.Count, i =>
        {
            bool success = RawVoronoiCellBuilder.TryBuildRawCell(rawSource, rawTransform, seedsWorld[i], seedsWorld, out RawMeshData cell);

            if (success && cell != null && cell.IsValid)
                rawCells[i] = cell;
        });

        for (int i = 0; i < rawCells.Length; i++)
        {
            RawMeshData rawCell = rawCells[i];

            if (rawCell == null || !rawCell.IsValid)
                continue;

            Mesh mesh = RawMeshConverter.ToMesh(rawCell, $"Voronoi_Cell_{i}");

            if (mesh == null)
                continue;

            List<SurfaceType> triangleTypes = new List<SurfaceType>(rawCell.TriangleSurfaceTypes);

            fragments.Add(MeshClipResult.FromMesh(mesh, triangleTypes));
        }

        return fragments;
    }

    public static List<MeshClipResult> GenerateFragmentsWithNearestNeighbors(Mesh sourceMesh, Transform sourceTransform, List<SurfaceType> sourceTriangleTypes, List<Vector3> seedsWorld, int neighborCount)
    {
        List<MeshClipResult> fragments = new List<MeshClipResult>();

        if (sourceMesh == null || sourceTransform == null)
            return fragments;

        if (seedsWorld == null || seedsWorld.Count == 0)
            return fragments;

        neighborCount = Mathf.Clamp(neighborCount, 1, seedsWorld.Count - 1);

        foreach (Vector3 seed in seedsWorld)
        {
            List<Vector3> neighbors = FindNearestSeeds(seed, seedsWorld, neighborCount);

            bool success = VoronoiCellBuilder.TryBuildCellWithNeighbors(sourceMesh, sourceTransform, sourceTriangleTypes, seed, neighbors, out MeshClipResult cellResult);

            if (!success)
                continue;

            if (cellResult == null || cellResult.IsEmpty)
                continue;

            fragments.Add(cellResult);
        }

        return fragments;
    }

    public static List<MeshClipResult> GenerateFragmentsWithNearestNeighborsParallel(Mesh sourceMesh, Transform sourceTransform, List<SurfaceType> sourceTriangleTypes, List<Vector3> seedsWorld, int neighborCount)
    {
        List<MeshClipResult> fragments = new List<MeshClipResult>();

        if (sourceMesh == null || sourceTransform == null)
            return fragments;

        if (seedsWorld == null || seedsWorld.Count == 0)
            return fragments;

        if (seedsWorld.Count == 1)
        {
            fragments.Add(MeshClipResult.FromMesh(sourceMesh, new List<SurfaceType>(sourceTriangleTypes)));
            return fragments;
        }

        neighborCount = Mathf.Clamp(neighborCount, 1, seedsWorld.Count - 1);

        RawMeshData rawSource = RawMeshConverter.FromMesh(sourceMesh, sourceTriangleTypes);

        RawTransformData rawTransform = new RawTransformData(sourceTransform);

        RawMeshData[] rawCells = new RawMeshData[seedsWorld.Count];

        Parallel.For(0, seedsWorld.Count, i =>
        {
            Vector3 seed = seedsWorld[i];

            List<Vector3> neighbors = FindNearestSeeds(seed, seedsWorld, neighborCount);

            bool success = RawVoronoiCellBuilder.TryBuildRawCellWithNeighbors(rawSource, rawTransform, seed, neighbors, out RawMeshData cell);

            if (success && cell != null && cell.IsValid)
                rawCells[i] = cell;
        });

        for (int i = 0; i < rawCells.Length; i++)
        {
            RawMeshData rawCell = rawCells[i];

            if (rawCell == null || !rawCell.IsValid)
                continue;

            Mesh mesh = RawMeshConverter.ToMesh(rawCell, $"Voronoi_Cell_Nearest_{i}");

            if (mesh == null)
                continue;

            List<SurfaceType> triangleTypes = new List<SurfaceType>(rawCell.TriangleSurfaceTypes);

            fragments.Add(MeshClipResult.FromMesh(mesh, triangleTypes));
        }

        return fragments;
    }

    private static List<Vector3> FindNearestSeeds(Vector3 seed, List<Vector3> allSeeds, int count)
    {
        List<Vector3> result = new List<Vector3>(count);

        List<(Vector3 point, float sqrDistance)> distances = new List<(Vector3 point, float sqrDistance)>();

        for (int i = 0; i < allSeeds.Count; i++)
        {
            Vector3 other = allSeeds[i];

            if ((seed - other).sqrMagnitude < 0.000001f)
                continue;

            distances.Add((other, (seed - other).sqrMagnitude));
        }

        distances.Sort((a, b) => a.sqrDistance.CompareTo(b.sqrDistance));

        int take = Mathf.Min(count, distances.Count);

        for (int i = 0; i < take; i++)
            result.Add(distances[i].point);

        return result;
    }
}