using GridFinder.Grid;
using GridFinder.Grid.GridHelper;
using GridFinder.Spawner;
using Reflex.Attributes;
using Unity.Entities;
using UnityEngine;

namespace GridFinder.GridInput
{
    public sealed class TargetSpawnRequestWriter : MonoBehaviour
    {
        [Inject] private readonly GridPointerState pointer = null!;
        [Inject] private readonly GridService grid = null!;

        private EntityManager em;
        private EntityQuery modeQuery;
        private EntityQuery requestQuery;

        private void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            em = world.EntityManager;

            modeQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<ToolModeSingletonTag>(),
                ComponentType.ReadOnly<ToolModeConfig>()
            );

            requestQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<TargetSpawnRequest>()
            );
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (!pointer.HasHover.Value)
                return;

            if (!modeQuery.TryGetSingleton(out ToolModeConfig mode))
                return;

            if (mode.Mode != ToolModeType.SetTarget)
                return;

            var cell = pointer.HoveredCell.Value;
            var worldPos = GridMath.CellToWorldCenter(cell, grid.Config);

            var e = requestQuery.GetSingletonEntity();
            var buf = em.GetBuffer<TargetSpawnRequest>(e);

            buf.Add(new TargetSpawnRequest
            {
                WorldPos = worldPos,
                AssignToAllAgents = 1
            });
        }
    }
}