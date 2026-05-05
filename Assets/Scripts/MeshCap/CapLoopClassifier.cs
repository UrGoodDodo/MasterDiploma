using System.Collections.Generic;
using UnityEngine;

public class CapLoopSet
{
    public List<int> OuterLoop;
    public List<List<int>> Holes = new List<List<int>>();

    public int OuterOriginalIndex = -1;
    public List<int> HoleOriginalIndices = new List<int>();

    public bool IsValid => OuterLoop != null && OuterLoop.Count >= 3;
    public bool HasHoles => Holes != null && Holes.Count > 0;
}

public static class CapLoopClassifier
{
    public static CapLoopSet ClassifyOuterAndHoles(ValidatedCapLoops validated)
    {
        CapLoopSet result = new CapLoopSet();

        if (validated == null || !validated.HasAny)
            return result;

        int outerIndex = validated.LargestLoopIndex;

        if (outerIndex < 0 || outerIndex >= validated.ValidLoops.Count)
            return result;

        result.OuterLoop = validated.ValidLoops[outerIndex];
        result.OuterOriginalIndex = outerIndex;

        for (int i = 0; i < validated.ValidLoops.Count; i++)
        {
            if (i == outerIndex)
                continue;

            result.Holes.Add(validated.ValidLoops[i]);
            result.HoleOriginalIndices.Add(i);
        }

        return result;
    }
}