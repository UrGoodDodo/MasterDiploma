using System.Collections.Generic;
using UnityEngine;

public class FanCap : ICapStrategy
{
    public void TriangulateCap(List<int> loop, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, Vector3 normal)
    {
        if (loop.Count < 3) return;

        List<int> capLoopIndices = new List<int>();
        foreach (int idx in loop)
        {
            capLoopIndices.Add(capVertices.Count);
            capVertices.Add(mainVertices[idx]);
        }

        Vector3 center = Vector3.zero;
        foreach (int idx in capLoopIndices)
        {
            center += capVertices[idx];
        }
        center /= capLoopIndices.Count;

        int centerIdx = capVertices.Count;
        capVertices.Add(center);

        for (int i = 0; i < capLoopIndices.Count; i++)
        {
            int next = (i + 1) % capLoopIndices.Count;

            int idxA = capLoopIndices[i];
            int idxB = capLoopIndices[next];

            Vector3 p1 = capVertices[idxA];
            Vector3 p2 = capVertices[idxB];
            Vector3 p3 = capVertices[centerIdx];

            Vector3 triNormal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

            if (Vector3.Dot(triNormal, normal) > 0)
            {
                outTriangles.Add(centerIdx);
                outTriangles.Add(idxA);
                outTriangles.Add(idxB);
            }
            else
            {
                outTriangles.Add(centerIdx);
                outTriangles.Add(idxB);
                outTriangles.Add(idxA);
            }
        }
    }
}