using UnityEngine;

namespace GridFinder.Spawner
{
    [CreateAssetMenu(menuName = "GridFinder/Prefab Registry")]
    public sealed class PrefabRegistryAsset : ScriptableObject
    {
        public PrefabEntry[] Entries;
    }
}