// Runtime/Visualization/GridTextureVisualizer.cs

using System;
using Runtime.Grid;
using UnityEngine;

namespace Visualization
{
    [ExecuteAlways]
    public class GridTextureVisualizer : MonoBehaviour, IGridVisualizer
    {
        [SerializeField] private Material unlitTextureMaterial; // optional; wenn null → Default
        [SerializeField, Range(0, 1)] private float opacity = 0.85f;
        [SerializeField] private bool drawOnXZ = true;
        [SerializeField] private GridVisMode mode = GridVisMode.CostHeatmap;
        [SerializeField] private byte costMin = 1;
        [SerializeField] private byte costMax = 50;

        private IGridReadOnly grid;
        private Texture2D tex;
        private Color32[] pixels;
        private Mesh quadMesh;
        private MeshRenderer mr;
        private MeshFilter mf;
        private Material runtimeMat;
        private bool pendingApply;

        public bool IsAttached => grid != null;

        #region IGridVisualizer API
        public void Attach(IGridReadOnly g)
        {
            grid = g ?? throw new ArgumentNullException(nameof(g));
            EnsureRenderObjects();
            BuildQuadMesh();
            AllocateTexture();
            RebuildAllPixels();
            ApplyTexture();
        }

        public void Detach()
        {
            grid = null;
            if (mr != null) mr.enabled = false;
        }

        public void SetMode(GridVisMode m)
        {
            mode = m;
            if (IsAttached) { RebuildAllPixels(); ApplyTexture(); }
        }

        public void SetOpacity(float a)
        {
            opacity = Mathf.Clamp01(a);
            SetMaterialOpacity();
        }

        public void SetDrawOnXZ(bool onXZ)
        {
            drawOnXZ = onXZ;
            if (IsAttached) BuildQuadMesh();
        }

        public void MarkDirty(RectInt rect)
        {
            if (!IsAttached || tex == null) return;

            rect.xMin = Mathf.Clamp(rect.xMin, 0, grid.Width - 1);
            rect.yMin = Mathf.Clamp(rect.yMin, 0, grid.Height - 1);
            rect.xMax = Mathf.Clamp(rect.xMax, 0, grid.Width - 1);
            rect.yMax = Mathf.Clamp(rect.yMax, 0, grid.Height - 1);

            for (int y = rect.yMin; y <= rect.yMax; y++)
            for (int x = rect.xMin; x <= rect.xMax; x++)
            {
                pixels[y * grid.Width + x] = ColorForCell(x, y);
            }

            // effizient: nur Dirty-Rect übertragen
            tex.SetPixels32(rect.xMin, rect.yMin, rect.width + 1, rect.height + 1, ExtractRectPixels(rect));
            tex.Apply(false, false);
        }

        public void MarkDirtyAll()
        {
            if (!IsAttached) return;
            RebuildAllPixels();
            ApplyTexture();
        }
        #endregion

        #region Unity lifecycle
        private void OnEnable()
        {
            if (mr) mr.enabled = true;
        }

        private void OnDisable()
        {
            if (mr) mr.enabled = false;
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (tex) Destroy(tex);
                if (runtimeMat) Destroy(runtimeMat);
                if (quadMesh) Destroy(quadMesh);
            }
            else
            {
                if (tex) DestroyImmediate(tex);
                if (runtimeMat) DestroyImmediate(runtimeMat);
                if (quadMesh) DestroyImmediate(quadMesh);
            }
        }

        private void LateUpdate()
        {
            if (pendingApply)
            {
                ApplyTexture();
            }
        }
        #endregion

        #region Internal helpers
        private void EnsureRenderObjects()
        {
            mr = GetComponent<MeshRenderer>();
            mf = GetComponent<MeshFilter>();
            if (!mr) mr = gameObject.AddComponent<MeshRenderer>();
            if (!mf) mf = gameObject.AddComponent<MeshFilter>();

            if (runtimeMat == null)
            {
                if (unlitTextureMaterial != null) runtimeMat = new Material(unlitTextureMaterial);
                else runtimeMat = new Material(Shader.Find("Unlit/Transparent"));
                mr.sharedMaterial = runtimeMat;
                SetMaterialOpacity();
            }
        }

        private void SetMaterialOpacity()
        {
            if (runtimeMat == null) return;
            var c = runtimeMat.color;
            c.a = opacity;
            runtimeMat.color = c;
        }

        private void BuildQuadMesh()
        {
            if (grid == null) return;

            // Größe in Weltkoordinaten
            float w = grid.Width * grid.CellSize;
            float h = grid.Height * grid.CellSize;
            Vector3 origin = drawOnXZ
                ? new Vector3(grid.OriginWorld.X, 0f, grid.OriginWorld.Y)
                : new Vector3(grid.OriginWorld.X, grid.OriginWorld.Y, 0f);

            if (quadMesh == null) quadMesh = new Mesh();
            quadMesh.name = "GridVisQuad";

            Vector3 v0, v1, v2, v3;
            if (drawOnXZ)
            {
                v0 = origin;
                v1 = origin + new Vector3(w, 0, 0);
                v2 = origin + new Vector3(0, 0, h);
                v3 = origin + new Vector3(w, 0, h);
            }
            else
            {
                v0 = origin;
                v1 = origin + new Vector3(w, 0, 0);
                v2 = origin + new Vector3(0, h, 0);
                v3 = origin + new Vector3(w, h, 0);
            }

            quadMesh.Clear();
            quadMesh.vertices = new[] { v0, v1, v2, v3 };
            quadMesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            quadMesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            quadMesh.RecalculateBounds();

            mf.sharedMesh = quadMesh;
        }

        private void AllocateTexture()
        {
            if (grid == null) return;

            if (tex == null || tex.width != grid.Width || tex.height != grid.Height)
            {
                if (tex != null)
                {
                    if (Application.isPlaying) Destroy(tex);
                    else DestroyImmediate(tex);
                }

                tex = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false, true);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;

                pixels = new Color32[grid.Width * grid.Height];
                runtimeMat.mainTexture = tex;
            }
        }

        private void RebuildAllPixels()
        {
            if (grid == null || pixels == null) return;
            int w = grid.Width, h = grid.Height;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = ColorForCell(x, y);

            pendingApply = true;
        }

        private void ApplyTexture()
        {
            if (tex == null || pixels == null) return;
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            pendingApply = false;
            if (mr) mr.enabled = true;
        }

        private Color32[] ExtractRectPixels(RectInt r)
        {
            int w = grid.Width;
            int width = r.width + 1;
            int height = r.height + 1;
            var buf = new Color32[width * height];
            int i = 0;
            for (int y = r.yMin; y <= r.yMax; y++)
            {
                int row = y * w + r.xMin;
                for (int x = 0; x < width; x++)
                    buf[i++] = pixels[row + x];
            }
            return buf;
        }

        private Color32 ColorForCell(int x, int y)
        {
            byte c = grid.GetCost(x, y);
            if (mode == GridVisMode.None) return new Color32(0, 0, 0, 0);

            if (c == byte.MaxValue)
                return mode == GridVisMode.WalkableMask
                    ? new Color32(0, 0, 0, 255) // schwarz = blockiert
                    : new Color32(0, 0, 0, 255);

            if (mode == GridVisMode.WalkableMask)
                return new Color32(255, 255, 255, 255); // weiß = begehbar

            // CostHeatmap
            // clamp & normalize
            float t = Mathf.InverseLerp(costMin, costMax, Mathf.Clamp(c, costMin, costMax));
            // grün -> gelb -> rot
            // grün (0,1,0)  gelb (1,1,0)  rot (1,0,0)
            Color col = t < 0.5f
                ? Color.Lerp(new Color(0f, 1f, 0f), new Color(1f, 1f, 0f), t * 2f)
                : Color.Lerp(new Color(1f, 1f, 0f), new Color(1f, 0f, 0f), (t - 0.5f) * 2f);

            col.a = 1f;
            return col;
        }
        #endregion
    }
}
