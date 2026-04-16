using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ObjectSlicer : MonoBehaviour
{
    [Header("Blade Setup")]
    public Transform bladeBase;
    public Transform bladeTip;

    [Header("Slicing")]
    public LayerMask sliceableLayer;
    public float minCutDistance = 0.01f;

    [Header("Attack State")]
    public bool canSlice = false;

    private MeshSlicer meshSlicer;
    private Dictionary<Sliceable, Vector3> entryPoints = new();

    private void Awake()
    {
        meshSlicer = FindFirstObjectByType<MeshSlicer>();

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void SetSliceEnabled(bool value)
    {
        canSlice = value;

        if (!canSlice)
            entryPoints.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canSlice)
            return;

        if (!IsInLayerMask(other.gameObject.layer, sliceableLayer))
            return;

        var sliceable = other.GetComponent<Sliceable>();
        if (sliceable == null) return;

        entryPoints[sliceable] = bladeTip.position;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!canSlice)
            return;

        if (!IsInLayerMask(other.gameObject.layer, sliceableLayer))
            return;

        var sliceable = other.GetComponent<Sliceable>();
        if (sliceable == null) return;

        if (!entryPoints.ContainsKey(sliceable))
            return;

        Vector3 entry = entryPoints[sliceable];
        Vector3 exit = bladeTip.position;

        entryPoints.Remove(sliceable);

        Vector3 cutDir = exit - entry;
        if (cutDir.magnitude < minCutDistance)
            return;

        cutDir.Normalize();

        Vector3 bladeDir = (bladeTip.position - bladeBase.position).normalized;
        Vector3 normal = Vector3.Cross(cutDir, bladeDir).normalized;

        if (normal.sqrMagnitude < 0.0001f)
            return;

        Plane slicePlane = new Plane(normal, entry);

        if (meshSlicer != null)
        {
            meshSlicer.SliceTarget(sliceable, slicePlane);
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}