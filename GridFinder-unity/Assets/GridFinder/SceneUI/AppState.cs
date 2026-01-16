using R3;
using UnityEngine;

namespace GridFinder.UI
{
    public enum ClickMode
    {
        SpawnAgent = 0,
        SetTargetPoint = 1
    }

    [CreateAssetMenu(menuName = "GridFinder/App State", fileName = "AppState")]
    public sealed class AppState : ScriptableObject
    {
        // UI -> Input systems
        public readonly ReactiveProperty<ClickMode> CurrentClickMode = new(ClickMode.SpawnAgent);

        // Systems -> UI
        public readonly ReactiveProperty<int> AgentCount = new(0);

        // "Speed" metric (simple + useful): fps and/or frame time
        public readonly ReactiveProperty<float> Fps = new(0f);
        public readonly ReactiveProperty<float> FrameMs = new(0f);
        
    }
}