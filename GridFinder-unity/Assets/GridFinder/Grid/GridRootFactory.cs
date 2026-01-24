using GridFinder.Grid.GridHelper;
using Reflex.Core;
using Reflex.Injectors;
using R3;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Grid
{
    /// <summary>
    /// Creates (and owns) a GridRoot prefab instance and keeps it in sync with the ECS GridConfig.
    /// </summary>
    public sealed class GridRootFactory
    {
        private const string FloorLayerName = "GridFloor";

        private readonly GameObject prefab;
        private readonly GridService grid; // NEW: bridge to GridConfig
        private readonly CompositeDisposable d = new();

        public GridRoot? Instance { get; private set; }

        public GridRootFactory(GameObject prefab, GridService grid)
        {
            this.prefab = prefab;
            this.grid = grid;
        }

        public GridRoot Create(Container container)
        {
            if (prefab == null)
            {
                Debug.LogError("[GridRootFactory] Prefab is null.");
                return null!;
            }

            if (Instance != null)
                return Instance;

            var go = Object.Instantiate(prefab);
            go.name = "GridRoot";

            var gridRoot = go.GetComponent<GridRoot>();
            if (gridRoot == null)
            {
                Debug.LogError("[GridRootFactory] Prefab is missing GridRoot component.");
                return null!;
            }

            // Ensure references are set (your updated GridRoot has ResolveReferences)
            gridRoot.ResolveReferences();

            // Inject recursively using THIS container (NOT scene container)
            GameObjectInjector.InjectRecursive(gridRoot.gameObject, container);

            // Apply initial layout if config is already available
            TryApplyFromConfig(gridRoot);

            // Keep it updated when GridConfig changes
            grid.Changed
                .Subscribe(_ => TryApplyFromConfig(gridRoot))
                .AddTo(d);

            gridRoot.gameObject.SetActive(true);

            Instance = gridRoot;
            return gridRoot;
        }

        private void TryApplyFromConfig(GridRoot root)
        {
            if (grid == null || !grid.HasConfig)
                return;

            ApplyFromConfig(grid.Config, root);
        }

        private static void ApplyFromConfig(in GridConfig cfg, GridRoot root)
        {
            if (!root.FloorTransform)
            {
                Debug.LogError("[GridRootFactory] FloorTransform is not assigned on GridRoot.");
                return;
            }

            // --- Compute world size + center from config ---
            var worldSize = GridMath.WorldSizeXZ(cfg);    // float2 (widthX, heightZ)
            var worldCenter = GridMath.WorldCenter(cfg);  // float3

            // Decide Y: keep current floor Y (so you can move it in prefab), but align XZ to grid
            var y = root.FloorTransform.position.y;

            // Apply transform to floor
            root.ApplyFloorLayout(
                worldCenter: new Vector3(worldCenter.x, worldCenter.y, worldCenter.z),
                worldSizeXZ: new Vector2(worldSize.x, worldSize.y),
                y: y
            );

            // Ensure correct raycast layer
            var layer = LayerMask.NameToLayer(FloorLayerName);
            if (layer >= 0)
                root.FloorTransform.gameObject.layer = layer;

            // Optional but useful: ensure renderer enabled
            if (root.FloorRenderer != null)
                root.FloorRenderer.enabled = true;
            Debug.LogError("[GridRootFactory] Config applied.");

        }
    }
}
