using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.Mathematics;

namespace GridFinder.Samples
{
    public static class PathfindingAStar
    {
        public struct Result
        {
            public List<int2> path;
            public long elapsedMicroseconds;
            public bool success;
            public int visited;
        }

        static readonly int2[] Neigh = new int2[]
        {
            new int2( 1, 0), new int2(-1, 0),
            new int2( 0, 1), new int2( 0,-1),
        };

        public static Result Find(int cols, int rows, int2 start, int2 goal, System.Func<int2,bool> isBlocked = null)
        {
            var sw = Stopwatch.StartNew();

            int Idx(int2 c) => c.y * cols + c.x;
            bool InBounds(int2 c) => c.x >= 0 && c.x < cols && c.y >= 0 && c.y < rows;
            int Heuristic(int2 a, int2 b) => math.abs(a.x - b.x) + math.abs(a.y - b.y);

            var open = new SimpleMinHeap(); // sehr simpler Heap
            var came = new int[cols * rows];
            var g    = new int[cols * rows];
            var f    = new int[cols * rows];
            var inOpen = new bool[cols * rows];
            var closed = new bool[cols * rows];

            for (int i=0;i<g.Length;i++){ g[i]=int.MaxValue; f[i]=int.MaxValue; came[i]=-1; }

            int sIdx = Idx(start), gIdx = Idx(goal);
            g[sIdx] = 0; f[sIdx] = Heuristic(start, goal);
            open.Push(sIdx, f[sIdx]); inOpen[sIdx]=true;

            int visited = 0;
            while (open.Count > 0)
            {
                int current = open.PopMin();
                inOpen[current]=false;
                if (current == gIdx) break;
                closed[current]=true; visited++;

                int2 cur = new int2(current % cols, current / cols);
                foreach (var d in Neigh)
                {
                    int2 n = cur + d;
                    if (!InBounds(n)) continue;
                    if (isBlocked != null && isBlocked(n)) continue;

                    int ni = Idx(n);
                    if (closed[ni]) continue;

                    int tentative = g[current] + 1;
                    if (tentative < g[ni])
                    {
                        came[ni] = current;
                        g[ni] = tentative;
                        f[ni] = tentative + Heuristic(n, goal);
                        if (!inOpen[ni])
                        {
                            open.Push(ni, f[ni]);
                            inOpen[ni]=true;
                        }
                        else open.DecreaseKey(ni, f[ni]);
                    }
                }
            }

            sw.Stop();
            var res = new Result { elapsedMicroseconds = sw.ElapsedTicks * 1000000L / System.TimeSpan.TicksPerSecond, visited = visited };

            // Pfad rekonstruieren
            if (came[gIdx] != -1 || (start.x==goal.x && start.y==goal.y))
            {
                res.success = true;
                res.path = new List<int2>();
                int cur = gIdx;
                if (!(start.x==goal.x && start.y==goal.y))
                {
                    while (cur != -1)
                    {
                        res.path.Add(new int2(cur % cols, cur / cols));
                        cur = came[cur];
                    }
                    res.path.Reverse();
                }
                else
                {
                    res.path.Add(start);
                }
            }
            else
            {
                res.success = false;
                res.path = new List<int2>();
            }

            return res;
        }

        // Minimaler Binär-Heap für (node, priority)
        class SimpleMinHeap
        {
            List<int> nodes = new List<int>();
            List<int> prio  = new List<int>();
            Dictionary<int,int> pos = new Dictionary<int,int>(); // node->index

            public int Count => nodes.Count;

            public void Push(int node, int priority)
            {
                nodes.Add(node); prio.Add(priority); pos[node]=nodes.Count-1; Up(nodes.Count-1);
            }
            public int PopMin()
            {
                int root = nodes[0];
                Swap(0, nodes.Count-1);
                nodes.RemoveAt(nodes.Count-1); prio.RemoveAt(prio.Count-1); pos.Remove(root);
                if (nodes.Count>0) Down(0);
                return root;
            }
            public void DecreaseKey(int node, int newPriority)
            {
                if (!pos.TryGetValue(node, out int i)) return;
                if (newPriority >= prio[i]) return;
                prio[i] = newPriority;
                Up(i);
            }
            void Up(int i)
            {
                while (i>0)
                {
                    int p=(i-1)/2;
                    if (prio[p] <= prio[i]) break;
                    Swap(p,i); i=p;
                }
            }
            void Down(int i)
            {
                while (true)
                {
                    int l=2*i+1, r=l+1, s=i;
                    if (l<nodes.Count && prio[l]<prio[s]) s=l;
                    if (r<nodes.Count && prio[r]<prio[s]) s=r;
                    if (s==i) break;
                    Swap(i,s); i=s;
                }
            }
            void Swap(int a,int b)
            {
                (nodes[a], nodes[b]) = (nodes[b], nodes[a]);
                (prio[a],  prio[b])  = (prio[b],  prio[a]);
                pos[nodes[a]]=a; pos[nodes[b]]=b;
            }
        }
    }
}
