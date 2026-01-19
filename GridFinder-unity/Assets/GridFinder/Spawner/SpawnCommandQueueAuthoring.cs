using Unity.Entities;
using UnityEngine;

namespace GridFinder.Spawner
{
    /// <summary>
    /// Creates the singleton SpawnCommand queue entity with a dynamic buffer.
    /// </summary>
    public sealed class SpawnCommandQueueAuthoring : MonoBehaviour
    {
        public sealed class Baker : Baker<SpawnCommandQueueAuthoring>
        {
            public override void Bake(SpawnCommandQueueAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                AddComponent<SpawnCommandQueueTag>(e);
                AddBuffer<SpawnCommandBufferElement>(e);
            }
        }
    }
}