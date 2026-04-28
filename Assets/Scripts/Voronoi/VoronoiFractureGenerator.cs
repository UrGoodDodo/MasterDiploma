using System.Collections.Generic;
using UnityEngine;

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