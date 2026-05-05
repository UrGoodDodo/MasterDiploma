using System.Collections.Generic;
using UnityEngine;

public static class MeshGeometrySlicer
{
    public static bool TrySlice(Mesh sourceMesh, Transform sourceTransform, Plane plane, List<SurfaceType> sourceTriangleTypes,out MeshSliceResult result)
    {
        result = null;

        if (sourceMesh == null || sourceTransform == null)
            return false;

        Vector3[] vertices = sourceMesh.vertices;

        int[] classified = ClassifyVertices(vertices, sourceTransform, plane);

        if (!HasBothSides(classified))
            return false;

        TriangleSlicer.ClassifyTriangles(sourceMesh, sourceTransform, classified, plane, sourceTriangleTypes, out List<Vector3> topVerts, out List<Vector2> topUVs, out List<int> topTris, out List<SurfaceType> topTriangleTypes, out List<Vector3> botVerts, out List<Vector2> botUVs, out List<int> botTris, out List<SurfaceType> botTriangleTypes, out HashSet<EdgeKey> topEdges, out HashSet<EdgeKey> botEdges);


        var mergedTopEdges = TriangleSlicer.RemapContourEdgesByPosition(topVerts, topEdges, 0.00001f);
        var mergedBotEdges = TriangleSlicer.RemapContourEdgesByPosition(botVerts, botEdges, 0.00001f);


        Vector3 localPlaneNormal = sourceTransform.InverseTransformDirection(plane.normal).normalized;



        //
        var topContours = ContourExtractor.ExtractContoursFromEdges(mergedTopEdges, topVerts, OpenChainMode.CloseSmallGaps, 0.001f);
        var botContours = ContourExtractor.ExtractContoursFromEdges(mergedBotEdges, botVerts, OpenChainMode.CloseSmallGaps, 0.001f);
        var topLoops = topContours.ClosedLoops;
        var botLoops = botContours.ClosedLoops;

        //SliceContourSphereDebugView.SetLoops(sourceTransform, topVerts, topLoops, botVerts, botLoops);

        foreach (string warning in topContours.Warnings) Debug.LogWarning($"Top cap: {warning}");
        foreach (string warning in botContours.Warnings) Debug.LogWarning($"Bottom cap: {warning}");
        var topProjectedLoops = CapProjection.ProjectLoopsTo2D(topLoops, topVerts, -localPlaneNormal);
        var botProjectedLoops = CapProjection.ProjectLoopsTo2D(botLoops, botVerts, localPlaneNormal);
        var topRegions = CapTopologyBuilder.BuildRegions(topProjectedLoops);
        var botRegions = CapTopologyBuilder.BuildRegions(botProjectedLoops);

        //CapTopologyBuilder.DebugRegions("Top", topRegions);
        //CapTopologyBuilder.DebugRegions("Bottom", botRegions);
        //for (int i = 0; i < topProjectedLoops.Count; i++) Debug.Log($"Top projected loop {i}: points={topProjectedLoops[i].Points.Count}, signedArea={topProjectedLoops[i].SignedArea}, absArea={topProjectedLoops[i].AbsArea}");
        //for (int i = 0; i < botProjectedLoops.Count; i++) Debug.Log($"Bottom projected loop {i}: points={botProjectedLoops[i].Points.Count}, signedArea={botProjectedLoops[i].SignedArea}, absArea={botProjectedLoops[i].AbsArea}");
        //CapProjection.DebugLoopContainment("Top", topProjectedLoops);
        //CapProjection.DebugLoopContainment("Bottom", botProjectedLoops);
        //

        if (topContours.HasOpenChains) Debug.LogWarning("Top cap has open chains. Cap may be incomplete.");
        if (botContours.HasOpenChains) Debug.LogWarning("Bottom cap has open chains. Cap may be incomplete.");

        List<Vector3> topCapVerts = new List<Vector3>();
        List<int> topCapTris = new List<int>();
        List<SurfaceType> topCapTriangleTypes = new List<SurfaceType>();

        bool topCapOk = CapCreator.TriangulateCapAuto(topLoops, topVerts, topCapVerts, topCapTris, topCapTriangleTypes, -localPlaneNormal);

        List<Vector3> botCapVerts = new List<Vector3>();
        List<int> botCapTris = new List<int>();
        List<SurfaceType> botCapTriangleTypes = new List<SurfaceType>();

        bool botCapOk = CapCreator.TriangulateCapAuto(botLoops, botVerts, botCapVerts, botCapTris, botCapTriangleTypes, localPlaneNormal);

        Mesh topMesh = MeshNObjCreator.CreateNewSubMesh(topVerts, topUVs, topTris, topTriangleTypes, topCapVerts, topCapTris, topCapTriangleTypes, "TopPart");

        Mesh bottomMesh = MeshNObjCreator.CreateNewSubMesh(botVerts, botUVs, botTris, botTriangleTypes, botCapVerts, botCapTris, botCapTriangleTypes, "BotPart");

        List<SurfaceType> topFinalTriangleTypes = new List<SurfaceType>();
        topFinalTriangleTypes.AddRange(topTriangleTypes);
        topFinalTriangleTypes.AddRange(topCapTriangleTypes);

        List<SurfaceType> bottomFinalTriangleTypes = new List<SurfaceType>();
        bottomFinalTriangleTypes.AddRange(botTriangleTypes);
        bottomFinalTriangleTypes.AddRange(botCapTriangleTypes);

        result = new MeshSliceResult
        {
            TopMesh = topMesh,
            BottomMesh = bottomMesh,
            TopTriangleTypes = topFinalTriangleTypes,
            BottomTriangleTypes = bottomFinalTriangleTypes
        };

        return true;
    }

    public static bool TrySlice(Sliceable sliceable, Plane plane, out MeshSliceResult result)
    {
        result = null;

        if (sliceable == null)
            return false;

        MeshFilter mf = sliceable.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return false;

        sliceable.EnsureTriangleTypesInitialized();

        return TrySlice(mf.sharedMesh, sliceable.transform, plane, sliceable.triangleSurfaceTypes, out result);
    }

    public static bool TryClip(Mesh sourceMesh, Transform sourceTransform, Plane plane, bool keepPositiveSide, List<SurfaceType> sourceTriangleTypes, out MeshClipResult result)
    {
        result = null;

        if (sourceMesh == null || sourceTransform == null)
            return false;

        int[] classified = ClassifyVertices(sourceMesh.vertices, sourceTransform, plane);

        bool hasPositive = false;
        bool hasNegative = false;

        foreach (int c in classified)
        {
            if (c > 0)
                hasPositive = true;
            else if (c < 0)
                hasNegative = true;
        }

        if (!hasPositive || !hasNegative)
        {
            bool meshIsPositive = hasPositive && !hasNegative;
            bool meshIsNegative = hasNegative && !hasPositive;

            if ((meshIsPositive && keepPositiveSide) || (meshIsNegative && !keepPositiveSide))
            {
                result = MeshClipResult.FromMesh(sourceMesh,sourceTriangleTypes != null ? new List<SurfaceType>(sourceTriangleTypes) : new List<SurfaceType>());
                return true;
            }

            result = MeshClipResult.Empty();
            return true;
        }

        if (!TrySlice(sourceMesh, sourceTransform, plane, sourceTriangleTypes, out MeshSliceResult sliceResult))
        {
            result = MeshClipResult.Empty();
            return false;
        }

        result = keepPositiveSide ? MeshClipResult.FromMesh(sliceResult.TopMesh, sliceResult.TopTriangleTypes) : MeshClipResult.FromMesh(sliceResult.BottomMesh, sliceResult.BottomTriangleTypes);
        return true;
    }

    public static bool TryClip(Sliceable sliceable, Plane plane, bool keepPositiveSide, out MeshClipResult result)
    {
        result = null;

        if (sliceable == null)
            return false;

        MeshFilter mf = sliceable.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return false;

        sliceable.EnsureTriangleTypesInitialized();

        return TryClip(mf.sharedMesh, sliceable.transform, plane, keepPositiveSide, sliceable.triangleSurfaceTypes, out result);
    }


    private static int[] ClassifyVertices(Vector3[] vertices, Transform t, Plane plane, float eps = 1e-4f)
    {
        int[] result = new int[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 world = t.TransformPoint(vertices[i]);
            float d = plane.GetDistanceToPoint(world);

            if (Mathf.Abs(d) < eps)
                result[i] = 0;
            else if (d > 0)
                result[i] = 1;
            else
                result[i] = -1;
        }

        return result;
    }

    private static bool HasBothSides(int[] classified)
    {
        bool above = false;
        bool below = false;

        foreach (var c in classified)
        {
            if (c > 0)
                above = true;
            else if (c < 0)
                below = true;

            if (above && below)
                return true;
        }

        return false;
    }
}