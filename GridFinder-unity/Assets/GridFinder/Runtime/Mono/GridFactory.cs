using System;
using System.Collections.Generic;
using GridFinder.Runtime.Grid;
using GridFinder.Runtime.Grid.Core;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Runtime.Mono
{
    public static class GridFactory
    {
        public static GridData CreateUniform(int width, int height, int chunkSize, in Cell defaultCell)
            => new GridData(width, height, chunkSize, defaultCell);
        
        public static GridData CreateFromGenerator(int width, int height, int chunkSize, Func<int,int,Cell> generator, in Cell defaultCell)
        {
            var grid = new GridData(width, height, chunkSize, defaultCell);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                grid.SetCell(x, y, generator(x, y));
            return grid;
        }
        
        public static GridData CreateNoiseCost(int width, int height, int chunkSize, float scale, ushort minCost, ushort maxCost, float walkableThreshold, Cell defaultCell)
            => CreateFromGenerator(width, height, chunkSize, (x,y) =>
            {
                float n = noise.snoise(new float2(x, y) / math.max(1e-3f, scale)); // [-1,1]
                float t = math.saturate((n * 0.5f + 0.5f));
                ushort cost = (ushort)math.round(math.lerp(minCost, maxCost, t));
                var c = defaultCell;
                c.Cost = cost;
                bool walkable = t >= walkableThreshold;
                c.Packed = Cell.SetWalkable(c.Packed, walkable);
                return c;
            }, defaultCell);
        
        public static GridData CreateFromTexture(Texture2D tex, int chunkSize, Dictionary<Color32,(ushort zone, byte colorIdx)> mapping, in Cell defaultCell)
        {
            var w = tex.width; var h = tex.height;
            var grid = new GridData(w, h, chunkSize, defaultCell);
            var pixels = tex.GetPixels32();
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var key = pixels[(h-1-y) * w + x];
                var c = defaultCell;
                if (mapping != null && mapping.TryGetValue(key, out var m))
                {
                    c.ZoneId = m.zone;
                    c.Packed = Cell.SetColorIndex(c.Packed, m.colorIdx);
                }
                grid.SetCell(x, y, c);
            }
            return grid;
        }
        
    }
}