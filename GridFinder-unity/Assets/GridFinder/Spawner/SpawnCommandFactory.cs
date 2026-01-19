using Unity.Mathematics;

namespace GridFinder.Spawner
{
    public sealed class SpawnCommandFactory : ISpawnCommandFactory
    {
        private uint _nextRequestId = 1;

        public bool TryCreate(in SpawnIntent intent, out SpawnCommandData cmd)
        {
            // TODO: Replace with real grid resolving/validation
            var resolvedCell = intent.GridCellIndex.HasValue ? intent.GridCellIndex.Value : -1;

            cmd = new SpawnCommandData
            {
                ContentId = intent.ContentId,
                WorldPos = intent.WorldPos,
                WorldRot = intent.WorldRot,
                UniformScale = math.clamp(intent.UniformScale, 0.01f, 100f),
                GridCellIndex = resolvedCell,
                RequestId = _nextRequestId++
            };

            return true;
        }
    }
}