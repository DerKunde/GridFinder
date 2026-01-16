using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace GridFinder.Spawner
{
    [BurstCompile]
    public partial struct SpawnFromBufferSystem : ISystem
    {
        private EntityQuery _querry;

        public void OnCreate(ref SystemState state)
        {
            _querry = state.GetEntityQuery(
                ComponentType.ReadOnly<SpawnPrefab>(),
                ComponentType.ReadWrite<SpawnRequest>());

            state.RequireForUpdate(_querry);
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var singleton = _querry.GetSingletonEntity();

            var prefab = state.EntityManager.GetComponentData<SpawnPrefab>(singleton).Prefab;
            var requests = state.EntityManager.GetBuffer<SpawnRequest>(singleton);

            if (requests.Length == 0)
                return;

            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // Batch: instantiate + set transform
            for (int i = 0; i < requests.Length; i++)
            {
                var r = requests[i];
                var spawned = ecb.Instantiate(prefab);

                ecb.SetComponent(spawned, LocalTransform.FromPositionRotationScale(r.Position, r.Rotation, r.Scale));
            }

            requests.Clear();
        }
    }
}