using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GridFinder.UI
{
    public sealed class BasicHudView : MonoBehaviour
    {
        [SerializeField] private AppState state = null!;

        [Header("Buttons")]
        [SerializeField] private Button spawnAgentButton = null!;
        [SerializeField] private Button setTargetButton = null!;

        [Header("Labels")]
        [SerializeField] private TMP_Text agentCountText = null!;
        [SerializeField] private TMP_Text fpsText = null!;
        [SerializeField] private TMP_Text frameMsText = null!;
        [SerializeField] private TMP_Text modeText = null!;

        private readonly CompositeDisposable disposables = new();

        void OnEnable()
        {
            // Buttons -> State
            spawnAgentButton.onClick.AddListener(() => state.CurrentClickMode.Value = ClickMode.SpawnAgent);
            setTargetButton.onClick.AddListener(() => state.CurrentClickMode.Value = ClickMode.SetTargetPoint);

            // State -> UI
            state.CurrentClickMode
                .Subscribe(mode => modeText.text = $"Mode: {mode}")
                .AddTo(disposables);

            state.AgentCount
                .Subscribe(n => agentCountText.text = $"Agents: {n}")
                .AddTo(disposables);

            state.Fps
                .Subscribe(f => fpsText.text = $"FPS: {f:0}")
                .AddTo(disposables);

            state.FrameMs
                .Subscribe(ms => frameMsText.text = $"Frame: {ms:0.0} ms")
                .AddTo(disposables);
        }

        void OnDisable()
        {
            disposables.Clear();
            spawnAgentButton.onClick.RemoveAllListeners();
            setTargetButton.onClick.RemoveAllListeners();
        }
    }
}