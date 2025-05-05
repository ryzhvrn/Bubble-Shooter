using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public class BarrierObstacle : BaseEnemy
    {
        [Header("Settings")]
        [SerializeField] bool spawnFoamOnHit = true;

        [Header("Explostion")]
        [SerializeField] Transform explosionPoint;
        [SerializeField] float explosionForce;
        [SerializeField] float explosionRadius;

        [Header("References")]
        [SerializeField] Rigidbody[] obstacleRigidbodies;
        [SerializeField] HitPoint[] hitPoints;
        [SerializeField] GameObject invisibleCollider;


        private TargetBehaviour activeTargetBehaviour;
        private Transform targetTransform;

        private int hitPointIndex = 0;

        protected void Awake()
        {
            for (int i = 0; i < hitPoints.Length; i++)
            {
                hitPoints[i].Init(this);
                hitPoints[i].ObjectCollider.enabled = false;
                invisibleCollider.SetActive(true);
            }

            hitPoints.Shuffle();
        }

        private void DisableActiveBodyPart()
        {
            hitPoints[hitPointIndex].ObjectCollider.enabled = false;

            DisableTarget();
        }

        private void EnableActiveBodyPart()
        {
            hitPoints[hitPointIndex].ObjectCollider.enabled = true;
            targetTransform = hitPoints[hitPointIndex].transform;

            EnableTarget();
        }

        public override void OnCombatStart()
        {
            EnableActiveBodyPart();
        }

        public override void OnHitPointTriggered(Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
            if (isDead)
                return;

            // Disable previous body part
            DisableActiveBodyPart();

            if (spawnFoamOnHit)
                hitPoints[hitPointIndex].SpawnFoam();

            hitPointIndex++;

            if (hitPointIndex == hitPoints.Length)
            {
                Die();
            }
            else
            {
                EnableActiveBodyPart();
            }
        }

        public override void Die()
        {
            if (isDead)
                return;

            isDead = true;

            invisibleCollider.SetActive(false);

            // Destroy
            for (int i = 0; i < obstacleRigidbodies.Length; i++)
            {
                obstacleRigidbodies[i].isKinematic = false;
                obstacleRigidbodies[i].AddExplosionForce(explosionForce, explosionPoint.position, explosionRadius);
                obstacleRigidbodies[i].AddRelativeTorque((obstacleRigidbodies[i].transform.position - explosionPoint.position).normalized * 200);
            }

            OnEnemyDied?.Invoke();
        }


        #region Target
        public void EnableTarget()
        {
            activeTargetBehaviour = LevelController.SpawnTarget(targetTransform, 0.01f);
        }

        public void DisableTarget()
        {
            if (activeTargetBehaviour != null)
            {
                activeTargetBehaviour.Disable();
                activeTargetBehaviour = null;
            }
        }

        public void DisableTargetImmediately()
        {
            if (activeTargetBehaviour != null)
            {
                activeTargetBehaviour.DisableImmediately();
                activeTargetBehaviour = null;
            }
        }
        #endregion

    }
}