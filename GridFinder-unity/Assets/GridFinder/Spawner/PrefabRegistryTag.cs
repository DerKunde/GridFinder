using Unity.Entities;

namespace GridFinder.Spawner
{
    public struct PrefabRegistryTag : IComponentData { }

    public struct PrefabRegistryEntry : IBufferElementData
    {
        public int Id;
        public Entity Prefab;
    }
}