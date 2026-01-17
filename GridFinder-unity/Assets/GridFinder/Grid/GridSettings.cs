using R3;
using Unity.Mathematics;

namespace GridFinder.Grid
{
    public sealed class GridSettings
    {
        // World size of the floor/grid in XZ (e.g. 10x10)
        public readonly ReactiveProperty<float2> WorldSizeXZ = new(new float2(10f, 10f));

        // Cell size in world units (e.g. 0.15)
        public readonly ReactiveProperty<float> CellSize = new(0.15f);

        // Floor center position
        public readonly ReactiveProperty<float3> CenterWorld = new(new float3(0f, 0f, 0f));
    }
}