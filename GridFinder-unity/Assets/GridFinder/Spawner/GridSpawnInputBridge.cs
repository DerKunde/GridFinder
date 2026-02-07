using GridFinder.Grid;
using GridFinder.Grid.GridHelper;
using GridFinder.GridInput;
using GridFinder.Targeting;
using Reflex.Attributes;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Spawner
{
    public sealed class GridSpawnInputBridge : MonoBehaviour
    {
        [Inject] private readonly GridPointerState pointer = null!;
        [Inject] private readonly ToolModeState toolMode = null!;
        [Inject] private readonly GridService grid = null!;

        [SerializeField] private float uniformScale = 1f;

        private World world = null!;
        private EntityManager em;

        private SpawnCommandWriter spawnWriter = null!;
        private ISpawnCommandFactory spawnFactory = null!;

        private EntityQuery toolModeQuery;
        private EntityQuery targetRequestQuery;

        private void Awake()
        {
            world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogWarning("[GridSpawnInputBridge] Default ECS World not available. Disabling.");
                enabled = false;
                return;
            }

            em = world.EntityManager;

            spawnWriter = new SpawnCommandWriter(world);
            spawnFactory = new SpawnCommandFactory();

            toolModeQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<ToolModeSingletonTag>(),
                ComponentType.ReadOnly<ToolModeConfig>()
            );

            targetRequestQuery = em.CreateEntityQuery(ComponentType.ReadWrite<TargetSpawnRequest>());
        }

        private void Update()
        {
            // Optional gate: only react in Click interaction mode
            // if (toolMode.Interaction != ToolInteraction.Click)
            //     return;

            if (!Input.GetMouseButtonDown(0))
                return;

            if (!grid.HasConfig)
                return;

            if (!pointer.HasHover.Value)
                return;

            if (toolModeQuery.IsEmptyIgnoreFilter)
                return;

            var cfg = grid.Config;
            var cell = pointer.HoveredCell.Value;

            if (!GridMath.IsInside(cell, cfg))
                return;

            var worldPos = GridMath.CellToWorldCenter(cell, cfg);
            var gridCellIndex = cell.x + cell.y * cfg.Size.x;

            var tool = em.GetComponentData<ToolModeConfig>(toolModeQuery.GetSingletonEntity());

            switch (tool.Mode)
            {
                case ToolModeType.SetTarget:
                    WriteTargetRequest(worldPos, assignToAllAgents: false);
                    return;

                case ToolModeType.SpawnAgent:
                case ToolModeType.SpawnWall:
                case ToolModeType.SetGridZone:
                default:
                {
                    // For everything that spawns: use PrimaryId as the content/payload id
                    var intent = new SpawnIntent(
                        contentId: tool.PrimaryId,
                        worldPos: worldPos,
                        worldRot: quaternion.identity,
                        uniformScale: uniformScale,
                        gridCellIndex: gridCellIndex
                    );

                    if (spawnFactory.TryCreate(in intent, out SpawnCommandData cmd))
                    {
                        spawnWriter.TryEnqueue(in cmd);
                    }

                    return;
                }
            }
        }

        private void WriteTargetRequest(float3 worldPos, bool assignToAllAgents)
        {
            if (targetRequestQuery.IsEmptyIgnoreFilter)
            {
                Debug.LogWarning("[GridSpawnInputBridge] No TargetSpawnRequest buffer entity found.");
                return;
            }

            var reqEntity = targetRequestQuery.GetSingletonEntity();
            var buf = em.GetBuffer<TargetSpawnRequest>(reqEntity);

            buf.Add(new TargetSpawnRequest
            {
                WorldPos = worldPos,
                AssignToAllAgents = (byte)(assignToAllAgents ? 1 : 0)
            });
        }
    }
}
