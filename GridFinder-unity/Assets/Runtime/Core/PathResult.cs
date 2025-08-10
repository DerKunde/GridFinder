using Runtime.Grid;

namespace Runtime.Core
{
    public readonly struct PathResult
    {
        public readonly int id;
        public readonly GridPos[] path; // leer/null = kein Pfad
        public PathResult(int id, GridPos[] path) { this.id = id; this.path = path; }
        public bool Success => path != null && path.Length > 0;
    }
}