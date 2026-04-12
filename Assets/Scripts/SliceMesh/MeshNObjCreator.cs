using System.Collections.Generic;
using UnityEngine;

public static class MeshNObjCreator
{
    public static Mesh CreateNewMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static void CreateNewObj(GameObject initObj, Mesh newMesh, Material material)
    {
        GameObject newObj = new GameObject(initObj.name + "_Top");
        newObj.transform.position = initObj.transform.position;
        newObj.transform.rotation = initObj.transform.rotation;
        newObj.transform.localScale = initObj.transform.localScale;
        newObj.AddComponent<MeshFilter>().mesh = newMesh;
        MeshRenderer topRend = newObj.AddComponent<MeshRenderer>();
        topRend.material = material != null ? material : initObj.GetComponent<MeshRenderer>().material;
    }

    //Поправить для новых капов!
    public static Mesh CreateNewSubMesh(List<Vector3> mainVertices, List<int> mainTriangles, List<Vector3> capVertices, List<int> capTriangles, GameObject initObj, string name = "")
    {
        Mesh originalMesh = initObj.GetComponent<MeshFilter>().mesh;

        List<Vector3> allVertices = new List<Vector3>();
        allVertices.AddRange(mainVertices);

        int capOffset = allVertices.Count;
        allVertices.AddRange(capVertices);

        List<int> newCapTriangles = new List<int>();
        foreach (int idx in capTriangles)
        {
            newCapTriangles.Add(idx + capOffset);
        }

        List<int> combinedCapTriangles = new List<int>();

        if (originalMesh != null && originalMesh.subMeshCount > 1)
        {
            var oldCapTriangles = originalMesh.GetTriangles(1);
        }

        combinedCapTriangles.AddRange(newCapTriangles);

        Mesh mesh = new Mesh();
        mesh.name = name;

        mesh.SetVertices(allVertices);
        mesh.subMeshCount = 2;

        mesh.SetTriangles(mainTriangles, 0);
        mesh.SetTriangles(combinedCapTriangles, 1);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static GameObject CreateNewSubMeshObj(GameObject initObj, Mesh mesh, Material mainMaterial = null, Material secondMaterial = null, string name = "", bool addSliceable = false) 
    {
        GameObject obj = new GameObject(name);

        obj.transform.position = initObj.transform.position;
        obj.transform.rotation = initObj.transform.rotation;
        obj.transform.localScale = initObj.transform.localScale;
        obj.layer = initObj.layer;

        var mf = obj.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        var renderer = obj.AddComponent<MeshRenderer>();
        Material[] materials = new Material[2];
        materials[0] = mainMaterial != null ? mainMaterial : initObj.GetComponent<MeshRenderer>().material;
        materials[1] = secondMaterial != null ? secondMaterial : materials[0];
        renderer.materials = materials;

        var collider = obj.AddComponent<MeshCollider>();
        collider.convex = true;
        collider.sharedMesh = mesh;

        if (addSliceable)
        {
            var originalSliceable = initObj.GetComponent<Sliceable>();

            var sliceable = obj.AddComponent<Sliceable>();

            if (originalSliceable != null)
            {
                sliceable.capMaterial = originalSliceable.capMaterial;
                sliceable.canBeSliced = originalSliceable.canBeSliced;
                sliceable.generateCollider = originalSliceable.generateCollider;
            }
        }

        if (initObj.TryGetComponent<Rigidbody>(out var initRb))
        {
            var rb = obj.AddComponent<Rigidbody>();

            float volume = mesh.bounds.size.x * mesh.bounds.size.y * mesh.bounds.size.z;
            rb.mass = Mathf.Clamp(volume, 0.1f, 10f);

            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Vector3 randomForce = Random.onUnitSphere * 2f;
            rb.AddForce(randomForce, ForceMode.Impulse);
        }

        return obj;
    }
}
