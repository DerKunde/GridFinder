using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace GridFinder.Agents
{
    [BurstCompile]
    public partial struct AgentSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            double elapsed = SystemAPI.Time.ElapsedTime;
            const float yOffset = 0.01f; // gegen Z-Fighting mit dem Grid

            foreach (var agentSpawner in SystemAPI.Query<RefRW<AgentSpawner>>())
            {
                if (agentSpawner.ValueRO.NextSpawnTime < 0)
                {
                    agentSpawner.ValueRW.NextSpawnTime = elapsed + agentSpawner.ValueRO.SpawnRate;
                    continue;
                }

                // Prefab-Transform EINMAL lesen (enthält z.B. -90° um X und richtige Scale)
                var baseLt = state.EntityManager.GetComponentData<LocalTransform>(agentSpawner.ValueRO.Prefab);

                while (agentSpawner.ValueRO.NextSpawnTime <= elapsed)
                {
                    var e = ecb.Instantiate(agentSpawner.ValueRO.Prefab);

                    // Nur Position austauschen, Rotation/Scale beibehalten
                    baseLt.Position = agentSpawner.ValueRO.SpawnPosition + new float3(0, yOffset, 0);
                    ecb.SetComponent(e, baseLt);

                    agentSpawner.ValueRW.NextSpawnTime += agentSpawner.ValueRO.SpawnRate;
                }
            }

            ecb.Playback(state.EntityManager);
        }
    }
}