using R3;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    public sealed class GridService
    {
        private readonly GridSettings settings;

        public float3 OriginWorld { get; private set; }
        public int2 SizeInCells { get; private set; }
        public float CellSize => settings.CellSize.Value;

        // Fires whenever derived grid properties change
        public readonly Subject<Unit> Changed = new();

        public GridService(GridSettings settings)
        {
            this.settings = settings;

            // Recompute whenever any setting changes
            settings.WorldSizeXZ.Subscribe(_ => Recompute());
            settings.CellSize.Subscribe(_ => Recompute());
            settings.CenterWorld.Subscribe(_ => Recompute());

            Recompute();
        }

        public void Recompute()
        {
            var cs = math.max(0.0001f, settings.CellSize.Value);
            var size = settings.WorldSizeXZ.Value;
            var center = settings.CenterWorld.Value;

            // Origin is min corner in XZ
            OriginWorld = new float3(
                center.x - size.x * 0.5f,
                center.y,
                center.z - size.y * 0.5f);

            SizeInCells = new int2(
                math.max(1, (int)math.floor(size.x / cs)),
                math.max(1, (int)math.floor(size.y / cs)));

            Changed.OnNext(Unit.Default);
        }

        public int2 WorldToCell(float3 world)
        {
            var localX = (world.x - OriginWorld.x) / CellSize;
            var localZ = (world.z - OriginWorld.z) / CellSize;
            var cell = (int2)math.floor(new float2(localX, localZ));
            return Clamp(cell);
        }

        public float3 CellToWorldCenter(int2 cell, float y)
        {
            cell = Clamp(cell);
            return new float3(
                OriginWorld.x + (cell.x + 0.5f) * CellSize,
                y,
                OriginWorld.z + (cell.y + 0.5f) * CellSize);
        }

        public float3 CellToWorldMinCorner(int2 cell, float y)
        {
            cell = Clamp(cell);
            return new float3(
                OriginWorld.x + cell.x * CellSize,
                y,
                OriginWorld.z + cell.y * CellSize);
        }

        private int2 Clamp(int2 cell)
        {
            return new int2(
                math.clamp(cell.x, 0, SizeInCells.x - 1),
                math.clamp(cell.y, 0, SizeInCells.y - 1));
        }
    }
}
