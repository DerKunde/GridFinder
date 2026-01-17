using R3;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Input
{
    public enum ToolInteraction
    {
        Click,
        Drag
    }
    
    public sealed class GridPointerInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] public UnityEngine.Camera cam;
        [SerializeField] public GridFinder.Grid.GridConfig grid = null!;
        
        
        [Header("Raycast")]
        [SerializeField] public LayerMask floorMask;
        [SerializeField] public float maxDistance = 500f;
        
        [Header("Interaction")]
        [SerializeField] public ToolInteraction interaction = ToolInteraction.Click;
        
         public ReadOnlyReactiveProperty<int2> HoveredCell => hoveredCell;
        public Observable<int2> ClickCommitted => clickCommitted;
        public Observable<(int2 start, int2 end)> DragCommitted => dragCommitted;
        public ReadOnlyReactiveProperty<(int2 start, int2 end)> DragPreview => dragPreview;

        private readonly ReactiveProperty<int2> hoveredCell = new(new int2(int.MinValue, int.MinValue));
        private readonly Subject<int2> clickCommitted = new();
        private readonly Subject<(int2 start, int2 end)> dragCommitted = new();
        private readonly ReactiveProperty<(int2 start, int2 end)> dragPreview = new((new int2(0,0), new int2(0,0)));

        private bool hasHover;
        private bool isDragging;
        private int2 dragStart;

        void Awake()
        {
            if (!cam) cam = UnityEngine.Camera.main;
        }

        void Update()
        {
            if (!cam || !grid)
                return;
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
                SetInteraction(interaction == ToolInteraction.Click ? ToolInteraction.Drag : ToolInteraction.Click);
            
            hasHover = TryGetHoveredCell(out var cell, out var floorY);

            if (hasHover)
                hoveredCell.Value = cell;

            // Start interaction only when we are hovering valid floor
            if (!hasHover)
                return;

            if (interaction == ToolInteraction.Click)
            {
                if (UnityEngine.Input.GetMouseButtonDown(0))
                    clickCommitted.OnNext(cell);

                // ensure dragging state is reset
                isDragging = false;
                return;
            }

            // Drag mode
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                dragStart = cell;
                dragPreview.Value = (dragStart, dragStart);
            }

            if (isDragging && UnityEngine.Input.GetMouseButton(0))
            {
                dragPreview.Value = (dragStart, cell);
            }

            if (isDragging && UnityEngine.Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                dragCommitted.OnNext((dragStart, cell));
            }
        }

        public void SetInteraction(ToolInteraction mode)
        {
            interaction = mode;
            isDragging = false;
        }

        private bool TryGetHoveredCell(out int2 cell, out float floorY)
        {
            cell = default;
            floorY = 0f;

            var ray = cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, maxDistance, floorMask, QueryTriggerInteraction.Ignore))
                return false;

            floorY = hit.point.y;
            cell = grid.WorldToCell((float3)hit.point);
            return true;
        }
    }
}
