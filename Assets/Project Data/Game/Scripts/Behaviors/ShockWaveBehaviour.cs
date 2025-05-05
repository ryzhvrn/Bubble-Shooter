using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class ShockWaveBehaviour : MonoBehaviour
    {
        private Collider objectCollider;
        private Rigidbody objectRigidbody;

        public void Init()
        {
            objectCollider = GetComponent<Collider>();
            objectRigidbody = GetComponent<Rigidbody>();
        }

        public void Explode(Transform directionTransform)
        {
            gameObject.SetActive(true);

            transform.position = directionTransform.position;
            transform.rotation = directionTransform.rotation;

            transform.DOMove(transform.position + transform.forward * 40, 0.5f).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                BodyHitPoint characterBodyPart = other.GetComponent<BodyHitPoint>();
                if (characterBodyPart != null)
                {
                    CharacterEnemy enemy = (CharacterEnemy)characterBodyPart.Enemy;
                    if (!enemy.IsDead)
                    {
                        enemy.Die();

                        Tween.NextFrame(delegate
                        {
                            enemy.AddForceToBodyParts(transform.forward.normalized, 5000);

                            for (int i = 0; i < enemy.BodyParts.Length; i++)
                            {
                                LevelController.SpawnFoam(enemy.BodyParts[i].transform.position, 1.2f, 1.8f, enemy.BodyParts[i].transform);
                            }
                        }, updateMethod: TweenType.FixedUpdate);
                    }
                }
            }
            else if (other.gameObject.layer == PhysicsHelper.LAYER_INTERACTABLE)
            {
                Rigidbody interactableRigidbody = other.GetComponent<Rigidbody>();
                if (interactableRigidbody != null)
                {
                    interactableRigidbody.AddForce(transform.forward.normalized * 1000);
                }
            }
        }
    }
}