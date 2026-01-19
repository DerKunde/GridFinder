using Unity.Mathematics;

namespace GridFinder.Spawner
{
    /// <summary>
    /// High-level "I want to spawn something" request (input/tool layer).
    /// Not executed directly; must be resolved into a SpawnCommandData.
    /// </summary>
    public readonly struct SpawnIntent
    {
        public readonly int ContentId;
        public readonly float3 WorldPos;
        public readonly quaternion WorldRot;
        public readonly float UniformScale;
        public readonly int? GridCellIndex; // optional (may be unresolved)

        public SpawnIntent(
            int contentId,
            float3 worldPos,
            quaternion worldRot,
            float uniformScale,
            int? gridCellIndex = null)
        {
            ContentId = contentId;
            WorldPos = worldPos;
            WorldRot = worldRot;
            UniformScale = uniformScale;
            GridCellIndex = gridCellIndex;
        }

        public override string ToString()
        {
            return "SpawnIntent(ContentId=" + ContentId
                                            + ", WorldPos=" + WorldPos
                                            + ", UniformScale=" + UniformScale
                                            + ", GridCellIndex=" + (GridCellIndex.HasValue ? GridCellIndex.Value.ToString() : "null")
                                            + ")";
        }
    }
}