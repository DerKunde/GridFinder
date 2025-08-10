using Runtime.Grid;

namespace Runtime.Core
{
    public interface IPathPlanner
    {
        PathResult FindPath(Grid2D grid, PathRequest request, HeuristicType heuristic = HeuristicType.Octile, bool allowDiagonal = true);
    }
}