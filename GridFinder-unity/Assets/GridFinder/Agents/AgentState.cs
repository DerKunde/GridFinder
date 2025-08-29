using Unity.Mathematics;

namespace GridFinder.Agents
{
    public struct AgentState
    {
        public int Pos;
        public int2 Target;
        public byte LOD;
    }
}