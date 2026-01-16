using UnityEngine;
using UnityEngine.UI;

namespace GridFinder.UI
{
    public sealed class ChangeClickModeButton : MonoBehaviour
    {
        [SerializeField] private AppState state = null!;
        [SerializeField] private Button spawnAgentButton = null!;
        [SerializeField] private Button setTargetButton = null!;

        void OnEnable()
        {
            spawnAgentButton.onClick.AddListener(() => state.CurrentClickMode.Value = ClickMode.SpawnAgent);
            setTargetButton.onClick.AddListener(() => state.CurrentClickMode.Value = ClickMode.SetTargetPoint);
        }

        void OnDisable()
        {
            spawnAgentButton.onClick.RemoveAllListeners();
            setTargetButton.onClick.RemoveAllListeners();
        }
    }
}