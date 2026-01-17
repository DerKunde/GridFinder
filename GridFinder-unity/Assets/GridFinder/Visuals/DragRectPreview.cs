using R3;
using Unity.Mathematics;
using UnityEngine;
using GridFinder.Grid;

namespace GridFinder.Visuals
{
    public sealed class DragRectPreview : MonoBehaviour
    {
        [SerializeField] private GridConfig grid = null!;
        [SerializeField] private GridFinder.Input.GridPointerInput input = null!;

        [Header("LineRenderer")]
        [SerializeField] private Material lineMaterial = null!;
        [SerializeField] private float lineWidth = 0.01f;
        [SerializeField] private float yOffset = 0.02f;

        private LineRenderer top, bottom, left, right;
        private readonly CompositeDisposable d = new();

        void Awake()
        {
            top = CreateLine("Top");
            bottom = CreateLine("Bottom");
            left = CreateLine("Left");
            right = CreateLine("Right");

            SetVisible(false);
        }

        void OnEnable()
        {
            if (!grid || input == null)
                return;

            input.DragPreview
                .Subscribe(pair =>
                {
                    // only show during drag mode + when mouse is held (preview updates)
                    DrawRect(pair.start, pair.end);
                })
                .AddTo(d);
        }

        void OnDisable() => d.Clear();

        private void DrawRect(int2 a, int2 b)
        {
            // If not in drag mode or no meaningful preview yet, hide
            // (We'll still draw even if a==b; looks like a small rect)
            var min = new int2(math.min(a.x, b.x), math.min(a.y, b.y));
            var max = new int2(math.max(a.x, b.x), math.max(a.y, b.y));

            var y = grid.originWorld.y + yOffset;

            // rectangle in world-space using cell corners
            // min corner = CellToWorldMinCorner(min)
            // max corner = min corner + (width,height) * cellSize
            var minCorner = grid.CellToWorldMinCorner(min, y);
            var maxCorner = new float3(
                grid.originWorld.x + (max.x + 1) * grid.cellSize,
                y,
                grid.originWorld.z + (max.y + 1) * grid.cellSize);

            var p0 = (Vector3)minCorner;                         // bottom-left
            var p1 = new Vector3(maxCorner.x, y, minCorner.z);   // bottom-right
            var p2 = (Vector3)maxCorner;                         // top-right
            var p3 = new Vector3(minCorner.x, y, maxCorner.z);   // top-left

            SetVisible(true);

            SetLine(bottom, p0, p1);
            SetLine(right,  p1, p2);
            SetLine(top,    p3, p2);
            SetLine(left,   p0, p3);
        }

        private LineRenderer CreateLine(string name)
        {
            var go = new GameObject($"DragRect_{name}");
            go.transform.SetParent(transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.material = lineMaterial;
            lr.widthMultiplier = lineWidth;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.alignment = LineAlignment.View;

            return lr;
        }

        private static void SetLine(LineRenderer lr, Vector3 a, Vector3 b)
        {
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
        }

        private void SetVisible(bool visible)
        {
            top.enabled = visible;
            bottom.enabled = visible;
            left.enabled = visible;
            right.enabled = visible;
        }
    }
}
