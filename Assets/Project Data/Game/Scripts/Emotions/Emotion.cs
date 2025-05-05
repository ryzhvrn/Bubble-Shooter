using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [System.Serializable]
    public class Emotion
    {
        [SerializeField] Type emotionType;
        public Type EmotionType => emotionType;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        private Pool pool;
        public Pool Pool => pool;

        public void Init()
        {
            pool = new Pool(new PoolSettings(string.Format("Emotion_{0}", emotionType.ToString()), prefab, 1, true, null));
        }

        public enum Type
        {
            Happy = 0,
            Angry = 1,
            Poop = 2,
            Sad = 3,
            Cry = 4,
            Dead = 5
        }
    }
}