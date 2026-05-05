using System.Collections.Generic;
using UnityEngine;

public static class CapCreator
{
    public enum CapType
    {
        Fan,
        EarClipping,
        HoleAware
    }

    public static bool TriangulateCapAuto(List<List<int>> loops, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal)
    {
        var validation = CapLoopValidator.ValidateLoops(loops, mainVertices, normal);

        if (validation == null || !validation.HasAny) return false;

        List<ProjectedLoop> projectedLoops = CapProjection.ProjectLoopsTo2D(validation.ValidLoops, mainVertices, normal);
        List<CapRegion> regions = CapTopologyBuilder.BuildRegions(projectedLoops);

        return TriangulateCapRegions(regions, mainVertices, capVertices, outTriangles, outTriangleTypes, normal);
    }

    public static bool TriangulateCapRegions(List<CapRegion> regions, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal)
    {
        if (regions == null || regions.Count == 0) return false;

        bool anySuccess = false;

        for (int i = 0; i < regions.Count; i++)
        {
            CapRegion region = regions[i];

            if (region == null || region.Outer == null || region.Outer.Indices == null || region.Outer.Indices.Count < 3) continue;

            if (region.Holes == null || region.Holes.Count == 0)
            {
                ICapStrategy simpleStrategy = ChooseSimpleCapStrategy(region.Outer.Indices, mainVertices);
                simpleStrategy.TriangulateCap(region.Outer.Indices, mainVertices, capVertices, outTriangles, outTriangleTypes, normal);
                anySuccess = true;
            }
            else
            {
                CapLoopSet loopSet = new CapLoopSet();
                loopSet.OuterLoop = region.Outer.Indices;

                for (int h = 0; h < region.Holes.Count; h++)
                {
                    if (region.Holes[h] == null || region.Holes[h].Indices == null || region.Holes[h].Indices.Count < 3) continue;
                    loopSet.Holes.Add(region.Holes[h].Indices);
                }

                IHoleCapStrategy holeStrategy = new HoleAwareCap();

                if (holeStrategy.TriangulateCap(loopSet, mainVertices, capVertices, outTriangles, outTriangleTypes, normal)) anySuccess = true;
            }
        }

        return anySuccess;
    }

    private static ICapStrategy ChooseSimpleCapStrategy(List<int> loop, List<Vector3> mainVertices)
    {
        if (loop.Count == 3)
            return new FanCap();

        return new EarCap();
    }

    public static void TriangulateCapByType(CapType capType, List<int> loop, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal)
    {
        ICapStrategy capStrategy;

        switch (capType)
        {
            case CapType.Fan:
                capStrategy = new FanCap();
                break;

            case CapType.EarClipping:
                capStrategy = new EarCap();
                break;

            default:
                capStrategy = new EarCap();
                break;
        }

        capStrategy.TriangulateCap(loop, mainVertices, capVertices, outTriangles, outTriangleTypes, normal);
    }

    public static bool TriangulateCapSetByType(CapType capType, CapLoopSet loopSet, List<Vector3> mainVertices, List<Vector3> capVertices, List<int> outTriangles, List<SurfaceType> outTriangleTypes, Vector3 normal)
    {
        if (loopSet == null || !loopSet.IsValid || loopSet.OuterLoop == null || loopSet.OuterLoop.Count < 3)
            return false;

        if (capType != CapType.HoleAware)
        {
            TriangulateCapByType(capType, loopSet.OuterLoop, mainVertices, capVertices, outTriangles, outTriangleTypes, normal);
            return true;
        }

        IHoleCapStrategy strategy = new HoleAwareCap();
        return strategy.TriangulateCap(loopSet, mainVertices, capVertices, outTriangles, outTriangleTypes, normal);
    }

    
}