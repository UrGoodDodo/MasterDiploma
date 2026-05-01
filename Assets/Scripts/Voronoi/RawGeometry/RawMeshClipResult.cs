public class RawMeshClipResult
{
    public RawMeshData MeshData;
    public bool IsEmpty;

    public bool HasMesh => MeshData != null && MeshData.IsValid && !IsEmpty;

    public static RawMeshClipResult Empty()
    {
        return new RawMeshClipResult
        {
            MeshData = null,
            IsEmpty = true
        };
    }

    public static RawMeshClipResult FromMeshData(RawMeshData meshData)
    {
        bool isEmpty = meshData == null || !meshData.IsValid;

        return new RawMeshClipResult
        {
            MeshData = meshData,
            IsEmpty = isEmpty
        };
    }
}