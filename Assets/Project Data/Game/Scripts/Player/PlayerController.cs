using Cinemachine;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour, IHittable
    {
        private static readonly int ANIMATION_RUNNING_BOOL_HASH = Animator.StringToHash("IsRunning");
        private static readonly int ANIMATION_DEAD_HASH = Animator.StringToHash("Dead");

        private static PlayerController playerController;

        [SerializeField] float movementSpeed = 3.0f;

        [Space(5f)]
        [SerializeField] Animator movementAnimator;

        private Vector3 movementTargetPoint;

        private static Transform playerTransform;
        public static Transform PlayerTransform => playerTransform;

        private Rigidbody playerRigidbody;
        public Rigidbody ObjectRigidbody => playerRigidbody;

        public bool IsHittableObjectActive => true;

        private bool isActive;
        private static bool isMovementEnabled;
        public static bool IsMovementEnabled => isMovementEnabled;

        private Vector3[] currentPath;
        private int nextPathPointIndex;
        private Vector3 movementVector;

        public void Init()
        {
            playerController = this;
            playerTransform = transform;

            playerRigidbody = GetComponent<Rigidbody>();

            isActive = false;
        }

        public void OnStageReached(LevelStage stage)
        {
            transform.position = stage.PlayerStartPosition.position;
            transform.rotation = stage.PlayerStartPosition.rotation;

            SetMovementState(false);
        }

        public void MoveAlongPath(Vector3[] path)
        {
            currentPath = path;
            nextPathPointIndex = 0;
            movementVector = transform.forward;

            AssignNextMovementTargetPoint();
            SetMovementState(true);
        }

        public void Activate()
        {
            isActive = true;
        }

        private void FixedUpdate()
        {
            if (!isMovementEnabled)
                return;

            Vector3 oldPosition = transform.position;

            transform.position = Vector3.MoveTowards(transform.position, movementTargetPoint, movementSpeed * Time.fixedDeltaTime);

            movementVector = transform.position - oldPosition;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movementVector, Vector3.up), 0.1f);

            if (Vector3.Distance(transform.position, movementTargetPoint) < 0.01f)
            {
                AssignNextMovementTargetPoint();
            }
        }

        private void AssignNextMovementTargetPoint()
        {
            if (nextPathPointIndex + 1 < currentPath.Length)
            {
                nextPathPointIndex++;
                movementTargetPoint = currentPath[nextPathPointIndex];
            }
            else
            {
                SetMovementState(false);

                transform.DOMove(LevelController.NextStage.PlayerStartPosition.position, 0.2f);
                transform.DORotate(LevelController.NextStage.PlayerStartPosition.rotation, 0.2f).OnComplete(() =>
                {
                    LevelController.OnPlayerReachedNextStage();
                });
            }
        }

        public void OnObjectHitted(Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
            Die();
        }

        public static void SetMovementState(bool state)
        {
            isMovementEnabled = state;

            ShootingController.ResetGunRotation();
            playerController.movementAnimator.SetBool(ANIMATION_RUNNING_BOOL_HASH, state);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                BaseEnemy enemy = other.GetComponent<BaseEnemy>();

                if (enemy != null && !enemy.IsDead)
                {
                    Die();
                }
            }
        }

        public static void OnEnemyTooClose()
        {
            playerController.Die();
        }

        private void Die()
        {
            if (!isActive || !GameController.IsGameActive)
                return;

            isActive = false;

            movementAnimator.Play(ANIMATION_DEAD_HASH);

            SetMovementState(false);
            ShootingController.SetShootingState(false);
            GameController.LevelFailed();
        }

        public static PlayerController GetPlayer()
        {
            return playerController;
        }

    }
}