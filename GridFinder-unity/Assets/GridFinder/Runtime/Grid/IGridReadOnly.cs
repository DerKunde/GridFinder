using System.Numerics;

namespace GridFinder.Runtime.Grid
{
    public interface IGridReadOnly
    {
        int Width { get; }
        int Height { get; }
        float CellSize { get; }
        Vector2 OriginWorld { get; } // linke-untere Ecke in Weltkoordinaten (XY)
        byte GetCost(int x, int y);   // 255 = blockiert; >=1 = begehbar (Kosten)
    }
}