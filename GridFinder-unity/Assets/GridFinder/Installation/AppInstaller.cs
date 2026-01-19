using GridFinder.Grid;
using GridFinder.GridInput;
using GridFinder.Visuals;
using Reflex.Core;
using Reflex.Enums;
using Unity.Mathematics;
using UnityEditor.EditorTools;
using UnityEngine;
using Resolution = Reflex.Enums.Resolution;

namespace GridFinder.Installation
{
    public sealed class AppInstaller : MonoBehaviour, IInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject gridPrefab = null!;
        [SerializeField] private GameObject gridFeedbackPrefab = null!;

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
            
            builder.RegisterSingleton<GridPointerState>(Resolution.Eager);
            builder.RegisterSingleton<ToolModeState>(Resolution.Eager);

            builder.RegisterValue(UnityEngine.Camera.main);

            var feedbackFactory = new GridFeedbackFactory(gridFeedbackPrefab, gridRootFactory);
            builder.RegisterValue(feedbackFactory);

            builder.RegisterFactory(
                c => feedbackFactory.Create(c),
                Lifetime.Singleton, Resolution.Eager
            );
        }
    }
}
