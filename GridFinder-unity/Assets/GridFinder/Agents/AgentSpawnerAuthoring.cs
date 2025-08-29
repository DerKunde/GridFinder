using Unity.Entities;
using UnityEngine;

namespace GridFinder.Agents
{
    public class AgentSpawnerAuthoring : MonoBehaviour
    {
        public GameObject prefab;
        [Min(0.01f)] public float spawnRate = 0.25f; // e.g. 4 per second
        public Vector3 SpawnerPosition;
        public float zOffest = 0.02f;

        private void Awake()
        {
            SpawnerPosition = new Vector3(transform.position.x, transform.position.y, zOffest);
        }
    }

    class AgentSpawnerBaker : Baker<AgentSpawnerAuthoring>
    {
        public override void Bake(AgentSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new AgentSpawner
            {
                Prefab         = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                SpawnPosition  = authoring.SpawnerPosition,
                SpawnRate      = authoring.spawnRate,
                NextSpawnTime  = -1,
            });
        }
    }
}