using GridFinder.Grid;
using Reflex.Core;
using Reflex.Enums;
using Unity.Mathematics;
using UnityEngine;
using Resolution = Reflex.Enums.Resolution;

namespace GridFinder.Installation
{
    public sealed class AppInstaller : MonoBehaviour, IInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private GridRoot gridPrefab = null!;

        [Header("Initial Grid Params")]
        [SerializeField] private Vector2 worldSizeXZ = new(10f, 10f);
        [SerializeField] private float cellSize = 0.15f;
        [SerializeField] private Vector3 centerWorld = Vector3.zero;

        public void InstallBindings(ContainerBuilder builder)
        {
            var settings = new GridSettings();
            settings.WorldSizeXZ.Value = new float2(worldSizeXZ.x, worldSizeXZ.y);
            settings.CellSize.Value = cellSize;
            settings.CenterWorld.Value = (float3)centerWorld;

            var gridService = new GridService(settings);

            builder.RegisterValue(settings);
            builder.RegisterValue(gridService);

            // Create the factory as a value (so it can hold the prefab reference)
            var gridRootFactory = new GridRootFactory(gridPrefab, settings);
            builder.RegisterValue(gridRootFactory);

            // Create GridRoot via factory using the container (Eager so it spawns immediately)
            builder.RegisterFactory(
                container => gridRootFactory.Create(container),
                Lifetime.Singleton, Resolution.Eager
            );
        }

        private static void ApplyFloor(GridSettings settings, GridRoot root)
        {
            var size = settings.WorldSizeXZ.Value;
            var center = settings.CenterWorld.Value;

            // Root at center
            root.transform.position = new Vector3(center.x, center.y, center.z);

            // IMPORTANT:
            // If your Floor is a Quad rotated X=90, its local X/Y correspond to world X/Z.
            root.FloorTransform.localScale = new Vector3(size.x, size.y, 1f);
        }
    }
}
