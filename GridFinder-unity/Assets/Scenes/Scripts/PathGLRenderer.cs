using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace GridFinder.Samples
{
    /// <summary>
    /// Zeichnet den letzten berechneten Pfad als Linie (Zentren der Zellen).
    /// Benötigt Referenz auf Grid + UIController (liefert Pfad).
    /// </summary>
    [DisallowMultipleComponent]
    public class PathGLRenderer : MonoBehaviour
    {
        [Header("Refs")]
        public SampleGridController grid;
        public SampleUIController ui;

        [Header("Look")]
        public Color lineColor = new Color(0.3f, 0.9f, 1f, 1f);
        [Tooltip("Kleine Marker pro Zelle (gefüllte Kästchen).")]
        public bool drawNodeMarkers = true;
        [Range(0.1f, 1.0f)]
        public float markerScale = 0.35f;
        [Tooltip("Z-Versatz, um Z-Fighting mit Grid zu vermeiden.")]
        public float zBias = 0.001f;

        Material _mat;
        List<int2> _pathCache = new List<int2>();

        void Awake()
        {
            if (grid == null) grid = FindObjectOfType<SampleGridController>();
            if (ui == null) ui = FindObjectOfType<SampleUIController>();

            // Unlit Farbe für GL
            Shader sh = Shader.Find("Hidden/Internal-Colored");
            _mat = new Material(sh){ hideFlags = HideFlags.HideAndDontSave };
            _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _mat.SetInt("_ZWrite", 0);
        }

        void OnEnable()
        {
            if (ui != null) ui.PathComputed += OnPathComputed;
        }

        void OnDisable()
        {
            if (ui != null) ui.PathComputed -= OnPathComputed;
        }

        void OnPathComputed(IReadOnlyList<int2> path)
        {
            _pathCache.Clear();
            if (path != null) _pathCache.AddRange(path);
        }

        void OnRenderObject()
        {
            if (_mat == null || grid == null || _pathCache == null || _pathCache.Count < 2)
                return;

            _mat.SetPass(0);

            // Linie
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);
            GL.Color(lineColor);

            float z = grid.zOffset + zBias;

            for (int i = 0; i < _pathCache.Count - 1; i++)
            {
                var a = _pathCache[i];
                var b = _pathCache[i + 1];
                Vector3 pa = new Vector3((a.x + 0.5f) * grid.cellSize, (a.y + 0.5f) * grid.cellSize, z);
                Vector3 pb = new Vector3((b.x + 0.5f) * grid.cellSize, (b.y + 0.5f) * grid.cellSize, z);
                GL.Vertex(pa);
                GL.Vertex(pb);
            }

            GL.End();
            GL.PopMatrix();

            if (!drawNodeMarkers) return;

            // kleine Marker pro Zelle (Quads)
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.QUADS);
            GL.Color(new Color(lineColor.r, lineColor.g, lineColor.b, 0.65f));

            float half = 0.5f * grid.cellSize * markerScale;
            foreach (var c in _pathCache)
            {
                float cx = (c.x + 0.5f) * grid.cellSize;
                float cy = (c.y + 0.5f) * grid.cellSize;

                GL.Vertex3(cx - half, cy - half, z);
                GL.Vertex3(cx + half, cy - half, z);
                GL.Vertex3(cx + half, cy + half, z);
                GL.Vertex3(cx - half, cy + half, z);
            }

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Falls du ohne Event arbeiten willst, kannst du den Pfad direkt setzen.
        /// </summary>
        public void SetPathDirect(IReadOnlyList<int2> path)
        {
            OnPathComputed(path);
        }
    }
}
