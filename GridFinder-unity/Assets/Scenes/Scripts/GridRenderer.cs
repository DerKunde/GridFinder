using System;
using System.Collections.Generic;
using GridFinder.Runtime.Grid;
using GridFinder.Runtime.Grid.Core;
using R3;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace GridFinder.Samples
{
    [RequireComponent(typeof(SampleGridController))]
    [DisallowMultipleComponent]
    public class GridGLRenderer : MonoBehaviour, IDisposable
    {
        [Header("Colors")]
        public Color borderColor    = new(1f, 1f, 1f, 0.20f);
        public Color hoverFillColor = new(1f, 0.9f, 0.2f, 0.45f);
        public Color stateFillColor = new(0.2f, 0.7f, 1f, 0.55f);
        public Color spawnerFillColor = new(0.5f, 0.0f, 1f, 0.55f);

        [Header("Z-Bias")]
        [Tooltip("Linien leicht vor die Grid-Ebene schieben, um Z-Fighting zu vermeiden.")]
        public float borderZBias = 0.0005f;
        [Tooltip("Füllflächen noch etwas davor.")]
        public float fillZBias   = 0.0010f;

        [Header("Optional: Fill-Quad Innenabstand (0..1)")]
        [Range(0.0f, 1.0f)] public float fillInset = 0.1f; // 0.1 = 90% der Kachel

        private Material _mat;
        public SampleGridController gridController;

        private readonly CompositeDisposable _disposables = new();

        private GridData _grid;

        // Overlay-Status
        private int2? _hoverCell;
        private readonly List<(int2 cell, Color color)> _stateCells = new();

        void Awake()
        {
            gridController = GetComponent<SampleGridController>();

            gridController.OnStartGoalChanged += (start, goal) =>
            {
                RenderCellState(start, gridController.startColor);
                RenderCellState(goal, gridController.goalColor);
            };

            gridController._gridCreated
                .Subscribe(g =>
                {
                    _grid = g;
                    ForceDraw();
                })
                .AddTo(_disposables);

            gridController._cellChanged().Subscribe(data =>
            {
                RenderCellState(data.cords, data.cell);
            });

            EnsureMaterial();
        }

        void OnDisable() => _disposables.Clear();
        void OnDestroy()
        {
            _disposables.Dispose();
            if (Application.isPlaying)
            {
                if (_mat != null) Destroy(_mat);
            }
            else
            {
                if (_mat != null) DestroyImmediate(_mat);
            }
        }

        private void EnsureMaterial()
        {
            if (_mat != null) return;
            var sh = Shader.Find("Hidden/Internal-Colored");
            _mat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _mat.SetInt("_ZWrite", 0);
        }

        // ---- Public API ------------------------------------------------------

        public void RenderHoverCell(int2 cell)
        {
            _hoverCell = cell;
            ForceDraw();
        }

        public void ClearHoverCell()
        {
            _hoverCell = null;
            ForceDraw();
        }

        public void RenderCellState(int2 cords, Cell cell)
        {
            // Zustand aus den Bits lesen
            bool isWalkable = Cell.GetWalkable(cell.Packed);
            // Optional: Farbindex, falls du den für Zonen/Typen verwenden willst
            byte colorIdx   = Cell.GetColorIndex(cell.Packed);

            // Farbe bestimmen:
            // - Unwalkable/blocked → rot-transparent
            // - Walkable → Standard-State-Farbe (oder per colorIdx spezifizieren)
            Color col = isWalkable
                ? stateFillColor
                : new Color(0.9f, 0.2f, 0.2f, 0.55f);

            // Falls du den ColorIndex nutzen willst, kannst du hier (nur wenn walkable)
            // eine einfache Palette berücksichtigen (Beispiel):
            // if (isWalkable)
            // {
            //     switch (colorIdx)
            //     {
            //         case 1: col = new Color(0.2f, 0.7f, 1f, 0.55f); break; // z.B. "Wasser"
            //         case 2: col = new Color(0.2f, 1f, 0.4f, 0.55f); break; // z.B. "Wiese"
            //         // ...
            //     }
            // }

            RenderCellState(cords, col);
        }


        private void RenderCellState(int2 cords, Color color)
        {
            // vorhandenen Eintrag ersetzen, sonst hinzufügen
            for (int i = 0; i < _stateCells.Count; i++)
            {
                if (_stateCells[i].cell.Equals(cords))
                {
                    _stateCells[i] = (cords, color);
                    ForceDraw();
                    return;
                }
            }
            _stateCells.Add((cords, color));
            ForceDraw();
        }

        public void ClearCellState(int2 cell)
        {
            _stateCells.RemoveAll(t => t.cell.Equals(cell));
            ForceDraw();
        }

        public void ClearAllCellStates()
        {
            _stateCells.Clear();
            ForceDraw();
        }

        private void ForceDraw()
        {
            if (_grid == null) return;
            EnsureMaterial();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.SceneView.RepaintAll();
#endif
        }

        // ---- Rendering -------------------------------------------------------

        void OnRenderObject()
        {
            if (_mat == null || _grid == null || gridController == null) return;

            _mat.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            float cs = gridController.cellSize;
            float w  = _grid.Width  * cs;
            float h  = _grid.Height * cs;

            // 1) Borders (geteilt pro Kachel via vertikale/horizontale Linien)
            GL.Begin(GL.LINES);
            GL.Color(borderColor);

            float zLine = gridController.zOffset + borderZBias;

            // Vertikale Linien (Kachel-Grenzen)
            for (int x = 0; x <= _grid.Width; x++)
            {
                float vx = x * cs;
                GL.Vertex3(vx, 0f, zLine);
                GL.Vertex3(vx, h,  zLine);
            }
            // Horizontale Linien
            for (int y = 0; y <= _grid.Height; y++)
            {
                float vy = y * cs;
                GL.Vertex3(0f, vy, zLine);
                GL.Vertex3(w,  vy, zLine);
            }
            GL.End();

            // 2) Füllungen (Hover + States)
            float zFill = gridController.zOffset + fillZBias;
            float half  = 0.5f * cs * (1f - fillInset);

            GL.Begin(GL.QUADS);

            // Hover
            if (_hoverCell.HasValue)
            {
                EmitCellQuad(_hoverCell.Value, hoverFillColor, zFill, half, cs);
            }

            // States
            for (int i = 0; i < _stateCells.Count; i++)
            {
                var (cell, color) = _stateCells[i];
                EmitCellQuad(cell, color, zFill, half, cs);
            }

            GL.End();

            GL.PopMatrix();
        }

        // Schreibt die 4 Eckpunkte einer Kachel (mit Farbe) in den aktuellen GL.QUADS-Block
        private void EmitCellQuad(in int2 c, in Color color, float z, float half, float cellSize)
        {
            float cx = (c.x + 0.5f) * cellSize;
            float cy = (c.y + 0.5f) * cellSize;

            GL.Color(color);
            GL.Vertex3(cx - half, cy - half, z);
            GL.Vertex3(cx + half, cy - half, z);
            GL.Vertex3(cx + half, cy + half, z);
            GL.Vertex3(cx - half, cy + half, z);
        }

        public void Dispose() { /* nothing extra */ }
    }
}
