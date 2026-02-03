using Unity.Entities;

namespace GridFinder.GridInput
{
    public enum ToolModeType : byte
    {
        None = 0,
        SetTarget = 1,
        SpawnAgent = 2,
        SpawnWall = 3,
        SetGridZone = 4
    }

    /// <summary>
    /// ECS singleton holding current editor/tool mode. Pure data, Burst-friendly.
    /// </summary>
    public struct ToolModeConfig : IComponentData
    {
        public ToolModeType Mode;

        // Generic payload: what UI selected (content, zone, etc.)
        public int PrimaryId;

        // Extra options packed as flags (e.g. add/remove, paint behavior)
        public uint Flags;

        // Optional: brush size / rectangle tool etc.
        public byte Brush;
    }

    public struct ToolModeSingletonTag : IComponentData { }
}