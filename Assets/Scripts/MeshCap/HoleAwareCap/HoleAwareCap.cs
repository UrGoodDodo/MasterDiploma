using System.Collections.Generic;
using UnityEngine;

public class HoleAwareCap : IHoleCapStrategy
{
    public static bool EnableDiagnostics = false;
    public static HoleCapMode Mode = HoleCapMode.Auto;

    public bool TriangulateCap(CapLoopSet loopSet, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal)
    {
        if (loopSet == null || !loopSet.IsValid) return false;
        if (loopSet.Holes == null || loopSet.Holes.Count == 0) return false;

        HoleCapMode resolvedMode = ResolveMode(loopSet);

        switch (resolvedMode)
        {
            case HoleCapMode.AngularRing:
                return AngularRingHoleCap.Triangulate(loopSet.OuterLoop, loopSet.Holes[0], mainVertices, capVertices, outTriangles, outTriangleTypes, normal, EnableDiagnostics);

            case HoleCapMode.BridgeEarClipping:
                return BridgeEarHoleCap.Triangulate(loopSet, mainVertices, capVertices, outTriangles, outTriangleTypes, normal, EnableDiagnostics);

            default:
                return false;
        }
    }

    private static HoleCapMode ResolveMode(CapLoopSet loopSet)
    {
        if (Mode != HoleCapMode.Auto) return Mode;
        if (loopSet.Holes.Count == 1) return HoleCapMode.AngularRing;
        return HoleCapMode.BridgeEarClipping;
    }
}