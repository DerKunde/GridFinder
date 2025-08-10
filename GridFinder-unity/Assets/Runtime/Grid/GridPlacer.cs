using UnityEngine;

namespace Runtime.Grid
{
    [RequireComponent(typeof(GridRuntime))]
    public class GridPlacer : MonoBehaviour {
        [SerializeField] Camera cam;
        [SerializeField] GridRuntime grid;
        [SerializeField] LayerMask groundMask;
        [SerializeField] GameObject[] placeablePrefabs;
        int prefabIndex = 0;

        void Reset() { grid = GetComponent<GridRuntime>(); }

        void Update() {
            if (!cam) cam = Camera.main;
            if (Input.GetMouseButtonDown(0) && RayToGrid(out int gx, out int gy, out int gz)) {
                if (grid.TrySetOccupied(gx, gy, gz, true)) {
                    var prefab = placeablePrefabs[prefabIndex % placeablePrefabs.Length];
                    var world = grid.WorldFromCell(gx, gy, gz);
                    Instantiate(prefab, world, Quaternion.identity);
                }
            }
            if (Input.mouseScrollDelta.y != 0) {
                prefabIndex = Mathf.Max(0, prefabIndex + (int)Mathf.Sign(Input.mouseScrollDelta.y));
            }
        }

        bool RayToGrid(out int x, out int y, out int z) {
            x=y=z=0;
            if (!cam) return false;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 500f, groundMask)) {
                z = 0; // Milestone 1
                return grid.CellFromWorld(hit.point, z, out x, out y);
            }
            return false;
        }
    }
}