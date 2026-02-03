using GridFinder.Grid;
using GridFinder.GridInput;
using Reflex.Attributes;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.GridInput
{
    public sealed class GridPaintWalkableBridge : MonoBehaviour
    {
        [Inject] private readonly GridPointerState pointer = null!;

        private EntityManager em;
        private EntityQuery q;

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            em = world.EntityManager;
            q = em.CreateEntityQuery(ComponentType.ReadOnly<GridEditQueueTag>());
        }

        private void Update()
        {
            if (pointer.IsDragging.Value)
                return;

            // On mouse up, apply edit (demo)
            if (!Input.GetMouseButtonUp(0))
                return;

            if (!pointer.HasHover.Value)
                return;

            var queueEntity = q.GetSingletonEntity();
            var buf = em.GetBuffer<GridEditCommand>(queueEntity);

            var a = pointer.DragStartCell.Value;
            var b = pointer.DragEndCell.Value;

            buf.Add(new GridEditCommand
            {
                Type = GridEditType.SetWalkable,
                Min = new int2(math.min(a.x, b.x), math.min(a.y, b.y)),
                Max = new int2(math.max(a.x, b.x), math.max(a.y, b.y)),
                Value = 0 // not walkable
            });
        }
    }
}