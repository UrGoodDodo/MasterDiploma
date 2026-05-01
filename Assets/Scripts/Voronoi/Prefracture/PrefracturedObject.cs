using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sliceable))]
[RequireComponent(typeof(MeshFilter))]
public class PrefracturedObject : MonoBehaviour
{
    [Header("Seeds")]
    public int seedCount = 20;
    public bool useRandomSeed = true;
    public int randomSeed = 12345;

    [Header("Neighbors")]
    public bool useNearestNeighbors = true;
    public int neighborCount = 8;

    [Header("Fragments")]
    public Transform fragmentsRoot;
    public bool hideFragmentsAfterGeneration = true;

    [Header("Phys")]
    public float explosionForce = 4f;
    public float explosionRadius = 2f;
    public float upwardsModifier = 0.1f;

    private Sliceable sliceable;
    private MeshFilter meshFilter;

    private void Awake()
    {
        sliceable = GetComponent<Sliceable>();
        meshFilter = GetComponent<MeshFilter>();
    }

    [ContextMenu("Generate Prefractured Fragments")]
    public void GenerateFragments()
    {
        sliceable = GetComponent<Sliceable>();
        meshFilter = GetComponent<MeshFilter>();

        if (sliceable == null || meshFilter == null || meshFilter.sharedMesh == null)
            return;

        sliceable.EnsureTriangleTypesInitialized();

        if (useRandomSeed)
            Random.InitState(randomSeed);

        if (fragmentsRoot == null)
        {
            GameObject root = new GameObject("PrefracturedFragments");
            root.transform.SetParent(transform.parent);
            root.transform.position = transform.position;
            root.transform.rotation = transform.rotation;
            root.transform.localScale = transform.localScale;
            fragmentsRoot = root.transform;
        }

        ClearOldFragments();

        Mesh sourceMesh = meshFilter.sharedMesh;
        Bounds bounds = sourceMesh.bounds;

        List<Vector3> seeds = VoronoiSeedGenerator.GenerateRandomSeedsInMesh(sourceMesh, transform, seedCount);

        List<MeshClipResult> fragments = useNearestNeighbors ? VoronoiFractureGenerator.GenerateFragmentsWithNearestNeighborsParallel(sourceMesh, transform, sliceable.triangleSurfaceTypes, seeds, neighborCount) : VoronoiFractureGenerator.GenerateFragmentsParallel(sourceMesh, transform, sliceable.triangleSurfaceTypes, seeds);

        for (int i = 0; i < fragments.Count; i++)
        {
            CreatePrefracturedFragment(fragments[i], i);
        }

        if (hideFragmentsAfterGeneration)
            SetFragmentsActive(false);
    }

    [ContextMenu("Activate")]
    public void ActivateFragments()
    {
        ActivateFragments(transform.position);
    }

    public void ActivateFragments(Vector3 explosionPoint)
    {
        if (fragmentsRoot == null)
            return;

        gameObject.SetActive(false);
        SetFragmentsActive(true);

        foreach (Transform child in fragmentsRoot)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb == null)
                rb = child.gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;

            MeshCollider collider = child.GetComponent<MeshCollider>();
            if (collider == null)
                collider = child.gameObject.AddComponent<MeshCollider>();

            collider.convex = true;

            rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius, upwardsModifier, ForceMode.Impulse);
        }
    }

    [ContextMenu("Clear")]
    public void ClearOldFragments()
    {
        if (fragmentsRoot == null)
            return;

        for (int i = fragmentsRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = fragmentsRoot.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private void CreatePrefracturedFragment(MeshClipResult fragment, int index)
    {
        GameObject obj = MeshNObjCreator.CreateNewSubMeshObj(gameObject, fragment.Mesh, fragment.TriangleTypes, null, sliceable.capMaterial, "Prefrac Fragment " + index, false);

        obj.transform.SetParent(fragmentsRoot, true);

        Sliceable fragmentSliceable = obj.GetComponent<Sliceable>();
        if (fragmentSliceable == null)
            fragmentSliceable = obj.AddComponent<Sliceable>();

        fragmentSliceable.canBeSliced = sliceable.canBeSliced;
        fragmentSliceable.capMaterial = sliceable.capMaterial;
        fragmentSliceable.triangleSurfaceTypes =
            new List<SurfaceType>(fragment.TriangleTypes);

        MeshCollider collider = obj.GetComponent<MeshCollider>();
        if (collider == null)
            collider = obj.AddComponent<MeshCollider>();

        collider.convex = true;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
            rb = obj.AddComponent<Rigidbody>();

        rb.isKinematic = true;
    }

    private void SetFragmentsActive(bool active)
    {
        if (fragmentsRoot == null)
            return;

        foreach (Transform child in fragmentsRoot)
            child.gameObject.SetActive(active);
    }
}