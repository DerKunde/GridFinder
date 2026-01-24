using GridFinder.Grid;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Installation
{
    [BurstCompile]
    public partial struct GridConfigBootstrapSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<GridConfig>())
            {
                state.Enabled = false;
                return;
            }

            var e = state.EntityManager.CreateEntity(typeof(GridConfig));
            state.EntityManager.SetComponentData(e, new GridConfig
            {
                Size = new int2(1000, 1000),
                CellSize = 0.5f,          // Default; später per UI/Input anpassbar
                Origin = new float3(0, 0, 0)
            });

            state.Enabled = false;
        }
    }
}