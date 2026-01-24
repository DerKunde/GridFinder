using UnityEngine;

namespace GridFinder.Grid
{
    /// <summary>
    /// Root component for the visual grid prefab.
    /// Holds references to the floor transform/renderer (a plane/mesh underneath the grid).
    /// </summary>
    public sealed class GridRoot : MonoBehaviour
    {
        [field: Header("Floor References")]
        [field: SerializeField] public Transform FloorTransform { get; private set; } = null!;
        [field: SerializeField] public Renderer FloorRenderer { get; private set; } = null!;

        [Header("Auto Resolve (Fallbacks)")]
        [Tooltip("If FloorTransform is not set, this child index will be used as fallback.")]
        [SerializeField] private int floorChildIndexFallback = 0;

        private void Awake()
        {
            ResolveReferences();
        }

        /// <summary>
        /// Ensures FloorTransform and FloorRenderer are assigned.
        /// </summary>
        public void ResolveReferences()
        {
            if (!FloorTransform)
            {
                if (transform.childCount > floorChildIndexFallback)
                {
                    FloorTransform = transform.GetChild(floorChildIndexFallback);
                }
                else
                {
                    Debug.LogWarning("[GridRoot] FloorTransform missing and fallback child index is invalid.");
                }
            }

            if (!FloorRenderer && FloorTransform)
            {
                FloorRenderer = FloorTransform.GetComponentInChildren<Renderer>();
                if (!FloorRenderer)
                {
                    Debug.LogWarning("[GridRoot] FloorRenderer missing and could not be found on FloorTransform.");
                }
            }
        }

        /// <summary>
        /// Applies world center + world size (XZ) to the floor.
        /// Assumes the floor mesh is aligned on XZ and uses a standard Unity Plane size of 10x10 units.
        /// If your mesh is not a Unity Plane, adjust the normalization factor.
        /// </summary>
        public void ApplyFloorLayout(Vector3 worldCenter, Vector2 worldSizeXZ, float y)
        {
            if (!FloorTransform)
            {
                Debug.LogWarning("[GridRoot] Cannot apply floor layout: FloorTransform missing.");
                return;
            }

            // Position
            FloorTransform.position = new Vector3(worldCenter.x, y, worldCenter.z);
            
            FloorTransform.localScale = new Vector3(
                worldSizeXZ.x,
                FloorTransform.localScale.y,
                worldSizeXZ.y
            );
        }

        /// <summary>
        /// Optional convenience: sets the floor material.
        /// </summary>
        public void SetFloorMaterial(Material material)
        {
            if (!FloorRenderer)
            {
                Debug.LogWarning("[GridRoot] Cannot set floor material: FloorRenderer missing.");
                return;
            }

            FloorRenderer.sharedMaterial = material;
        }
    }
}