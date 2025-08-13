using Unity.Burst;
using Unity.Entities;

namespace GridFinder.Structs
{
    [BurstCompile]
    public partial struct PaintRectSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<PaintCommand>(out var paint))
                return; // Kein Malbefehl vorhanden

            // Hier: Über alle betroffenen Chunks iterieren, Zellen anpassen
            // paint.Min.x .. paint.Max.x und paint.Min.y .. paint.Max.y
            // -> ZoneId = paint.ZoneId, ColorIdx = paint.ColorIdx

            // Optional: Befehl nach Ausführung löschen
            var entity = SystemAPI.GetSingletonEntity<PaintCommand>();
            state.EntityManager.DestroyEntity(entity);
        }
    }
}