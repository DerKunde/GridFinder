using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct GridDataSystem : ISystem
    {
        private NativeArray<GridCellData> cells;
        private int2 size;

        private EntityQuery gridConfigQuery;
        private EntityQuery gridDataInfoQuery;

        public void OnCreate(ref SystemState state)
        {
            size = int2.zero;

            gridConfigQuery = state.GetEntityQuery(ComponentType.ReadOnly<GridConfig>());
            gridDataInfoQuery = state.GetEntityQuery(ComponentType.ReadOnly<GridDataInfo>());
        }

        public void OnDestroy(ref SystemState state)
        {
            if (cells.IsCreated)
                cells.Dispose();

            // IMPORTANT: do not dispose queries from GetEntityQuery()
        }

        public void OnUpdate(ref SystemState state)
        {
            if (gridConfigQuery.IsEmptyIgnoreFilter)
                return;

            var cfg = gridConfigQuery.GetSingleton<GridConfig>();

            if (!cells.IsCreated || cfg.Size.x != size.x || cfg.Size.y != size.y)
            {
                if (cells.IsCreated)
                    cells.Dispose();

                size = cfg.Size;
                cells = new NativeArray<GridCellData>(size.x * size.y, Allocator.Persistent);

                for (int i = 0; i < cells.Length; i++)
                {
                    cells[i] = new GridCellData
                    {
                        Walkable = 1,
                        Zones = ZoneMask.General,
                        Features = FeatureMask.None,
                        Cost = 1
                    };
                }

                EnsureGridDataInfoSingleton(state.EntityManager, size);
            }

            state.Enabled = false;
        }

        private void EnsureGridDataInfoSingleton(EntityManager em, int2 gridSize)
        {
            if (gridDataInfoQuery.IsEmptyIgnoreFilter)
            {
                var e = em.CreateEntity(typeof(GridDataInfo));
                em.SetComponentData(e, new GridDataInfo { Size = gridSize, Version = 1u });
                return;
            }

            var infoEntity = gridDataInfoQuery.GetSingletonEntity();
            var info = em.GetComponentData<GridDataInfo>(infoEntity);

            if (info.Size.x != gridSize.x || info.Size.y != gridSize.y)
            {
                info.Size = gridSize;
                info.Version++;
                em.SetComponentData(infoEntity, info);
            }
        }

        public NativeArray<GridCellData> Cells => cells;
        public int2 Size => size;
    }

    public struct GridDataInfo : IComponentData
    {
        public int2 Size;
        public uint Version;
    }
}
