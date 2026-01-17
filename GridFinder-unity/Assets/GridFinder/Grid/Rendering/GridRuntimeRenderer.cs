using Reflex.Attributes;
using R3;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Grid
{
    public sealed class GridRuntimeRenderer : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField] private float lineWidth = 0.01f;
        [SerializeField] private float yOffset = 0.01f;

        [Tooltip("Optional. If null, a URP Unlit material will be created at runtime.")]
        [SerializeField] private Material? lineMaterialOverride;

        [Inject] private readonly GridService grid = null!;
        [Inject] private readonly GridRoot gridRoot = null!;

        private Material lineMaterial = null!;
        private readonly CompositeDisposable d = new();

        private void Awake()
        {
            lineMaterial = lineMaterialOverride != null
                ? lineMaterialOverride
                : CreateDefaultLineMaterial();
        }

        private void Start()
        {
            if (grid == null || gridRoot == null)
            {
                Debug.LogWarning("[GridRuntimeRenderer] Missing injected dependencies. No grid will be rendered.");
                return;
            }

            grid.Changed
                .Subscribe(_ => Rebuild())
                .AddTo(d);

            Rebuild();
        }

        private void OnDisable()
        {
            d.Clear();
        }

        [ContextMenu("Rebuild Grid")]
        public void Rebuild()
        {
            ClearChildren();

            var cs = grid.CellSize;
            if (cs <= 0.0001f)
                return;

            var origin = grid.OriginWorld;
            var w = math.max(1, grid.SizeInCells.x);
            var h = math.max(1, grid.SizeInCells.y);

            var floorY = gridRoot.FloorTransform.position.y;
            var y = floorY + yOffset;

            // Vertical lines (parallel to Z)
            for (int x = 0; x <= w; x++)
            {
                var xw = origin.x + x * cs;
                CreateLine(
                    new Vector3(xw, y, origin.z),
                    new Vector3(xw, y, origin.z + h * cs));
            }

            // Horizontal lines (parallel to X)
            for (int z = 0; z <= h; z++)
            {
                var zw = origin.z + z * cs;
                CreateLine(
                    new Vector3(origin.x, y, zw),
                    new Vector3(origin.x + w * cs, y, zw));
            }
        }

        private void CreateLine(Vector3 a, Vector3 b)
        {
            var go = new GameObject("GridLine");
            go.transform.SetParent(transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);

            lr.material = lineMaterial;
            lr.widthMultiplier = lineWidth;

            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.alignment = LineAlignment.View;
            lr.numCapVertices = 0;
            lr.numCornerVertices = 0;
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        private static Material CreateDefaultLineMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            return shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
        }
    }
}
