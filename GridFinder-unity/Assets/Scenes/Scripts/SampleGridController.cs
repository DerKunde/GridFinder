using System;
using UnityEngine;
using Unity.Mathematics;

namespace GridFinder.Samples
{
    [DisallowMultipleComponent]
    public class SampleGridController : MonoBehaviour
    {
        [Header("Grid")]
        [Min(2)] public int cols = 32;
        [Min(2)] public int rows = 32;
        [Min(0.1f)] public float cellSize = 1f;
        public float zOffset = 0f; // Grid liegt in X/Y bei Z=zOffset

        [Header("Selection")]
        public Color startColor = new Color(0.2f, 0.9f, 0.2f, 1f);
        public Color goalColor  = new Color(0.9f, 0.2f, 0.2f, 1f);

        public int2 StartCell { get; private set; }
        public int2 GoalCell  { get; private set; }

        public event Action<int2,int2> OnStartGoalChanged;

        Camera _cam;
        Material _markerMat;

        void Awake()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                var camGO = new GameObject("Main Camera");
                _cam = camGO.AddComponent<Camera>();
                _cam.tag = "MainCamera";
            }

            // Top-Down Orthographic
            _cam.orthographic = true;
            _cam.transform.position = new Vector3(cols * cellSize * 0.5f, rows * cellSize * 0.5f, -10f + zOffset);
            _cam.transform.rotation = Quaternion.Euler(0, 0, 0); // Blick Richtung +Z? Wir schauen von -Z nach +Z
            _cam.orthographicSize = 0.55f * Mathf.Max(cols * cellSize, rows * cellSize);

            // Anfangswerte
            StartCell = new int2(1, 1);
            GoalCell  = new int2(cols - 2, rows - 2);

            // einfacher Marker-Mat (Sprites/Unlit/Color)
            _markerMat = new Material(Shader.Find("Sprites/Default"));
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0)) // LMB -> Start
            {
                if (TryPickCell(out var c))
                {
                    StartCell = c;
                    OnStartGoalChanged?.Invoke(StartCell, GoalCell);
                }
            }
            if (Input.GetMouseButtonDown(1)) // RMB -> Ziel
            {
                if (TryPickCell(out var c))
                {
                    GoalCell = c;
                    OnStartGoalChanged?.Invoke(StartCell, GoalCell);
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
            int x = Mathf.FloorToInt(hit.x / cellSize);
            int y = Mathf.FloorToInt(hit.y / cellSize);
            x = Mathf.Clamp(x, 0, cols - 1);
            y = Mathf.Clamp(y, 0, rows - 1);
            cell = new int2(x, y);
            return true;
        }

        void OnDrawGizmos()
        {
            // Start/Ziel Marker als halbtransparente Quads (Editor-Ansicht)
            if (!Application.isPlaying) return;
            DrawCellFill(StartCell, startColor * new Color(1,1,1,0.5f));
            DrawCellFill(GoalCell,  goalColor  * new Color(1,1,1,0.5f));
        }

        void OnRenderObject()
        {
            if (_markerMat == null) return;
            _markerMat.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            // Start
            DrawCellFillGL(StartCell, startColor, 0.6f);
            // Ziel
            DrawCellFillGL(GoalCell, goalColor, 0.6f);

            GL.PopMatrix();
        }

        void DrawCellFill(int2 c, Color col)
        {
            Gizmos.color = col;
            Vector3 p = new Vector3((c.x + 0.5f) * cellSize, (c.y + 0.5f) * cellSize, zOffset);
            Vector3 sz = new Vector3(0.9f * cellSize, 0.9f * cellSize, 0.0f);
            Gizmos.DrawCube(p, sz);
        }

        void DrawCellFillGL(int2 c, Color col, float scale)
        {
            float half = 0.5f * cellSize * scale;
            float cx = (c.x + 0.5f) * cellSize;
            float cy = (c.y + 0.5f) * cellSize;
            float z = zOffset;

            GL.Begin(GL.QUADS);
            GL.Color(col);
            GL.Vertex3(cx - half, cy - half, z);
            GL.Vertex3(cx + half, cy - half, z);
            GL.Vertex3(cx + half, cy + half, z);
            GL.Vertex3(cx - half, cy + half, z);
            GL.End();
        }
    }
}
