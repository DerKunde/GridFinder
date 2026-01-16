using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Spawner
{
    public struct TargetSpawnRequest : IBufferElementData
    {
        public float3 WorldPos;
        public byte AssignToAllAgents; // 0/1 (Milestone-Shortcut)
    }
}