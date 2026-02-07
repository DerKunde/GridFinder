using GridFinder.Grid;
using GridFinder.Grid.GridHelper;
using Reflex.Attributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GridFinder.GridInput
{
    /// <summary>
    /// Reads pointer input (mouse for now) and writes hover/drag state into GridPointerState.
    /// Updated to use GridService + GridMath + GridConfig (cell center convention).
    /// </summary>
    public sealed class GridPointerInputBehaviour : MonoBehaviour
    {
        private const string FloorLayerName = "GridFloor";

        [Inject] private readonly GridPointerState pointer = null!;
        [Inject] private readonly ToolModeState toolMode = null!;
        [Inject] private readonly GridService grid = null!;
        [Inject] private readonly UnityEngine.Camera cam = null!;

        [SerializeField] private float maxDistance = 500f;

        private int floorMask;
        private bool isDragging;
        private int2 dragStart;

        private void Awake()
        {
            var layer = LayerMask.NameToLayer(FloorLayerName);
            floorMask = layer >= 0 ? (1 << layer) : 0;

            if (floorMask == 0)
                Debug.LogWarning($"[GridPointerInput] Layer '{FloorLayerName}' not found. Raycast will fail.");
        }

        private void Update()
        {
            if (floorMask == 0 || cam == null || grid == null)
                return;
            
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                pointer.HasHover.Value = false;
                pointer.IsDragging.Value = false;
                return;
            }

            // Need config to map world -> cell reliably
            if (!grid.HasConfig)
            {
                pointer.HasHover.Value = false;
                pointer.IsDragging.Value = false;
                isDragging = false;
                return;
            }

            if (!TryGetHoveredCell(out var cell, out _))
            {
                pointer.HasHover.Value = false;
                pointer.IsDragging.Value = false;
                isDragging = false;
                return;
            }

            pointer.HasHover.Value = true;
            pointer.HoveredCell.Value = cell;

            if (toolMode.Interaction == ToolInteraction.Click)
            {
                // For now we only need hover feedback
                pointer.IsDragging.Value = false;
                isDragging = false;
                return;
            }

            // Drag mode (visual preview only)
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                dragStart = cell;

                pointer.IsDragging.Value = true;
                pointer.DragStartCell.Value = dragStart;
                pointer.DragEndCell.Value = cell;
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                pointer.IsDragging.Value = true;
                pointer.DragEndCell.Value = cell;
            }

            if (isDragging && Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                pointer.DragEndCell.Value = cell;
                pointer.IsDragging.Value = false;
            }
        }

        private bool TryGetHoveredCell(out int2 cell, out float floorY)
        {
            cell = default;
            floorY = 0f;

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, maxDistance, floorMask, QueryTriggerInteraction.Ignore))
                return false;

            var cfg = grid.Config;

            // Convert world -> cell using GridMath + config
            var world = new float3(hit.point.x, hit.point.y, hit.point.z);
            var c = GridMath.WorldToCell(world, cfg);

            // Treat "no hover" if outside grid bounds
            if (!GridMath.IsInside(c, cfg))
                return false;

            floorY = hit.point.y;
            cell = c;
            return true;
        }
    }
}
