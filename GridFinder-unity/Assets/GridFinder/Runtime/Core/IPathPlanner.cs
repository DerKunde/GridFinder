namespace GridFinder.Runtime.Grid.Core
{
    public interface IPathPlanner
    {
        PathResult FindPath(Grid2D grid, PathRequest request, HeuristicType heuristic = HeuristicType.Octile, bool allowDiagonal = true);
    }
}