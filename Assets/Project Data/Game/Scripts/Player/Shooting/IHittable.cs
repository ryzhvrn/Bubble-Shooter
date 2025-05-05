using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public interface IHittable
    {
        GameObject gameObject { get; }
        Transform transform { get; }

        Rigidbody ObjectRigidbody { get; }

        bool IsHittableObjectActive { get; }

        void OnObjectHitted(Vector3 direction, Vector3 hitPosition, BulletData bulletData);

        public delegate void OnObjectDiedCallback();
    }
}