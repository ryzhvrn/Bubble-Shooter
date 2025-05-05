using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public class WarningBehaviour : MonoBehaviour
    {
        private Transform lookAtTarget;

        public void Init()
        {
            lookAtTarget = PlayerController.PlayerTransform;
        }

        private void FixedUpdate()
        {
            transform.LookAt(new Vector3(lookAtTarget.position.x, transform.position.y, lookAtTarget.position.z));
        }
    }
}