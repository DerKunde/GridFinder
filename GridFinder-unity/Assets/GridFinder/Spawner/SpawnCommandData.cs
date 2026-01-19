using Unity.Mathematics;

namespace GridFinder.Spawner
{
    /// <summary>
    /// Fully-resolved spawn command that can be executed by ECS.
    /// Keep this blittable-friendly (no references).
    /// </summary>
    public struct SpawnCommandData
    {
        public int ContentId;
        public float3 WorldPos;
        public quaternion WorldRot;
        public float UniformScale;
        public int GridCellIndex;
        public uint RequestId;
    }
}