using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public class EmotionsController : MonoBehaviour
    {
        [SerializeField] Emotion[] emotions;
        private static Dictionary<Emotion.Type, Emotion> emotionsLink;

        private void Awake()
        {
            emotionsLink = new Dictionary<Emotion.Type, Emotion>();
            for (int i = 0; i < emotions.Length; i++)
            {
                emotionsLink.Add(emotions[i].EmotionType, emotions[i]);

                emotions[i].Init();
            }
        }

        public static ParticleSystem SpawnEmotion(Emotion.Type emotionType, Vector3 position, float scale = 1.0f)
        {
            if (emotionsLink.ContainsKey(emotionType))
            {
                GameObject emotionObject = emotionsLink[emotionType].Pool.GetPooledObject();
                emotionObject.transform.position = position;
                emotionObject.transform.localScale = scale.ToVector3();
                emotionObject.SetActive(true);

                ParticleSystem emotionParticle = emotionObject.GetComponent<ParticleSystem>();
                emotionParticle.Play();

                return emotionParticle;
            }

            return null;
        }
    }
}