public class RawSliceResult
{
    public RawMeshData Top;
    public RawMeshData Bottom;

    public bool HasTop => Top != null && Top.IsValid;
    public bool HasBottom => Bottom != null && Bottom.IsValid;

    public static RawSliceResult Empty()
    {
        return new RawSliceResult
        {
            Top = null,
            Bottom = null
        };
    }
}