using System.Runtime.InteropServices;

namespace GridFinder.Runtime.Grid
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Cell
    {
        public uint Packed;
        public ushort ZoneId;
        public ushort Cost;
        
        // Bitlayout in Packed:
        // [0]   Walkable (1 bit)
        // [1]   Reserved (1 bit)
        // [2..9]  ColorIndex (8 bit)
        // [10..31] frei (22 bit)
        
        public const int BIT_WALKABLE   = 0;
        public const int BIT_RESERVED   = 1;
        public const int BIT_BLOCKED    = 2;
        public const int SHIFT_COLORIDX = 3;
        public const uint MASK_COLORIDX = 0xFFu << SHIFT_COLORIDX;
        
        public static bool GetWalkable(uint packed) => ((packed >> BIT_WALKABLE) & 1u) != 0u;
        public static uint SetWalkable(uint packed, bool on)
            => on ? (packed | (1u << BIT_WALKABLE)) : (packed & ~(1u << BIT_WALKABLE));

        public static byte GetColorIndex(uint packed) => (byte)((packed & MASK_COLORIDX) >> SHIFT_COLORIDX);
        public static uint SetColorIndex(uint packed, byte idx)
            => (packed & ~MASK_COLORIDX) | ((uint)idx << SHIFT_COLORIDX);

        public static Cell Default => new Cell {
            Packed = (1u << BIT_WALKABLE), // begehbar
            ZoneId = 0,
            Cost = 0
        };
    }
}