using Unity.Entities;
using UnityEngine;

namespace GridFinder.Targeting
{
    public sealed class TargetMarkerAuthoring : MonoBehaviour
    {
        [Tooltip("Prefab of the red cube marker (must be a GameObject prefab).")]
        public GameObject markerPrefab = null!;
    }

    public struct TargetMarkerPrefab : IComponentData
    {
        public Entity Prefab;
    }

    public class TargetMarkerBaker : Baker<TargetMarkerAuthoring>
    {
        public override void Bake(TargetMarkerAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);

            // Convert prefab GO -> entity prefab reference
            var prefabEntity = GetEntity(authoring.markerPrefab, TransformUsageFlags.Dynamic);

            AddComponent(e, new TargetMarkerPrefab { Prefab = prefabEntity });
        }
    }
}