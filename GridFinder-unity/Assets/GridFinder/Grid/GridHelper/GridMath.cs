using Unity.Mathematics;

namespace GridFinder.Grid.GridHelper
{
    public class GridMath
    {
        /// <summary>
        /// Converts a world position to the cell that contains it.
        /// Uses grid origin and cell size. (Cell refers to its center.)
        /// </summary>
        public static int2 WorldToCell(float3 world, in GridConfig cfg)
        {
            var local = world - cfg.Origin;

            return new int2(
                (int)math.floor(local.x / cfg.CellSize),
                (int)math.floor(local.z / cfg.CellSize)
            );
        }

        /// <summary>
        /// Returns the world position of the *cell center*.
        /// </summary>
        public static float3 CellToWorldCenter(int2 cell, in GridConfig cfg)
        {
            return cfg.Origin + new float3(
                (cell.x + 0.5f) * cfg.CellSize,
                0f,
                (cell.y + 0.5f) * cfg.CellSize
            );
        }

        /// <summary>
        /// Optional helper: checks whether a cell is inside the grid bounds.
        /// </summary>
        public static bool IsInside(int2 cell, in GridConfig cfg)
        {
            return (uint)cell.x < (uint)cfg.Size.x && (uint)cell.y < (uint)cfg.Size.y;
        }

        /// <summary>
        /// Optional helper: maps a cell to a linear index (row-major).
        /// </summary>
        public static int CellToIndex(int2 cell, in GridConfig cfg)
        {
            return cell.x + cell.y * cfg.Size.x;
        }
        
        /// <summary>
        /// Returns the world-space size (width in X, height in Z) of the whole grid in meters.
        /// </summary>
        public static float2 WorldSizeXZ(in GridConfig cfg)
        {
            return new float2(
                cfg.Size.x * cfg.CellSize,
                cfg.Size.y * cfg.CellSize
            );
        }

        /// <summary>
        /// Returns the world-space center of the whole grid (on XZ), using the grid origin (min-corner)
        /// and the full world size. Y is cfg.Origin.y.
        /// </summary>
        public static float3 WorldCenter(in GridConfig cfg)
        {
            var size = WorldSizeXZ(cfg);

            return new float3(
                cfg.Origin.x + size.x * 0.5f,
                cfg.Origin.y,
                cfg.Origin.z + size.y * 0.5f
            );
        }

        /// <summary>
        /// Returns the world-space min-corner of the grid rectangle (same as cfg.Origin, but with explicit Y).
        /// </summary>
        public static float3 WorldMinCorner(in GridConfig cfg, float y)
        {
            return new float3(cfg.Origin.x, y, cfg.Origin.z);
        }

        /// <summary>
        /// Returns the world-space max-corner of the grid rectangle (min + size).
        /// </summary>
        public static float3 WorldMaxCorner(in GridConfig cfg, float y)
        {
            var size = WorldSizeXZ(cfg);
            return new float3(
                cfg.Origin.x + size.x,
                y,
                cfg.Origin.z + size.y
            );
        }

        /// <summary>
        /// Returns the world-space position of the grid's center.
        /// Name variant you asked for.
        /// </summary>
        public static float3 WorldCenterFromGrid(in GridConfig cfg) => WorldCenter(cfg);
    }
}