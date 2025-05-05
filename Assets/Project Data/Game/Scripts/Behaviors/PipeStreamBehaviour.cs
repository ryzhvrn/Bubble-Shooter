using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class PipeStreamBehaviour : MonoBehaviour
    {
        [SerializeField] ParticleSystem splashParticleSystem;

        [Space]
        [SerializeField] ShockWaveBehaviour shockWaveBehaviour;

        private bool isActive;

        public void Activate()
        {
            if (isActive)
                return;

            isActive = true;
            

            Tween.DelayedCall(0.6f, delegate
            {
                AudioController.PlaySound(AudioController.Sounds.explostion, 0.2f);
                splashParticleSystem.Play();
                splashParticleSystem.transform.DOScale(0.3f, 2.0f).SetEasing(Ease.Type.QuadOut);

                shockWaveBehaviour.Explode(splashParticleSystem.transform);

                isActive = false;
            });
        }
    }
}