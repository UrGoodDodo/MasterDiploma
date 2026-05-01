using UnityEngine;

public struct RawTransformData
{
    public Matrix4x4 LocalToWorld;
    public Matrix4x4 WorldToLocal;

    public RawTransformData(Transform transform)
    {
        LocalToWorld = transform.localToWorldMatrix;
        WorldToLocal = transform.worldToLocalMatrix;
    }

    public Vector3 TransformPoint(Vector3 localPoint)
    {
        return LocalToWorld.MultiplyPoint3x4(localPoint);
    }

    public Vector3 InverseTransformPoint(Vector3 worldPoint)
    {
        return WorldToLocal.MultiplyPoint3x4(worldPoint);
    }

    public Vector3 TransformDirection(Vector3 localDirection)
    {
        return LocalToWorld.MultiplyVector(localDirection).normalized;
    }

    public Vector3 InverseTransformDirection(Vector3 worldDirection)
    {
        return WorldToLocal.MultiplyVector(worldDirection).normalized;
    }
}