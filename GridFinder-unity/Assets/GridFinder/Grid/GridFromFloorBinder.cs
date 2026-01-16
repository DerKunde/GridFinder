using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Grid
{
    [ExecuteAlways]
    public sealed class GridFromFloorBinder : MonoBehaviour
    {
        [SerializeField] private GridConfig grid = null!;
        [SerializeField] private Renderer floorRenderer = null!;
        [SerializeField] private bool writeBackToAsset = true;

        [Tooltip("Use the floor's bounds Y as origin Y (useful if floor is not at y=0).")]
        [SerializeField] private bool useFloorY = true;

        void OnEnable() => Apply();
        void OnValidate() => Apply();

        private void Apply()
        {
            if (!grid || !floorRenderer)
                return;

            var b = floorRenderer.bounds;

            var originY = useFloorY ? b.min.y : grid.originWorld.y;

            var origin = new float3(b.min.x, originY, b.min.z);
            var sizeWorld = new float2(b.size.x, b.size.z);

            // Compute cell counts from world size
            var sizeCells = grid.ComputeSizeInCells(sizeWorld);

            if (writeBackToAsset)
            {
                grid.originWorld = origin;
                grid.sizeInCells = sizeCells;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(grid);
#endif
            }
        }
    }
}