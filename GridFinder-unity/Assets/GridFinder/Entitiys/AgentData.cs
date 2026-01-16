using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Entitiys
{
    public struct AgentData : IComponentData
    {
        public float Speed;
        public float3 Direction;
    }
}
