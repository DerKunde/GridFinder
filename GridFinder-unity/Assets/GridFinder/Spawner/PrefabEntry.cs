using Unity.Entities;
using UnityEngine;

namespace GridFinder.Spawner
{
    [CreateAssetMenu(menuName = "GridFinder/Prefab Entry")]
    public sealed class PrefabEntry : ScriptableObject
    {
        [Tooltip("Stable, never-changing ID")]
        public int Id;

        public GameObject Prefab;
    }
}