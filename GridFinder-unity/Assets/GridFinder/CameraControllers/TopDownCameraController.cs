using System;
using GridFinder.Samples;
using R3;

namespace GridFinder.CameraControllers
{
    using UnityEngine;
#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class TopDownCameraController : MonoBehaviour
    {
        public enum GridPlane
        {
            XY,
            XZ,
            YZ
        }

        public SampleGridController gridController;
        private CompositeDisposable _disposable;

        [Header("Grid Plane")] [Tooltip("Wähle die Ebene, in der das Grid liegt.")]
        public GridPlane gridPlane = GridPlane.XY;

        [Header("Grid Bounds (plane coords)")] [Tooltip("Linke-untere Ecke (u,v) des Grids in Ebenen-Koordinaten.")]
        public Vector2 gridMin = new Vector2(0, 0);

        [Tooltip("Breite und Höhe des Grids in Ebenen-Koordinaten.")]
        public Vector2 gridSize = new Vector2(100, 100);

        [Header("Movement")] [Tooltip("Pan-Geschwindigkeit in Welt-Einheiten pro Sekunde bei OrthoSize=1.")]
        public float panSpeed = 20f;

        [Tooltip("Skaliert die Pan-Geschwindigkeit proportional zur aktuellen Ortho-Größe.")]
        public bool speedScalesWithZoom = true;

        [Tooltip("Zusätzliche Pan-Geschwindigkeit beim Halten von Left Shift.")]
        public float fastMultiplier = 2f;

        [Header("Zoom (Orthographic)")]
        [Tooltip("Minimale orthographische Größe (halbe Viewport-Höhe in Welt-Einheiten).")]
        public float minOrthoSize = 1.5f;

        [Tooltip("Maximale orthographische Größe.")]
        public float maxOrthoSize = 200f;

        [Tooltip("Zoomgeschwindigkeit pro Mausrad-Einheit.")]
        public float zoomSpeed = 10f;

        [Tooltip("Sanftes Ausblenden der Zoomschritte.")]
        public bool smoothZoom = true;

        [Tooltip("Glättungsfaktor für Zoom (höher = weicher).")] [Range(0.0f, 1.0f)]
        public float zoomSmoothing = 0.15f;

        [Header("Plane Distance")]
        [Tooltip("Abstand der Kamera entlang der Ebenennormalen (bei 0 wird der aktuelle Abstand beibehalten).")]
        public float initialPlaneDistance = 10f;

        Camera _cam;
        float _targetOrtho;

        // Orthonormales Basis-Dreibein für die gewählte Ebene
        struct Frame
        {
            public Vector3 U, V, N;
        } // U = rechts, V = oben, N = Normalenrichtung (Kamera schaut entlang N)

        Frame _frame;

        Rect GridRectUV => new Rect(gridMin, gridSize);

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;

            BuildFrame(); // U,V,N bestimmen und Rotation setzen
            EnsurePlaneDistance(); // sinnvollen Start-Abstand setzen, falls gewünscht

            _targetOrtho = Mathf.Clamp(_cam.orthographicSize, minOrthoSize, maxOrthoSize);
            _cam.orthographicSize = _targetOrtho;

            _disposable = new CompositeDisposable(
                gridController
                    .ObserveGridDimensions()
                    .Subscribe(dimensions =>
            {
                gridSize.x = dimensions.Item1;
                gridSize.y = dimensions.Item2;
            }));
            ClampPositionToGrid(); // Startposition in Bounds zentrieren/clampen
        }

        void Update()
        {
            HandlePan();
            HandleZoom();
            ClampPositionToGrid();
        }

        // ---------- Core ----------

        void BuildFrame()
        {
            // Lege U (rechts), V (oben) und N (Vorwärts) für die gewählte Ebene fest
            switch (gridPlane)
            {
                case GridPlane.XY:
                    _frame.U = Vector3.right; // +X
                    _frame.V = Vector3.up; // +Y
                    _frame.N = Vector3.forward; // +Z
                    break;

                case GridPlane.XZ:
                    _frame.U = Vector3.right; // +X
                    _frame.V = Vector3.forward; // +Z
                    _frame.N = Vector3.up; // +Y
                    break;

                case GridPlane.YZ:
                    _frame.U = Vector3.up; // +Y
                    _frame.V = Vector3.forward; // +Z
                    _frame.N = Vector3.right; // +X
                    break;
            }

            // Kamera so ausrichten, dass sie entlang N schaut und "oben" auf V zeigt
            transform.rotation = Quaternion.LookRotation(_frame.N, _frame.V);
        }

        void EnsurePlaneDistance()
        {
            if (initialPlaneDistance <= 0f) return;

            // Zerlege aktuelle Position in UVN-Koordinaten
            Vector3 p = transform.position;
            float u = Vector3.Dot(p, _frame.U);
            float v = Vector3.Dot(p, _frame.V);
            float n = Vector3.Dot(p, _frame.N);

            // Setze einen sinnvollen Startabstand vor der Ebene (negativ, da die Kamera entlang +N schaut)
            n = -Mathf.Abs(initialPlaneDistance);

            transform.position = u * _frame.U + v * _frame.V + n * _frame.N;
        }

        // ---------- Input / Movement ----------

        void HandlePan()
        {
            Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
            }

            bool isFast = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
#else
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  input.y -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    input.y += 1f;
        bool isFast = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif

            if (input.sqrMagnitude > 1f) input.Normalize();

            float speed = panSpeed * (speedScalesWithZoom ? _cam.orthographicSize : 1f);
            if (isFast) speed *= fastMultiplier;

            // Bewege entlang U (rechts/links) und V (hoch/runter) der Ebenenbasis
            Vector3 delta = (_frame.U * input.x + _frame.V * input.y) * speed * Time.deltaTime;
            transform.position += delta;
        }

        void HandleZoom()
        {
            float scroll = 0f;

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null) scroll = mouse.scroll.ReadValue().y;
#else
        scroll = Input.mouseScrollDelta.y;
#endif

            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
                _targetOrtho -= scroll * (zoomSpeed * 0.1f) * Mathf.Max(1f, _targetOrtho * 0.5f);
                _targetOrtho = Mathf.Clamp(_targetOrtho, minOrthoSize, maxOrthoSize);
            }

            if (smoothZoom)
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetOrtho, 1f - zoomSmoothing);
            else
                _cam.orthographicSize = _targetOrtho;
        }

        void ClampPositionToGrid()
        {
            var rect = GridRectUV;

            // Zerlege aktuelle Position in UVN
            Vector3 p = transform.position;
            float u = Vector3.Dot(p, _frame.U);
            float v = Vector3.Dot(p, _frame.V);
            float n = Vector3.Dot(p, _frame.N); // Normalenanteil unverändert lassen

            // Sichtbare halbe Ausdehnung in U/V (orthographisch!)
            float halfV = _cam.orthographicSize; // Vertikal = V
            float halfU = halfV * _cam.aspect; // Horizontal = U

            // Bei zu kleinem Grid: auf Zentrum setzen, sonst clampen
            float minU = rect.xMin + halfU;
            float maxU = rect.xMax - halfU;
            float minV = rect.yMin + halfV;
            float maxV = rect.yMax - halfV;

            if (rect.width <= halfU * 2f) u = rect.center.x;
            else u = Mathf.Clamp(u, minU, maxU);

            if (rect.height <= halfV * 2f) v = rect.center.y;
            else v = Mathf.Clamp(v, minV, maxV);

            transform.position = u * _frame.U + v * _frame.V + n * _frame.N;
        }


#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // Zeichne die Grid-Bounds in der gewählten Ebene
            BuildFrame(); // sicherstellen, dass _frame im Editor stimmt

            var r = GridRectUV;
            Vector2 centerUV = r.center;

            // Weltzentrum des Rechtecks
            Vector3 center = centerUV.x * _frame.U + centerUV.y * _frame.V;

            // Lokale Achsenmatrix, um den Gizmo zu orientieren
            var m = new Matrix4x4();
            m.SetColumn(0, new Vector4(_frame.U.x, _frame.U.y, _frame.U.z, 0));
            m.SetColumn(1, new Vector4(_frame.V.x, _frame.V.y, _frame.V.z, 0));
            m.SetColumn(2, new Vector4(_frame.N.x, _frame.N.y, _frame.N.z, 0));
            m.SetColumn(3, new Vector4(center.x, center.y, center.z, 1));

            var prev = Gizmos.matrix;
            Gizmos.matrix = m;

            // Dünne Box (Breite=rect.width entlang U, Höhe=rect.height entlang V, kaum Dicke entlang N)
            Vector3 size = new Vector3(r.width, r.height, 0.01f);

            Gizmos.color = new Color(0, 1, 1, 0.25f);
            Gizmos.DrawCube(Vector3.zero, size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, size);

            Gizmos.matrix = prev;
        }
#endif
    }
}