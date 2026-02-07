using Unity.Entities;

namespace GridFinder.Spawner
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct TargetSpawnRequestBootstrapSystem : ISystem
    {
        private EntityQuery q;

        public void OnCreate(ref SystemState state)
        {
            q = state.GetEntityQuery(typeof(TargetSpawnRequest));
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!q.IsEmptyIgnoreFilter)
            {
                state.Enabled = false;
                return;
            }

            var e = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<TargetSpawnRequest>(e);
            state.Enabled = false;
        }
    }

}