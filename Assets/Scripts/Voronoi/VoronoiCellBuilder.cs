using System.Collections.Generic;
using UnityEngine;

public static class VoronoiCellBuilder
{
    public static bool TryBuildCell(Mesh sourceMesh, Transform sourceTransform, List<SurfaceType> sourceTriangleTypes, Vector3 seedWorld, List<Vector3> allSeedsWorld, out MeshClipResult cellResult)
    {
        cellResult = null;

        if (sourceMesh == null || sourceTransform == null)
            return false;

        if (allSeedsWorld == null || allSeedsWorld.Count == 0)
            return false;

        Mesh currentMesh = sourceMesh;
        List<SurfaceType> currentTypes = new List<SurfaceType>(sourceTriangleTypes);

        for (int i = 0; i < allSeedsWorld.Count; i++)
        {
            Vector3 otherSeed = allSeedsWorld[i];

            if (IsSamePoint(seedWorld, otherSeed))
                continue;

            Plane bisector = BuildBisectorPlane(seedWorld, otherSeed);

            bool keepPositiveSide = bisector.GetDistanceToPoint(seedWorld) > 0f;

            if (!MeshGeometrySlicer.TryClip(currentMesh, sourceTransform, bisector, keepPositiveSide, currentTypes,out MeshClipResult clipResult))
            {
                cellResult = MeshClipResult.Empty();
                return false;
            }

            if (clipResult.IsEmpty)
            {
                cellResult = MeshClipResult.Empty();
                return true;
            }

            currentMesh = clipResult.Mesh;
            currentTypes = clipResult.TriangleTypes;
        }

        cellResult = MeshClipResult.FromMesh(currentMesh, currentTypes);
        return true;
    }

    public static bool TryBuildCellWithNeighbors(Mesh sourceMesh, Transform sourceTransform, List<SurfaceType> sourceTriangleTypes, Vector3 seedWorld, List<Vector3> neighborSeedsWorld, out MeshClipResult cellResult)
    {
        cellResult = null;

        if (sourceMesh == null || sourceTransform == null)
            return false;

        if (neighborSeedsWorld == null || neighborSeedsWorld.Count == 0)
            return false;

        Mesh currentMesh = sourceMesh;
        List<SurfaceType> currentTypes = new List<SurfaceType>(sourceTriangleTypes);

        for (int i = 0; i < neighborSeedsWorld.Count; i++)
        {
            Vector3 otherSeed = neighborSeedsWorld[i];

            if (IsSamePoint(seedWorld, otherSeed))
                continue;

            Plane bisector = BuildBisectorPlane(seedWorld, otherSeed);
            bool keepPositiveSide = bisector.GetDistanceToPoint(seedWorld) > 0f;

            if (!MeshGeometrySlicer.TryClip(currentMesh, sourceTransform, bisector, keepPositiveSide, currentTypes, out MeshClipResult clipResult))
            {
                cellResult = MeshClipResult.Empty();
                return false;
            }

            if (clipResult.IsEmpty)
            {
                cellResult = MeshClipResult.Empty();
                return true;
            }

            currentMesh = clipResult.Mesh;
            currentTypes = clipResult.TriangleTypes;
        }

        cellResult = MeshClipResult.FromMesh(currentMesh, currentTypes);
        return true;
    }

    private static Plane BuildBisectorPlane(Vector3 a, Vector3 b)
    {
        Vector3 mid = (a + b) * 0.5f;
        Vector3 normal = (b - a).normalized;

        return new Plane(normal, mid);
    }

    private static bool IsSamePoint(Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude < 0.000001f;
    }
}