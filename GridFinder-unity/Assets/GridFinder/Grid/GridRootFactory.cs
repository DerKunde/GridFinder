using Reflex.Core;
using Reflex.Injectors;
using R3;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Grid
{
    /// <summary>
    /// Creates (and owns) a GridRoot prefab instance and keeps it in sync with GridSettings.
    /// This replaces the old FloorFactory.
    /// </summary>
    public sealed class GridRootFactory
    {
        private readonly GameObject prefab;
        private readonly GridSettings settings;

        private readonly CompositeDisposable d = new();

        public GridRoot? Instance { get; private set; }

        public GridRootFactory(GameObject prefab, GridSettings settings)
        {
            this.prefab = prefab;
            this.settings = settings;
        }

        public GridRoot Create(Container container)
        {
            if (prefab == null)
            {
                Debug.LogError("[GridRootFactory] Prefab is null.");
                return null!;
            }

            // If already created, reuse (prevents duplicates if called again)
            if (Instance != null)
                return Instance;

            var go = Object.Instantiate(prefab);
            go.name = "GridRoot";

            Debug.Log($"[GridRootFactory] Instantiated '{go.name}' children={go.transform.childCount}");
            for (int i = 0; i < go.transform.childCount; i++)
                Debug.Log($"  child[{i}] = {go.transform.GetChild(i).name} active={go.transform.GetChild(i).gameObject.activeSelf}");
            
            var gridRoot = go.GetComponent<GridRoot>();
            Apply(settings, gridRoot);

            // Inject recursively using THIS container (NOT scene.GetSceneContainer())
            GameObjectInjector.InjectRecursive(gridRoot.gameObject, container);

            // Keep it updated when settings change
            settings.WorldSizeXZ.Subscribe(_ => Apply(settings, gridRoot)).AddTo(d);
            settings.CenterWorld.Subscribe(_ => Apply(settings, gridRoot)).AddTo(d);

            gridRoot.gameObject.SetActive(true);

            Instance = gridRoot;
            return gridRoot;
        }

        private static void Apply(GridSettings s, GridRoot root)
        {
            var size = s.WorldSizeXZ.Value;
            var center = s.CenterWorld.Value;

            root.transform.position = new Vector3(center.x, center.y, center.z);

            if (!root.FloorTransform)
            {
                Debug.LogError("[GridRootFactory] FloorTransform is not assigned on GridRoot.");
                return;
            }

            // Ensure the floor sits at the root
            root.FloorTransform.localPosition = Vector3.zero;
            root.FloorTransform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // Quad rotated X=90: local X/Y -> world X/Z
            root.FloorTransform.localScale = new Vector3(size.x, size.y, 1f);

            // Optional but useful: ensure renderer enabled
            if (root.FloorRenderer != null)
                root.FloorRenderer.enabled = true;

            // Ensure correct raycast layer
            var layer = LayerMask.NameToLayer("GridFloor");
            if (layer >= 0)
                root.FloorTransform.gameObject.layer = layer;
        }
    }
}
