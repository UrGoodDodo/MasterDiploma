using UnityEngine;

public static class RawTriangleSlicer
{
    public static void ClassifyTriangles(RawMeshData source, int[] classifiedVertices, ref RawSliceContext ctx)
    {
        int[] triangles = source.Triangles;
        SurfaceType[] triangleTypes = source.TriangleSurfaceTypes;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int triIndex = i / 3;

            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            int s0 = classifiedVertices[i0];
            int s1 = classifiedVertices[i1];
            int s2 = classifiedVertices[i2];

            SurfaceType surfaceType = triangleTypes[triIndex];

            int positiveCount =
                (s0 > 0 ? 1 : 0) +
                (s1 > 0 ? 1 : 0) +
                (s2 > 0 ? 1 : 0);

            int negativeCount =
                (s0 < 0 ? 1 : 0) +
                (s1 < 0 ? 1 : 0) +
                (s2 < 0 ? 1 : 0);

            int onCount =
                (s0 == 0 ? 1 : 0) +
                (s1 == 0 ? 1 : 0) +
                (s2 == 0 ? 1 : 0);

            Vector3 origNormal = Vector3.Cross(source.Vertices[i1] - source.Vertices[i0], source.Vertices[i2] - source.Vertices[i0]).normalized;

            if (positiveCount == 3)
            {
                RawTriangleSlicerHelper.ProcessAllTop(i0, i1, i2, origNormal, surfaceType, ref ctx);
            }
            else if (negativeCount == 3)
            {
                RawTriangleSlicerHelper.ProcessAllBottom(i0, i1, i2, origNormal, surfaceType, ref ctx);
            }
            else if (positiveCount == 2 && onCount == 1)
            {
                RawTriangleSlicerHelper.ProcessTwoAboveOneOnPlane(i0, i1, i2, s0, s1, s2, origNormal, surfaceType, ref ctx);
            }
            else if (negativeCount == 2 && onCount == 1)
            {
                RawTriangleSlicerHelper.ProcessTwoBelowOneOnPlane(i0, i1, i2, s0, s1, s2, origNormal, surfaceType, ref ctx);
            }
            else if (positiveCount == 1 && onCount == 2)
            {
                RawTriangleSlicerHelper.ProcessOneAboveTwoOnPlane(i0, i1, i2,s0, s1, s2, origNormal, surfaceType, ref ctx);
            }
            else if (negativeCount == 1 && onCount == 2)
            {
                RawTriangleSlicerHelper.ProcessOneBelowTwoOnPlane(i0, i1, i2, s0, s1, s2, origNormal, surfaceType, ref ctx);
            }
            else if (positiveCount == 2 && negativeCount == 1)
            {
                RawTriangleSlicerHelper.ProcessTwoAboveOneBelow(i0, i1, i2, s0, s1, s2, origNormal, surfaceType, ref ctx);
            }
            else if (positiveCount == 1 && negativeCount == 2)
            {
                RawTriangleSlicerHelper.ProcessOneAboveTwoBelow(i0, i1, i2, s0, s1, s2, origNormal, surfaceType, ref ctx);
            }
            else if (positiveCount == 1 && negativeCount == 1 && onCount == 1)
            {
                RawTriangleSlicerHelper.ProcessOneAboveOneBelowOneOnPlane(i0, i1, i2, s0, s1, s2, origNormal, surfaceType, ref ctx);
            }
            else if (onCount == 3)
            {
                RawTriangleSlicerHelper.ProcessAllOnPlane(i0, i1, i2, origNormal, surfaceType, ref ctx);
            }
        }
    }
}