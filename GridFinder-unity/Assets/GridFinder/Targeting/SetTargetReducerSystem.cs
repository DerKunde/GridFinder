using GridFinder.Spawner;
using Unity.Burst;
using Unity.Entities;

namespace GridFinder.Targeting
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct TargetSpawnReducerSystem : ISystem
    {
        private EntityQuery requestQuery;
        private EntityQuery targetQuery;

        public void OnCreate(ref SystemState state)
        {
            requestQuery = state.GetEntityQuery(typeof(TargetSpawnRequest));
            targetQuery  = state.GetEntityQuery(typeof(TargetSingletonTag), typeof(TargetState));
        }

        public void OnUpdate(ref SystemState state)
        {
            if (requestQuery.IsEmptyIgnoreFilter || targetQuery.IsEmptyIgnoreFilter)
                return;

            var em = state.EntityManager;

            var reqEntity = requestQuery.GetSingletonEntity();
            var requests  = em.GetBuffer<TargetSpawnRequest>(reqEntity);

            if (requests.Length == 0)
                return;

            // Last request wins
            var req = requests[requests.Length - 1];

            var targetEntity = targetQuery.GetSingletonEntity();
            var target = em.GetComponentData<TargetState>(targetEntity);

            target.HasTarget = 1;
            target.WorldPos = req.WorldPos;
            target.Version++;

            em.SetComponentData(targetEntity, target);

            // Optional milestone shortcut
            if (req.AssignToAllAgents != 0)
            {
                // später: AgentTargetSyncSystem
            }

            requests.Clear();
        }
    }
}