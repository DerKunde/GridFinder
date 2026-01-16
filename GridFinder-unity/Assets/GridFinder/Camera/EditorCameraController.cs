using UnityEngine;

namespace GridFinder.Camera
{
    /// <summary>
    /// Editor-like camera controller (Scene View style) for runtime:
    /// - Alt + LMB : Orbit around pivot
    /// - MMB       : Pan (move pivot)
    /// - Scroll    : Zoom (dolly towards pivot)
    /// - F         : Recenter pivot (optional focus target)
    ///
    /// Works best for a ground plane on XZ (Y up).
    /// Attach to your Camera (or a parent "Rig" object).
    /// </summary>
    public sealed class EditorCameraController : MonoBehaviour
    {
        [Header("Pivot")]
        [SerializeField] private Transform focusTarget; // optional
        [SerializeField] private Vector3 pivot = Vector3.zero;

        [Header("Speeds")]
        [SerializeField] private float orbitSpeed = 180f;   // degrees per mouse delta
        [SerializeField] private float panSpeed = 1.0f;     // scaled by distance
        [SerializeField] private float zoomSpeed = 5.0f;    // scaled by distance

        [Header("Zoom Limits")]
        [SerializeField] private float minDistance = 0.5f;
        [SerializeField] private float maxDistance = 100f;

        [Header("Pitch Limits")]
        [SerializeField] private float minPitch = 10f;  // degrees
        [SerializeField] private float maxPitch = 85f;  // degrees

        // Internal camera spherical params around pivot
        private float distance;
        private float yaw;
        private float pitch;

        private void Awake()
        {
            // Initialize orbit parameters from current transform
            if (focusTarget != null)
                pivot = focusTarget.position;

            var offset = transform.position - pivot;
            distance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);

            // Convert to yaw/pitch
            var dir = offset.normalized;
            pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
            yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg; // note: x,z order for Y-up

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            ApplyTransform();
        }

        private void Update()
        {
            // Focus pivot
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (focusTarget != null)
                    pivot = focusTarget.position;

                ApplyTransform();
            }

            // Orbit: Alt + LMB
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
            {
                var dx = Input.GetAxisRaw("Mouse X");
                var dy = Input.GetAxisRaw("Mouse Y");

                yaw += dx * orbitSpeed * Time.unscaledDeltaTime;
                pitch -= dy * orbitSpeed * Time.unscaledDeltaTime;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

                ApplyTransform();
            }

            // Pan: MMB
            if (Input.GetMouseButton(2))
            {
                var dx = Input.GetAxisRaw("Mouse X");
                var dy = Input.GetAxisRaw("Mouse Y");

                // Pan speed scales with distance (feels more editor-like)
                var scale = panSpeed * Mathf.Max(0.1f, distance);

                // Move pivot along camera right and camera up projected onto XZ plane
                var right = transform.right;
                var up = transform.up;

                // For a ground-based world it's usually nicer to pan in camera right + world forward
                // but editor pans in screen space; we'll keep it screen-space:
                pivot -= right * (dx * scale * Time.unscaledDeltaTime);
                pivot -= up    * (dy * scale * Time.unscaledDeltaTime);

                ApplyTransform();
            }

            // Zoom: scroll wheel (dolly)
            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                var zoomScale = zoomSpeed * Mathf.Max(0.1f, distance);
                distance -= scroll * zoomScale * Time.unscaledDeltaTime;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);

                ApplyTransform();
            }
        }

        private void ApplyTransform()
        {
            // Build rotation from yaw/pitch (Y-up)
            var rot = Quaternion.Euler(pitch, yaw, 0f);

            // Camera position is pivot + rotated backward vector
            var pos = pivot + rot * new Vector3(0f, 0f, -distance);

            transform.SetPositionAndRotation(pos, rot);
        }

        // Optional: expose pivot for other systems (e.g., UI or grid selection)
        public Vector3 Pivot => pivot;

        public void SetPivot(Vector3 newPivot)
        {
            pivot = newPivot;
            ApplyTransform();
        }
    }
}
