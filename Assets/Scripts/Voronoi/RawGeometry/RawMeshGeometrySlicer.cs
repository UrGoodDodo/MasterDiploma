using System.Collections.Generic;
using UnityEngine;

public static class RawMeshGeometrySlicer
{
    public static bool TryClip(RawMeshData source, RawTransformData transformData, Plane plane, bool keepPositiveSide, out RawMeshClipResult result)
    {
        result = null;

        if (source == null || !source.IsValid)
            return false;

        int[] classified = ClassifyVertices(source.Vertices, transformData, plane);

        bool hasPositive = false;
        bool hasNegative = false;

        foreach (int c in classified)
        {
            if (c > 0)
                hasPositive = true;
            else if (c < 0)
                hasNegative = true;
        }

        if (!hasPositive && !hasNegative)
        {
            result = RawMeshClipResult.FromMeshData(source.Clone());
            return true;
        }

        if (!hasPositive || !hasNegative)
        {
            bool meshIsPositive = hasPositive && !hasNegative;
            bool meshIsNegative = hasNegative && !hasPositive;

            if ((meshIsPositive && keepPositiveSide) || (meshIsNegative && !keepPositiveSide))
            {
                result = RawMeshClipResult.FromMeshData(source.Clone());
                return true;
            }

            result = RawMeshClipResult.Empty();
            return true;
        }

        if (!TrySlice(source, transformData, plane, out RawSliceResult sliceResult))
        {
            result = RawMeshClipResult.Empty();
            return false;
        }

        RawMeshData keptMesh = keepPositiveSide ? sliceResult.Top : sliceResult.Bottom;

        result = RawMeshClipResult.FromMeshData(keptMesh);
        return true;
    }

    public static bool TrySlice(RawMeshData source, RawTransformData transformData, Plane plane, out RawSliceResult result)
    {
        result = null;

        if (source == null || !source.IsValid)
            return false;

        int[] classified = ClassifyVertices(source.Vertices, transformData, plane);

        if (!HasBothSides(classified))
            return false;

        RawSliceBuildData top = new RawSliceBuildData();
        RawSliceBuildData bottom = new RawSliceBuildData();

        Vector3 localPlanePoint = transformData.InverseTransformPoint(plane.normal * -plane.distance);

        Vector3 localPlaneNormal = transformData.InverseTransformDirection(plane.normal).normalized;

        Plane localPlane = new Plane(localPlaneNormal, localPlanePoint);

        RawSliceContext ctx = new RawSliceContext(source, localPlane, top, bottom);

        RawTriangleSlicer.ClassifyTriangles(source, classified, ref ctx);

        //
        RawSliceDiagnostics.LogAfterTriangles(top, bottom, ctx);
        //

        HashSet<EdgeKey> mergedTopEdges = RemapContourEdgesByPosition(top.Vertices, ctx.TopContourEdges, 0.00001f);

        HashSet<EdgeKey> mergedBottomEdges = RemapContourEdgesByPosition(bottom.Vertices, ctx.BottomContourEdges, 0.00001f);

        //
        RawSliceDiagnostics.LogAfterRemap(mergedTopEdges, mergedBottomEdges);
        //

        ContourExtractionResult topContours = ContourExtractor.ExtractContoursFromEdges(mergedTopEdges,top.Vertices, OpenChainMode.CloseSmallGaps, 0.001f);

        ContourExtractionResult bottomContours = ContourExtractor.ExtractContoursFromEdges(mergedBottomEdges, bottom.Vertices, OpenChainMode.CloseSmallGaps, 0.001f);

        AddCapsToBuildData("TOP", top, topContours.ClosedLoops, -localPlaneNormal);

        AddCapsToBuildData("BOT", bottom, bottomContours.ClosedLoops, localPlaneNormal);

        //
        RawSliceDiagnostics.LogAfterAllCaps(top, bottom);
        //

        RawMeshData topMesh = top.ToRawMeshData();
        RawMeshData bottomMesh = bottom.ToRawMeshData();
        RawSliceDiagnostics.LogFinal(topMesh, bottomMesh);

        result = new RawSliceResult
        {
            Top = top.ToRawMeshData(),
            Bottom = bottom.ToRawMeshData()
        };

        return true;
    }

    private static void AddCapsToBuildData(string label, RawSliceBuildData buildData, List<List<int>> loops, Vector3 capNormal)
    {
        //
        RawSliceDiagnostics.LogBeforeCaps(label, buildData, loops);
        //

        if (loops == null || loops.Count == 0) return;

        List<Vector3> capVertices = new List<Vector3>();
        List<int> capTriangles = new List<int>();
        List<SurfaceType> capTriangleTypes = new List<SurfaceType>();

        bool success = CapCreator.TriangulateCapAuto(loops, buildData.Vertices, capVertices, capTriangles, capTriangleTypes, capNormal);
        if (!success) return;

        //
        RawSliceDiagnostics.LogCapResult(label, success, capVertices, capTriangles, capTriangleTypes);
        //

        int vertexOffset = buildData.Vertices.Count;

        buildData.Vertices.AddRange(capVertices);

        for (int i = 0; i < capVertices.Count; i++) buildData.UVs.Add(Vector2.zero);

        for (int i = 0; i < capTriangles.Count; i++) buildData.Triangles.Add(capTriangles[i] + vertexOffset);

        buildData.TriangleSurfaceTypes.AddRange(capTriangleTypes);

        //
        RawSliceDiagnostics.LogAfterCaps(label, buildData);
        //
    }

    private static HashSet<EdgeKey> RemapContourEdgesByPosition(List<Vector3> vertices, HashSet<EdgeKey> contourEdges, float epsilon = 0.00001f)
    {
        if (vertices == null || contourEdges == null || contourEdges.Count == 0)
            return new HashSet<EdgeKey>();

        HashSet<int> contourIndices = CollectContourIndices(vertices, contourEdges);

        var oldToRepresentative = new Dictionary<int, int>();

        foreach (int index in contourIndices)
            oldToRepresentative[index] = index;

        List<int> orderedContourIndices = new List<int>(contourIndices);

        for (int i = 0; i < orderedContourIndices.Count; i++)
        {
            int current = orderedContourIndices[i];

            for (int j = 0; j < i; j++)
            {
                int previous = orderedContourIndices[j];

                if (Vector3.Distance(vertices[current], vertices[previous]) < epsilon)
                {
                    oldToRepresentative[current] = oldToRepresentative[previous];
                    break;
                }
            }
        }

        var representativeToMembers = BuildRepresentativeGroups(oldToRepresentative);
        var candidateEdges = BuildRemappedEdges(oldToRepresentative, contourEdges);
        var candidateDegrees = CountDegrees(candidateEdges);

        foreach (var pair in representativeToMembers)
        {
            int representative = pair.Key;

            if (!candidateDegrees.TryGetValue(representative, out int degree))
                continue;

            if (degree <= 2)
                continue;

            foreach (int member in pair.Value)
                oldToRepresentative[member] = member;
        }

        return BuildRemappedEdges(oldToRepresentative, contourEdges);
    }

    private static HashSet<int> CollectContourIndices(List<Vector3> vertices, HashSet<EdgeKey> contourEdges)
    {
        HashSet<int> indices = new HashSet<int>();

        foreach (EdgeKey edge in contourEdges)
        {
            if (edge.A >= 0 && edge.A < vertices.Count)
                indices.Add(edge.A);

            if (edge.B >= 0 && edge.B < vertices.Count)
                indices.Add(edge.B);
        }

        return indices;
    }

    private static Dictionary<int, List<int>> BuildRepresentativeGroups(Dictionary<int, int> oldToRepresentative)
    {
        var representativeToMembers = new Dictionary<int, List<int>>();

        foreach (var pair in oldToRepresentative)
        {
            int member = pair.Key;
            int representative = pair.Value;

            if (!representativeToMembers.ContainsKey(representative))
                representativeToMembers[representative] = new List<int>();

            representativeToMembers[representative].Add(member);
        }

        return representativeToMembers;
    }

    private static HashSet<EdgeKey> BuildRemappedEdges(Dictionary<int, int> oldToRepresentative, HashSet<EdgeKey> contourEdges)
    {
        var remappedEdges = new HashSet<EdgeKey>();

        foreach (EdgeKey edge in contourEdges)
        {
            if (!oldToRepresentative.TryGetValue(edge.A, out int a))
                continue;

            if (!oldToRepresentative.TryGetValue(edge.B, out int b))
                continue;

            if (a != b)
                remappedEdges.Add(new EdgeKey(a, b));
        }

        return remappedEdges;
    }

    private static Dictionary<int, int> CountDegrees(HashSet<EdgeKey> edges)
    {
        var degrees = new Dictionary<int, int>();

        foreach (EdgeKey edge in edges)
        {
            if (!degrees.ContainsKey(edge.A))
                degrees[edge.A] = 0;

            if (!degrees.ContainsKey(edge.B))
                degrees[edge.B] = 0;

            degrees[edge.A]++;
            degrees[edge.B]++;
        }

        return degrees;
    }

    private static bool HasBothSides(int[] classified)
    {
        bool positive = false;
        bool negative = false;

        foreach (int c in classified)
        {
            if (c > 0)
                positive = true;
            else if (c < 0)
                negative = true;

            if (positive && negative)
                return true;
        }

        return false;
    }

    private static int[] ClassifyVertices(Vector3[] vertices, RawTransformData transformData, Plane plane, float eps = 1e-4f)
    {
        int[] result = new int[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 world = transformData.TransformPoint(vertices[i]);
            float d = plane.GetDistanceToPoint(world);

            if (Mathf.Abs(d) < eps)
                result[i] = 0;
            else if (d > 0f)
                result[i] = 1;
            else
                result[i] = -1;
        }

        return result;
    }
}