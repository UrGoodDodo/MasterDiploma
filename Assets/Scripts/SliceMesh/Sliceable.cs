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

    public MeshFilter MeshFilter => GetComponent<MeshFilter>();
    public MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();
}