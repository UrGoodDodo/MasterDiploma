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

    public static Mesh CreateNewSubMesh(List<Vector3> mainVertices, List<int> mainTriangles, List<Vector3> capVertices, List<int> capTriangles, string name = "")
    {
        List<Vector3> allVertices = new List<Vector3>();
        allVertices.AddRange(mainVertices);
        int capOffset = mainVertices.Count;
        allVertices.AddRange(capVertices);
        List<int> finalCapTriangles = new List<int>();
        foreach (int idx in capTriangles)
        {
            finalCapTriangles.Add(idx + capOffset);
        }

        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.SetVertices(allVertices);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(mainTriangles, 0);
        mesh.SetTriangles(finalCapTriangles, 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;     
    }

    public static void CreateNewSubMeshObj(GameObject initObj, Mesh mesh, Material mainMaterial = null, Material secondMaterial = null, string name = "") 
    {
        GameObject obj = new GameObject(name);
        obj.transform.localPosition = initObj.transform.position;
        obj.transform.localRotation = initObj.transform.rotation;
        obj.transform.localScale = initObj.transform.localScale;
        obj.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        Material[] materials = new Material[2];
        materials[0] = mainMaterial != null ? mainMaterial : initObj.GetComponent<MeshRenderer>().material;  // SubMesh 0
        materials[1] = secondMaterial != null ? secondMaterial : initObj.GetComponent<MeshRenderer>().material;   // SubMesh 1
        renderer.materials = materials;
        //obj.AddComponent<NormalsShow>();
    }
}
