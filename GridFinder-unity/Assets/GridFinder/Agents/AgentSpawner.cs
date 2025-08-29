using Unity.Entities;
using Unity.Mathematics;

public struct AgentSpawner : IComponentData
{
    public Entity Prefab;
    public float3 SpawnPosition;
    public double NextSpawnTime;
    public float SpawnRate;     // in sec
}
