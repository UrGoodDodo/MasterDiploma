using System.Collections.Generic;
using UnityEngine;

public static class VoronoiSeedGenerator
{
    public static List<Vector3> GenerateRandomSeedsInMesh(Mesh mesh, Transform sourceTransform, int count, int maxAttemptsMultiplier = 50)
    {
        List<Vector3> seeds = new List<Vector3>(Mathf.Max(0, count));

        if (mesh == null || sourceTransform == null || count <= 0)
            return seeds;

        Bounds localBounds = mesh.bounds;

        int maxAttempts = count * maxAttemptsMultiplier;
        int attempts = 0;

        while (seeds.Count < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 localPoint = new Vector3(
                Random.Range(localBounds.min.x, localBounds.max.x),
                Random.Range(localBounds.min.y, localBounds.max.y),
                Random.Range(localBounds.min.z, localBounds.max.z)
            );

            if (!MeshVolumeUtility.IsPointInsideMeshLocal(mesh, localPoint))
                continue;

            seeds.Add(sourceTransform.TransformPoint(localPoint));
        }

        return seeds;
    }

    public static List<Vector3> GenerateImpactSeedsInMesh(Mesh mesh, Transform sourceTransform, Vector3 impactWorldPoint, int count, float radius, int maxAttemptsMultiplier = 80)
    {
        List<Vector3> seeds = new List<Vector3>(Mathf.Max(0, count));

        if (mesh == null || sourceTransform == null || count <= 0)
            return seeds;

        Bounds localBounds = mesh.bounds;
        Vector3 impactLocal = sourceTransform.InverseTransformPoint(impactWorldPoint);

        int maxAttempts = count * maxAttemptsMultiplier;
        int attempts = 0;

        while (seeds.Count < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 offsetWorld = Random.insideUnitSphere * radius;
            Vector3 offsetLocal = sourceTransform.InverseTransformVector(offsetWorld);

            Vector3 localPoint = impactLocal + offsetLocal;

            if (!localBounds.Contains(localPoint))
                continue;

            if (!MeshVolumeUtility.IsPointInsideMeshLocal(mesh, localPoint))
                continue;

            seeds.Add(sourceTransform.TransformPoint(localPoint));
        }

        return seeds;
    }

    public static List<Vector3> GenerateRandomSeedsInBounds(Bounds localBounds, Transform sourceTransform, int count)
    {
        List<Vector3> seeds = new List<Vector3>(Mathf.Max(0, count));

        if (sourceTransform == null || count <= 0)
            return seeds;

        for (int i = 0; i < count; i++)
        {
            Vector3 localPoint = new Vector3(
                Random.Range(localBounds.min.x, localBounds.max.x),
                Random.Range(localBounds.min.y, localBounds.max.y),
                Random.Range(localBounds.min.z, localBounds.max.z)
            );

            seeds.Add(sourceTransform.TransformPoint(localPoint));
        }

        return seeds;
    }

    public static List<Vector3> GenerateImpactSeeds(Bounds localBounds, Transform sourceTransform, Vector3 impactWorldPoint, int count, float radius)
    {
        List<Vector3> seeds = new List<Vector3>(Mathf.Max(0, count));

        if (sourceTransform == null || count <= 0)
            return seeds;

        Vector3 impactLocal = sourceTransform.InverseTransformPoint(impactWorldPoint);

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Random.insideUnitSphere * radius;
            Vector3 localPoint = impactLocal + sourceTransform.InverseTransformVector(offset);

            localPoint.x = Mathf.Clamp(localPoint.x, localBounds.min.x, localBounds.max.x);
            localPoint.y = Mathf.Clamp(localPoint.y, localBounds.min.y, localBounds.max.y);
            localPoint.z = Mathf.Clamp(localPoint.z, localBounds.min.z, localBounds.max.z);

            seeds.Add(sourceTransform.TransformPoint(localPoint));
        }

        return seeds;
    }
}