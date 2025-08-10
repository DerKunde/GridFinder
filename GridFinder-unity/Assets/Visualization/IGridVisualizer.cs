using Runtime.Grid;
using UnityEngine;

namespace Visualization
{
    public interface IGridVisualizer
    {
        void Attach(IGridReadOnly grid);
        void Detach();
        void SetMode(GridVisMode mode);
        void SetOpacity(float alpha);             // 0..1
        void SetDrawOnXZ(bool onXZ);              // true: XZ-Ebene, false: XY
        void MarkDirty(RectInt rect);             // teilweises Update
        void MarkDirtyAll();                      // komplettes Rebuild
        bool IsAttached { get; }
    }
}