using Unity.Mathematics;

namespace GridFinder.Agents
{
    public struct PathRequest
    {
        public int agentIndex;
        public int2 start, goal;
    }

    public struct PathResult
    {
        public int agentIndex;
        public int status;
        public int offset, length;
    }
}