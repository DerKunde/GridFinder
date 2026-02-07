using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Targeting
{
    /// <summary>Global target for all agents.</summary>
    public struct TargetState : IComponentData
    {
        public int2 Cell;
        public float3 WorldPos;
        public uint Version;
        public byte HasTarget; // 0/1
    }

    public struct TargetSingletonTag : IComponentData { }
}
