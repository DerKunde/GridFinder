using GridFinder.Agents;
using Unity.Entities;
using UnityEngine;

public class AgentTagAuthoring : MonoBehaviour {}

public class AgentTagBaker : Baker<AgentTagAuthoring>
{
    public override void Bake(AgentTagAuthoring authoring)
        => AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Agent());
}