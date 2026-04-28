using UnityEngine;

public static class MeshVolumeUtility
{
    public static bool IsPointInsideMeshLocal(Mesh mesh, Vector3 localPoint)
    {
        if (mesh == null)
            return false;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Vector3 rayOrigin = localPoint;
        Vector3 rayDirection = Vector3.right;

        int hitCount = 0;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = vertices[triangles[i]];
            Vector3 b = vertices[triangles[i + 1]];
            Vector3 c = vertices[triangles[i + 2]];

            if (RayIntersectsTriangle(rayOrigin, rayDirection, a, b, c))
                hitCount++;
        }

        return hitCount % 2 == 1;
    }

    private static bool RayIntersectsTriangle(Vector3 origin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        const float epsilon = 0.000001f;

        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;

        Vector3 h = Vector3.Cross(direction, edge2);
        float a = Vector3.Dot(edge1, h);

        if (Mathf.Abs(a) < epsilon)
            return false;

        float f = 1f / a;
        Vector3 s = origin - v0;

        float u = f * Vector3.Dot(s, h);
        if (u < 0f || u > 1f)
            return false;

        Vector3 q = Vector3.Cross(s, edge1);

        float v = f * Vector3.Dot(direction, q);
        if (v < 0f || u + v > 1f)
            return false;

        float t = f * Vector3.Dot(edge2, q);

        return t > epsilon;
    }
}