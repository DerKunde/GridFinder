using System;

namespace GridFinder.Structs
{
    [Serializable]
    public struct CellCoord
    {
        public int x, y, z;
        public CellCoord(int x,int y,int z){ this.x=x; this.y=y; this.z=z; }
        public override string ToString() => $"({x},{y},{z})";
    }
}