using System.Collections.Generic;
using UnityEngine;

public class MeshSliceResult
{
    public Mesh TopMesh;
    public Mesh BottomMesh;

    public List<SurfaceType> TopTriangleTypes;
    public List<SurfaceType> BottomTriangleTypes;

    public bool HasTop => TopMesh != null && TopMesh.vertexCount > 0;
    public bool HasBottom => BottomMesh != null && BottomMesh.vertexCount > 0;
}