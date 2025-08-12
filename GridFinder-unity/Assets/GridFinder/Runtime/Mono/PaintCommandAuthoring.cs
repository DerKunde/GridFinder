using UnityEngine;

namespace GridFinder.Runtime.Mono
{
    public class PaintCommandAuthoring : MonoBehaviour
    {
        public Vector2Int min;
        public Vector2Int max;
        public ushort zoneId;
        public byte colorIdx;

        
    }
    
    protected class Baker : Baker<PaintCommandAuthoring>
    {
    
    }
}