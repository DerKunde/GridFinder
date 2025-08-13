using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;

namespace GridFinder.Samples
{
    public class SampleUIController : MonoBehaviour
    {
        [Header("Refs")]
        public SampleGridController grid;
        public GridGLRenderer gridRenderer;

        [Header("UI")]
        public Button runOnceButton;
        public Toggle autoToggle;
        public TMP_Text statsText;

        [Header("Blocks (optional)")]
        public float blockedRatio = 0f; // 0..0.4 z.B.

        System.Random _rand = new System.Random();
        bool[,] _blocked;
        
        public event System.Action<System.Collections.Generic.IReadOnlyList<int2>> PathComputed;
        public System.Collections.Generic.IReadOnlyList<int2> LastPath => _lastPath;
        System.Collections.Generic.List<int2> _lastPath = new System.Collections.Generic.List<int2>();

        void Start()
        {
            if (grid == null) grid = FindObjectOfType<SampleGridController>();
            if (runOnceButton != null) runOnceButton.onClick.AddListener(RunOnce);
            if (autoToggle != null) autoToggle.isOn = false;

            BuildBlocked();
            if (grid != null) grid.OnStartGoalChanged += (_, __) => UpdateStatsHeader();
            UpdateStatsHeader();
        }

        void Update()
        {
            if (autoToggle != null && autoToggle.isOn)
                RunOnce();
        }

        void BuildBlocked()
        {
            if (grid == null) return;
            _blocked = new bool[grid.cols, grid.rows];
            int total = Mathf.RoundToInt(blockedRatio * grid.cols * grid.rows);
            for (int i = 0; i < total; i++)
            {
                int x = _rand.Next(0, grid.cols);
                int y = _rand.Next(0, grid.rows);
                _blocked[x, y] = true;
            }
        }

        bool IsBlocked(int2 c)
        {
            if (_blocked == null) return false;
            if (c.x < 0 || c.y < 0 || c.x >= grid.cols || c.y >= grid.rows) return true;
            // stelle sicher dass Start/Ziel nie blockiert sind
            if (c.x == grid.StartCell.x && c.y == grid.StartCell.y) return false;
            if (c.x == grid.GoalCell.x  && c.y == grid.GoalCell.y ) return false;
            return _blocked[c.x, c.y];
        }

        void RunOnce()
        {
            if (grid == null) return;

            var res = PathfindingAStar.Find(grid.cols, grid.rows, grid.StartCell, grid.GoalCell, IsBlocked);

            // NEU: Pfad puffern + Event feuern
            _lastPath.Clear();
            if (res.path != null) _lastPath.AddRange(res.path);
            PathComputed?.Invoke(_lastPath);

            // Anzeige
            if (statsText != null)
            {
                float ms = res.elapsedMicroseconds / 1000f;
                var sb = new StringBuilder(128);
                sb.Append("A*: ").Append(res.success ? "OK" : "NO PATH").AppendLine();
                sb.Append("Time: ").Append(res.elapsedMicroseconds).Append(" µs  (")
                    .Append(ms.ToString("0.###", CultureInfo.InvariantCulture)).Append(" ms)").AppendLine();
                sb.Append("Visited: ").Append(res.visited).AppendLine();
                sb.Append("Path len: ").Append(res.path?.Count ?? 0).AppendLine();
                sb.Append("Start: ").Append(grid.StartCell.x).Append(',').Append(grid.StartCell.y)
                    .Append("  → Goal: ").Append(grid.GoalCell.x).Append(',').Append(grid.GoalCell.y);

                statsText.text = sb.ToString();
            }
        }

        void UpdateStatsHeader()
        {
            if (statsText != null)
            {
                statsText.text = $"Ready. Click LEFT for Start, RIGHT for Goal.\nGrid {grid.cols}x{grid.rows}, cell {grid.cellSize}";
            }
        }
    }
}
