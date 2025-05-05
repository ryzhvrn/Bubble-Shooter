using UnityEngine;
using UnityEngine.Events;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class InteractableButtonBehaviour : MonoBehaviour, IHittable
    {
        private static readonly Vector3 ACTIVE_POSITION = new Vector3(0, -0.12f, 0);

        [SerializeField] Transform buttonTransform;

        [SerializeField] UnityEvent onButtonActivated;

        public Rigidbody ObjectRigidbody => null;
        public bool IsHittableObjectActive => true;

        private bool isButtonClicked;

        public void OnObjectHitted(Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
            if (isButtonClicked)
                return;

            isButtonClicked = true;

            buttonTransform.DOLocalMove(ACTIVE_POSITION, 0.4f);
            AudioController.PlaySound(AudioController.Sounds.wallButton);

            LevelController.SpawnFoam(transform.position, 1.2f, 1.8f, transform);

            onButtonActivated?.Invoke();
        }
    }
}