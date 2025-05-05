using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public class FluidObstacle : BaseEnemy
    {
        private List<FluidHitPoint> hitPoints = new List<FluidHitPoint>();

        private int pointsHited = 0;

        private void Awake()
        {
            isDead = true;
            hitPoints.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                FluidHitPoint point = transform.GetChild(i).GetComponent<FluidHitPoint>();

                if (point != null)
                {
                    hitPoints.Add(point);
                }
            }

            for (int i = 0; i < hitPoints.Count; i++)
            {
                hitPoints[i].Init(this);
            }

            pointsHited = 0;
        }

        public override void OnCombatStart()
        {
            isDead = false;
        }

        public override void OnHitPointTriggered(Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
            if (isDead)
                return;

            pointsHited++;

            if (pointsHited >= hitPoints.Count)
            {
                Die();
            }
        }

        public override void Die()
        {
            if (isDead)
                return;

            isDead = true;

            OnEnemyDied?.Invoke();
        }
    }
}