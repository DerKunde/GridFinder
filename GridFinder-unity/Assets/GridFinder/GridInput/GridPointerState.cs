using R3;
using Unity.Mathematics;

namespace GridFinder.GridInput
{
    public sealed class GridPointerState
    {
        public readonly ReactiveProperty<bool> HasHover = new(false);
        public readonly ReactiveProperty<int2> HoveredCell = new(new int2(0, 0));

        public readonly ReactiveProperty<bool> IsDragging = new(false);
        public readonly ReactiveProperty<int2> DragStartCell = new(new int2(0, 0));
        public readonly ReactiveProperty<int2> DragEndCell = new(new int2(0, 0));
    }
}