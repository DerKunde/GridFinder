using Unity.Entities;

namespace GridFinder.Spawner
{
    /// <summary>
    /// Tag identifying the singleton queue entity.
    /// </summary>
    public struct SpawnCommandQueueTag : IComponentData
    {
    }

    /// <summary>
    /// Buffer element wrapper for spawn commands.
    /// </summary>
    public struct SpawnCommandBufferElement : IBufferElementData
    {
        public SpawnCommandData Value;
    }
}