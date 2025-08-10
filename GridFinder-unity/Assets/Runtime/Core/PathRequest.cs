using Runtime.Grid;

namespace Runtime.Core
{
    public readonly struct PathRequest
    {
        public readonly int id;
        public readonly GridPos start;
        public readonly GridPos goal;

        public PathRequest(int id, GridPos start, GridPos goal)
        {
            this.id = id; this.start = start; this.goal = goal;
        }
    }
}