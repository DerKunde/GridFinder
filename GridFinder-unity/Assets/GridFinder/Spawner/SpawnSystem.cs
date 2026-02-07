using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace GridFinder.Spawner
{
    /// <summary>
    /// Consumes SpawnCommand buffer and instantiates entities.
    /// Resolves prefabs via PrefabRegistry using ContentId.
    /// </summary>
    [BurstCompile]
    public partial struct SpawnSystem : ISystem
    {
        private EntityQuery _queueQuery;
        private EntityQuery _registryQuery;

        public void OnCreate(ref SystemState state)
        {
            _queueQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<SpawnCommandQueueTag>(),
                ComponentType.ReadWrite<SpawnCommandBufferElement>()
            );

            _registryQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<PrefabRegistryTag>(),
                ComponentType.ReadOnly<PrefabRegistryEntry>()
            );

            state.RequireForUpdate(_queueQuery);
            state.RequireForUpdate(_registryQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            var queueEntity = _queueQuery.GetSingletonEntity();
            var buffer = state.EntityManager.GetBuffer<SpawnCommandBufferElement>(queueEntity);

            if (buffer.IsEmpty)
                return;

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            for (int i = 0; i < buffer.Length; i++)
            {
                var cmd = buffer[i].Value;

                var prefab = FindPrefab(state.EntityManager, cmd.ContentId);
                Debug.Log($"[SpawnSystem] ContentID: {cmd.ContentId}");
                if (prefab == Entity.Null)
                {
                    Debug.LogWarning($"[SpawnSystem] No prefab found for ContentId {cmd.ContentId}. Skipping spawn.");
                    continue;
                }

                var prefabScale = state.EntityManager.GetComponentData<LocalTransform>(prefab).Scale;

                var spawned = ecb.Instantiate(prefab);

                ecb.SetComponent(spawned, LocalTransform.FromPositionRotationScale(
                    cmd.WorldPos,
                    cmd.WorldRot,
                    prefabScale
                ));

                ecb.AddComponent(spawned, new SpawnedContentId { Value = cmd.ContentId });
                ecb.AddComponent(spawned, new GridCellIndex { Value = cmd.GridCellIndex });
                ecb.AddComponent(spawned, new SpawnRequestId { Value = cmd.RequestId });
            }

            buffer.Clear();
            ecb.Playback(state.EntityManager);
        }

        private Entity FindPrefab(EntityManager em, int id)
        {
            var registryEntity = _registryQuery.GetSingletonEntity();
            var entries = em.GetBuffer<PrefabRegistryEntry>(registryEntity);

            for (int i = 0; i < entries.Length; i++)
                if (entries[i].Id == id)
                    return entries[i].Prefab;

            return Entity.Null;
        }
    }
}