using System;
using System.Collections.Generic;

namespace GridFinder.Runtime.Grid
{
    public sealed class Chunk
    {
        public const int Size = 64; // 64x64
        private CellData[] _cells; // nur angelegt, wenn gebraucht

        public bool IsAllocated => _cells != null;

        public ref CellData GetOrCreate(int localX, int localY)
        {
            _cells ??= new CellData[Size * Size];
            return ref _cells[localY * Size + localX];
        }

        public bool TryGet(int lx, int ly, out CellData cell)
        {
            if (_cells == null)
            {
                cell = default;
                return false;
            }

            cell = _cells[ly * Size + lx];
            return true;
        }
    }

    public sealed class LevelGrid
    {
        private readonly Dictionary<long, Chunk> _chunks = new();

        public int LevelIndex { get; }

        public LevelGrid(int levelIndex) => LevelIndex = levelIndex;

        private static long Key(int cx, int cy) => ((long)cx << 32) | (uint)cy;

        public Chunk GetOrCreateChunk(int cx, int cy)
        {
            var key = Key(cx, cy);
            if (!_chunks.TryGetValue(key, out var chunk))
                _chunks[key] = chunk = new Chunk();
            return chunk;
        }

        public bool TryGetChunk(int cx, int cy, out Chunk chunk)
            => _chunks.TryGetValue(Key(cx, cy), out chunk);

        public static void ToChunkSpace(int x, int y, out int cx, out int cy, out int lx, out int ly)
        {
            cx = Math.DivRem(x, Chunk.Size, out lx);
            cy = Math.DivRem(y, Chunk.Size, out ly);
            if (x < 0 && lx != 0)
            {
                cx--;
                lx += Chunk.Size;
            }

            if (y < 0 && ly != 0)
            {
                cy--;
                ly += Chunk.Size;
            }
        }

        public ref CellData GetOrCreateCell(int x, int y)
        {
            ToChunkSpace(x, y, out var cx, out var cy, out var lx, out var ly);
            var chunk = GetOrCreateChunk(cx, cy);
            return ref chunk.GetOrCreate(lx, ly);
        }

        public bool TryGetCell(int x, int y, out CellData cell)
        {
            ToChunkSpace(x, y, out var cx, out var cy, out var lx, out var ly);
            if (TryGetChunk(cx, cy, out var chunk))
                return chunk.TryGet(lx, ly, out cell);
            cell = default;
            return false;
        }
    }

    public sealed class MultiLevelGrid
    {
        private readonly Dictionary<int, LevelGrid> _levels = new();

        public LevelGrid GetOrCreateLevel(int levelIndex)
        {
            if (!_levels.TryGetValue(levelIndex, out var level))
                _levels[levelIndex] = level = new LevelGrid(levelIndex);
            return level;
        }

        public bool TryGetLevel(int levelIndex, out LevelGrid level)
            => _levels.TryGetValue(levelIndex, out level);
    }
}