using System;
using System.Collections.Generic;
using GridFinder.Runtime.Grid.Core;

namespace GridFinder.Runtime.Grid.Planners
{
    public class AStarPlanner : IPathPlanner
    {
        private readonly struct Node : IComparable<Node>
        {
            public readonly int x, y;
            public readonly int g; // cost so far (int, um Akkumulation zu vermeiden)
            public readonly int f; // g + h
            public readonly int parentIdx; // Index in cameFrom

            public Node(int x, int y, int g, int f, int parentIdx)
            {
                this.x = x;
                this.y = y;
                this.g = g;
                this.f = f;
                this.parentIdx = parentIdx;
            }

            public int CompareTo(Node other) => f.CompareTo(other.f);
        }

        // Reusable buffers (verringert GC)
        private Node[] openHeap = new Node[256];
        private int openCount = 0;

        private void HeapClear() => openCount = 0;

        private void HeapPush(Node n)
        {
            if (openCount == openHeap.Length) Array.Resize(ref openHeap, openHeap.Length * 2);
            openHeap[openCount] = n;
            SiftUp(openCount++);
        }

        private Node HeapPop()
        {
            var root = openHeap[0];
            openHeap[0] = openHeap[--openCount];
            SiftDown(0);
            return root;
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (openHeap[i].f >= openHeap[p].f) break;
                (openHeap[i], openHeap[p]) = (openHeap[p], openHeap[i]);
                i = p;
            }
        }

        private void SiftDown(int i)
        {
            while (true)
            {
                int l = (i << 1) + 1;
                if (l >= openCount) break;
                int r = l + 1;
                int s = (r < openCount && openHeap[r].f < openHeap[l].f) ? r : l;
                if (openHeap[i].f <= openHeap[s].f) break;
                (openHeap[i], openHeap[s]) = (openHeap[s], openHeap[i]);
                i = s;
            }
        }

        private static int Heuristic(int dx, int dy, HeuristicType h)
        {
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);
            return h switch
            {
                HeuristicType.Manhattan => 10 * (dx + dy),
                HeuristicType.Euclidean => (int)(10 * Math.Sqrt(dx * dx + dy * dy)),
                _ /* Octile */ => 10 * (Math.Max(dx, dy)) + 4 * (Math.Min(dx, dy)),
            };
        }

        public PathResult FindPath(Grid2D grid, PathRequest req, HeuristicType h = HeuristicType.Octile,
            bool allowDiagonal = true)
        {
            // Visited/Closed & CameFrom
            int w = grid.Width, hgt = grid.Height;
            int size = w * hgt;

            Span<int> gScore =
                size <= 1024 * 1024 ? stackalloc int[size] : new int[size]; // bis 1M Zellen stack-freundlich
            Span<int> parent = size <= 1024 * 1024 ? stackalloc int[size] : new int[size];
            Span<byte> closed = size <= 1024 * 1024 ? stackalloc byte[size] : new byte[size];

            for (int i = 0; i < size; i++)
            {
                gScore[i] = int.MaxValue;
                parent[i] = -1;
                closed[i] = 0;
            }

            int StartIdx(int x, int y) => y * w + x;

            if (!grid.InBounds(req.start.x, req.start.y) || !grid.InBounds(req.goal.x, req.goal.y))
                return new PathResult(req.id, null);
            if (grid.GetCost(req.start.x, req.start.y) == byte.MaxValue ||
                grid.GetCost(req.goal.x, req.goal.y) == byte.MaxValue)
                return new PathResult(req.id, null);

            HeapClear();

            int sIdx = StartIdx(req.start.x, req.start.y);
            gScore[sIdx] = 0;
            int h0 = Heuristic(req.goal.x - req.start.x, req.goal.y - req.start.y, h);
            HeapPush(new Node(req.start.x, req.start.y, 0, h0, -1));

            // Nachbarn (4/8)
            ReadOnlySpan<(int dx, int dy, int cost)> dirs = allowDiagonal
                ? new (int, int, int)[]
                {
                    (1, 0, 10), (-1, 0, 10), (0, 1, 10), (0, -1, 10),
                    (1, 1, 14), (-1, 1, 14), (1, -1, 14), (-1, -1, 14)
                }
                : new (int, int, int)[] { (1, 0, 10), (-1, 0, 10), (0, 1, 10), (0, -1, 10) };

            while (openCount > 0)
            {
                var cur = HeapPop();
                int curIdx = StartIdx(cur.x, cur.y);

                if (closed[curIdx] != 0) continue;
                closed[curIdx] = 1;
                parent[curIdx] = cur.parentIdx;

                if (cur.x == req.goal.x && cur.y == req.goal.y)
                {
                    // Pfad rekonstruieren
                    var list = new List<GridPos>(32);
                    int idx = curIdx;
                    while (idx != -1)
                    {
                        int x = idx % w;
                        int y = idx / w;
                        list.Add(new GridPos(x, y));
                        idx = parent[idx];
                    }

                    list.Reverse();
                    return new PathResult(req.id, list.ToArray());
                }

                for (int k = 0; k < dirs.Length; k++)
                {
                    int nx = cur.x + dirs[k].dx;
                    int ny = cur.y + dirs[k].dy;
                    if (!grid.InBounds(nx, ny)) continue;

                    byte tileCost = grid.GetCost(nx, ny);
                    if (tileCost == byte.MaxValue) continue;

                    int nIdx = StartIdx(nx, ny);
                    if (closed[nIdx] != 0) continue;

                    // Bewegungskosten (Basis + Zellkosten)
                    int tentativeG = cur.g + dirs[k].cost + tileCost - 10; // Basis 10 → tileCost 10 = neutral
                    if (tentativeG >= gScore[nIdx]) continue;

                    gScore[nIdx] = tentativeG;
                    int hCost = Heuristic(req.goal.x - nx, req.goal.y - ny, h);
                    HeapPush(new Node(nx, ny, tentativeG, tentativeG + hCost, curIdx));
                }
            }

            return new PathResult(req.id, null);
        }
    }
}