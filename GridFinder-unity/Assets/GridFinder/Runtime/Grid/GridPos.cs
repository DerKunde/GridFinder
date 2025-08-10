namespace GridFinder.Runtime.Grid
{
    public enum HeuristicType { Manhattan, Euclidean, Octile }
    
    public readonly struct GridPos
    {
        public readonly int x, y;

        public GridPos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}