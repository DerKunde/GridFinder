using GridFinder.Grid;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace GridFinder.Visuals
{
    public sealed class GridFeedbackFactory
    {
        private readonly GameObject prefab;
        private readonly GridRootFactory gridRootFactory;

        public GameObject? Instance { get; private set; }

        public GridFeedbackFactory(GameObject prefab, GridRootFactory gridRootFactory)
        {
            this.prefab = prefab;
            this.gridRootFactory = gridRootFactory;
        }

        public GameObject Create(Container container)
        {
            if (Instance != null)
                return Instance;

            // // Ensure GridRoot exists so we can parent under it (optional but recommended)
            // var gridRoot = gridRootFactory.Instance ?? gridRootFactory.Create(container);

            var go = Object.Instantiate(prefab);
            go.name = "GridFeedback";
            go.layer = LayerMask.NameToLayer("GridFloor");
            go.SetActive(false);

            GameObjectInjector.InjectRecursive(go, container);

            go.SetActive(true);
            Instance = go;
            return go;
        }
    }
}