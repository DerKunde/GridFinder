using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Grid
{
    [ExecuteAlways]
    public sealed class GridGizmoVisualizer : MonoBehaviour
    {
        [Header("Grid Source")]
        [SerializeField] private GridConfig grid = null!;

        [Tooltip("Optional. If set, the gizmo grid will align to this renderer's world bounds (e.g. your floor plane).")]
        [SerializeField] private Renderer floorRenderer;

        [Tooltip("If true, use floor bounds to compute origin and size. If false, use GridConfig values.")]
        [SerializeField] private bool alignToFloor = true;

        [Header("Draw")]
        [SerializeField] private float yOffset = 0.01f;
        [SerializeField] private bool drawCells = false;

        [SerializeField] private Color gridColor = Color.blue;
        [SerializeField] private Color cellColor = Color.cyan;
        
        [SerializeField] private bool drawInPlayMode = true;

        void OnDrawGizmos()
        {
            if (!grid)
                return;

            // Decide where to take origin/size from
            float3 origin;
            float cs;
            int w, h;

            cs = grid.cellSize;
            if (cs <= 0.0001f)
                return;

            if (alignToFloor && floorRenderer != null)
            {
                var b = floorRenderer.bounds;

                // Use world-space bounds min corner as origin (cell 0,0 = min XZ)
                origin = new float3(b.min.x, b.min.y, b.min.z);

                // Compute cell counts from bounds size (XZ)
                w = math.max(1, (int)math.floor(b.size.x / cs));
                h = math.max(1, (int)math.floor(b.size.z / cs));
            }
            else
            {
                if (!grid.HasBounds)
                    return;

                origin = grid.originWorld;
                w = grid.sizeInCells.x;
                h = grid.sizeInCells.y;
            }

            var y = origin.y + yOffset;

            // Draw grid lines (cell borders)
            Gizmos.color = gridColor;

            // Lines parallel to Z (vary X)
            for (int x = 0; x <= w; x++)
            {
                var xw = origin.x + x * cs;
                var a = new Vector3(xw, y, origin.z);
                var b = new Vector3(xw, y, origin.z + h * cs);
                Gizmos.DrawLine(a, b);
            }

            // Lines parallel to X (vary Z)
            for (int z = 0; z <= h; z++)
            {
                var zw = origin.z + z * cs;
                var a = new Vector3(origin.x, y, zw);
                var b = new Vector3(origin.x + w * cs, y, zw);
                Gizmos.DrawLine(a, b);
            }

            if (!drawCells)
                return;

            // Optional: draw cell centers as small cubes
            Gizmos.color = cellColor;
            var cubeSize = new Vector3(cs * 0.05f, 0.001f, cs * 0.05f);

            for (int x = 0; x < w; x++)
            for (int z = 0; z < h; z++)
            {
                // Use our computed origin instead of grid.CellToWorldCenter (which uses grid.originWorld)
                var center = new float3(
                    origin.x + (x + 0.5f) * cs,
                    y,
                    origin.z + (z + 0.5f) * cs);

                Gizmos.DrawCube((Vector3)center, cubeSize);
            }
        }
        
        void Update()
        {
            if (!Application.isPlaying || !drawInPlayMode)
                return;

            DrawRuntimeLines();
        }
        
        private void DrawRuntimeLines()
        {
            if (!grid) return;
            if (alignToFloor && floorRenderer == null) return;

            var cs = grid.cellSize;
            if (cs <= 0.0001f) return;

            float3 origin;
            int w, h;
            float y;

            if (alignToFloor && floorRenderer != null)
            {
                var b = floorRenderer.bounds;
                origin = new float3(b.min.x, b.min.y, b.min.z);
                w = math.max(1, (int)math.floor(b.size.x / cs));
                h = math.max(1, (int)math.floor(b.size.z / cs));
                y = origin.y + yOffset;
            }
            else
            {
                if (!grid.HasBounds) return;
                origin = grid.originWorld;
                w = grid.sizeInCells.x;
                h = grid.sizeInCells.y;
                y = origin.y + yOffset;
            }

            // Draw for one frame; Update repeats it
            for (int x = 0; x <= w; x++)
            {
                var xw = origin.x + x * cs;
                Debug.DrawLine(new Vector3(xw, y, origin.z), new Vector3(xw, y, origin.z + h * cs), gridColor);
            }

            for (int z = 0; z <= h; z++)
            {
                var zw = origin.z + z * cs;
                Debug.DrawLine(new Vector3(origin.x, y, zw), new Vector3(origin.x + w * cs, y, zw), gridColor);
            }
        }
    }
}
