using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace GridFinder.Targeting
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct TargetMarkerFollowSystem : ISystem
    {
        private EntityQuery _targetQuery;
        private EntityQuery _markerQuery;
        private EntityQuery _markerPrefabQuery;

        public void OnCreate(ref SystemState state)
        {
            _targetQuery = state.GetEntityQuery(typeof(TargetSingletonTag), typeof(TargetState));
            _markerQuery = state.GetEntityQuery(typeof(TargetMarkerInstanceTag), typeof(LocalTransform));
            _markerPrefabQuery = state.GetEntityQuery(typeof(TargetMarkerPrefab));

            state.RequireForUpdate(_targetQuery);
            state.RequireForUpdate(_markerPrefabQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            // ensure marker instance exists
            if (_markerQuery.IsEmptyIgnoreFilter)
            {
                var prefab = em.GetComponentData<TargetMarkerPrefab>(_markerPrefabQuery.GetSingletonEntity()).Prefab;
                var marker = em.Instantiate(prefab);

                // make sure the new instance is discoverable by this system next frame
                em.AddComponent<TargetMarkerInstanceTag>(marker);

                // optional: if you want it to be hidden until HasTarget==1
                // var t0 = em.GetComponentData<LocalTransform>(marker);
                // t0.Scale = 0f;
                // em.SetComponentData(marker, t0);

                return; // wait one frame; keeps things simple
            }

            var targetEntity = _targetQuery.GetSingletonEntity();
            var target = em.GetComponentData<TargetState>(targetEntity);

            var markerEntity = _markerQuery.GetSingletonEntity();
            var t = em.GetComponentData<LocalTransform>(markerEntity);

            if (target.HasTarget == 0)
            {
                t.Scale = 0f;
                em.SetComponentData(markerEntity, t);
                return;
            }

            var prefabEntity = em.GetComponentData<TargetMarkerPrefab>(_markerPrefabQuery.GetSingletonEntity()).Prefab;
            var prefabScale = em.GetComponentData<LocalTransform>(prefabEntity).Scale;

            t.Position = target.WorldPos;
            t.Scale = prefabScale;
            em.SetComponentData(markerEntity, t);
        }
    }
}