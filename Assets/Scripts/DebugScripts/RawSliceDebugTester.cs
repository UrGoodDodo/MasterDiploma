using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class RawSliceDebugTester : MonoBehaviour
{
    [SerializeField] private MeshFilter sourceMeshFilter;
    [SerializeField] private Transform planeTransform;
    [SerializeField] private Material mainMaterial;
    [SerializeField] private Material capMaterial;
    [SerializeField] private bool keepPositiveSide = true;

    [ContextMenu("Test Raw Slice")]
    private void TestRawSlice()
    {
        if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null)
        {
            Debug.LogError("Source MeshFilter is missing.");
            return;
        }

        Mesh sourceMesh = sourceMeshFilter.sharedMesh;

        int triangleCount = sourceMesh.triangles.Length / 3;
        List<SurfaceType> triangleTypes = new List<SurfaceType>(triangleCount);

        for (int i = 0; i < triangleCount; i++)
            triangleTypes.Add(SurfaceType.Main);

        RawMeshData raw = RawMeshConverter.FromMesh(sourceMesh, triangleTypes);
        RawTransformData rawTransform = new RawTransformData(sourceMeshFilter.transform);

        Vector3 planeNormal = planeTransform != null ? planeTransform.up : Vector3.up;

        Vector3 planePoint = planeTransform != null ? planeTransform.position : sourceMeshFilter.transform.position;

        Plane plane = new Plane(planeNormal, planePoint);

        bool ok = RawMeshGeometrySlicer.TryClip(
            raw,
            rawTransform,
            plane,
            keepPositiveSide,
            out RawMeshClipResult result
        );

        if (!ok || result == null || !result.HasMesh)
        {
            Debug.LogError("Raw slice failed or result is empty.");
            return;
        }

        Mesh slicedMesh = RawMeshConverter.ToMesh(result.MeshData, "Raw Slice Test");

        GameObject obj = new GameObject("Raw Slice Test");
        obj.transform.position = sourceMeshFilter.transform.position;
        obj.transform.rotation = sourceMeshFilter.transform.rotation;
        obj.transform.localScale = sourceMeshFilter.transform.localScale;

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        mf.sharedMesh = slicedMesh;

        MeshRenderer mr = obj.AddComponent<MeshRenderer>();

        if (mainMaterial != null && capMaterial != null)
            mr.sharedMaterials = new[] { mainMaterial, capMaterial };
        else
            mr.sharedMaterials = sourceMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;

        Debug.Log(
            $"Raw slice OK. Vertices: {result.MeshData.Vertices.Length}, " +
            $"Triangles: {result.MeshData.Triangles.Length / 3}, " +
            $"SurfaceTypes: {result.MeshData.TriangleSurfaceTypes.Length}"
        );
    }

    [ContextMenu("Test One Voronoi Cell")]
    private void TestOneCell()
    {
        if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null)
        {
            Debug.LogError("No mesh");
            return;
        }

        Mesh sourceMesh = sourceMeshFilter.sharedMesh;

        // 1. triangle types (всё Main)
        int triCount = sourceMesh.triangles.Length / 3;
        List<SurfaceType> types = new List<SurfaceType>(triCount);
        for (int i = 0; i < triCount; i++)
            types.Add(SurfaceType.Main);

        // 2. raw данные
        RawMeshData raw = RawMeshConverter.FromMesh(sourceMesh, types);
        RawTransformData rawTransform = new RawTransformData(sourceMeshFilter.transform);

        // 3. seeds (временно вручную)
        List<Vector3> seeds = new List<Vector3>
    {
        sourceMeshFilter.transform.position + Vector3.left * 0.2f,
        sourceMeshFilter.transform.position + Vector3.right * 0.2f,
        sourceMeshFilter.transform.position + Vector3.forward * 0.2f
    };

        Vector3 seed = seeds[0]; // одну клетку строим

        // 4. билд клетки
        bool ok = RawVoronoiCellBuilder.TryBuildRawCell(
            raw,
            rawTransform,
            seed,
            seeds,
            out RawMeshData cell
        );

        if (!ok || cell == null)
        {
            Debug.LogError("Cell build failed");
            return;
        }

        // 5. создаём меш
        Mesh mesh = RawMeshConverter.ToMesh(cell, "Raw Cell");

        GameObject obj = new GameObject("Raw Cell Test");
        obj.transform.position = sourceMeshFilter.transform.position;
        obj.transform.rotation = sourceMeshFilter.transform.rotation;
        obj.transform.localScale = sourceMeshFilter.transform.localScale;

        var mf = obj.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = obj.AddComponent<MeshRenderer>();

        if (mainMaterial != null && capMaterial != null)
            mr.sharedMaterials = new[] { mainMaterial, capMaterial };
        else
            mr.sharedMaterials = sourceMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;

        Debug.Log("Cell built OK");
    }

    [ContextMenu("Test All Voronoi Cells Raw")]
    private void TestAllCellsRaw()
    {
        if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null)
        {
            Debug.LogError("No mesh");
            return;
        }

        Mesh sourceMesh = sourceMeshFilter.sharedMesh;

        int triCount = sourceMesh.triangles.Length / 3;
        List<SurfaceType> types = new List<SurfaceType>(triCount);

        for (int i = 0; i < triCount; i++)
            types.Add(SurfaceType.Main);

        RawMeshData raw = RawMeshConverter.FromMesh(sourceMesh, types);
        RawTransformData rawTransform = new RawTransformData(sourceMeshFilter.transform);

        List<Vector3> seeds = new List<Vector3>
    {
        sourceMeshFilter.transform.position + Vector3.left * 0.25f,
        sourceMeshFilter.transform.position + Vector3.right * 0.25f,
        sourceMeshFilter.transform.position + Vector3.forward * 0.25f,
        sourceMeshFilter.transform.position + Vector3.back * 0.25f,
        sourceMeshFilter.transform.position + Vector3.up * 0.25f
    };

        GameObject root = new GameObject("Raw Voronoi Cells Test");

        int created = 0;

        for (int i = 0; i < seeds.Count; i++)
        {
            bool ok = RawVoronoiCellBuilder.TryBuildRawCell(
                raw,
                rawTransform,
                seeds[i],
                seeds,
                out RawMeshData cell
            );

            if (!ok || cell == null || !cell.IsValid)
            {
                Debug.LogWarning($"Cell {i} failed.");
                continue;
            }

            Mesh mesh = RawMeshConverter.ToMesh(cell, $"Raw Cell {i}");

            GameObject obj = new GameObject($"Raw Cell {i}");
            obj.transform.SetParent(root.transform);

            obj.transform.position = sourceMeshFilter.transform.position;
            obj.transform.rotation = sourceMeshFilter.transform.rotation;
            obj.transform.localScale = sourceMeshFilter.transform.localScale;

            MeshFilter mf = obj.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = obj.AddComponent<MeshRenderer>();

            if (mainMaterial != null && capMaterial != null)
                mr.sharedMaterials = new[] { mainMaterial, capMaterial };
            else
                mr.sharedMaterials = sourceMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;

            created++;
        }

        Debug.Log($"Raw Voronoi cells created: {created}/{seeds.Count}");
    }

    [ContextMenu("Test All Voronoi Cells Raw Parallel")]
    private void TestAllCellsRawParallel()
    {
        if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null)
        {
            Debug.LogError("No mesh");
            return;
        }

        Mesh sourceMesh = sourceMeshFilter.sharedMesh;

        int triCount = sourceMesh.triangles.Length / 3;
        List<SurfaceType> types = new List<SurfaceType>(triCount);

        for (int i = 0; i < triCount; i++)
            types.Add(SurfaceType.Main);

        RawMeshData raw = RawMeshConverter.FromMesh(sourceMesh, types);
        RawTransformData rawTransform = new RawTransformData(sourceMeshFilter.transform);

        List<Vector3> seeds = new List<Vector3>
    {
        sourceMeshFilter.transform.position + Vector3.left * 0.25f,
        sourceMeshFilter.transform.position + Vector3.right * 0.25f,
        sourceMeshFilter.transform.position + Vector3.forward * 0.25f,
        sourceMeshFilter.transform.position + Vector3.back * 0.25f,
        sourceMeshFilter.transform.position + Vector3.up * 0.25f
    };

        RawMeshData[] cells = new RawMeshData[seeds.Count];

        Parallel.For(0, seeds.Count, i =>
        {
            bool ok = RawVoronoiCellBuilder.TryBuildRawCell(
                raw,
                rawTransform,
                seeds[i],
                seeds,
                out RawMeshData cell
            );

            if (ok && cell != null && cell.IsValid)
                cells[i] = cell;
        });

        GameObject root = new GameObject("Raw Voronoi Cells Parallel Test");

        int created = 0;

        for (int i = 0; i < cells.Length; i++)
        {
            RawMeshData cell = cells[i];

            if (cell == null || !cell.IsValid)
                continue;

            Mesh mesh = RawMeshConverter.ToMesh(cell, $"Raw Parallel Cell {i}");

            GameObject obj = new GameObject($"Raw Parallel Cell {i}");
            obj.transform.SetParent(root.transform);

            obj.transform.position = sourceMeshFilter.transform.position;
            obj.transform.rotation = sourceMeshFilter.transform.rotation;
            obj.transform.localScale = sourceMeshFilter.transform.localScale;

            MeshFilter mf = obj.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = obj.AddComponent<MeshRenderer>();

            if (mainMaterial != null && capMaterial != null)
                mr.sharedMaterials = new[] { mainMaterial, capMaterial };
            else
                mr.sharedMaterials = sourceMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;

            created++;
        }

        Debug.Log($"Raw parallel Voronoi cells created: {created}/{seeds.Count}");
    }
}