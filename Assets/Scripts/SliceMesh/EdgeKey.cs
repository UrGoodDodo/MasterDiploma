using UnityEngine;

public struct EdgeKey
{
    public int A;
    public int B;

    public EdgeKey(int a, int b)
    {
        //(5,8) и (8,5) давали один ключ
        if (a < b)
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }
    }

    public override bool Equals(object obj)
    {
        if (!(obj is EdgeKey other)) return false;
        return A == other.A && B == other.B;
    }

    public override int GetHashCode()
    {
        return A.GetHashCode() ^ B.GetHashCode();
    }

    public static bool operator ==(EdgeKey a, EdgeKey b) => a.Equals(b);
    public static bool operator !=(EdgeKey a, EdgeKey b) => !a.Equals(b);

}
