using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.ProBuilder.Shapes;

public class MeshSlicer : MonoBehaviour
{
    [Header("Target")]
    public Sliceable sliceableObject;

    [Header("Plane source")]
    public Transform planeTransform;

    [Header("Options")]
    public bool sliceOnStart = false;
    public bool disableOriginalAfterSlice = true;
    public bool usePlaneUp = true;

    private void Start()
    {
        if (sliceOnStart && sliceableObject != null)
            SliceUsingPlaneTransform();
    }

    public bool SliceUsingPlaneTransform()
    {
        if (planeTransform == null)
        {
            //Debug.Log("ыфваыва");
            return false;
        }

        Vector3 normal = usePlaneUp ? planeTransform.up : planeTransform.forward;
        Plane plane = new Plane(normal, planeTransform.position);

        return SliceTarget(sliceableObject, plane);
    }

    public bool SliceTarget(Sliceable sliceable, Plane plane)
    {
        if (sliceable == null || !sliceable.canBeSliced)
            return false;

        if (!MeshGeometrySlicer.TrySlice(sliceable, plane, out MeshSliceResult result))
            return false;

        GameObject target = sliceable.gameObject;

        MeshNObjCreator.CreateNewSubMeshObj(target, result.TopMesh, result.TopTriangleTypes, null, sliceable.capMaterial, "TopPart", true);

        MeshNObjCreator.CreateNewSubMeshObj(target, result.BottomMesh, result.BottomTriangleTypes, null, sliceable.capMaterial, "BotPart", true);

        if (disableOriginalAfterSlice)
            target.SetActive(false);

        return true;
    }
}