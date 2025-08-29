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

            foreach (var sp in SystemAPI.Query<RefRW<AgentSpawner>>())
            {
                if (sp.ValueRO.NextSpawnTime < 0)
                {
                    sp.ValueRW.NextSpawnTime = elapsed + sp.ValueRO.SpawnRate;
                    continue;
                }

                // Prefab-Transform EINMAL lesen (enthält z.B. -90° um X und richtige Scale)
                var baseLt = state.EntityManager.GetComponentData<LocalTransform>(sp.ValueRO.Prefab);

                while (sp.ValueRO.NextSpawnTime <= elapsed)
                {
                    var e = ecb.Instantiate(sp.ValueRO.Prefab);

                    // Nur Position austauschen, Rotation/Scale beibehalten
                    baseLt.Position = sp.ValueRO.SpawnPosition + new float3(0, yOffset, 0);
                    ecb.SetComponent(e, baseLt);

                    sp.ValueRW.NextSpawnTime += sp.ValueRO.SpawnRate;
                }
            }

            ecb.Playback(state.EntityManager);
        }
    }
}