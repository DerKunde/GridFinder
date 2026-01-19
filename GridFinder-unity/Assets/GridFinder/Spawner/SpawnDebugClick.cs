using Unity.Mathematics;
using UnityEngine;

namespace GridFinder.Spawner
{
    public sealed class SpawnDebugClick : MonoBehaviour
    {
        private SpawnCommandWriter _writer;
        private ISpawnCommandFactory _factory;

        private void Awake()
        {
            _writer = new SpawnCommandWriter(Unity.Entities.World.DefaultGameObjectInjectionWorld);
            _factory = new SpawnCommandFactory();
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            var ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit))
                return;

            var intent = new SpawnIntent(
                contentId: 0,
                worldPos: (float3)hit.point,
                worldRot: quaternion.identity,
                uniformScale: 1f,
                gridCellIndex: null
            );

            SpawnCommandData cmd;
            if (_factory.TryCreate(in intent, out cmd))
            {
                _writer.TryEnqueue(in cmd);
            }
        }
    }
}