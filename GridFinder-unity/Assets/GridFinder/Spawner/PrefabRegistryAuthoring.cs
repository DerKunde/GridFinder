using Unity.Entities;
using UnityEngine;

namespace GridFinder.Spawner
{
    public sealed class PrefabRegistryAuthoring : MonoBehaviour
    {
        public PrefabRegistryAsset Registry;
    }

    public sealed class PrefabRegistryBaker : Baker<PrefabRegistryAuthoring>
    {
        public override void Bake(PrefabRegistryAuthoring authoring)
        {
            if (authoring.Registry == null)
                return;

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<PrefabRegistryTag>(entity);

            var buffer = AddBuffer<PrefabRegistryEntry>(entity);

            foreach (var entry in authoring.Registry.Entries)
            {
                if (entry == null || entry.Prefab == null)
                    continue;

                var prefabEntity = GetEntity(entry.Prefab, TransformUsageFlags.Dynamic);

                buffer.Add(new PrefabRegistryEntry
                {
                    Id = entry.Id,
                    Prefab = prefabEntity
                });
            }
        }
    }
}