using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GridFinder.Samples
{

    public enum EditModes
    {
        setWalkable,
        unsetWalkable,
        setUnwalkable,
        unsetUnwalkable,
        setSpawnpoint,
        unsetSpawnpoint,
        setTarget
    }
    
    public class SampleUIController : MonoBehaviour
    {
        [Header("Refs")]
        public SampleGridController grid;
        public GridGLRenderer gridRenderer;
        public float zOffset = 0f; // Grid liegt in X/Y bei Z=zOffset

        private Camera _cam;

        [Header("UI")]
        [Header("Buttons")]
        public Button startSimulationButton;
        public Button pauseSimulationButton;
        public Button createNewGridButton;
        
        public Button editToWalkableButton;
        public Button editToUnwalkableButton;
        public Button setSpawnpointsButton;
        public Button setGoalButton;
        
        public TMP_Text statsText;

        [Header("Blocks (optional)")]
        public float blockedRatio = 0f; // 0..0.4 z.B.

        System.Random _rand = new System.Random();
        bool[,] _blocked;
        
        public event System.Action<System.Collections.Generic.IReadOnlyList<int2>> PathComputed;
        public System.Collections.Generic.IReadOnlyList<int2> LastPath => _lastPath;
        System.Collections.Generic.List<int2> _lastPath = new System.Collections.Generic.List<int2>();

        private EditModes currentEditMode = EditModes.setWalkable;

        void Start()
        {
            if (grid == null) grid = FindObjectsByType<SampleGridController>(FindObjectsSortMode.None)[0];
            
            if(editToWalkableButton != null) editToWalkableButton.onClick.AddListener(SwitchEditMode(EditModes.setWalkable));
            if(editToUnwalkableButton != null) editToUnwalkableButton.onClick.AddListener(SwitchEditMode(EditModes.setUnwalkable));
            if(setSpawnpointsButton != null) setSpawnpointsButton.onClick.AddListener(SwitchEditMode(EditModes.setSpawnpoint));
            if(setGoalButton != null) setGoalButton.onClick.AddListener(SwitchEditMode(EditModes.setTarget));
            if(startSimulationButton != null) startSimulationButton.onClick.AddListener(StartSimulation);
            if(pauseSimulationButton != null) pauseSimulationButton.onClick.AddListener(PauseSimulation);
            if(createNewGridButton != null) createNewGridButton.onClick.AddListener(CreateNewGrid);

            
            BuildBlocked();
            if (grid != null) grid.OnStartGoalChanged += (_, __) => UpdateStatsHeader();
            UpdateStatsHeader();
        }

        private void CreateNewGrid()
        {
            throw new System.NotImplementedException();
        }

        private void StartSimulation()
        {
            throw new System.NotImplementedException();
        }

        private void PauseSimulation()
        {
            throw new System.NotImplementedException();
        }

        private UnityAction SwitchEditMode(EditModes editMode)
        {
            Debug.Log("EditMode = " + currentEditMode.ToString());
            return () => currentEditMode = editMode;
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
        
        void Update()
        {
            // NEU: Maus über UI? Dann Grid-Auswahl blockieren
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;   
            }

            if (TryPickCell(out var hc))
            {
                //TODO: color hoverd cell
            }
            
            if (Input.GetMouseButtonUp(0)) // LMB -> Start
            {
                if (TryPickCell(out var c))
                {
                    //TODO: Click -> Set cell according to current editMode
                    //TODO: Click & Drag -> Mark area and set Cell according to current editMode
                }
            }
        }
        
        bool TryPickCell(out int2 cell)
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            // Ebene z = zOffset
            if (Mathf.Abs(ray.direction.z) < 1e-6f)
            {
                cell = default;
                return false;
            }
            float t = (zOffset - ray.origin.z) / ray.direction.z;
            if (t < 0)
            {
                cell = default;
                return false;
            }
            Vector3 hit = ray.origin + t * ray.direction;
            int x = Mathf.FloorToInt(hit.x / grid.cellSize);
            int y = Mathf.FloorToInt(hit.y / grid.cellSize);
            x = Mathf.Clamp(x, 0, grid.cols - 1);
            y = Mathf.Clamp(y, 0, grid.rows - 1);
            cell = new int2(x, y);
            return true;
        }
        
        void RunOnce()
        {
            if (grid == null)
            {
                grid.CreateGrid(grid.cols,grid.rows);
            }

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
