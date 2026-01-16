using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Grid
{
    public sealed class GridRuntimeRenderer : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private GridConfig grid = null!;
        [SerializeField] private Renderer floorRenderer = null!;

        [Header("Rendering")]
        [SerializeField] private Material lineMaterial = null!;
        [SerializeField] private float lineWidth = 0.01f;
        [SerializeField] private float yOffset = 0.01f;

        [Header("Lifecycle")]
        [SerializeField] private bool rebuildOnEnable = true;

        private void OnEnable()
        {
            if (rebuildOnEnable)
                Rebuild();
        }

        [ContextMenu("Rebuild Grid")]
        public void Rebuild()
        {
            ClearChildren();

            if (!grid || !floorRenderer || !lineMaterial)
                return;

            var cs = grid.cellSize;
            if (cs <= 0.0001f)
                return;

            var bounds = floorRenderer.bounds;

            var origin = new float3(bounds.min.x, bounds.min.y + yOffset, bounds.min.z);
            var sizeWorld = new float2(bounds.size.x, bounds.size.z);

            var w = math.max(1, (int)math.floor(sizeWorld.x / cs));
            var h = math.max(1, (int)math.floor(sizeWorld.y / cs));

            // Vertical lines (parallel to Z)
            for (int x = 0; x <= w; x++)
            {
                var xw = origin.x + x * cs;
                CreateLine(
                    new Vector3(xw, origin.y, origin.z),
                    new Vector3(xw, origin.y, origin.z + h * cs));
            }

            // Horizontal lines (parallel to X)
            for (int z = 0; z <= h; z++)
            {
                var zw = origin.z + z * cs;
                CreateLine(
                    new Vector3(origin.x, origin.y, zw),
                    new Vector3(origin.x + w * cs, origin.y, zw));
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
            {
                if (Application.isPlaying)
                    Destroy(transform.GetChild(i).gameObject);
                else
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}
