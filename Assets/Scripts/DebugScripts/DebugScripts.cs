using System.Collections.Generic;
using UnityEngine;

public static class DebugScripts
{
    public static void DebugDrawLoops(List<List<int>> loops, List<Vector3> vertices, Color color, float duration = 5f, int loopIndexOffset = 0)
    {

        if (loops == null || vertices == null)
        {
            Debug.LogWarning("DebugDrawLoops: loops или vertices = null!");
            return;
        }

        for (int loopIdx = 0; loopIdx < loops.Count; loopIdx++)
        {
            var loop = loops[loopIdx];
            if (loop == null || loop.Count < 2) continue;

            Color loopColor = loopIndexOffset > 0
                ? new Color(color.r, color.g, color.b, color.a * 0.5f)
                : color;

            for (int i = 0; i < loop.Count; i++)
            {
                int currentIdx = loop[i];
                int nextIdx = loop[(i + 1) % loop.Count];


                if (currentIdx >= vertices.Count || nextIdx >= vertices.Count)
                {
                    Debug.LogError($"DebugDrawLoops: индекс {currentIdx} или {nextIdx} вне диапазона vertices (count={vertices.Count})");
                    continue;
                }

                Vector3 p1 = vertices[currentIdx];
                Vector3 p2 = vertices[nextIdx];

                Debug.DrawLine(p1, p2, loopColor, duration);
            }

            foreach (int idx in loop)
            {
                if (idx < vertices.Count)
                {
                    Debug.DrawRay(vertices[idx], Vector3.up * 0.1f, Color.yellow, duration);
                }
            }

        }

    }
}
