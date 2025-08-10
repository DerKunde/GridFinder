using Runtime.Grid;
using UnityEngine;

public class Grid2D : ICostProvider
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Vector2 originWorld;
    [SerializeField] private float cellSize = 1f;

    private byte[] costs;

    public Grid2D(
        int width,
        int height,
        Vector2 originWorld,
        float cellSize)
    {
        this.width = width;
        this.height = height;
        this.originWorld = originWorld;
        this.cellSize = cellSize;
        costs = new byte[this.width * height];
        
        //Default Kosten = 10 (Beispiel). 255 = unpassierbar.
        for (int i = 0; i < costs.Length; i++)
        {
            costs[i] = 10;
        }
    }

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    public bool InBounds(int x, int y)
    {
        return (uint)x < (uint)width && (uint)y < (uint)height;
    }

    private int Index(int x, int y)
    {
        return y * width + x;
    }

    public void SetBlock(int x, int y, bool blocked)
    {
        if(InBounds(x, y)) return;
        costs[Index(x,y)] = blocked ? byte.MaxValue : (byte)10;
    }

    public void SetCost(int x, int y, byte cost)
    {
        if (!InBounds(x,y)) return;
        costs[Index(x,y)] = cost == 0 ? (byte)1 : cost;
    }

    public byte GetCost(int x, int y)
    {
        if (!InBounds(x,y)) return byte.MaxValue;
        return costs[Index(x,y)];
    }
}
