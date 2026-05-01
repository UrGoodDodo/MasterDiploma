using System.Collections.Generic;
using UnityEngine;

public static class RawVoronoiCellBuilder
{
    public static bool TryBuildRawCell(RawMeshData sourceMesh, RawTransformData transformData, Vector3 seed, IReadOnlyList<Vector3> allSeeds, out RawMeshData cellMesh)
    {
        cellMesh = null;

        if (sourceMesh == null || !sourceMesh.IsValid)
            return false;

        RawMeshData current = sourceMesh.Clone();

        for (int i = 0; i < allSeeds.Count; i++)
        {
            Vector3 otherSeed = allSeeds[i];

            if ((otherSeed - seed).sqrMagnitude < 0.000001f)
                continue;

            Vector3 mid = (seed + otherSeed) * 0.5f;
            Vector3 normal = (otherSeed - seed).normalized;

            Plane bisector = new Plane(normal, mid);

            bool ok = RawMeshGeometrySlicer.TryClip(current, transformData, bisector, false, out RawMeshClipResult clipResult);

            if (!ok || clipResult == null || !clipResult.HasMesh)
                return false;

            current = clipResult.MeshData;
        }

        cellMesh = current;
        return cellMesh != null && cellMesh.IsValid;
    }

    public static bool TryBuildRawCellWithNeighbors(RawMeshData sourceMesh, RawTransformData transformData, Vector3 seedWorld, IReadOnlyList<Vector3> neighborSeedsWorld, out RawMeshData cellMesh)
    {
        cellMesh = null;

        if (sourceMesh == null || !sourceMesh.IsValid)
            return false;

        if (neighborSeedsWorld == null || neighborSeedsWorld.Count == 0)
            return false;

        RawMeshData current = sourceMesh.Clone();

        for (int i = 0; i < neighborSeedsWorld.Count; i++)
        {
            Vector3 otherSeed = neighborSeedsWorld[i];

            if ((seedWorld - otherSeed).sqrMagnitude < 0.000001f)
                continue;

            Vector3 mid = (seedWorld + otherSeed) * 0.5f;
            Vector3 normal = (otherSeed - seedWorld).normalized;

            Plane bisector = new Plane(normal, mid);

            bool keepPositiveSide =
                bisector.GetDistanceToPoint(seedWorld) > 0f;

            bool ok = RawMeshGeometrySlicer.TryClip(current, transformData, bisector, keepPositiveSide, out RawMeshClipResult clipResult);

            if (!ok || clipResult == null)
                return false;

            if (!clipResult.HasMesh)
            {
                cellMesh = null;
                return true;
            }

            current = clipResult.MeshData;
        }

        cellMesh = current;
        return cellMesh != null && cellMesh.IsValid;
    }
}