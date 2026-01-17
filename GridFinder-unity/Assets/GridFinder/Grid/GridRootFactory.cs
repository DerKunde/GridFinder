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
        private readonly GridRoot prefab;
        private readonly GridSettings settings;

        private readonly CompositeDisposable d = new();

        public GridRoot? Instance { get; private set; }

        public GridRootFactory(GridRoot prefab, GridSettings settings)
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

            var gridRoot = Object.Instantiate(prefab);
            gridRoot.name = "GridRoot";
            gridRoot.gameObject.SetActive(false);

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
            var size = s.WorldSizeXZ.Value;   // float2 (x,z)
            var center = s.CenterWorld.Value; // float3

            // Root sits at center
            root.transform.position = new Vector3(center.x, center.y, center.z);

            // If Floor is a Quad rotated X=90, its local X/Y correspond to world X/Z.
            // So we scale local X by size.x and local Y by size.z.
            root.FloorTransform.localScale = new Vector3(size.x, size.y, 1f);
        }
    }
}
