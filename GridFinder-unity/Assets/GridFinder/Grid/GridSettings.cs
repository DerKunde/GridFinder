using R3;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    public sealed class GridSettings
    {
        public static GridSettings Instance { get; private set; } = null!;

        public readonly ReactiveProperty<int2> GridSizeInCells = new();
        public readonly ReactiveProperty<float> CellSize = new();
        public readonly ReactiveProperty<float3> CenterWorld = new();

        public GridSettings()
        {
            Instance = this;
        }
    }
}