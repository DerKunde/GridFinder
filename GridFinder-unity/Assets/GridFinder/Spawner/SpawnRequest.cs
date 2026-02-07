using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Spawner
{
    public struct SpawnRequest : IBufferElementData
    {
        public float3 Position;
        public quaternion Rotation; // optional, aber direkt skalierbar
        public float Scale;
        public int ContentID;
    }
}