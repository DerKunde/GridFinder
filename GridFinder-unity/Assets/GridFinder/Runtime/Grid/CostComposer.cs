using System.Collections.Generic;

namespace GridFinder.Runtime.Grid
{
    public sealed class CostComposer
    {
        private readonly List<ICostLayer> _layers = new();

        public void AddLayer(ICostLayer layer) => _layers.Add(layer);

        public bool IsWalkable(in CellData cell)
            => (cell.Flags & (uint)CellFlags.Blocked) == 0 && (cell.Flags & (uint)CellFlags.Walkable) != 0;

        public int ComputeCost(int level, int x, int y, in CellData baseCell, out uint effectiveFlags)
        {
            int cost = baseCell.BaseCost;
            uint flags = baseCell.Flags;
            foreach (var layer in _layers)
            {
                var q = new TraversalQuery(level, x, y, baseCell);
                cost += layer.GetAdditiveCost(in q);
                flags |= layer.GetFlagsOr(in q);
                flags &= layer.GetFlagsAnd(in q) == 0 ? 0xFFFFFFFF : layer.GetFlagsAnd(in q);
            }
            effectiveFlags = flags;
            if ((flags & (uint)CellFlags.Blocked) != 0) return int.MaxValue; // unpassierbar
            return cost;
        }
    }
}