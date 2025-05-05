using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class StickBehaviour : MonoBehaviour
    {
        private Rigidbody objectRigidbody;
        public Rigidbody ObjectRigidbody => objectRigidbody;

        public SimpleCallback OnObjectSticked;

        private CharacterEnemy characterEnemy;
        private Transform defaultParentTransform;
        private Collider objectCollider;

        private bool isSticked;

        public void Init(CharacterEnemy characterEnemy)
        {
            this.characterEnemy = characterEnemy;

            objectCollider = GetComponent<Collider>();
            objectRigidbody = GetComponent<Rigidbody>();
            objectRigidbody.isKinematic = false;

            defaultParentTransform = transform.parent;

            isSticked = false;
        }

        public void ResetParent()
        {
            transform.SetParent(characterEnemy.transform);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isSticked)
                return;

            if (other.gameObject.layer == PhysicsHelper.LAYER_ENVIRONMENT)
            {
                isSticked = true;

                objectCollider.enabled = false;
                objectRigidbody.isKinematic = true;

                LevelController.SpawnFoamSplash(transform.position, 1.0f);

                OnObjectSticked?.Invoke();
            }
        }
    }
}