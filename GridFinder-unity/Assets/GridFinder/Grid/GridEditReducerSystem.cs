using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct GridEditReducerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<GridEditQueueTag>(out var queueEntity))
                return;

            var cmds = SystemAPI.GetBuffer<GridEditCommand>(queueEntity);
            if (cmds.Length == 0)
                return;

            if (!SystemAPI.TryGetSingleton<GridConfig>(out var cfg))
            {
                cmds.Clear();
                return;
            }

            // Get GridDataSystem storage
            var gridDataState = state.WorldUnmanaged.GetExistingSystemState<GridDataSystem>();
            var gridData = state.WorldUnmanaged.GetUnsafeSystemRef<GridDataSystem>(gridDataState.SystemHandle); // read-only ref
            var cells = gridData.Cells; // NativeArray<GridCellData>

            // Apply commands
            for (int i = 0; i < cmds.Length; i++)
            {
                ApplyCommand(cmds[i], cfg.Size, ref cells);
            }

            // bump version so dependents can react
            var infoRW = SystemAPI.GetSingletonRW<GridDataInfo>();
            infoRW.ValueRW.Version++;

            cmds.Clear();
        }

        private static void ApplyCommand(in GridEditCommand cmd, int2 size, ref NativeArray<GridCellData> cells)
        {
            var min = new int2(math.clamp(cmd.Min.x, 0, size.x - 1), math.clamp(cmd.Min.y, 0, size.y - 1));
            var max = new int2(math.clamp(cmd.Max.x, 0, size.x - 1), math.clamp(cmd.Max.y, 0, size.y - 1));

            for (int y = min.y; y <= max.y; y++)
            for (int x = min.x; x <= max.x; x++)
            {
                var idx = x + y * size.x;
                var c = cells[idx];

                switch (cmd.Type)
                {
                    case GridEditType.SetWalkable:
                        c.Walkable = (byte)(cmd.Value != 0 ? 1 : 0);
                        break;

                    case GridEditType.SetZone:
                        c.Zones = (ZoneMask)cmd.Value;
                        break;

                    case GridEditType.AddFeature:
                        c.Features |= (FeatureMask)cmd.Value;
                        break;

                    case GridEditType.RemoveFeature:
                        c.Features &= ~(FeatureMask)cmd.Value;
                        break;

                    case GridEditType.SetCost:
                        c.Cost = (byte)math.clamp(cmd.Value, 0, 255);
                        break;
                }

                cells[idx] = c;
            }
        }
    }
}
