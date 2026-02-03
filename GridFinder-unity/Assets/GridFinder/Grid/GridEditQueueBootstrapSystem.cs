using Unity.Burst;
using Unity.Entities;

namespace GridFinder.Grid
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct GridEditQueueBootstrapSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<GridEditQueueTag>())
            {
                state.Enabled = false;
                return;
            }

            var e = state.EntityManager.CreateEntity(typeof(GridEditQueueTag));
            state.EntityManager.AddBuffer<GridEditCommand>(e);
            state.Enabled = false;
        }
    }
}