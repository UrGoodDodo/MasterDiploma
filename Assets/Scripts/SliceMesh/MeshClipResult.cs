using System.Collections.Generic;
using UnityEngine;

public class MeshClipResult
{
    public Mesh Mesh;
    public List<SurfaceType> TriangleTypes;
    public bool IsEmpty;

    public static MeshClipResult Empty()
    {
        return new MeshClipResult
        {
            Mesh = null,
            TriangleTypes = null,
            IsEmpty = true
        };
    }

    public static MeshClipResult FromMesh(Mesh mesh, List<SurfaceType> triangleTypes)
    {
        return new MeshClipResult
        {
            Mesh = mesh,
            TriangleTypes = triangleTypes,
            IsEmpty = mesh == null || mesh.vertexCount == 0
        };
    }
}