// Runtime/Interaction/GridInteractor.cs

using System;
using GridFinder.Structs;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace GridFinder.Runtime.Grid
{
    [RequireComponent(typeof(GridRuntime))]
    public class GridInteractor : MonoBehaviour
    {
        public Camera cam;
        public LayerMask groundMask; // Layer deiner Boden-/Ray-Zielfläche
        public Transform hoverCursor; // Optional: ein dünner Quad/Cube als Cursor
        public Material hoverMaterial; // Optional: wenn du per Renderer zeichnen willst

        public int defaultLayer = 0; // Welche Grid-Layer wir anvisieren
        public event Action<CellCoord> CellClicked;

        private GridRuntime grid;
        private bool hasHover;
        private CellCoord hoverCell;

        void Awake()
        {
            grid = GetComponent<GridRuntime>();
            if (!cam) cam = Camera.main;
            SetupDefaultHoverCursor();
        }

        void Update()
        {
            if (TryGetHover(out var cell))
            {
                hasHover = true;
                hoverCell = cell;
                UpdateHoverCursor(cell);

                if (GetPrimaryClickDown())
                {
                    CellClicked?.Invoke(cell);
                }
            }
            else
            {
                hasHover = false;
                if (hoverCursor) hoverCursor.gameObject.SetActive(false);
            }
        }

        bool TryGetHover(out CellCoord cell)
        {
            cell = default;
            if (!cam) return false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var pos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
        var ray = cam.ScreenPointToRay(pos);
#else
            var ray = cam.ScreenPointToRay(Input.mousePosition);
#endif
            if (Physics.Raycast(ray, out var hit, 500f, groundMask))
            {
                if (grid.CellFromWorld(hit.point, defaultLayer, out var gx, out var gy))
                {
                    cell = new CellCoord(gx, gy, defaultLayer);
                    return true;
                }
            }

            return false;
        }

        bool GetPrimaryClickDown()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        void SetupDefaultHoverCursor()
        {
            if (hoverCursor) return;
            // Erzeuge einen dünnen, sichtbaren Cursor (Quad/Cube) in Laufzeit
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "GF_HoverCursor";
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(90, 0, 0); // liegend
            var rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = hoverMaterial ? hoverMaterial : DefaultHoverMaterial();
            // dünn skalieren: XZ = cellSize, Y = ignoriert bei Quad
            go.layer = LayerMask.NameToLayer("Default");
            Destroy(go.GetComponent<Collider>());
            hoverCursor = go.transform;
            hoverCursor.gameObject.SetActive(false);
        }

        Material DefaultHoverMaterial()
        {
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (!sh) sh = Shader.Find("Unlit/Color");
            var m = new Material(sh);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", new Color(1, 1, 0, 0.35f));
            else if (m.HasProperty("_Color")) m.SetColor("_Color", new Color(1, 1, 0, 0.35f));
            // doppelseitig, damit von oben sichtbar
            if (m.HasProperty("_Cull")) m.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            return m;
        }

        void UpdateHoverCursor(CellCoord cell)
        {
            if (!hoverCursor) return;
            var s = grid.CellSize;
            var p = grid.WorldFromCell(cell.x, cell.y, cell.z);
            hoverCursor.position = p + Vector3.up * 0.001f; // Z‑Fighting vermeiden
            hoverCursor.localScale = new Vector3(s, s, 1f);
            hoverCursor.gameObject.SetActive(true);
        }

        // Optional public getter
        public bool TryGetHoveredCell(out CellCoord cell)
        {
            cell = hoverCell;
            return hasHover;
        }
    }
}
