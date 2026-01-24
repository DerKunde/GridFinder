using R3;
using Unity.Entities;
using Unity.Mathematics;
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

        private EntityQuery gridConfigQuery;
        private bool queryCreated;

        private uint lastVersion = uint.MaxValue; // force first update

        private void Awake()
        {
            TryCreateQuery();
        }

        private void Update()
        {
            TryCreateQuery();
            if (!queryCreated)
                return;

            if (!gridConfigQuery.TryGetSingleton(out GridConfig cfg))
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

        private void TryCreateQuery()
        {
            if (queryCreated)
                return;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;

            gridConfigQuery = world.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<GridConfig>());
            queryCreated = true;
        }

        private void OnDestroy()
        {
            // Only dispose if we actually created it.
            if (queryCreated)
                gridConfigQuery.Dispose();
        }
    }
}