using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public abstract class BaseEnemy : MonoBehaviour
    {
        protected bool isDead = false;
        public bool IsDead => isDead;

        public SimpleCallback OnEnemyDied;

        public abstract void Die();
        public abstract void OnCombatStart();

        public abstract void OnHitPointTriggered(Vector3 direction, Vector3 hitPosition, BulletData bulletData);
    }
}