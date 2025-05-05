using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class TargetBehaviour : MonoBehaviour
    {
        [SerializeField] Image targetImage;

        private Transform lookAtTarget;

        private void Awake()
        {
            targetImage.color = targetImage.color.SetAlpha(0.0f);
        }

        public void SetTarget(Transform target)
        {
            transform.SetParent(target);
            transform.localPosition = Vector3.zero;
        }

        public void Enable()
        {
            lookAtTarget = PlayerController.PlayerTransform;

            targetImage.color = targetImage.color.SetAlpha(0.0f);
            targetImage.DOFade(1.0f, 0.4f, 0, true);
        }

        public void Disable()
        {
            targetImage.DOFade(0.0f, 0.4f, 0, true).OnComplete(delegate
            {
                // Return target to pool
                transform.SetParent(null);
                gameObject.SetActive(false);
            });
        }

        public void DisableImmediately()
        {
            // Return target to pool
            transform.SetParent(null);
            gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            transform.LookAt(new Vector3(lookAtTarget.position.x, transform.position.y, lookAtTarget.position.z));
        }
    }
}