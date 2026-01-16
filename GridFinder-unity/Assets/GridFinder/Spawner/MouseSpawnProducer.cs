using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Spawner
{
    public class MouseSpawnProducer : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera cam;

        private EntityManager em;
        private EntityQuery q;

        void Start()
        {
            em = World.DefaultGameObjectInjectionWorld.EntityManager;
            q = em.CreateEntityQuery(
                ComponentType.ReadOnly<SpawnPrefab>(),
                ComponentType.ReadWrite<SpawnRequest>());
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.K))
                return;

            if (!cam) cam = UnityEngine.Camera.main;
            if (!cam)
                return;

            var ray = cam.ScreenPointToRay(Input.mousePosition);

            // XY-plane in world space: z = 0  -> plane normal is +Z, plane passes through origin
            var plane = new Plane(Vector3.forward, Vector3.zero);

            if (!plane.Raycast(ray, out var distance))
                return;

            var worldPoint = ray.GetPoint(distance);

            var singleton = q.GetSingletonEntity();
            var buffer = em.GetBuffer<SpawnRequest>(singleton);

            buffer.Add(new SpawnRequest
            {
                Position = new float3(worldPoint.x, worldPoint.y, 0f),
                Rotation = quaternion.identity
            });
        }
    }
}