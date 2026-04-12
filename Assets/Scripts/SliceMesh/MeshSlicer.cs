using System.Collections.Generic;
using UnityEngine;

public class MeshSlicer : MonoBehaviour
{
    [Header("Target")]
    public Sliceable sliceableObject;

    [Header("Plane source")]
    public Transform planeTransform;

    [Header("Options")]
    public bool sliceOnStart = false;
    public bool disableOriginalAfterSlice = true;
    public bool usePlaneUp = true; // если false → forward

    private void Start()
    {
        if (sliceOnStart && sliceableObject != null)
            SliceUsingPlaneTransform();
    }

    public bool SliceUsingPlaneTransform()
    {
        if (planeTransform == null)
        {
            Debug.LogWarning("Plane transform not assigned");
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

        GameObject target = sliceable.gameObject;

        if (!TryBuildSlice(target, plane, out Mesh topMesh, out Mesh bottomMesh))
            return false;

        MeshNObjCreator.CreateNewSubMeshObj(target, topMesh, null, sliceable.capMaterial, "TopPart", true);
        MeshNObjCreator.CreateNewSubMeshObj(target, bottomMesh, null, sliceable.capMaterial, "BotPart", true);

        target.SetActive(false);

        return true;
    }

    private bool TryBuildSlice(GameObject target, Plane plane, out Mesh topMesh, out Mesh bottomMesh)
    {
        topMesh = null;
        bottomMesh = null;

        MeshFilter mf = target.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return false;

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        int[] classified = ClassifyVertices(vertices, target.transform, plane);

        if (!HasBothSides(classified))
        {
            Debug.LogWarning("Plane does not cut object");
            return false;
        }

        TriangleSlicer.ClassifyTriangles(
            target,
            classified,
            plane,
            out List<Vector3> topVerts,
            out List<int> topTris,
            out List<Vector3> botVerts,
            out List<int> botTris,
            out HashSet<EdgeKey> topEdges,
            out HashSet<EdgeKey> botEdges
        );

        TriangleSlicer.MergeDuplicateVertices(ref topVerts, ref topEdges);
        TriangleSlicer.MergeDuplicateVertices(ref botVerts, ref botEdges);

        var topLoops = CapCreator.ExtractLoopsFromEdges(topEdges);
        var botLoops = CapCreator.ExtractLoopsFromEdges(botEdges);

        //CAP TOP
        List<Vector3> topCapVerts = new List<Vector3>();
        List<int> topCapTris = new List<int>();

        foreach (var loop in topLoops)
        {
            CapCreator.TriangulateCapByType(
                CapCreator.CapType.EarClipping,
                loop,
                topVerts,
                topCapVerts,
                topCapTris,
                -plane.normal
            );
        }

        //CAP BOTTOM
        List<Vector3> botCapVerts = new List<Vector3>();
        List<int> botCapTris = new List<int>();

        foreach (var loop in botLoops)
        {
            CapCreator.TriangulateCapByType(
                CapCreator.CapType.EarClipping,
                loop,
                botVerts,
                botCapVerts,
                botCapTris,
                plane.normal
            );
        }

        topMesh = MeshNObjCreator.CreateNewSubMesh(
            topVerts,
            topTris,
            topCapVerts,
            topCapTris,
            target,
            "TopPart"
        );

        bottomMesh = MeshNObjCreator.CreateNewSubMesh(
            botVerts,
            botTris,
            botCapVerts,
            botCapTris,
            target,
            "BotPart"
        );

        return true;
    }

    private int[] ClassifyVertices(Vector3[] vertices, Transform t, Plane plane, float eps = 1e-4f)
    {
        int[] result = new int[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 world = t.TransformPoint(vertices[i]);
            float d = plane.GetDistanceToPoint(world);

            if (Mathf.Abs(d) < eps) result[i] = 0;
            else if (d > 0) result[i] = 1;
            else result[i] = -1;
        }

        return result;
    }

    private bool HasBothSides(int[] classified)
    {
        bool above = false;
        bool below = false;

        foreach (var c in classified)
        {
            if (c > 0) above = true;
            else if (c < 0) below = true;

            if (above && below) return true;
        }

        return false;
    }
}