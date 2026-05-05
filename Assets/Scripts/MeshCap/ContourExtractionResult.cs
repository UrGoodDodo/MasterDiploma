using System.Collections.Generic;

public class ContourExtractionResult
{
    public List<List<int>> ClosedLoops = new List<List<int>>();
    public List<List<int>> OpenChains = new List<List<int>>();
    public List<int> BranchingVertices = new List<int>();
    public Dictionary<int, List<int>> BranchingNeighbors = new Dictionary<int, List<int>>();
    public List<string> Warnings = new List<string>();
    public bool HasClosedLoops => ClosedLoops.Count > 0;
    public bool HasOpenChains => OpenChains != null && OpenChains.Count > 0;
    public bool HasBranching => BranchingVertices != null && BranchingVertices.Count > 0;

}