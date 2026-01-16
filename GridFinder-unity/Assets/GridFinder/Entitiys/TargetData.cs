using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Entitiys
{
    public struct TargetTag : IComponentData {}
    
    public struct TargetData : IComponentData
    {
        public float3 WorldPos;
        public byte IsSet;
    }
}