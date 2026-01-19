using GridFinder.Grid;
using GridFinder.GridInput;
using Reflex.Attributes;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Spawner
{
    /// <summary>
    /// Bridges grid pointer input (state-based) to the ECS spawn pipeline.
    /// Listens for click interactions while hovering the grid and enqueues spawn commands.
    /// </summary>
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
            writer = new SpawnCommandWriter(
                Unity.Entities.World.DefaultGameObjectInjectionWorld
            );

            factory = new SpawnCommandFactory();
        }

        private void Update()
        {
            // // We only react in Click mode
            // if (toolMode.Interaction != ToolInteraction.Click)
            //     return;

            // Only on mouse down
            if (!Input.GetMouseButtonDown(0))
                return;

            // Must be hovering a valid grid cell
            if (!pointer.HasHover.Value)
                return;

            var cell = pointer.HoveredCell.Value;

            // Convert cell -> world position
            float3 worldPos = grid.CellToWorld(cell);

            var intent = new SpawnIntent(
                contentId: contentId,
                worldPos: worldPos,
                worldRot: quaternion.identity,
                uniformScale: uniformScale,
                gridCellIndex: 1
            );

            SpawnCommandData cmd;
            if (factory.TryCreate(in intent, out cmd))
            {
                writer.TryEnqueue(in cmd);
            }
        }
    }
}