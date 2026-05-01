using System.Collections.Generic;
using UnityEngine;

public class RawSliceContext
{
    public Plane LocalPlane;

    public Vector3[] MainVertices;
    public Vector2[] MainUVs;

    public RawSliceBuildData Top;
    public RawSliceBuildData Bottom;

    public HashSet<EdgeKey> TopContourEdges;
    public HashSet<EdgeKey> BottomContourEdges;

    public Dictionary<EdgeKey, (int top, int bottom)> EdgeCache;
    public Dictionary<int, (int top, int bottom)> OnPlaneVertexCache;

    public RawSliceContext(RawMeshData source, Plane localPlane, RawSliceBuildData top, RawSliceBuildData bottom)
    {
        LocalPlane = localPlane;

        MainVertices = source.Vertices;
        MainUVs = source.UVs;

        Top = top;
        Bottom = bottom;

        TopContourEdges = new HashSet<EdgeKey>();
        BottomContourEdges = new HashSet<EdgeKey>();

        EdgeCache = new Dictionary<EdgeKey, (int top, int bottom)>();
        OnPlaneVertexCache = new Dictionary<int, (int top, int bottom)>();
    }
}