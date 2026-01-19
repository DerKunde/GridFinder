using Unity.Entities;

namespace GridFinder.Spawner
{
    public struct SpawnedContentId : IComponentData
    {
        public int Value;
    }

    public struct GridCellIndex : IComponentData
    {
        public int Value;
    }

    public struct SpawnRequestId : IComponentData
    {
        public uint Value;
    }
}