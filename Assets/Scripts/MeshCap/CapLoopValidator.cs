using System.Collections.Generic;
using UnityEngine;

public class ValidatedCapLoops
{
    public List<List<int>> ValidLoops = new List<List<int>>();
    public List<float> Areas = new List<float>();
    public List<float> Lengths = new List<float>();

    public int LargestLoopIndex = -1;

    public bool HasAny => ValidLoops.Count > 0;
    public bool HasMultipleLoops => ValidLoops.Count > 1;
}

public static class CapLoopValidator
{
    public static ValidatedCapLoops ValidateLoops(
        List<List<int>> loops,
        List<Vector3> vertices,
        Vector3 capNormal,
        float minArea = 0.000001f,
        float minLength = 0.0001f
    )
    {
        ValidatedCapLoops result = new ValidatedCapLoops();

        if (loops == null || vertices == null || vertices.Count == 0)
            return result;

        float largestAbsArea = 0f;

        for (int i = 0; i < loops.Count; i++)
        {
            List<int> cleaned = CleanLoop(loops[i]);

            if (cleaned.Count < 3)
                continue;

            if (!AllIndicesValid(cleaned, vertices))
                continue;

            float length = EstimateLength(cleaned, vertices);
            float area = EstimateSignedArea3D(cleaned, vertices, capNormal);

            if (length < minLength)
                continue;

            if (Mathf.Abs(area) < minArea)
                continue;

            int newIndex = result.ValidLoops.Count;

            result.ValidLoops.Add(cleaned);
            result.Areas.Add(area);
            result.Lengths.Add(length);

            if (Mathf.Abs(area) > largestAbsArea)
            {
                largestAbsArea = Mathf.Abs(area);
                result.LargestLoopIndex = newIndex;
            }
        }

        return result;
    }

    private static List<int> CleanLoop(List<int> loop)
    {
        List<int> cleaned = new List<int>();

        if (loop == null)
            return cleaned;

        for (int i = 0; i < loop.Count; i++)
        {
            int index = loop[i];

            if (cleaned.Count == 0 || cleaned[cleaned.Count - 1] != index)
                cleaned.Add(index);
        }

        if (cleaned.Count > 1 && cleaned[0] == cleaned[cleaned.Count - 1])
            cleaned.RemoveAt(cleaned.Count - 1);

        return cleaned;
    }

    private static bool AllIndicesValid(List<int> loop, List<Vector3> vertices)
    {
        for (int i = 0; i < loop.Count; i++)
        {
            int index = loop[i];

            if (index < 0 || index >= vertices.Count)
                return false;
        }

        return true;
    }

    private static float EstimateLength(List<int> loop, List<Vector3> vertices)
    {
        float length = 0f;

        for (int i = 0; i < loop.Count; i++)
        {
            int a = loop[i];
            int b = loop[(i + 1) % loop.Count];

            length += Vector3.Distance(vertices[a], vertices[b]);
        }

        return length;
    }

    private static float EstimateSignedArea3D(
        List<int> loop,
        List<Vector3> vertices,
        Vector3 normal
    )
    {
        if (loop == null || loop.Count < 3)
            return 0f;

        Vector3 areaVector = Vector3.zero;

        for (int i = 0; i < loop.Count; i++)
        {
            Vector3 current = vertices[loop[i]];
            Vector3 next = vertices[loop[(i + 1) % loop.Count]];

            areaVector += Vector3.Cross(current, next);
        }

        return Vector3.Dot(areaVector, normal.normalized) * 0.5f;
    }
}