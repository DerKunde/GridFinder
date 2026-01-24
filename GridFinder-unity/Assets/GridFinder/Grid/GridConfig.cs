using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    public struct GridConfig : IComponentData
    {
        public int2 Size;
        public float CellSize;
        public float3 Origin;
        public uint Version;
    }
}