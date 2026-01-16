using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Entitiys
{
    public class AgentDataAuthoring : MonoBehaviour
    {
        public float Speed;
        public float3 Direction;

        private class Baker : Baker<AgentDataAuthoring>
        {
            public override void Bake(AgentDataAuthoring dataAuthor)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new AgentData
                {
                    Speed = dataAuthor.Speed,
                    Direction = dataAuthor.Direction
                });
                
            }
        }
    }
}