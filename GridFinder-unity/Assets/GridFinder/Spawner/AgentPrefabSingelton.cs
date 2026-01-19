using Unity.Entities;

namespace GridFinder.Spawner
{
    /// <summary>
    /// MVP registry: single prefab used for all spawns.
    /// Replace later with ContentId->Prefab map.
    /// </summary>
    public struct AgentPrefabSingleton : IComponentData
    {
        public Entity Prefab;
    }
}