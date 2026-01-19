using Unity.Entities;

namespace GridFinder.Spawner
{
    public sealed class SpawnCommandWriter
    {
        private readonly EntityManager _em;
        private readonly EntityQuery _queueQuery;

        public SpawnCommandWriter(World world)
        {
            _em = world.EntityManager;
            _queueQuery = _em.CreateEntityQuery(
                ComponentType.ReadOnly<SpawnCommandQueueTag>(),
                ComponentType.ReadWrite<SpawnCommandBufferElement>()
            );
        }

        public bool TryEnqueue(in SpawnCommandData cmd)
        {
            if (_queueQuery.IsEmpty)
                return false;

            var queueEntity = _queueQuery.GetSingletonEntity();
            var buffer = _em.GetBuffer<SpawnCommandBufferElement>(queueEntity);
            buffer.Add(new SpawnCommandBufferElement { Value = cmd });
            return true;
        }
    }
}