using UnityEngine;

namespace GridFinder.Grid
{
    public sealed class GridRoot : MonoBehaviour
    {
        [field: SerializeField] public Transform FloorTransform { get; private set; } = null!;
        [field: SerializeField] public Renderer FloorRenderer { get; private set; } = null!;
    }
}