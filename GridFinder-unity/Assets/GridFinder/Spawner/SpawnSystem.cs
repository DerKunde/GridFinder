using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace GridFinder.Spawner
{
    /// <summary>
    /// Consumes SpawnCommand buffer and instantiates entities.
    /// </summary>
    [BurstCompile]
    public partial struct SpawnSystem : ISystem
    {
        private EntityQuery _queueQuery;

        public void OnCreate(ref SystemState state)
        {
            _queueQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<SpawnCommandQueueTag>(),
                ComponentType.ReadWrite<SpawnCommandBufferElement>()
            );

            state.RequireForUpdate(_queueQuery);
            state.RequireForUpdate<AgentPrefabSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var queueEntity = _queueQuery.GetSingletonEntity();
            var buffer = state.EntityManager.GetBuffer<SpawnCommandBufferElement>(queueEntity);

            if (buffer.IsEmpty)
                return;

            var prefab = SystemAPI.GetSingleton<AgentPrefabSingleton>().Prefab;

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            for (int i = 0; i < buffer.Length; i++)
            {
                var cmd = buffer[i].Value;

                var spawned = ecb.Instantiate(prefab);

                ecb.SetComponent(spawned, LocalTransform.FromPositionRotationScale(
                    cmd.WorldPos,
                    cmd.WorldRot,
                    0.1f
                ));

                // Optional metadata
                ecb.AddComponent(spawned, new SpawnedContentId { Value = cmd.ContentId });
                ecb.AddComponent(spawned, new GridCellIndex { Value = cmd.GridCellIndex });
                ecb.AddComponent(spawned, new SpawnRequestId { Value = cmd.RequestId });
            }

            buffer.Clear();
            ecb.Playback(state.EntityManager);
        }
    }
}