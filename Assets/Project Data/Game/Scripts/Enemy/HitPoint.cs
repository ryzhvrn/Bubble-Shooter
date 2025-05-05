using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BubbleShooter
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class HitPoint : MonoBehaviour, IHittable
    {
        [SerializeField]
        protected Transform foamSpawnTransform;
        public Transform FoamSpawnTransform => foamSpawnTransform;

        protected Rigidbody objectRigidbody;
        public Rigidbody ObjectRigidbody => objectRigidbody;

        protected Collider objectCollider;
        public Collider ObjectCollider => objectCollider;

        protected bool isHittableObjectActive = true;
        public bool IsHittableObjectActive => isHittableObjectActive;

        protected BaseEnemy enemy;
        public BaseEnemy Enemy => enemy;

        protected FoamBehaviour foamBehaviour;
        public FoamBehaviour FoamBehaviour => foamBehaviour;


        public virtual void Init(BaseEnemy enemy)
        {
            this.enemy = enemy;

            objectRigidbody = GetComponent<Rigidbody>();
            objectCollider = GetComponent<Collider>();
        }

        public virtual void OnObjectHitted(Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
            enemy?.OnHitPointTriggered(direction, hitPosition, bulletData);
        }

        public virtual void SpawnFoam()
        {
            if (foamBehaviour == null)
            {
                foamBehaviour = LevelController.SpawnFoam(foamSpawnTransform.position, 1.3f, 1.5f, foamSpawnTransform);
            }
            else
            {
                foamBehaviour.Resize(1.2f);
            }
        }

    }
}