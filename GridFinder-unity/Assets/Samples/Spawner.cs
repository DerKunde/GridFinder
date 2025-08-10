using GridPF.Visualization;
using UnityEngine;
using Visualization;

namespace Samples
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private GridTextureVisualizer visualizer;
        private Grid2D grid;

        void Start()
        {
            grid = new Grid2D(128, 96, new Vector2(-64, -48), 0.5f); // 64×48 Einheiten groß, Ursprung mittig links unten

            // ein paar Hindernisse & Kosten
            for (int x = 20; x < 40; x++)
                grid.SetBlocked(x, 40, true);

            for (int y = 10; y < 30; y++)
                grid.SetCost(80, y, 40); // teurer Streifen

            visualizer.Attach(grid);
            visualizer.SetMode(GridVisMode.CostHeatmap);
            visualizer.SetOpacity(0.85f);
            visualizer.SetDrawOnXZ(true); // auf dem Boden

            // später irgendwo: Teilbereich aktualisieren
            // visualizer.MarkDirty(new RectInt(78, 8, 6, 24));
        }
    }
}