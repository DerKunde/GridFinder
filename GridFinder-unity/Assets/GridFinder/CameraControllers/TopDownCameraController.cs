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
    [Header("Grid Bounds (world units)")]
    [Tooltip("Linke-untere Ecke (x,y) des Grids in Weltkoordinaten.")]
    public Vector2 gridMin = new Vector2(0, 0);

    [Tooltip("Breite und Höhe des Grids in Weltkoordinaten.")]
    public Vector2 gridSize = new Vector2(100, 100);

    [Header("Movement")]
    [Tooltip("Pan-Geschwindigkeit in Welt-Einheiten pro Sekunde bei OrthoSize=1.")]
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

    [Tooltip("Glättungsfaktor für Zoom (höher = weicher).")]
    [Range(0.0f, 1.0f)]
    public float zoomSmoothing = 0.15f;

    Camera _cam;
    float _targetOrtho;

    Rect GridRect => new Rect(gridMin, gridSize);

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        // Top-Down Blick von oben auf die X/Y-Ebene:
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Start-Ortho sauber clampen
        _targetOrtho = Mathf.Clamp(_cam.orthographicSize, minOrthoSize, maxOrthoSize);
        _cam.orthographicSize = _targetOrtho;

        // Startposition in Bounds zentrieren (optional)
        ClampPositionToGrid();
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
        ClampPositionToGrid();
    }

    void HandlePan()
    {
        Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  input.y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    input.y += 1f;
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

        // Da die Kamera nach unten (−Z) schaut, bewegen wir uns in Welt-X und Welt-Y.
        Vector3 delta = new Vector3(input.x, input.y, 0f) * speed * Time.deltaTime;
        transform.position += delta;
    }

    void HandleZoom()
    {
        float scroll = 0f;

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null)
        {
            // Unity: scroll.y > 0 = Zoom rein (nach oben)
            scroll = mouse.scroll.ReadValue().y;
        }
#else
        scroll = Input.mouseScrollDelta.y;
#endif

        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            _targetOrtho -= scroll * (zoomSpeed * 0.1f) * Mathf.Max(1f, _targetOrtho * 0.5f);
            _targetOrtho = Mathf.Clamp(_targetOrtho, minOrthoSize, maxOrthoSize);
        }

        if (smoothZoom)
        {
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetOrtho, 1f - zoomSmoothing);
        }
        else
        {
            _cam.orthographicSize = _targetOrtho;
        }
    }

    void ClampPositionToGrid()
    {
        var rect = GridRect;

        // Sichtbare halbe Ausdehnung (in Welt) auf Basis der aktuellen Ortho-Größe:
        float halfHeight = _cam.orthographicSize;
        float halfWidth  = halfHeight * _cam.aspect;

        // Wenn der Viewport größer als das Grid ist, zentrieren wir auf das Grid.
        float minX = rect.xMin + halfWidth;
        float maxX = rect.xMax - halfWidth;
        float minY = rect.yMin + halfHeight;
        float maxY = rect.yMax - halfHeight;

        Vector3 pos = transform.position;

        if (rect.width <= halfWidth * 2f)
            pos.x = rect.center.x;
        else
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

        if (rect.height <= halfHeight * 2f)
            pos.y = rect.center.y;
        else
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Z bleibt unverändert (bei Top-Down Ortho egal, typischerweise 0)
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Grid-Rechteck
        var r = GridRect;
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawCube(new Vector3(r.center.x, r.center.y, 0f), new Vector3(r.width, r.height, 0.01f));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(r.center.x, r.center.y, 0f), new Vector3(r.width, r.height, 0.01f));
    }
#endif
}
}