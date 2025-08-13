using UnityEngine;

namespace GridFinder.Samples
{
    [RequireComponent(typeof(SampleGridController))]
    [DisallowMultipleComponent]
    public class GridGLRenderer : MonoBehaviour
    {
        public Color lineColor = new Color(1, 1, 1, 0.2f);
        [Tooltip("Kleiner Z-Versatz um Z-Fighting mit Markern zu vermeiden.")]
        public float zBias = 0.0005f;

        Material _mat;
        SampleGridController _grid;

        void Awake()
        {
            _grid = GetComponent<SampleGridController>();

            // Unlit-Farb-Material für GL
            var sh = Shader.Find("Hidden/Internal-Colored");
            _mat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _mat.SetInt("_ZWrite", 0); // kein Depth-Write, damit Linien sauber über dem Grid liegen
        }

        void OnRenderObject()
        {
            if (_mat == null || _grid == null) return;

            _mat.SetPass(0);

            GL.PushMatrix();
            // Wir nutzen die aktuelle Kamera-Projektion und zeichnen in Weltkoordinaten
            GL.MultMatrix(Matrix4x4.identity);

            GL.Begin(GL.LINES);
            GL.Color(lineColor);

            float w = _grid.cols * _grid.cellSize;
            float h = _grid.rows * _grid.cellSize;
            float z = _grid.zOffset + zBias;

            // Vertikale Linien
            for (int x = 0; x <= _grid.cols; x++)
            {
                float vx = x * _grid.cellSize;
                GL.Vertex3(vx, 0f, z);
                GL.Vertex3(vx, h,  z);
            }

            // Horizontale Linien
            for (int y = 0; y <= _grid.rows; y++)
            {
                float vy = y * _grid.cellSize;
                GL.Vertex3(0f, vy, z);
                GL.Vertex3(w,  vy, z);
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}
