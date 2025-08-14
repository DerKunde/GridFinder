using Unity.Entities;
using Unity.Mathematics;

namespace GridFinder.Timer
{
    [WorldSystemFilter((WorldSystemFilterFlags.Default))]
    public partial class SimulationTickrate : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            const float hz = 30f;

            var world = World.DefaultGameObjectInjectionWorld;
            var simGroup = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
            simGroup.RateManager = new RateUtils.FixedRateSimpleManager(1 / hz);
        }

        protected override void OnUpdate()
        {
        }
    }

    public struct TickrateConfig : IComponentData
    {
        public float SimulationHz;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial class TickrateApplierSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            // Default-Werte anlegen, falls noch nicht vorhanden
            if (!SystemAPI.TryGetSingleton<TickrateConfig>(out _))
                EntityManager.CreateEntity(typeof(TickrateConfig));
            EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<TickrateConfig>(),
                new TickrateConfig { SimulationHz = 30 });
        }

        protected override void OnUpdate()
        {
            var cfg = SystemAPI.GetSingleton<TickrateConfig>();
            var world = World.DefaultGameObjectInjectionWorld;

            var sim = world.GetExistingSystemManaged<SimulationSystemGroup>();

            sim.RateManager = new RateUtils.FixedRateSimpleManager(1f / math.max(1f, cfg.SimulationHz));
        }
    }
}