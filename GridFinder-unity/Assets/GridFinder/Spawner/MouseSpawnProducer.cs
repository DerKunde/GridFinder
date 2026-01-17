using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using GridFinder.UI; // AppState + ClickMode
using GridFinder.Grid;
using UnityEngine.UIElements;

namespace GridFinder.Spawner
{
    public sealed class MouseSpawnProducer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityEngine.Camera cam;
        [SerializeField] private GridConfig grid = null!;
        [SerializeField] private AppState state = null!;

        [Header("Raycast")]
        [SerializeField] private LayerMask floorMask; // set to GridFloor

        private EntityManager em;
        private EntityQuery q;
        private Entity singleton = Entity.Null;

        void Start()
        {
            em = World.DefaultGameObjectInjectionWorld.EntityManager;

            q = em.CreateEntityQuery(
                ComponentType.ReadOnly<SpawnPrefab>(),
                ComponentType.ReadWrite<SpawnRequest>());

            if (!cam) cam = UnityEngine.Camera.main;
        }

        void Update()
        {
            // Acquire singleton (SubScene may load a frame later)
            if (singleton == Entity.Null)
            {
                var count = q.CalculateEntityCount();
                if (count != 1) return;
                singleton = q.GetSingletonEntity();
            }

            // Keep LMB reserved for gameplay click
            if (!UnityEngine.Input.GetMouseButtonDown(0))
                return;

            if (!cam) cam = UnityEngine.Camera.main;
            if (!cam) return;

            // If later you want to ignore clicks on UI, add EventSystem check here.

            var ray = cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 500f, floorMask, QueryTriggerInteraction.Ignore))
                return;

            // World point on the floor
            var worldPoint = hit.point;

            // Snap to grid
            var cell = grid.WorldToCell((float3)worldPoint);

            // Place agent at cell center on the floor's Y
            // If your floor is perfectly flat: hit.point.y is correct.
            var spawnPos = grid.CellToWorldCenter(cell, worldPoint.y);

            // Branch by click mode (spawn or set target)
            switch (state.CurrentClickMode.Value)
            {
                case ClickMode.SpawnAgent:
                {
                    var buffer = em.GetBuffer<SpawnRequest>(singleton);
                    buffer.Add(new SpawnRequest
                    {
                        Position = spawnPos,
                        Rotation = quaternion.identity
                    });

                    // Milestone 1 metric: count locally
                    state.AgentCount.Value += 1;
                    break;
                }

                case ClickMode.SetTargetPoint:
                {
                    // For now just store it in state (we can add these properties next)
                    // state.LastTargetCell.Value = cell;
                    // state.LastTargetWorld.Value = spawnPos;
                    break;
                }
            }
        }
    }
}
