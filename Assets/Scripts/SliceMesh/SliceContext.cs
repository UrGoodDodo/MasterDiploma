using System.Collections.Generic;
using UnityEngine;

public class SliceContext
{
    public Transform Transform;
    public Plane Plane;
    public Vector3[] MainVertices;

    public List<Vector3> TopVertices;
    public List<int> TopTriangles;
    public List<SurfaceType> TopTriangleTypes;

    public List<Vector3> BotVertices;
    public List<int> BotTriangles;
    public List<SurfaceType> BotTriangleTypes;

    public HashSet<EdgeKey> TopContourEdges;
    public HashSet<EdgeKey> BotContourEdges;

    public Dictionary<EdgeKey, (int top, int bottom)> EdgeCache;
    public Dictionary<int, (int top, int bottom)> OnPlaneVertexCache;


}