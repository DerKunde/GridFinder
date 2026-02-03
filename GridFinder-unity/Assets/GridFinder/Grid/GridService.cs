using R3;
using Unity.Entities;
using UnityEngine;

namespace GridFinder.Grid
{
    public sealed class GridService : MonoBehaviour
    {
        public float CellSize { get; private set; }
        public Vector3 OriginWorld { get; private set; }
        public Vector2Int SizeInCells { get; private set; }

        public GridConfig Config { get; private set; }
        public bool HasConfig { get; private set; }

        public readonly Subject<Unit> Changed = new();

        private uint lastVersion = uint.MaxValue;

        private void Update()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                HasConfig = false;
                return;
            }

            var em = world.EntityManager;

            // local query; disposed immediately during valid world lifetime
            using var q = em.CreateEntityQuery(ComponentType.ReadOnly<GridConfig>());

            if (!q.TryGetSingleton(out GridConfig cfg))
            {
                HasConfig = false;
                return;
            }

            HasConfig = true;
            Config = cfg;

            if (cfg.Version == lastVersion)
                return;

            lastVersion = cfg.Version;

            CellSize = cfg.CellSize;
            OriginWorld = new Vector3(cfg.Origin.x, cfg.Origin.y, cfg.Origin.z);
            SizeInCells = new Vector2Int(cfg.Size.x, cfg.Size.y);

            Changed.OnNext(Unit.Default);
        }
    }
}