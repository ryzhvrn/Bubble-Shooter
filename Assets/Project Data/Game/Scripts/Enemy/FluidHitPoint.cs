using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class FluidHitPoint : HitPoint
    {
        [SerializeField] ParticleSystem streamParticleSystem;
        
        public override void Init(BaseEnemy enemy)
        {
            base.Init(enemy);

            isHittableObjectActive = true;
            streamParticleSystem.Play();
        }

        public override void OnObjectHitted(Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
            if (!isHittableObjectActive)
                return;

            isHittableObjectActive = false;

            LevelController.SpawnFoam(transform.position, 1.1f, 1.3f);

            streamParticleSystem.transform.DOScale(0.3f, 1.0f).SetEasing(Ease.Type.QuadOut).OnComplete(delegate
            {
                streamParticleSystem.Stop();
                streamParticleSystem.gameObject.SetActive(false);
            });

            SpawnFoam();

            enemy?.OnHitPointTriggered(direction, hitPosition, bulletData);
        }
    }
}