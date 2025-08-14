using System;
using GridFinder.Runtime.Grid;
using GridFinder.Runtime.Grid.Core;
using GridFinder.Runtime.Mono;
using R3;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.EventSystems;

namespace GridFinder.Samples
{
    [DisallowMultipleComponent]
    public class SampleGridController : MonoBehaviour
    {
        [Header("Grid")]
        [Min(2)] public int cols = 32;
        [Min(2)] public int rows = 32;
        public int chunkSize = 8;
        [Min(0.1f)] public float cellSize = 1f;
        public float zOffset = 0f; // Grid liegt in X/Y bei Z=zOffset
        public GridData Grid { get; private set; }

        [Header("Selection")]
        public Color startColor = new Color(0.2f, 0.9f, 0.2f, 1f);
        public Color goalColor  = new Color(0.9f, 0.2f, 0.2f, 1f);

        public int2 StartCell { get; private set; }
        public int2 GoalCell  { get; private set; }

        public event Action<int2,int2> OnStartGoalChanged;
        public GridData CurrentGrid { get; private set; }

        public ReactiveProperty<GridData> _gridCreated = new ReactiveProperty<GridData>();


        Camera _cam;
        Material _markerMat;

        void Awake()
        {
            _markerMat = new Material(Shader.Find("Sprites/Default"));
            StartCell = new int2(0, 0);
            GoalCell = new int2(cols - 1, rows - 1);
        }

        void Start()
        {
            CreateGrid(cols, rows);
        }
        

        public void CreateGrid(int width, int height)
        {
            CurrentGrid = GridFactory.CreateUniform(width, height, chunkSize, Cell.Default);
            _gridCreated.OnNext(CurrentGrid);
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
            DrawCellFillGL(StartCell, startColor, 1f);
            // Ziel
            DrawCellFillGL(GoalCell, goalColor, 1f);

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
