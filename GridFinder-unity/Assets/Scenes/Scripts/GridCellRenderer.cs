using System.Collections.Generic;
using GridFinder.Runtime.Grid;
using Unity.Collections;
using UnityEngine;

namespace GridFinder.Samples
{
    [RequireComponent(typeof(SampleGridController))]
    public class GridCellsInstancedRenderer : MonoBehaviour
    {
        public Material material;      // Shader: "Grid/InstancedUnlitPalette" (s. vorherige Antwort)
        public Texture2D paletteTex;   // 256x1 Farbpalette
        public bool tintUnwalkable = true;
        public float cellZ = 0f;

        Mesh _quad;
        Camera _cam;
        SampleGridController _ctrl;

        class BatchData {
            public Bounds bounds;
            public readonly List<Matrix4x4[]> matrices = new();
            public readonly List<MaterialPropertyBlock> mpbs = new();
        }
        readonly Dictionary<(int cx,int cy), BatchData> _batches = new();

        void Awake()
        {
            _ctrl = GetComponent<SampleGridController>();
            _cam = Camera.main;
            _quad = BuildQuad();
            if (material && paletteTex) material.SetTexture("_PaletteTex", paletteTex);
        }

        void Update()
        {
            if (_ctrl?.Grid == null || material == null || _quad == null) return;

            // nur geänderte Chunks neu packen
            foreach (var (cx, cy, isUniform, uniform, cells) in _ctrl.Grid.GetDirtyChunks(true))
                RebuildChunk(cx, cy, isUniform, uniform, cells, _ctrl.Grid.ChunkSize, _ctrl.cellSize);

            // zeichnen (sichtbare Chunks)
            foreach (var kv in _batches)
            {
                var bd = kv.Value;
                if (!IsVisible(bd.bounds)) continue;

                for (int i = 0; i < bd.matrices.Count; i++)
                {
                    Graphics.DrawMeshInstanced(_quad, 0, material, bd.matrices[i], bd.matrices[i].Length,
                        bd.mpbs[i], UnityEngine.Rendering.ShadowCastingMode.Off, false, gameObject.layer, _cam);
                }
            }
        }

        void RebuildChunk(int cx, int cy, bool isUniform, Cell uniform, NativeArray<Cell> cells, int chunkSize, float cellSize)
        {
            var key = (cx, cy);
            if (!_batches.TryGetValue(key, out var bd))
            {
                bd = new BatchData();
                _batches[key] = bd;
            }
            bd.matrices.Clear();
            bd.mpbs.Clear();

            // Bounds
            float sx = chunkSize * cellSize;
            float sy = chunkSize * cellSize;
            bd.bounds = new Bounds(
                new Vector3(cx * sx + sx * 0.5f, cy * sy + sy * 0.5f, cellZ),
                new Vector3(sx, sy, 0.1f)
            );

            const int MaxPerBatch = 1023;
            var mats  = new List<Matrix4x4>(MaxPerBatch);
            var cIdxs = new List<float>(MaxPerBatch);
            var flags = new List<float>(MaxPerBatch);

            void Flush()
            {
                if (mats.Count == 0) return;
                bd.matrices.Add(mats.ToArray());
                var mpb = new MaterialPropertyBlock();
                mpb.SetFloatArray("_ColorIndex", cIdxs);
                mpb.SetFloatArray("_Flags", flags);
                if (paletteTex) mpb.SetTexture("_PaletteTex", paletteTex);
                bd.mpbs.Add(mpb);
                mats  = new List<Matrix4x4>(MaxPerBatch);
                cIdxs = new List<float>(MaxPerBatch);
                flags = new List<float>(MaxPerBatch);
            }

            byte uniformIdx = Cell.GetColorIndex(uniform.Packed);
            for (int ly = 0; ly < chunkSize; ly++)
            {
                for (int lx = 0; lx < chunkSize; lx++)
                {
                    float x = (cx * chunkSize + lx + 0.5f) * cellSize;
                    float y = (cy * chunkSize + ly + 0.5f) * cellSize;

                    mats.Add(Matrix4x4.TRS(new Vector3(x, y, cellZ), Quaternion.identity, new Vector3(cellSize, cellSize, 1f)));

                    if (isUniform)
                    {
                        cIdxs.Add(uniformIdx);
                        flags.Add(uniform.Packed);
                    }
                    else
                    {
                        int idx = ly * chunkSize + lx;
                        var c = cells[idx];
                        // Optionaler Tint für nicht begehbar: z.B. oberes Bit im Flags-Float markieren
                        if (tintUnwalkable && !Cell.GetWalkable(c.Packed))
                        {
                            // z.B. ColorIndex + 128 als Hinweis auf „dunkler“
                            cIdxs.Add((Cell.GetColorIndex(c.Packed) + 128) % 256);
                        }
                        else
                        {
                            cIdxs.Add(Cell.GetColorIndex(c.Packed));
                        }
                        flags.Add(c.Packed);
                    }

                    if (mats.Count >= MaxPerBatch) Flush();
                }
            }
            Flush();
        }

        bool IsVisible(Bounds b)
        {
            if (_cam == null) return true;
            var planes = GeometryUtility.CalculateFrustumPlanes(_cam);
            return GeometryUtility.TestPlanesAABB(planes, b);
        }

        static Mesh BuildQuad()
        {
            var m = new Mesh { name = "GridCellQuad" };
            m.vertices = new[] {
                new Vector3(-0.5f,-0.5f,0), new Vector3(0.5f,-0.5f,0),
                new Vector3( 0.5f, 0.5f,0), new Vector3(-0.5f, 0.5f,0)
            };
            m.uv = new[] { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
            m.triangles = new[] { 0,1,2, 0,2,3 };
            m.RecalculateBounds(); m.UploadMeshData(true);
            return m;
        }
    }
}