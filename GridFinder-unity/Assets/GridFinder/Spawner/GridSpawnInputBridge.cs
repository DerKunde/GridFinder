using GridFinder.Grid;
using GridFinder.Grid.GridHelper;
using GridFinder.GridInput;
using Reflex.Attributes;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Spawner
{
    public sealed class GridSpawnInputBridge : MonoBehaviour
    {
        [Inject] private readonly GridPointerState pointer = null!;
        [Inject] private readonly ToolModeState toolMode = null!;
        [Inject] private readonly GridService grid = null!;

        [Header("Spawn Mapping")]
        [SerializeField] private int contentId = 0;
        [SerializeField] private float uniformScale = 1f;

        private SpawnCommandWriter writer = null!;
        private ISpawnCommandFactory factory = null!;

        private void Awake()
        {
            var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogWarning("[GridSpawnInputBridge] Default ECS World not available. Disabling.");
                enabled = false;
                return;
            }

            writer = new SpawnCommandWriter(world);
            factory = new SpawnCommandFactory();
        }

        private void Update()
        {
            // We only react in Click mode (uncomment if desired)
            // if (toolMode.Interaction != ToolInteraction.Click)
            //     return;

            if (!Input.GetMouseButtonDown(0))
                return;

            if (!grid.HasConfig)
                return;

            if (!pointer.HasHover.Value)
                return;

            var cfg = grid.Config;
            var cell = pointer.HoveredCell.Value;

            // Optional: ignore clicks outside bounds (if pointer could ever produce that)
            if (!GridMath.IsInside(cell, cfg))
                return;

            var worldPos = GridMath.CellToWorldCenter(cell, cfg);

            // Proper linear index
            var gridCellIndex = cell.x + cell.y * cfg.Size.x;

            var intent = new SpawnIntent(
                contentId: contentId,
                worldPos: worldPos,
                worldRot: quaternion.identity,
                uniformScale: uniformScale,
                gridCellIndex: gridCellIndex
            );

            if (factory.TryCreate(in intent, out SpawnCommandData cmd))
            {
                writer.TryEnqueue(in cmd);
            }
        }
    }
}
