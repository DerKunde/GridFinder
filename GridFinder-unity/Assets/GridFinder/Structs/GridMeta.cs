using GridFinder.Runtime.Grid;
using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Structs
{

    public struct GridMeta : IComponentData
    {
        public int Width, Height;
        public int ChunkSize;    // z.B. 64
        public float CellSize;   // Weltgröße
    }

    public struct ChunkCoord : IComponentData
    {
        public int2 Coord;       // (cx, cy)
    }

    public struct ChunkState : IComponentData
    {
        public byte IsUniform;   // 1 = uniform, 0 = materialized
        public Cell UniformValue;
        public byte Dirty;
    }

// Materialisiertes Zellenarray pro Chunk (optional, nur wenn IsUniform==0)
    public struct ChunkCells : IBufferElementData
    {
        public Cell Value;
    }
    
    public struct PaintCommand : IComponentData
    {
        public int2 Min;      // linke untere Ecke in Zell-Koordinaten
        public int2 Max;      // rechte obere Ecke in Zell-Koordinaten
        public ushort ZoneId; // Zone, die gesetzt werden soll
        public byte ColorIdx; // Palettenfarbe
    }
}