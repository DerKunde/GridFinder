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
        [SerializeField] private int2 gridSizeInCells = default;
        [SerializeField] private float cellSize = 0.01f;
        [SerializeField] private Vector3 centerWorld = Vector3.zero;

        public void InstallBindings(ContainerBuilder builder)
        {
            var settings = new GridSettings();
            settings.GridSizeInCells.Value = gridSizeInCells;
            settings.CellSize.Value = cellSize;
            settings.CenterWorld.Value = (float3)centerWorld;
            builder.RegisterValue(settings);

            // GridService (bridge)
            var gridServiceGo = new GameObject("GridService");
            gridServiceGo.transform.SetParent(transform, worldPositionStays: false);
            var gridService = gridServiceGo.AddComponent<GridService>();
            builder.RegisterValue(gridService);

            // ✅ Register GridRootFactory itself (so other scripts can inject it)
            var gridRootFactory = new GridRootFactory(gridPrefab, gridService);
            builder.RegisterValue(gridRootFactory);

            // ✅ Eager-create GridRoot (Singleton instance) using the factory
            builder.RegisterFactory(
                container => gridRootFactory.Create(container),
                Lifetime.Singleton,
                Resolution.Eager
            );

            builder.RegisterSingleton<GridPointerState>(Resolution.Eager);
            builder.RegisterSingleton<ToolModeState>(Resolution.Eager);

            builder.RegisterValue(UnityEngine.Camera.main);

            var feedbackFactory = new GridFeedbackFactory(gridFeedbackPrefab, gridRootFactory);
            builder.RegisterValue(feedbackFactory);

            builder.RegisterFactory(
                c => feedbackFactory.Create(c),
                Lifetime.Singleton,
                Resolution.Eager
            );
        }
    }
}
