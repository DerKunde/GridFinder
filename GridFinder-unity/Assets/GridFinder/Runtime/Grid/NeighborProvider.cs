using System.Collections.Generic;

namespace GridFinder.Runtime.Grid
{
    public enum Neighborhood { Von4, Von8 }

    public static class NeighborProvider
    {
        private static readonly (int dx,int dy)[] Von4 = { (1,0), (-1,0), (0,1), (0,-1) };
        private static readonly (int dx,int dy)[] Von8 = {
            (1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)
        };

        public static IEnumerable<(int nx,int ny,bool diagonal)> Get(int x, int y, Neighborhood nhood)
        {
            var dirs = nhood == Neighborhood.Von4 ? Von4 : Von8;
            foreach (var (dx,dy) in dirs)
                yield return (x + dx, y + dy, dx != 0 && dy != 0);
        }

        public static bool PassesOneWay(uint flagsFrom, int dx, int dy)
        {
            // Wenn OneWay vorhanden, prüfen ob Richtung erlaubt ist
            if ((flagsFrom & ((uint)CellFlags.OneWayN | (uint)CellFlags.OneWayE |
                              (uint)CellFlags.OneWayS | (uint)CellFlags.OneWayW)) == 0)
                return true;

            if (dx == 0 && dy == -1) return (flagsFrom & (uint)CellFlags.OneWayN) != 0;
            if (dx == 1 && dy == 0)  return (flagsFrom & (uint)CellFlags.OneWayE) != 0;
            if (dx == 0 && dy == 1)  return (flagsFrom & (uint)CellFlags.OneWayS) != 0;
            if (dx == -1 && dy == 0) return (flagsFrom & (uint)CellFlags.OneWayW) != 0;

            // Diagonalen optional verbieten oder Corner‑Cutting prüfen (nicht gezeigt)
            return true;
        }
    }
}