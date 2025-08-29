using System;
using GridFinder.Runtime.Grid;
using GridFinder.Runtime.Grid.Core;
using GridFinder.Runtime.Mono;
using R3;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.EventSystems;

namespace GridFinder.Samples
{
    [DisallowMultipleComponent]
    public class SampleGridController : MonoBehaviour
    {
        [Header("Grid")]
        [Min(2)] public int cols = 32;
        [Min(2)] public int rows = 32;
        public int chunkSize = 8;
        [Min(0.1f)] public float cellSize = 1f;
        public float zOffset = 0f; // Grid liegt in X/Y bei Z=zOffset
        public GridData Grid { get; private set; }

        [Header("Selection")]
        public Color startColor = new Color(0.2f, 0.9f, 0.2f, 1f);
        public Color goalColor  = new Color(0.9f, 0.2f, 0.2f, 1f);

        public int2 StartCell { get; private set; }
        public int2 GoalCell  { get; private set; }

        public event Action<int2,int2> OnStartGoalChanged;
        public GridData CurrentGrid { get; private set; }

        public ReactiveProperty<GridData> _gridCreated = new ReactiveProperty<GridData>();
        private readonly Subject<(int2 cords, Cell cell)> _cellChangedSubject = new();


        Camera _cam;
        Material _markerMat;

        void Awake()
        {
            _markerMat = new Material(Shader.Find("Sprites/Default"));
            StartCell = new int2(0, 0);
            GoalCell = new int2(cols - 1, rows - 1);
        }

        void Start()
        {
            CreateGrid(cols, rows);
        }
        

        public void CreateGrid(int width, int height)
        {
            CurrentGrid = GridFactory.CreateUniform(width, height, chunkSize, Cell.Default);
            _gridCreated.OnNext(CurrentGrid);
        }

// Read-only Stream für Kamera & Co.
        public Observable<(int, int)> ObserveGridDimensions()
        {
            // _gridCreated ist deine ReactiveProperty<GridData>
            return _gridCreated
                .Select(g => (g.Width, g.Height))
                .DistinctUntilChanged();
        }
        
        public bool TrySetWalkable(int2 c)
        {
            if (!InBounds(c)) return false;

            var cell = CurrentGrid.GetCell(c.x, c.y); // passe an, falls deine API anders heißt
            cell = Cell.WalkableCell;                  // ggf. IsWalkable/Flags → hier anpassen
            CurrentGrid.SetCell(c.x, c.y, cell);
            
            _cellChangedSubject.OnNext((c, cell));                    // Renderer/Listener informieren
            return true;
        }
        
        public bool TrySetUnwalkable(int2 c)
        {
            if (!InBounds(c)) return false;

            var cell = CurrentGrid.GetCell(c.x, c.y); // passe an, falls deine API anders heißt
            cell = Cell.UnwalkableCell;                  // ggf. IsWalkable/Flags → hier anpassen
            CurrentGrid.SetCell(c.x, c.y, cell);
            
            _cellChangedSubject.OnNext((c, cell));                    // Renderer/Listener informieren
            return true;
        }

        public void SetTarget(int2 target)
        {
            GoalCell = target;
                        OnStartGoalChanged?.Invoke(StartCell, GoalCell);

        }

        public void SetSpawnPoint(int2 start)
        {
            if (!InBounds(start)) return;

            StartCell = start;
            var worldPos = new float3(
                start.x * cellSize + cellSize * 0.5f,
                start.y * cellSize + cellSize * 0.5f,
                zOffset
            );
            SetSpawnOnAgentSpawners(worldPos);

            OnStartGoalChanged?.Invoke(StartCell, GoalCell);
        }

        public Observable<(int2 cords, Cell cell)> _cellChanged()
        {
            return _cellChangedSubject.AsObservable();
        }
        
        
        void SetSpawnOnAgentSpawners(Vector3 worldPos)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var em = world.EntityManager;
            var q = em.CreateEntityQuery(ComponentType.ReadWrite<AgentSpawner>());

            using var ents = q.ToEntityArray(Allocator.Temp);
            foreach (var e in ents)
            {
                var s = em.GetComponentData<AgentSpawner>(e);
                s.SpawnPosition = new float3(worldPos.x, worldPos.y, worldPos.z);
                em.SetComponentData(e, s);
            }
        }

        private bool InBounds(int2 c) =>
            CurrentGrid != null &&
            c.x >= 0 && c.x < CurrentGrid.Width &&
            c.y >= 0 && c.y < CurrentGrid.Height;
    }
}
