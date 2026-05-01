using System;
using UnityEngine;

[Serializable]
public class RawMeshData
{
    public Vector3[] Vertices;
    public Vector2[] UVs;
    public int[] Triangles;
    public SurfaceType[] TriangleSurfaceTypes;

    public int VertexCount => Vertices != null ? Vertices.Length : 0;
    public int TriangleCount => Triangles != null ? Triangles.Length / 3 : 0;

    public bool IsValid =>
        Vertices != null &&
        UVs != null &&
        UVs.Length == Vertices.Length &&
        Triangles != null &&
        TriangleSurfaceTypes != null &&
        Triangles.Length % 3 == 0 &&
        TriangleSurfaceTypes.Length == Triangles.Length / 3 &&
        Triangles.Length > 0;

    public RawMeshData(Vector3[] vertices, Vector2[] uvs, int[] triangles, SurfaceType[] triangleSurfaceTypes)
    {
        Vertices = vertices;
        UVs = uvs;
        Triangles = triangles ?? Array.Empty<int>();
        TriangleSurfaceTypes = triangleSurfaceTypes ?? Array.Empty<SurfaceType>();
    }

    public RawMeshData Clone()
    {
        return new RawMeshData(
            Vertices != null ? (Vector3[])Vertices.Clone() : null,
            UVs != null ? (Vector2[])UVs.Clone() : null,
            Triangles != null ? (int[])Triangles.Clone() : null,
            TriangleSurfaceTypes != null ? (SurfaceType[])TriangleSurfaceTypes.Clone() : null
        );
    }
}