using System;
using System.Collections.Generic;
using Unity.Collections;

namespace GridFinder.Runtime.Grid.Core
{
    public class GridData : IDisposable
    {
        public readonly int Width, Height, ChunkSize;
        public readonly Cell DefaultCell;

        struct Chunk
        {
            public bool IsUniform;
            public Cell UniformValue;
            public NativeArray<Cell> Cells;
            public bool Dirty;
        }

        private readonly Dictionary<long, Chunk> _chunks = new();
        private long Key(int cx, int cy) => ((long)cx << 32) | (uint)cy;

        public GridData(int width, int height, int chunkSize, Cell defaultCell)
        {
            Width = width; Height = height; ChunkSize = chunkSize; DefaultCell = defaultCell;  
        }

        public Cell GetCell(int x, int y)
        {
            Split(x, y, out int cx, out int cy, out int lx, out int ly);
            if (!_chunks.TryGetValue(Key(cx, cy), out var ch)) return DefaultCell;
            return ch.IsUniform ? ch.UniformValue : ch.Cells[ly * ChunkSize + lx];
        }

        public void SetCell(int x, int y, in Cell c)
        {
            Split(x, y, out int cx, out int cy, out int lx, out int ly);
            EnsureChunk(cx, cy, out var ch);
            if (ch.IsUniform)
            {
                if (!c.Equals(ch.UniformValue))
                {
                    Promote(ref ch);
                    ch.Cells[ly * ChunkSize + lx] = c;
                    ch.Dirty = true;
                    _chunks[Key(cx,cy)] = ch;
                }
            }
            else
            {
                int idx = ly * ChunkSize + lx;
                if (!c.Equals(ch.Cells[idx]))
                {
                    ch.Cells[idx] = c;
                    ch.Dirty = true;
                    _chunks[Key(cx,cy)] = ch;
                }
            }
        }
        
        public void Dispose()
        {
            foreach (var kv in _chunks)
            {
                if (!kv.Value.IsUniform && kv.Value.Cells.IsCreated)
                    kv.Value.Cells.Dispose();
                _chunks.Clear();
            }
        }
        
        
        public void PaintRect(int x0, int y0, int x1, int y1, Func<Cell, Cell> painter)
        {
            if (x0 > x1) (x0, x1) = (x1, x0);
            if (y0 > y1) (y0, y1) = (y1, y0);
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                SetCell(x, y, painter(GetCell(x, y)));
        }
        
        public IEnumerable<(int cx, int cy, bool isUniform, Cell uniform, NativeArray<Cell> cells)> GetDirtyChunks(bool clearDirty = true)
        {
            foreach (var (k,ch) in _chunks)
            {
                if (!ch.Dirty) continue;
                if (clearDirty) { var tmp = ch; tmp.Dirty = false; _chunks[k] = tmp; }
                int cx = (int)(k >> 32);
                int cy = (int) k;
                yield return (cx, cy, ch.IsUniform, ch.UniformValue, ch.Cells);
            }
        }

        
        private void Promote(ref Chunk ch)
        {
            if (!ch.IsUniform) return;
            ch.Cells = new NativeArray<Cell>(ChunkSize * ChunkSize, Allocator.Persistent);
            for (int i = 0; i < ch.Cells.Length; i++) ch.Cells[i] = ch.UniformValue;
            ch.IsUniform = false;
        }
        
        private void EnsureChunk(int cx, int cy, out Chunk ch)
        {
            var key = Key(cx, cy);
            if (!_chunks.TryGetValue(key, out ch))
            {
                ch = new Chunk { IsUniform = true, UniformValue = DefaultCell, Dirty = true };
                _chunks[key] = ch;
            }
        }
        
        private void Split(int x, int y, out int cx, out int cy, out int lx, out int ly)
        {
            cx = Math.DivRem(x, ChunkSize, out lx);
            cy = Math.DivRem(y, ChunkSize, out ly);
        }
    }
}