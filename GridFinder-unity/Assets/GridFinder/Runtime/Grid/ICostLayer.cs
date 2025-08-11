using System.Collections.Generic;

namespace GridFinder.Runtime.Grid
{
    public struct AgentMask
    {
        public uint RequiredTags;   // Agent muss diese Tags unterstützen (z.B. Treppen-fähig)
        public uint ForbiddenTags;  // Agent darf diese Flags nicht haben
        public bool AllowDiagonal;  // Nachbarschaftsregel
    }

    public readonly struct TraversalQuery
    {
        public readonly int Level, X, Y;
        public readonly CellData Cell;
        public TraversalQuery(int level, int x, int y, in CellData cell)
        { Level = level; X = x; Y = y; Cell = cell; }
    }

    public interface ICostLayer
    {
        /// Additive Kosten (können negativ sein)
        int GetAdditiveCost(in TraversalQuery q);

        /// Optional harte Overrides (z. B. Blocked setzen)
        uint GetFlagsOr(in TraversalQuery q);     // Flags hinzuschalten
        uint GetFlagsAnd(in TraversalQuery q);    // Flags rausmaskieren (Optional, meist ~uint.MaxValue)
    }

    /// Beispiel: simple Blockade-Layer per HashSet
    public sealed class ObstacleLayer : ICostLayer
    {
        private readonly HashSet<(int level, int x, int y)> _blocked = new();

        public void SetBlocked(int level, int x, int y, bool blocked)
        {
            var key = (level, x, y);
            if (blocked) _blocked.Add(key); else _blocked.Remove(key);
        }

        public int GetAdditiveCost(in TraversalQuery q) => 0;

        public uint GetFlagsOr(in TraversalQuery q)
            => _blocked.Contains((q.Level, q.X, q.Y)) ? (uint)CellFlags.Blocked : 0;

        public uint GetFlagsAnd(in TraversalQuery q) => 0xFFFFFFFF;
    }
}