using UnityEngine;

namespace GridFinder.UI
{
    public sealed class PerformanceMetricsUpdater : MonoBehaviour
    {
        [SerializeField] private AppState state = null!;
        [SerializeField, Range(0.01f, 1f)] private float smoothing = 0.15f;

        private float smoothedDelta;

        void Update()
        {
            var dt = Time.unscaledDeltaTime;
            smoothedDelta = Mathf.Lerp(smoothedDelta, dt, smoothing);

            var ms = smoothedDelta * 1000f;
            var fps = smoothedDelta > 0.00001f ? (1f / smoothedDelta) : 0f;

            state.FrameMs.Value = ms;
            state.Fps.Value = fps;
        }
    }
}