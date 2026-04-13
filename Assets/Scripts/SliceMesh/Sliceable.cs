using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Sliceable : MonoBehaviour
{
    [Header("Settings")]
    public bool canBeSliced = true;

    [Header("Materials")]
    public Material capMaterial;

    [Header("Options")]
    public bool generateCollider = true;

    [HideInInspector]
    public List<SurfaceType> triangleSurfaceTypes = new List<SurfaceType>();

    public MeshFilter MeshFilter => GetComponent<MeshFilter>();
    public MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

    public void EnsureTriangleTypesInitialized()
    {
        Mesh mesh = MeshFilter != null ? MeshFilter.sharedMesh : null;
        if (mesh == null)
            return;

        int triangleCount = mesh.triangles.Length / 3;

        // Если список уже синхронизирован с мешем — ничего не делаем
        if (triangleSurfaceTypes != null && triangleSurfaceTypes.Count == triangleCount)
            return;

        // Инициализация по умолчанию: весь исходный объект считается Main
        triangleSurfaceTypes = new List<SurfaceType>(triangleCount);
        for (int i = 0; i < triangleCount; i++)
        {
            triangleSurfaceTypes.Add(SurfaceType.Main);
        }
    }
}