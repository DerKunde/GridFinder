using System.Collections.Generic;

namespace GridFinder.Runtime.Grid
{
public sealed class GridService
    {
        private readonly MultiLevelGrid _grid = new();
        private readonly Dictionary<int, CostComposer> _costsPerLevel = new();

        private CostComposer Costs(int level)
        {
            if (!_costsPerLevel.TryGetValue(level, out var c))
                _costsPerLevel[level] = c = new CostComposer();
            return c;
        }

        public LevelGrid GetOrCreateLevel(int level) => _grid.GetOrCreateLevel(level);

        public void AddLayer(int level, ICostLayer layer) => Costs(level).AddLayer(layer);

        public void SetCell(int level, int x, int y, ushort baseCost, uint flags, short elevation = 0, ushort terrainId = 0)
        {
            var lvl = _grid.GetOrCreateLevel(level);
            ref var cell = ref lvl.GetOrCreateCell(x, y);
            cell.BaseCost = baseCost;
            cell.Flags = flags;
            cell.Elevation = elevation;
            cell.TerrainId = terrainId;
        }

        public bool TryGetEffectiveCost(int level, int x, int y, out int cost, out uint effectiveFlags)
        {
            if (!_grid.TryGetLevel(level, out var lvl) || !lvl.TryGetCell(x, y, out var baseCell))
            { cost = int.MaxValue; effectiveFlags = 0; return false; }

            cost = Costs(level).ComputeCost(level, x, y, in baseCell, out effectiveFlags);
            return cost != int.MaxValue;
        }

        public IEnumerable<(int nx,int ny,int stepCost)> GetNeighborsWithCost(
            int level, int x, int y, Neighborhood nhood, bool forbidDiagonalsIfBlockedCorners = true)
        {
            if (!_grid.TryGetLevel(level, out var lvl) || !lvl.TryGetCell(x, y, out var from))
                yield break;

            foreach (var (nx, ny, diag) in NeighborProvider.Get(x, y, nhood))
            {
                if (!lvl.TryGetCell(nx, ny, out var to)) continue;

                var step = Costs(level).ComputeCost(level, nx, ny, in to, out var toFlags);
                if (step == int.MaxValue) continue;

                // One‑way prüfen (ausgehend von 'from'):
                var dx = nx - x; var dy = ny - y;
                if (!NeighborProvider.PassesOneWay(from.Flags, dx, dy)) continue;

                // Optional: Diagonalen nur erlauben, wenn beide orthogonalen Nachbarn passierbar sind
                if (diag && forbidDiagonalsIfBlockedCorners)
                {
                    bool ok1 = lvl.TryGetCell(x, ny, out var side1) && Costs(level).ComputeCost(level, x, ny, in side1, out _) != int.MaxValue;
                    bool ok2 = lvl.TryGetCell(nx, y, out var side2) && Costs(level).ComputeCost(level, nx, y, in side2, out _) != int.MaxValue;
                    if (!(ok1 && ok2)) continue;
                }

                yield return (nx, ny, step);
            }
        }
    }
}