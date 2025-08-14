using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Timer
{
    // Setzt/aktualisiert die Tickrate VOR der Simulation
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial class SimulationTickrateBootstrap : SystemBase
    {
        SimulationSystemGroup _simGroup;
        RateUtils.FixedRateSimpleManager _manager;
        float _appliedHz;

        protected override void OnCreate()
        {
            base.OnCreate();

            _simGroup = World.GetExistingSystemManaged<SimulationSystemGroup>();

            // Singleton anlegen (einmalig)
            if (!SystemAPI.TryGetSingleton<TickrateConfig>(out _))
            {
                var e = EntityManager.CreateEntity(typeof(TickrateConfig));
                EntityManager.SetComponentData(e, new TickrateConfig { SimulationHz = 30f });
            }

            // Erste Anwendung
            Apply(SystemAPI.GetSingleton<TickrateConfig>().SimulationHz);
        }

        protected override void OnUpdate()
        {
            var desiredHz = SystemAPI.GetSingleton<TickrateConfig>().SimulationHz;
            if (!math.isfinite(desiredHz)) desiredHz = 30f;           // NaN/Inf abfangen
            desiredHz = math.clamp(desiredHz, 1f, 1000f);             // 1..1000 Hz

            if (math.abs(desiredHz - _appliedHz) > 0.001f)
                Apply(desiredHz);
        }

        void Apply(float hz)
        {
            // Manager nur erstellen/setzen, wenn nötig
            _manager = new RateUtils.FixedRateSimpleManager(1f / hz);
            _simGroup.RateManager = _manager;
            _appliedHz = hz;
        }

        protected override void OnDestroy()
        {
            // Beim Teardown aufräumen (wichtig für Editor/Playmode-Wechsel)
            if (_simGroup != null)
                _simGroup.RateManager = null;

            _manager = null;
            _simGroup = null;
        }
    }

    public struct TickrateConfig : IComponentData
    {
        public float SimulationHz;
    }
}