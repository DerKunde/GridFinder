using GridFinder.Structs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Runtime.Mono
{
    public class PaintCommandAuthoring : MonoBehaviour
    {
        public Vector2Int min;
        public Vector2Int max;
        public ushort zoneId;
        public byte colorIdx;

        class Baker : Baker<PaintCommandAuthoring>
        {
            public override void Bake(PaintCommandAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PaintCommand {
                    Min = new int2(authoring.min.x, authoring.min.y),
                    Max = new int2(authoring.max.x, authoring.max.y),
                    ZoneId = authoring.zoneId,
                    ColorIdx = authoring.colorIdx
                });
            }
        }
    }
}