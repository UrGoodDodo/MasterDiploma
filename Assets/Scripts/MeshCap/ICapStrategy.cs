using System.Collections.Generic;
using UnityEngine;

public interface ICapStrategy
{
    void TriangulateCap(List<int> loop, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, Vector3 normal);
}