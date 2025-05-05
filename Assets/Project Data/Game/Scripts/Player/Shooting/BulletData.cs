using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public struct BulletData
    {
        [SerializeField] int damage;
        public int Damage => damage;

        [SerializeField] OnTargetHittedCallback onTagetHitted;
        public OnTargetHittedCallback OnTargetHitted => onTagetHitted;

        public BulletData(int damage, OnTargetHittedCallback onTagetHitted)
        {
            this.damage = damage;
            this.onTagetHitted = onTagetHitted;
        }
    }

    public delegate void OnTargetHittedCallback(IHittable hittable);
}
