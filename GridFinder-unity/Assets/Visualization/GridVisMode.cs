namespace Visualization
{
    public enum GridVisMode
    {
        None,
        WalkableMask,   // weiß = begehbar, schwarz = blockiert
        CostHeatmap     // grün → gelb → rot je nach Kosten
    }
}