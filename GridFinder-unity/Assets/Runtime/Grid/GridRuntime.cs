using Unity.Collections;
using UnityEngine;

namespace Runtime.Grid
{
    public class GridRuntime : MonoBehaviour
    {
        [SerializeField] private GridSettings settings;

        public GridSettings Settings => settings;

        private NativeArray<byte> occupancy;
        
        public int Columns => settings.columns;
        public int Rows => settings.rows;
        public int Layers => settings.layers;
        public float CellSize => settings.cellSize;
        public int CellCount => Columns * Rows * Layers;
        
        int Index(int x,int y,int z) => (z * Rows + y) * Columns + x;
        
        public bool InBounds(int x,int y,int z)
            => x>=0 && x<Columns && y>=0 && y<Rows && z>=0 && z<Layers;

        public Vector3 WorldFromCell(int x,int y,int z) 
        {
            var o = settings.origin;
            return new Vector3(
                o.x + (x + 0.5f) * CellSize,
                o.y + z * CellSize,            // Layer auf Y oder Z verschieben? frei wählbar
                o.z + (y + 0.5f) * CellSize);
        }

        public bool CellFromWorld(Vector3 world, int layer, out int x, out int y) 
        {
            var local = world - settings.origin;
            x = Mathf.FloorToInt(local.x / CellSize);
            y = Mathf.FloorToInt(local.z / CellSize);
            return InBounds(x,y,layer);
        }

        public bool IsOccupied(int x,int y,int z) => occupancy[Index(x,y,z)] != 0;

        public bool TrySetOccupied(int x,int y,int z, bool occupied) {
            if (!InBounds(x,y,z)) return false;
            if (occupied && IsOccupied(x,y,z)) return false;
            occupancy[Index(x,y,z)] = (byte)(occupied ? 1 : 0);
            return true;
        }

        void OnEnable() 
        {
            occupancy = new NativeArray<byte>(CellCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }
        
        void OnDisable() 
        {
            if (occupancy.IsCreated) occupancy.Dispose();
        }
    }
}