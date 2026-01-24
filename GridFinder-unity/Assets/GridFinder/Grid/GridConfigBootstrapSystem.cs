using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    /// <summary>
    /// Creates and initializes the GridConfig ECS singleton from GridSettings.
    /// Runs once on startup.
    /// </summary>
    [BurstCompile]
    public partial struct GridConfigBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Run once
            state.RequireForUpdate<GridConfigBootstrapTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            if (SystemAPI.HasSingleton<GridConfig>())
            {
                state.Enabled = false;
                return;
            }

            // Read GridSettings from managed world
            var settings = GridSettings.Instance; // see note below

            var sizeInCells = new int2(
                (int)math.round(settings.WorldSizeXZ.Value.x / settings.CellSize.Value),
                (int)math.round(settings.WorldSizeXZ.Value.y / settings.CellSize.Value)
            );

            var entity = em.CreateEntity(typeof(GridConfig));
            em.SetComponentData(entity, new GridConfig
            {
                Size = sizeInCells,
                CellSize = settings.CellSize.Value,
                Origin = settings.CenterWorld.Value -
                         new float3(sizeInCells.x, 0, sizeInCells.y) * settings.CellSize.Value * 0.5f,
                Version = 1u
            });

            state.Enabled = false;
        }
    }

    /// <summary>
    /// Dummy tag to ensure controlled startup.
    /// </summary>
    public struct GridConfigBootstrapTag : IComponentData { }
}