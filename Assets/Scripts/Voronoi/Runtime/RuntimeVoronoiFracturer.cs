using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sliceable))]
[RequireComponent(typeof(MeshFilter))]
public class RuntimeVoronoiFracturer : MonoBehaviour
{
    [Header("Seeds")]
    public int seedCount = 12;
    public bool useImpactSeeds = true;
    public float impactSeedRadius = 1f;

    [Header("Neighbors")]
    public bool useNearestNeighbors = true;
    public int neighborCount = 8;

    [Header("Phys")]
    public bool addPhysics = true;
    public float explosionForce = 4f;
    public float explosionRadius = 2f;
    public float upwardsModifier = 0.1f;

    [Header("Orig")]
    public bool disableOriginalAfterFracture = true;

    private Sliceable sliceable;
    private MeshFilter meshFilter;

    [ContextMenu("Test Runtime Voronoi Fracture")]
    private void TestRuntimeFracture()
    {
        Fracture(transform.position);
    }

    private void Awake()
    {
        sliceable = GetComponent<Sliceable>();
        meshFilter = GetComponent<MeshFilter>();
    }

    public void Fracture(Vector3 impactWorldPoint)
    {
        if (sliceable == null || meshFilter == null || meshFilter.sharedMesh == null)
            return;

        if (!sliceable.canBeSliced)
            return;

        sliceable.EnsureTriangleTypesInitialized();

        Mesh sourceMesh = meshFilter.sharedMesh;
        Bounds bounds = sourceMesh.bounds;

        List<Vector3> seeds = useImpactSeeds ? VoronoiSeedGenerator.GenerateImpactSeedsInMesh(sourceMesh, transform, impactWorldPoint, seedCount, impactSeedRadius) : VoronoiSeedGenerator.GenerateRandomSeedsInMesh(sourceMesh, transform, seedCount);

        List<MeshClipResult> fragments = useNearestNeighbors ? VoronoiFractureGenerator.GenerateFragmentsWithNearestNeighbors(sourceMesh, transform, sliceable.triangleSurfaceTypes, seeds, neighborCount) : VoronoiFractureGenerator.GenerateFragments(sourceMesh, transform, sliceable.triangleSurfaceTypes, seeds);

        if (fragments == null || fragments.Count == 0)
            return;

        for (int i = 0; i < fragments.Count; i++)
        {
            CreateRuntimeFragment(fragments[i], i, impactWorldPoint);
        }

        if (disableOriginalAfterFracture)
            gameObject.SetActive(false);
    }

    private void CreateRuntimeFragment(MeshClipResult fragment, int index, Vector3 impactWorldPoint)
    {
        GameObject obj = MeshNObjCreator.CreateNewSubMeshObj(gameObject, fragment.Mesh, fragment.TriangleTypes, null, sliceable.capMaterial, "NewFragment " + index, addPhysics);

        Sliceable fragmentSliceable = obj.GetComponent<Sliceable>();
        if (fragmentSliceable == null)
            fragmentSliceable = obj.AddComponent<Sliceable>();

        fragmentSliceable.canBeSliced = sliceable.canBeSliced;
        fragmentSliceable.capMaterial = sliceable.capMaterial;
        fragmentSliceable.triangleSurfaceTypes = new List<SurfaceType>(fragment.TriangleTypes);

        if (!addPhysics)
            return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
            rb = obj.AddComponent<Rigidbody>();

        rb.AddExplosionForce(explosionForce, impactWorldPoint, explosionRadius, upwardsModifier, ForceMode.Impulse);
    }
}