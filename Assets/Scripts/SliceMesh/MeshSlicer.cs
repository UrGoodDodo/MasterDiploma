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


        if (!TryBuildSlice(sliceable, plane, out Mesh topMesh, out List<SurfaceType> topFinalTriangleTypes, out Mesh bottomMesh, out List<SurfaceType> bottomFinalTriangleTypes))
            return false;

        GameObject target = sliceable.gameObject;


        MeshNObjCreator.CreateNewSubMeshObj(target, topMesh, topFinalTriangleTypes, null, sliceable.capMaterial, "TopPart", true);
        MeshNObjCreator.CreateNewSubMeshObj(target, bottomMesh, bottomFinalTriangleTypes, null, sliceable.capMaterial, "BotPart", true);

        target.SetActive(false);

        return true;
    }

    private bool TryBuildSlice(Sliceable sliceable, Plane plane, out Mesh topMesh, out List<SurfaceType> topFinalTriangleTypes, out Mesh bottomMesh, out List<SurfaceType> bottomFinalTriangleTypes)
    {
        topMesh = null;
        bottomMesh = null;
        topFinalTriangleTypes = null;
        bottomFinalTriangleTypes = null;

        if (sliceable == null)
            return false;

        GameObject target = sliceable.gameObject;

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

        sliceable.EnsureTriangleTypesInitialized();

        TriangleSlicer.ClassifyTriangles(
            target,
            classified,
            plane,
            sliceable.triangleSurfaceTypes,
            out List<Vector3> topVerts,
            out List<Vector2> topUVs,
            out List<int> topTris,
            out List<SurfaceType> topTriangleTypes,
            out List<Vector3> botVerts,
            out List<Vector2> botUVs,
            out List<int> botTris,
            out List<SurfaceType> botTriangleTypes,
            out HashSet<EdgeKey> topEdges,
            out HashSet<EdgeKey> botEdges
        );

        var mergedTopEdges = TriangleSlicer.RemapContourEdgesByPosition(topVerts, topEdges);
        var mergedBotEdges = TriangleSlicer.RemapContourEdgesByPosition(botVerts, botEdges);

        var topLoops = CapCreator.ExtractLoopsFromEdges(mergedTopEdges);
        var botLoops = CapCreator.ExtractLoopsFromEdges(mergedBotEdges);
        Vector3 localPlaneNormal = target.transform.InverseTransformDirection(plane.normal).normalized;

        //CAP TOP
        List<Vector3> topCapVerts = new List<Vector3>();
        List<int> topCapTris = new List<int>();
        List<SurfaceType> topCapTriangleTypes = new List<SurfaceType>();

        foreach (var loop in topLoops)
        {
            CapCreator.TriangulateCapByType(
                CapCreator.CapType.EarClipping,
                loop,
                topVerts,
                topCapVerts,
                topCapTris,
                topCapTriangleTypes,
                -localPlaneNormal
            );
        }

        topFinalTriangleTypes = new List<SurfaceType>();
        topFinalTriangleTypes.AddRange(topTriangleTypes);
        topFinalTriangleTypes.AddRange(topCapTriangleTypes);

        //CAP BOTTOM
        List<Vector3> botCapVerts = new List<Vector3>();
        List<int> botCapTris = new List<int>();
        List<SurfaceType> botCapTriangleTypes = new List<SurfaceType>();


        foreach (var loop in botLoops)
        {
            CapCreator.TriangulateCapByType(
                CapCreator.CapType.EarClipping,
                loop,
                botVerts,
                botCapVerts,
                botCapTris,
                botCapTriangleTypes,
                localPlaneNormal
            );
        }

        bottomFinalTriangleTypes = new List<SurfaceType>();
        bottomFinalTriangleTypes.AddRange(botTriangleTypes);
        bottomFinalTriangleTypes.AddRange(botCapTriangleTypes);

        topMesh = MeshNObjCreator.CreateNewSubMesh(
            topVerts,
            topUVs,
            topTris,
            topTriangleTypes,
            topCapVerts,
            topCapTris,
            topCapTriangleTypes,
            "TopPart"
        );

        bottomMesh = MeshNObjCreator.CreateNewSubMesh(
            botVerts,
            botUVs,
            botTris,
            botTriangleTypes,
            botCapVerts,
            botCapTris,
            botCapTriangleTypes,
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