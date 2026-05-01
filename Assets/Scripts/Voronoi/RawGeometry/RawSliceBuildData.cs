using System.Collections.Generic;
using UnityEngine;

public class RawSliceBuildData
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector2> UVs = new List<Vector2>();
    public List<int> Triangles = new List<int>();
    public List<SurfaceType> TriangleSurfaceTypes = new List<SurfaceType>();

    public int AddVertex(Vector3 vertex, Vector2 uv)
    {
        int index = Vertices.Count;
        Vertices.Add(vertex);
        UVs.Add(uv);
        return index;
    }

    public void AddTriangle(int a, int b, int c, SurfaceType surfaceType)
    {
        Triangles.Add(a);
        Triangles.Add(b);
        Triangles.Add(c);
        TriangleSurfaceTypes.Add(surfaceType);
    }

    public RawMeshData ToRawMeshData()
    {
        return new RawMeshData(Vertices.ToArray(),UVs.ToArray(), Triangles.ToArray(), TriangleSurfaceTypes.ToArray());
    }
}