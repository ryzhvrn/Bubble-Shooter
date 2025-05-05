using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class GunBehaviour : MonoBehaviour
    {
        [SerializeField] Transform shootPoint;
        public Transform ShootPoint => shootPoint;

        [SerializeField] ParticleSystem shootParticleSystem;

        private Rigidbody objectRigidbody;
        public Rigidbody ObjectRigidbody => objectRigidbody;

        private Collider objectCollider;
        public Collider ObjectCollider => objectCollider;

        private GunData data;

        private void Awake()
        {
            objectRigidbody = GetComponent<Rigidbody>();
            objectCollider = GetComponent<Collider>();
        }

        public void Init(GunData data, int layer)
        {
            this.data = data;

            SetPhysicsState(false);

            // Change gun layer
            gameObject.layer = layer;

            int childCount = gameObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.layer = layer;
            }
        }

        public void Shoot(Vector3 direction, bool isPlayerShooting)
        {
            if (!GameController.IsGameActive)
                return;

            GameObject bulletObject = data.BulletsPool.GetPooledObject();
            bulletObject.transform.position = shootPoint.transform.position;
            bulletObject.transform.forward = direction;

            ProjectileBehaviour projectileBehaviour = bulletObject.GetComponent<ProjectileBehaviour>();
            projectileBehaviour.Shoot(data, new BulletData(data.Damage, null), direction.normalized, data.BulletSpeed, isPlayerShooting);

            shootParticleSystem.Play();
            AudioController.PlaySound(AudioController.Sounds.shoot);
        }

        public void SetPhysicsState(bool state)
        {
            objectRigidbody.isKinematic = !state;
            objectCollider.enabled = state;
        }


    }
}