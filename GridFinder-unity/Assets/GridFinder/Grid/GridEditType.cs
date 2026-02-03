using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    public enum GridEditType : byte
    {
        SetWalkable,
        SetZone,
        AddFeature,
        RemoveFeature,
        SetCost
    }

    /// <summary>Command to edit a rectangle [Min..Max] inclusive.</summary>
    public struct GridEditCommand : IBufferElementData
    {
        public GridEditType Type;
        public int2 Min;     // inclusive
        public int2 Max;     // inclusive
        public int Value;    // packed value (walkable/zone/feature/cost)
    }

    public struct GridEditQueueTag : IComponentData { }
}