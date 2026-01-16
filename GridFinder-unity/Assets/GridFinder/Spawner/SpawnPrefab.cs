using Unity.Entities;

namespace GridFinder.Spawner
{
    public struct SpawnPrefab : IComponentData
    {
        public Entity Prefab;
    }
}