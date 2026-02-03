using Unity.Burst;
using Unity.Entities;

namespace GridFinder.GridInput
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct ToolModeBootstrapSystem : ISystem
    {
        private EntityQuery singletonQuery;
        
        public void OnCreate(ref SystemState state)
        {
            // Cache query once (owned by system; do NOT dispose manually)
            singletonQuery = state.GetEntityQuery(ComponentType.ReadOnly<ToolModeSingletonTag>());
        }
        
        public void OnUpdate(ref SystemState state)
        {
            // If singleton already exists, stop
            if (!singletonQuery.IsEmptyIgnoreFilter)
            {
                state.Enabled = false;
                return;
            }

            var e = state.EntityManager.CreateEntity(
                typeof(ToolModeSingletonTag),
                typeof(ToolModeConfig)
            );

            state.EntityManager.SetComponentData(e, new ToolModeConfig
            {
                Mode = ToolModeType.SetTarget,
                PrimaryId = 0,
                Flags = 0,
                Brush = 1
            });

            state.Enabled = false;
        }
    }
}