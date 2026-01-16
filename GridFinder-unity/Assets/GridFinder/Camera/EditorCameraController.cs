using UnityEngine;

namespace GridFinder.Camera
{
    /// <summary>
    /// Runtime editor-like camera:
    /// - WASD : move on ground plane (XZ) relative to camera yaw
    /// - Q/E  : move down/up (optional but useful)
    /// - Shift: speed boost
    /// - RMB  : freelook (mouse to rotate)
    /// - MMB  : pan (drag camera parallel to view)
    /// - Scroll: zoom (dolly forward/back)
    /// - F    : snap back to target transform (focus)
    ///
    /// LMB is intentionally unused to keep it free for gameplay clicks (spawn, selection, etc.).
    /// </summary>
    public sealed class EditorCameraController : MonoBehaviour
    {
        [Header("Focus")]
        [SerializeField] private Transform targetTransform; // optional
        [SerializeField] private Vector3 focusOffset = new(0f, 8f, -8f);

        [Header("Move")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float boostMultiplier = 4f;
        [SerializeField] private float verticalSpeed = 4f;

        [Header("Look")]
        [SerializeField] private bool requireRmbForLook = true;
        [SerializeField] private float lookSensitivity = 180f; // degrees/sec scaled by mouse delta
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        [Header("Pan (MMB)")]
        [SerializeField] private float panSpeed = 0.8f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 25f;

        private float yaw;
        private float pitch;

        void Awake()
        {
            // Initialize yaw/pitch from current rotation
            var e = transform.rotation.eulerAngles;
            yaw = e.y;
            pitch = NormalizePitch(e.x);
        }

        void Update()
        {
            // Focus
            if (Input.GetKeyDown(KeyCode.F))
                FocusOnTarget();

            // Look (RMB freelook)
            if (!requireRmbForLook || Input.GetMouseButton(1))
                UpdateLook();

            // Pan (MMB drag)
            if (Input.GetMouseButton(2))
                UpdatePan();

            // Zoom (scroll)
            UpdateZoom();

            // Move (WASD + QE)
            UpdateMove();
        }

        private void FocusOnTarget()
        {
            if (!targetTransform)
                return;

            transform.position = targetTransform.position + focusOffset;

            // Look at target (keep a sane pitch/yaw)
            var dir = (targetTransform.position - transform.position).normalized;
            var rot = Quaternion.LookRotation(dir, Vector3.up);

            var e = rot.eulerAngles;
            yaw = e.y;
            pitch = NormalizePitch(e.x);

            ApplyRotation();
        }

        private void UpdateLook()
        {
            var dx = Input.GetAxisRaw("Mouse X");
            var dy = Input.GetAxisRaw("Mouse Y");

            yaw += dx * lookSensitivity * Time.unscaledDeltaTime;
            pitch -= dy * lookSensitivity * Time.unscaledDeltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            ApplyRotation();
        }

        private void UpdateMove()
        {
            // Keep movement ground-relative: forward/right projected to XZ plane
            var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

            var inputX = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            var inputZ = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);

            var move = (right * inputX + forward * inputZ);

            // Vertical (optional)
            var inputY = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
            move += Vector3.up * (inputY * (verticalSpeed / Mathf.Max(0.0001f, moveSpeed)));

            var speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f);

            transform.position += move * (speed * Time.unscaledDeltaTime);
        }

        private void UpdatePan()
        {
            var dx = Input.GetAxisRaw("Mouse X");
            var dy = Input.GetAxisRaw("Mouse Y");

            // Screen-space pan: move opposite to mouse drag
            // Scale by distance to target if available (more editor-like)
            float scale = panSpeed;
            if (targetTransform)
            {
                var dist = Vector3.Distance(transform.position, targetTransform.position);
                scale *= Mathf.Max(0.2f, dist);
            }

            var right = transform.right;
            var up = transform.up;

            transform.position -= right * (dx * scale * Time.unscaledDeltaTime);
            transform.position -= up * (dy * scale * Time.unscaledDeltaTime);
        }

        private void UpdateZoom()
        {
            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) < 0.0001f)
                return;

            // Dolly along view direction
            transform.position += transform.forward * (scroll * zoomSpeed * Time.unscaledDeltaTime);
        }

        private void ApplyRotation()
        {
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private static float NormalizePitch(float pitchDeg)
        {
            // Unity eulerAngles returns 0..360, convert to -180..180
            if (pitchDeg > 180f) pitchDeg -= 360f;
            return pitchDeg;
        }
    }
}
