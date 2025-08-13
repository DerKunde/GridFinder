using UnityEngine;

namespace GridFinder.Runtime.Grid
{
    [CreateAssetMenu(menuName = "GridFinder/Grid Settings")]
    public class GridSettings : ScriptableObject
    {
        [Min(1)] public int columns = 50;
        [Min(1)] public int rows = 30;
        [Min(1)] public int layers = 1;
        [Min(0.01f)] public float cellSize = 1f;
        public Vector3 origin = Vector3.zero;
        public Color baseColor = new(0.2f, 0.2f, 0.2f, 0.6f);
        public Material cellMaterial; // unlit, instanced 
    }
}