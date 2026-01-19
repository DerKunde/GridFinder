using Reflex.Attributes;
using R3;
using Unity.Mathematics;
using UnityEngine;
using GridFinder.Grid;
using GridFinder.GridInput;

namespace GridFinder.Visuals
{
    public sealed class GridFeedbackRendererBehaviour : MonoBehaviour
    {
        [Header("Hover")]
        [SerializeField] private float hoverInset = 0.95f;
        [SerializeField] private float hoverYOffset = 0.01f;

        [Header("Drag Rect")]
        [SerializeField] private float rectYOffset = 0.02f;
        [SerializeField] private float lineWidth = 0.01f;

        [Header("Materials (optional)")]
        [SerializeField] private Material? hoverMaterialOverride;
        [SerializeField] private Material? lineMaterialOverride;

        [Inject] private readonly GridPointerState pointer = null!;
        [Inject] private readonly GridService grid = null!;
        [Inject] private readonly GridRootFactory gridRootFactory = null!;

        private Transform hoverQuad = null!;
        private LineRenderer top = null!, bottom = null!, left = null!, right = null!;
        private Material hoverMat = null!;
        private Material lineMat = null!;
        private readonly CompositeDisposable d = new();

        private void Awake()
        {
            hoverMat = hoverMaterialOverride != null ? hoverMaterialOverride : CreateUnlitMaterial();
            lineMat = lineMaterialOverride != null ? lineMaterialOverride : CreateUnlitMaterial();

            hoverQuad = CreateHoverQuad();
            (top, bottom, left, right) = CreateRectLines();

            SetRectVisible(false);
            hoverQuad.gameObject.SetActive(false);
        }

        private void Start()
        {
            // Hover
            pointer.HasHover
                .Subscribe(has =>
                {
                    if (hoverQuad != null)
                        hoverQuad.gameObject.SetActive(has);
                })
                .AddTo(d);

            pointer.HoveredCell
                .Subscribe(cell =>
                {
                    if (!pointer.HasHover.Value) return;
                    UpdateHover(cell);
                })
                .AddTo(d);

            // Drag rect
            pointer.IsDragging
                .Subscribe(isDrag => SetRectVisible(isDrag))
                .AddTo(d);

            // Update rect while dragging or when end changes
            pointer.DragEndCell
                .Subscribe(_ =>
                {
                    if (!pointer.IsDragging.Value) return;
                    DrawRect(pointer.DragStartCell.Value, pointer.DragEndCell.Value);
                })
                .AddTo(d);
        }

        private void OnDisable()
        {
            d.Clear();
        }

        private void UpdateHover(int2 cell)
        {
            var floorY = GetFloorY();
            var pos = grid.CellToWorldCenter(cell, floorY + hoverYOffset);

            hoverQuad.position = (Vector3)pos;

            var s = grid.CellSize * hoverInset;
            // Quad is rotated to lie on XZ, so scale X/Y
            hoverQuad.localScale = new Vector3(s, s, 1f);
        }

        private void DrawRect(int2 a, int2 b)
        {
            var min = new int2(math.min(a.x, b.x), math.min(a.y, b.y));
            var max = new int2(math.max(a.x, b.x), math.max(a.y, b.y));

            var y = GetFloorY() + rectYOffset;

            // World corners from grid origin/cell size
            var minCorner = grid.CellToWorldMinCorner(min, y);
            var maxCorner = new float3(
                grid.OriginWorld.x + (max.x + 1) * grid.CellSize,
                y,
                grid.OriginWorld.z + (max.y + 1) * grid.CellSize);

            var p0 = (Vector3)minCorner;                       // bottom-left
            var p1 = new Vector3(maxCorner.x, y, minCorner.z); // bottom-right
            var p2 = (Vector3)maxCorner;                       // top-right
            var p3 = new Vector3(minCorner.x, y, maxCorner.z); // top-left

            SetLine(bottom, p0, p1);
            SetLine(right,  p1, p2);
            SetLine(top,    p3, p2);
            SetLine(left,   p0, p3);
        }

        private float GetFloorY()
        {
            // Prefer the actual spawned GridRoot floor Y
            var root = gridRootFactory.Instance;
            if (root != null && root.FloorTransform != null)
                return root.FloorTransform.position.y;

            return grid.OriginWorld.y;
        }

        private Transform CreateHoverQuad()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "HoverCell";
            Destroy(go.GetComponent<Collider>());

            var t = go.transform;
            t.SetParent(transform, false);
            t.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = hoverMat;

            return t;
        }

        private (LineRenderer top, LineRenderer bottom, LineRenderer left, LineRenderer right) CreateRectLines()
        {
            var bottom = CreateLine("DragRect_Bottom");
            var right = CreateLine("DragRect_Right");
            var top = CreateLine("DragRect_Top");
            var left = CreateLine("DragRect_Left");
            return (top, bottom, left, right);
        }

        private LineRenderer CreateLine(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.material = lineMat;
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

        private void SetRectVisible(bool visible)
        {
            if (top) top.enabled = visible;
            if (bottom) bottom.enabled = visible;
            if (left) left.enabled = visible;
            if (right) right.enabled = visible;
        }

        private static Material CreateUnlitMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            return shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
        }
    }
}
