using System;
using System.Collections.Generic;
using Runtime.Core;
using Runtime.Grid;
using Runtime.Planners;
using UnityEngine;

namespace Runtime.RuntimeController
{
    public class PathfindingManager : MonoBehaviour
    {
        [SerializeField] private int maxPerFrame = 64;
        [SerializeField] private bool allowDiagonal = true;
        [SerializeField] private HeuristicType heuristic = HeuristicType.Octile;

        private readonly Queue<PathRequest> queue = new();
        private IPathPlanner planner;
        private Grid2D grid;

        public event Action<PathResult> OnPathReady;

        public void Init(Grid2D grid, IPathPlanner planner = null)
        {
            this.grid = grid;
            this.planner = planner ?? new AStarPlanner();
        }

        public void Enqueue(PathRequest req) => queue.Enqueue(req);

        private void Update()
        {
            if (grid == null || planner == null) return;

            int budget = maxPerFrame;
            while (budget-- > 0 && queue.Count > 0)
            {
                var req = queue.Dequeue();
                var res = planner.FindPath(grid, req, heuristic, allowDiagonal);
                OnPathReady?.Invoke(res);
            }
        }
    }
}