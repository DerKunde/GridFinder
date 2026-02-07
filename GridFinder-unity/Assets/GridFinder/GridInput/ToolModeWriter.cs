using Unity.Entities;
using UnityEngine;

namespace GridFinder.GridInput
{
    /// <summary>
    /// Managed bridge used by UI. Writes to ECS ToolMode singleton.
    /// </summary>
    public sealed class ToolModeWriter : MonoBehaviour
    {
        private EntityManager em;
        private EntityQuery q;

        private void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            em = world.EntityManager;
            q = em.CreateEntityQuery(
                ComponentType.ReadOnly<ToolModeSingletonTag>(),
                ComponentType.ReadWrite<ToolModeConfig>()
            );
        }

        public void SetTargetMode()
            => SetMode(ToolModeType.SetTarget, -1);

        public void SpawnAgentMode(int contentId)
            => SetMode(ToolModeType.SpawnAgent, contentId);

        public void SpawnWallMode(int wallTypeId = 0)
            => SetMode(ToolModeType.SpawnWall, wallTypeId);

        public void SetGridZoneMode(int zoneId, bool remove = false)
            => SetMode(ToolModeType.SetGridZone, zoneId, flags: remove ? 1u : 0u);

        private void SetMode(ToolModeType mode, int primaryId, uint flags = 0u, byte brush = 1)
        {
            if (em == default || q.IsEmptyIgnoreFilter)
            {
                Debug.LogWarning("[ToolModeWriter] ToolMode singleton not ready yet.");
                return;
            }
            
            var e = q.GetSingletonEntity();
            var cfg = em.GetComponentData<ToolModeConfig>(e);

            cfg.Mode = mode;
            cfg.PrimaryId = primaryId;
            cfg.Flags = flags;
            cfg.Brush = brush;

            Debug.Log("[ToolModeWriter] Setting mode to: " + mode);
            em.SetComponentData(e, cfg);
        }
    }
}