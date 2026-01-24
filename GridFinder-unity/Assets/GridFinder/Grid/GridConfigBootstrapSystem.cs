using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    [BurstCompile]
    public partial struct GridConfigBootstrapSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // If already created, stop
            if (SystemAPI.HasSingleton<GridConfig>())
            {
                state.Enabled = false;
                return;
            }

            var settings = GridSettings.Instance;
            if (settings == null)
                return; // wait until AppInstaller created settings

            var sizeInCells = settings.GridSizeInCells.Value;
            var cs = math.max(0.0001f, settings.CellSize.Value);
            var center = settings.CenterWorld.Value;

            var origin = center - new float3(
                sizeInCells.x * cs * 0.5f,
                0f,
                sizeInCells.y * cs * 0.5f
            );

            var e = state.EntityManager.CreateEntity(typeof(GridConfig));
            state.EntityManager.SetComponentData(e, new GridConfig
            {
                Size = sizeInCells,
                CellSize = cs,
                Origin = origin,
                Version = 1u
            });

            state.Enabled = false;
        }
    }
}