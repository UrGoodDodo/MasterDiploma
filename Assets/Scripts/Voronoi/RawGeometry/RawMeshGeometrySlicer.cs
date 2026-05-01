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

        HashSet<EdgeKey> mergedTopEdges = RemapContourEdgesByPosition(top.Vertices, ctx.TopContourEdges);

        HashSet<EdgeKey> mergedBottomEdges = RemapContourEdgesByPosition(bottom.Vertices, ctx.BottomContourEdges);

        List<List<int>> topLoops = CapCreator.ExtractLoopsFromEdges(mergedTopEdges);

        List<List<int>> bottomLoops = CapCreator.ExtractLoopsFromEdges(mergedBottomEdges);

        AddCapsToBuildData(top, topLoops, -localPlaneNormal);

        AddCapsToBuildData(bottom, bottomLoops, localPlaneNormal);

        result = new RawSliceResult
        {
            Top = top.ToRawMeshData(),
            Bottom = bottom.ToRawMeshData()
        };

        return true;
    }

    private static void AddCapsToBuildData(RawSliceBuildData buildData, List<List<int>> loops, Vector3 capNormal)
    {
        if (loops == null || loops.Count == 0)
            return;

        foreach (List<int> loop in loops)
        {
            List<Vector3> capVertices = new List<Vector3>();
            List<int> capTriangles = new List<int>();
            List<SurfaceType> capTriangleTypes = new List<SurfaceType>();

            CapCreator.TriangulateCapByType(CapCreator.CapType.EarClipping, loop, buildData.Vertices, capVertices, capTriangles, capTriangleTypes, capNormal);

            int capOffset = buildData.Vertices.Count;

            for (int i = 0; i < capVertices.Count; i++)
            {
                buildData.Vertices.Add(capVertices[i]);
                buildData.UVs.Add(Vector2.zero);
            }

            for (int i = 0; i < capTriangles.Count; i += 3)
            {
                int triIndex = i / 3;

                SurfaceType type = SurfaceType.Cap;

                if (capTriangleTypes != null && triIndex < capTriangleTypes.Count)
                    type = capTriangleTypes[triIndex];

                buildData.AddTriangle(capTriangles[i] + capOffset, capTriangles[i + 1] + capOffset, capTriangles[i + 2] + capOffset, type);
            }
        }
    }

    private static HashSet<EdgeKey> RemapContourEdgesByPosition(List<Vector3> vertices, HashSet<EdgeKey> contourEdges, float epsilon = 0.001f)
    {
        int[] oldToRepresentative = new int[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            oldToRepresentative[i] = i;

            for (int j = 0; j < i; j++)
            {
                if (Vector3.Distance(vertices[i], vertices[j]) < epsilon)
                {
                    oldToRepresentative[i] = oldToRepresentative[j];
                    break;
                }
            }
        }

        HashSet<EdgeKey> remappedEdges = new HashSet<EdgeKey>();

        foreach (EdgeKey edge in contourEdges)
        {
            int a = oldToRepresentative[edge.A];
            int b = oldToRepresentative[edge.B];

            if (a != b)
                remappedEdges.Add(new EdgeKey(a, b));
        }

        return remappedEdges;
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