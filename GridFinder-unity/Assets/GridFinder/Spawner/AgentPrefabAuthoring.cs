using Unity.Entities;
using UnityEngine;

namespace GridFinder.Spawner
{
    public sealed class AgentPrefabAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject agentPrefab = null;

        public sealed class Baker : Baker<AgentPrefabAuthoring>
        {
            public override void Bake(AgentPrefabAuthoring authoring)
            {
                var singleton = GetEntity(TransformUsageFlags.None);

                var prefabEntity = GetEntity(authoring.agentPrefab, TransformUsageFlags.Dynamic);

                AddComponent(singleton, new AgentPrefabSingleton
                {
                    Prefab = prefabEntity
                });
            }
        }
    }
}