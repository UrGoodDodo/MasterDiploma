using System;
using System.Collections.Generic;
using UnityEngine;

public static class RawMeshConverter
{
    public static RawMeshData FromMesh(Mesh mesh, List<SurfaceType> triangleTypes)
    {
        if (mesh == null)
            return null;

        Vector3[] vertices = mesh.vertices;

        Vector2[] uvs = mesh.uv;
        if (uvs == null || uvs.Length != vertices.Length)
            uvs = new Vector2[vertices.Length];

        int[] triangles = mesh.triangles;
        int triangleCount = triangles.Length / 3;

        SurfaceType[] surfaceTypes = new SurfaceType[triangleCount];

        for (int tri = 0; tri < triangleCount; tri++)
        {
            if (triangleTypes != null && tri < triangleTypes.Count)
                surfaceTypes[tri] = triangleTypes[tri];
            else
                surfaceTypes[tri] = SurfaceType.Main;
        }

        return new RawMeshData(vertices, uvs, triangles, surfaceTypes);
    }

    public static Mesh ToMesh(RawMeshData raw, string name = "")
    {
        if (raw == null || !raw.IsValid)
            return null;

        List<int> subMeshMain = new List<int>();
        List<int> subMeshCap = new List<int>();

        for (int tri = 0; tri < raw.TriangleSurfaceTypes.Length; tri++)
        {
            int baseIndex = tri * 3;

            int i0 = raw.Triangles[baseIndex];
            int i1 = raw.Triangles[baseIndex + 1];
            int i2 = raw.Triangles[baseIndex + 2];

            if (raw.TriangleSurfaceTypes[tri] == SurfaceType.Cap)
            {
                subMeshCap.Add(i0);
                subMeshCap.Add(i1);
                subMeshCap.Add(i2);
            }
            else
            {
                subMeshMain.Add(i0);
                subMeshMain.Add(i1);
                subMeshMain.Add(i2);
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = name;

        mesh.SetVertices(raw.Vertices);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(subMeshMain, 0);
        mesh.SetTriangles(subMeshCap, 1);
        mesh.SetUVs(0, raw.UVs);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}