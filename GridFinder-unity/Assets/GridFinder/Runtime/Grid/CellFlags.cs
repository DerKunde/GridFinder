namespace GridFinder.Runtime.Grid
{
    [System.Flags]
    public enum CellFlags : uint
    {
        None        = 0,
        Walkable    = 1 << 0,
        Blocked     = 1 << 1,   // harte Sperre (überschreibt Walkable)
        Stairs      = 1 << 2,
        Door        = 1 << 3,
        OneWayN     = 1 << 4,
        OneWayE     = 1 << 5,
        OneWayS     = 1 << 6,
        OneWayW     = 1 << 7,

        // Agent-Tags (bis zu 16 Bits reserviert)
        Tag_0       = 1 << 8,
        Tag_1       = 1 << 9,
        Tag_2       = 1 << 10,
        Tag_3       = 1 << 11,
        Tag_4       = 1 << 12,
        Tag_5       = 1 << 13,
        Tag_6       = 1 << 14,
        Tag_7       = 1 << 15,
        // ... erweitern bei Bedarf
    }

    /// <summary>
    /// Rohdaten je Zelle – klein halten!
    /// ~12 Bytes pro Zelle in Chunks.
    /// </summary>
    public struct CellData
    {
        public ushort BaseCost;   // Grundkosten (z.B. 10 = normal, 20 = Sand, 65535 = unpassierbar sentinel)
        public short Elevation;   // optional: Höhe (z.B. für Ebenenwechselkosten)
        public ushort TerrainId;  // Lookups für Materialien/Zonen
        public uint Flags;        // siehe CellFlags
    }
}