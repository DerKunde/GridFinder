using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace GridFinder.Entitiys
{
    public partial struct AgentSystem : ISystem 
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AgentData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            AgentJob job = new AgentJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            job.ScheduleParallel();

        }

        public partial struct AgentJob : IJobEntity
        {
            public float DeltaTime;
            
            public void Execute(ref AgentData agentData, ref LocalTransform transform)
            {
                transform = transform.Translate(agentData.Direction * agentData.Speed * DeltaTime);
            }
        }
    }
}