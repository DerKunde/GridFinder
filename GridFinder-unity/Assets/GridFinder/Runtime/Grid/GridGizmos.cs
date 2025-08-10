using UnityEngine;

namespace GridFinder.Runtime.Grid
{
    [ExecuteAlways, RequireComponent(typeof(GridRuntime))]
    public class GridGizmos : MonoBehaviour {
        public Color lineColor = new(1,1,1,0.1f);
        void OnDrawGizmos() {
            var g = GetComponent<GridRuntime>();
            if (g == null) return;
            Gizmos.color = lineColor;
            var s = g.CellSize;
            var o = g.Settings.origin;
            for (int y=0; y<=g.Rows; y++) {
                var z0 = o + new Vector3(0,0,y*s);
                var z1 = o + new Vector3(g.Columns*s,0,y*s);
                Gizmos.DrawLine(z0, z1);
            }
            for (int x=0; x<=g.Columns; x++) {
                var x0 = o + new Vector3(x*s,0,0);
                var x1 = o + new Vector3(x*s,0,g.Rows*s);
                Gizmos.DrawLine(x0, x1);
            }
        }
    }
}