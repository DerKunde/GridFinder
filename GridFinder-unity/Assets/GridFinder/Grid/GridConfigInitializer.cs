using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Grid
{
    /// <summary>
    /// Writes initial GridConfig from GridSettings into ECS once at startup.
    /// This makes the AppInstaller inspector values actually take effect.
    /// </summary>
    public sealed class GridConfigInitializer : MonoBehaviour
    {
        private EntityQuery q;
        
        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogError("[GridConfigInitializer] Default ECS World not available.");
                return;
            }

            var em = world.EntityManager;

            q = em.CreateEntityQuery(ComponentType.ReadOnly<GridConfig>());

            if (!q.IsEmptyIgnoreFilter) // GridConfig already exists
            {
                enabled = false;
                return;
            }


            var settings = GridSettings.Instance;
            if (settings == null)
            {
                Debug.LogError("[GridConfigInitializer] GridSettings.Instance is null.");
                return;
            }

            var cs = math.max(0.0001f, settings.CellSize.Value);
            var worldSize = settings.WorldSizeXZ.Value;
            var center = settings.CenterWorld.Value;

            var sizeInCells = new int2(
                math.max(1, (int)math.round(worldSize.x / cs)),
                math.max(1, (int)math.round(worldSize.y / cs))
            );

            var origin = center - new float3(
                sizeInCells.x * cs * 0.5f,
                0f,
                sizeInCells.y * cs * 0.5f
            );

            var e = em.CreateEntity(typeof(GridConfig));
            em.SetComponentData(e, new GridConfig
            {
                Size = sizeInCells,
                CellSize = cs,
                Origin = origin,
                Version = 1u   // ❗ wichtig
            });

            Debug.Log($"[GridConfigInitializer] GridConfig created: size={sizeInCells}, cellSize={cs}");
        }
    }
}