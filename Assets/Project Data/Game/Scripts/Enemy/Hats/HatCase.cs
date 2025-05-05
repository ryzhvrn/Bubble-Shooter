using UnityEngine;

namespace Watermelon.BubbleShooter
{
    [System.Serializable]
    public class HatCase
    {
        [SerializeField] HatType hatType;
        public HatType HatType => hatType;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;
    }
}