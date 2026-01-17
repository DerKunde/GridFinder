using R3;
using Unity.Mathematics;
using UnityEngine;
using GridFinder.Grid;

namespace GridFinder.Visuals
{
    public sealed class HoveredCellHighlighter : MonoBehaviour
    {
        [SerializeField] private GridConfig grid = null!;
        [SerializeField] private Input.GridPointerInput input = null!;
        [SerializeField] private Transform highlight; // assign a quad/plane transform
        [SerializeField] private float yOffset = 0.01f;
        [SerializeField] private float inset = 0.95f; // slightly smaller than cell

        private readonly CompositeDisposable d = new();

        void OnEnable()
        {
            if (!grid || input == null || !highlight)
                return;

            input!.HoveredCell
                .Subscribe(cell =>
                {
                    if (cell.x == int.MinValue) return;

                    var pos = grid.CellToWorldCenter(cell, grid.originWorld.y + yOffset);
                    highlight.position = (Vector3)pos;

                    var s = grid.cellSize * inset;
                    highlight.localScale = new Vector3(s, s, 1f);
                })
                .AddTo(d);
        }

        void OnDisable() => d.Clear();
    }
}