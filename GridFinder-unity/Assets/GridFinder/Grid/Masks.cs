namespace GridFinder.Grid
{
    [System.Flags]
    public enum ZoneMask : byte
    {
        None      = 0,
        General   = 1 << 0,
        StaffOnly = 1 << 1,
        Secured   = 1 << 2
    }

    [System.Flags]
    public enum FeatureMask : ushort
    {
        None    = 0,
        Shop    = 1 << 0,
        Gate    = 1 << 1,
        Toilet  = 1 << 2,
        Baggage = 1 << 3
    }

    /// <summary>Unmanaged, Burst-friendly cell payload.</summary>
    public struct GridCellData
    {
        public byte Walkable;      // 0/1
        public ZoneMask Zones;     // bitmask
        public FeatureMask Features;
        public byte Cost;          // optional (1..255), 0 = default
    }
}