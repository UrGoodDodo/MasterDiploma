using System.Collections.Generic;
using UnityEngine;

public interface IHoleCapStrategy
{
    bool TriangulateCap(
        CapLoopSet loopSet,
        List<Vector3> mainVertices,
        List<Vector3> capVertices,
        List<int> outTriangles,
        List<SurfaceType> outTriangleTypes,
        Vector3 normal
    );
}