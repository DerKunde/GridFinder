using GridFinder.Runtime.Grid;
using GridFinder.Structs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Runtime.Mono
{
    public static class GridFactoryEntities
    {
        // Erzeugt NUR uniform-Chunks (kein Speicher für Zellen belegt)
        public static Entity CreateGrid(World world, int width, int height, int chunkSize, float cellSize,
            in Cell defaultCell)
        {
            var em = world.EntityManager;
            var root = em.CreateEntity(typeof(GridMeta));
            em.SetComponentData(root, new GridMeta
            {
                Size = new int2(width, height),
                ChunkSize = chunkSize,
                CellSize = cellSize,
                DefaultCell = defaultCell
            });

            int cxCount = (width + chunkSize - 1) / chunkSize;
            int cyCount = (height + chunkSize - 1) / chunkSize;

            for (int cy = 0; cy < cyCount; cy++)
            for (int cx = 0; cx < cxCount; cx++)
            {
                var e = em.CreateEntity();
                em.AddComponentData(e, new ChunkCoord { Coord = new int2(cx, cy) });
                em.AddComponentData(e, new ChunkState
                {
                    IsUniform = 1,
                    UniformValue = defaultCell,
                    Dirty = 1
                });
                em.AddBuffer<ChunkCells>(e); // leer lassen bis Materialisierung
            }

            return root;
        }

        // Materialisiert ALLE Chunks mit einem Generator (vorsicht bei riesigen Grids)
        // generator(lx,ly, worldX, worldY) -> Cell
        public static void MaterializeAll(World world, System.Func<int, int, int, int, Cell> generator)
        {
            var em = world.EntityManager;
            var meta = em.CreateEntityQuery(typeof(GridMeta)).GetSingleton<GridMeta>();
            var chunkQuery = em.CreateEntityQuery(typeof(ChunkCoord), typeof(ChunkState), typeof(ChunkCells));
            using var entities = chunkQuery.ToEntityArray(Allocator.Temp);
            foreach (var e in entities)
            {
                var coord = em.GetComponentData<ChunkCoord>(e).Coord;
                var state = em.GetComponentData<ChunkState>(e);
                var buffer = em.GetBuffer<ChunkCells>(e);

                // Promote (nur wenn nötig)
                if (state.IsUniform == 1)
                {
                    state.IsUniform = 0;
                    em.SetComponentData(e, state);
                    buffer.ResizeUninitialized(meta.ChunkSize * meta.ChunkSize);
                    // init mit default:
                    for (int i = 0; i < buffer.Length; i++) buffer[i] = new ChunkCells { Value = meta.DefaultCell };
                }

                // Füllen
                for (int ly = 0; ly < meta.ChunkSize; ly++)
                for (int lx = 0; lx < meta.ChunkSize; lx++)
                {
                    int worldX = coord.x * meta.ChunkSize + lx;
                    int worldY = coord.y * meta.ChunkSize + ly;
                    if (worldX >= meta.Size.x || worldY >= meta.Size.y) continue;

                    int idx = ly * meta.ChunkSize + lx;
                    var cell = generator(lx, ly, worldX, worldY);
                    buffer[idx] = new ChunkCells { Value = cell };
                }

                state.Dirty = 1; // Renderer darf uploaden
                em.SetComponentData(e, state);
            }
        }
    }
}