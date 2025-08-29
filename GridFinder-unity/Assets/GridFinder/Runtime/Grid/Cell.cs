using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GridFinder.Runtime.Grid
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Cell
    {
        // ===== Speicherlayout: 8 Bytes gesamt =====
        public uint   Packed;  // Flags + ColorIndex etc. (32 Bit)
        public ushort ZoneId;  // 16 Bit
        public ushort Cost;    // 16 Bit

        // ----- Bitlayout in Packed -----
        // [0]       Walkable (1 Bit)
        // [1]       Reserved (1 Bit)
        // [2]       Blocked  (optional, 1 Bit)
        // [3..10]   ColorIndex (8 Bit)
        // [11..31]  frei (21 Bit)

        public const int  BIT_WALKABLE   = 0;
        public const int  BIT_RESERVED   = 1;
        public const int  BIT_BLOCKED    = 2;   // optionales Flag, nicht nötig für "unwalkable"
        public const int  SHIFT_COLORIDX = 3;   // -> Bits 3..10
        public const uint MASK_WALKABLE  = 1u << BIT_WALKABLE;
        public const uint MASK_BLOCKED   = 1u << BIT_BLOCKED;
        public const uint MASK_COLORIDX  = 0xFFu << SHIFT_COLORIDX;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetWalkable(uint packed) => (packed & MASK_WALKABLE) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetWalkable(uint packed, bool on)
            => on ? (packed | MASK_WALKABLE) : (packed & ~MASK_WALKABLE);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetColorIndex(uint packed)
            => (byte)((packed & MASK_COLORIDX) >> SHIFT_COLORIDX);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetColorIndex(uint packed, byte idx)
            => (packed & ~MASK_COLORIDX) | ((uint)idx << SHIFT_COLORIDX);

        // Fertige Vorlagen (value types; Kopie = 8 Bytes, keine GC-Alloc)
        public static readonly Cell WalkableCell   = new() { Packed = MASK_WALKABLE, ZoneId = 0, Cost = 0 };
        public static readonly Cell UnwalkableCell = new() { Packed = 0u,           ZoneId = 0, Cost = 0 };
        public static readonly Cell SpawnerCell = new() { Packed = MASK_WALKABLE, ZoneId = 0, Cost = 0 };

        // Bequemlichkeit
        public static Cell Default => WalkableCell;
    }
}
