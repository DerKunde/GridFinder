using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace GridFinder.Targeting
{
[UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct TargetBootstrapSystem : ISystem
    {
        private EntityQuery targetQuery;
        private EntityQuery markerPrefabQuery;
        private EntityQuery markerInstanceQuery;

        public void OnCreate(ref SystemState state)
        {
            targetQuery = state.GetEntityQuery(typeof(TargetSingletonTag), typeof(TargetState));
            markerPrefabQuery = state.GetEntityQuery(typeof(TargetMarkerPrefab));
            markerInstanceQuery = state.GetEntityQuery(typeof(TargetMarkerInstanceTag));
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            // Ensure TargetState singleton exists
            if (targetQuery.IsEmptyIgnoreFilter)
            {
                var e = em.CreateEntity(typeof(TargetSingletonTag), typeof(TargetState));
                em.SetComponentData(e, new TargetState
                {
                    HasTarget = 0,
                    Cell = new int2(0, 0),
                    WorldPos = float3.zero,
                    Version = 1u
                });
            }

            // Wait until marker prefab authoring exists
            if (markerPrefabQuery.IsEmptyIgnoreFilter)
                return;

            // Ensure marker instance exists once
            if (!markerInstanceQuery.IsEmptyIgnoreFilter)
            {
                state.Enabled = false;
                return;
            }

            var prefabHolder = markerPrefabQuery.GetSingletonEntity();
            var prefab = em.GetComponentData<TargetMarkerPrefab>(prefabHolder).Prefab;

            var marker = em.Instantiate(prefab);
            
            // ensure it has transform
            if (!em.HasComponent<LocalTransform>(marker))
                em.AddComponentData(marker, LocalTransform.Identity);

            em.AddComponent<TargetMarkerInstanceTag>(marker);

            state.Enabled = false;
        }
    }

    public struct TargetMarkerInstanceTag : IComponentData { }
}