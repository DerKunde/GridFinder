using GridFinder.Entitiys;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace GridFinder.Spawner
{
    public class SpawnBootstrapAuthoring : MonoBehaviour
    {
        public GameObject Prefab;

        class Baker : Baker<SpawnBootstrapAuthoring>
        {
            public override void Bake(SpawnBootstrapAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                var prefabEntity = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);
                
                AddComponent(e, new SpawnPrefab{Prefab = prefabEntity} );
                AddBuffer<SpawnRequest>(e);
            }
        }
    }
}