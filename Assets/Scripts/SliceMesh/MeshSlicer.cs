using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MeshSlicer : MonoBehaviour
{
    public GameObject testGameObj;

    public Material capMat;

    public int[] ClassifyVerticies(Vector3[] vertices, Transform objTransform, Plane plane, float eps = 1e-4f) 
    {
        int[] classified_vertices = new int[vertices.Length]; // 1 - выше, -1 - ниже, 0 - примерно на plane

        for (int i = 0; i < vertices.Length; i++) 
        {
            var worldPos = objTransform.TransformPoint(vertices[i]); // Переводим из локальных в мировые
            float distance = plane.GetDistanceToPoint(worldPos); // Вычисляем расстояние до пересекаемой плоскости
            
            if (Mathf.Abs(distance) < eps)
                classified_vertices[i] = 0;
            else if (distance > 0)
                classified_vertices[i] = 1;
            else
                classified_vertices[i] = -1;
        }

        return classified_vertices;
    }

    

    private void Start()
    {
        TestClassOnObj();
    }

    public void TestClassOnObj() 
    {
        MeshFilter mf = testGameObj.GetComponent<MeshFilter>();
        if (mf == null) return;
        Vector3[] vertices = mf.mesh.vertices;
        Plane testPlane = new Plane(Vector3.up, testGameObj.transform.position);

        var clasvert = ClassifyVerticies(vertices, testGameObj.transform, testPlane);
        
        int above = 0, onPlane = 0, below = 0;
        foreach (int v in clasvert)
        {
            if (v == 1) above++;
            else if (v == 0) onPlane++;
            else below++;
        }
        Debug.Log($"Above: {above}, OnPlane: {onPlane}, Below: {below}");

        TriangleSlicer.ClassifyTriangles(testGameObj, clasvert, testPlane, out List<Vector3> topVerts, out List<int> topTriangles, out List<Vector3> bottomVerts, out List<int> bottomTriangles, out HashSet<EdgeKey> topContourEdges, out HashSet<EdgeKey> bottomContourEdges);

        TriangleSlicer.MergeDuplicateVertices(ref topVerts, ref topContourEdges);
        TriangleSlicer.MergeDuplicateVertices(ref bottomVerts, ref bottomContourEdges);

        //ContourEdgeDebugger.ValidateContourEdges("TOP", topContourEdges, topVerts);
        //ContourEdgeDebugger.ValidateContourEdges("BOTTOM", bottomContourEdges, bottomVerts);

        var topLoops = CapCreator.ExtractLoopsFromEdges(topContourEdges);
        var bottomLoops = CapCreator.ExtractLoopsFromEdges(bottomContourEdges);

        List<Vector3> topCapVertices = new List<Vector3>();
        List<int> topCapTris = new List<int>();
        foreach (var loop in topLoops)
        {
            CapCreator.TriangulateCapByType(CapCreator.CapType.EarClipping, loop, topVerts, topCapVertices, topCapTris, -testPlane.normal);
        }

        List<Vector3> bottomCapVertices = new List<Vector3>();
        List<int> botCapTris = new List<int>();
        foreach (var loop in bottomLoops)
        {
            CapCreator.TriangulateCapByType(CapCreator.CapType.Fan, loop, bottomVerts, bottomCapVertices, botCapTris, testPlane.normal);
        }

        Mesh topMesh = MeshNObjCreator.CreateNewSubMesh(topVerts, topTriangles, topCapVertices, topCapTris, "TopPart");
        Mesh bottomMesh = MeshNObjCreator.CreateNewSubMesh(bottomVerts, bottomTriangles, bottomCapVertices, botCapTris, "BotPart");

        //MeshNObjCreator.CreateNewObj(testGameObj, topMesh, null);

        //MeshNObjCreator.CreateNewObj(testGameObj, bottomMesh, null);

        MeshNObjCreator.CreateNewSubMeshObj(testGameObj, topMesh, null, capMat, "TopPart");

        MeshNObjCreator.CreateNewSubMeshObj(testGameObj, bottomMesh, null, capMat, "BotPart");

        testGameObj.SetActive(false);
    }
}
