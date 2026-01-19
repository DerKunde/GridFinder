namespace GridFinder.GridInput
{
    public enum ToolInteraction
    {
        Click,
        Drag
    }

    public sealed class ToolModeState
    {
        public ToolInteraction Interaction { get; set; } = ToolInteraction.Drag; // default
    }
}