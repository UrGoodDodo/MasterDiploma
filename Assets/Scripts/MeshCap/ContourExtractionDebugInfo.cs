using System.Collections.Generic;

public class ContourExtractionDebugInfo
{
    public int TotalEdges;
    public int DegreeOneCount;
    public int DegreeTwoCount;
    public int DegreeMoreThanTwoCount;
    public int ClosedLoopsCount;
    public int OpenChainsCount;
    public List<int> BranchingVertices = new List<int>();
    public Dictionary<int, List<int>> BranchingNeighbors = new Dictionary<int, List<int>>();
    public List<string> Warnings = new List<string>();
}