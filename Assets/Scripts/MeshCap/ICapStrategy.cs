using System.Collections.Generic;
using UnityEngine;

public interface ICapStrategy
{
    void TriangulateCap(List<int> loop, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal);
}