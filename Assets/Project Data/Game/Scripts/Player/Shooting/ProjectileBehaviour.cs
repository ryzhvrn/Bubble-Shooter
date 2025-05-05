using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileBehaviour : MonoBehaviour
    {
        private BulletData bulletData;
        private Vector3 direction;

        private Rigidbody projectileRigidbody;
        private GunData gun;
        private TweenCase disableTweenCase;

        private bool isPlayerShooting;

        private void Awake()
        {
            projectileRigidbody = GetComponent<Rigidbody>();
        }

        public void Shoot(GunData gun, BulletData bulletData, Vector3 direction, float speed, bool isPlayerShooting)
        {
            this.gun = gun;
            this.bulletData = bulletData;

            this.direction = direction;
            this.isPlayerShooting = isPlayerShooting;

            transform.LookAt(transform.position + direction);

            projectileRigidbody.isKinematic = false;
            projectileRigidbody.velocity = Vector3.zero;

            projectileRigidbody.AddForce(direction * speed, ForceMode.Impulse);

            disableTweenCase = Tween.DelayedCall(5.0f, delegate
            {
                DisableProjectile();
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_INTERACTABLE || other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                // prevent player to shoot itself
                if (isPlayerShooting && other.CompareTag(PhysicsHelper.TAG_PLAYER))
                    return;

                IHittable hittable = other.gameObject.GetComponent<IHittable>();
                if (hittable != null)
                {
                    if (hittable.IsHittableObjectActive)
                        hittable.OnObjectHitted(transform.forward, transform.position, bulletData);
                }
            }

            SpawnSplash();

            DisableProjectile();
        }

        private void SpawnSplash()
        {
            GameObject hitSplash = gun.BulletSplashPool.GetPooledObject();
            hitSplash.transform.position = transform.position - direction * 2;
            hitSplash.transform.rotation = Quaternion.Euler(direction);
            hitSplash.transform.localScale = Vector3.one;
            hitSplash.SetActive(true);

            ParticleSystem splashParticle = hitSplash.GetComponent<ParticleSystem>();
            splashParticle.Play();

            Tween.DelayedCall(3.0f, delegate
            {
                hitSplash.SetActive(false);
            });
        }

        private void DisableProjectile()
        {
            if (disableTweenCase != null && !disableTweenCase.isCompleted)
                disableTweenCase.Kill();

            gameObject.SetActive(false);

            projectileRigidbody.velocity = Vector3.zero;
            projectileRigidbody.isKinematic = true;
        }
    }
}