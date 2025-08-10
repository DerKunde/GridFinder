using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Runtime.Grid
{
    [RequireComponent(typeof(GridRuntime))]
    public class GridRenderer : MonoBehaviour
    {
        GridRuntime grid;
        readonly List<Matrix4x4> matrices = new();
        const int BatchSize = 1023; // DrawMeshInstanced Limit

        [SerializeField] Mesh quadMesh;
        [SerializeField] bool drawOnPlay = true;

        void Awake() => grid = GetComponent<GridRuntime>();

        void LateUpdate()
        {
            if (!drawOnPlay) return;
            Draw();
        }

        public void Draw()
        {
            if (!quadMesh) quadMesh = BuildQuad();
            matrices.Clear();
            var s = grid.CellSize;
            var scale = new Vector3(s, 0.001f, s); // extrem dünn

            for (int z = 0; z < grid.Layers; z++)
            for (int y = 0; y < grid.Rows; y++)
            for (int x = 0; x < grid.Columns; x++)
            {
                var pos = grid.WorldFromCell(x, y, z);
                var m = Matrix4x4.TRS(pos, Quaternion.identity, scale);
                matrices.Add(m);

                if (matrices.Count == BatchSize)
                {
                    Graphics.DrawMeshInstanced(quadMesh, 0, grid.Settings.cellMaterial, matrices, null,
                        ShadowCastingMode.Off, false);
                    matrices.Clear();
                }
            }

            if (matrices.Count > 0)
            {
                Graphics.DrawMeshInstanced(quadMesh, 0, grid.Settings.cellMaterial, matrices, null,
                    ShadowCastingMode.Off, false);
            }
        }

        static Mesh BuildQuad()
        {
            var m = new Mesh { name = "GF_Quad" };
            m.vertices = new[]
            {
                new Vector3(-.5f, 0, -.5f), new Vector3(.5f, 0, -.5f),
                new Vector3(.5f, 0, .5f), new Vector3(-.5f, 0, .5f)
            };
            m.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            m.triangles = new[]  { 0,2,1, 0,3,2 };
            m.RecalculateBounds();
            return m;
        }
    }
}