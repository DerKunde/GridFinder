using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Grid
{
// <summary>
    /// XZ grid configuration + helpers.
    /// - Grid lives on the XZ plane (Y is height).
    /// - originWorld is the world-space position of the grid's (0,0) cell corner (min XZ).
    /// - cellSize is the width/height of one cell in world units.
    /// - sizeInCells is optional; if set (>0) we can clamp positions to a fixed grid area.
    /// </summary>
    [CreateAssetMenu(menuName = "GridFinder/Grid Config", fileName = "GridConfig")]
    public sealed class GridConfig : ScriptableObject
    {
        [Header("Grid Placement")]
        [Tooltip("World-space corner of cell (0,0). This is the minimum XZ corner of your grid.")]
        public float3 originWorld = float3.zero;

        [Tooltip("Size of one cell in world units (e.g. 0.15).")]
        [Min(0.0001f)]
        public float cellSize = 0.15f;

        [Header("Optional Bounds (for e.g. a 1x1 floor)")]
        [Tooltip("If > 0, grid is clamped to [0..sizeInCells-1]. If 0, grid is unbounded.")]
        public int2 sizeInCells = int2.zero; // e.g. (6,6) for ~1x1 with 0.15

        public float2 OriginXZ => new(originWorld.x, originWorld.z);

        public float2 WorldToLocalXZ(float3 world) =>
            new(world.x - originWorld.x, world.z - originWorld.z);

        /// <summary>
        /// Converts a world position to a cell coordinate (x,z).
        /// Uses floor(), so values on boundaries fall into the lower cell.
        /// </summary>
        public int2 WorldToCell(float3 world)
        {
            var local = WorldToLocalXZ(world);
            var cell = (int2)math.floor(local / cellSize);

            if (HasBounds)
                cell = ClampCell(cell);

            return cell;
        }

        /// <summary>
        /// Returns the world-space center position of a cell.
        /// </summary>
        public float3 CellToWorldCenter(int2 cell, float y = 0f)
        {
            if (HasBounds)
                cell = ClampCell(cell);

            var x = originWorld.x + (cell.x + 0.5f) * cellSize;
            var z = originWorld.z + (cell.y + 0.5f) * cellSize; // int2.y is "z"
            return new float3(x, y, z);
        }

        /// <summary>
        /// Returns the world-space min corner (lower-left) of a cell (the cell's XZ corner).
        /// </summary>
        public float3 CellToWorldMinCorner(int2 cell, float y = 0f)
        {
            if (HasBounds)
                cell = ClampCell(cell);

            var x = originWorld.x + cell.x * cellSize;
            var z = originWorld.z + cell.y * cellSize;
            return new float3(x, y, z);
        }

        public bool HasBounds => sizeInCells.x > 0 && sizeInCells.y > 0;

        public int2 ClampCell(int2 cell)
        {
            if (!HasBounds)
                return cell;

            return new int2(
                math.clamp(cell.x, 0, sizeInCells.x - 1),
                math.clamp(cell.y, 0, sizeInCells.y - 1));
        }

        /// <summary>
        /// Helper: compute a recommended sizeInCells for a rectangular area (world units),
        /// e.g. 1x1 floor. Result is floor(area / cellSize).
        /// </summary>
        public int2 ComputeSizeInCells(float2 areaWorldSize)
        {
            return new int2(
                math.max(1, (int)math.floor(areaWorldSize.x / cellSize)),
                math.max(1, (int)math.floor(areaWorldSize.y / cellSize)));
        }
    }
}