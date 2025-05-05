using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class ShootingController : MonoBehaviour
    {
        private static ShootingController shootingController;

        [SerializeField] Camera fpsCamera;
        [SerializeField] Transform gunHolderContainer;
        [SerializeField] Transform handContainer;

        [Space]
        [SerializeField] GunData gunData;

        private GunBehaviour gunBehaviour;
        private TweenCase gunMovementTweenCase;

        private Quaternion targetHandRotation = Quaternion.identity;

        private static bool isShootingActive = false;
        public static bool IsShootingActive => isShootingActive;

        private void Awake()
        {
            shootingController = this;
        }

        private void Start()
        {
            InitGun((GunSkinProduct)StoreController.GetSelectedProduct(StoreProductType.GunSkin));

            StoreController.OnProductSelected += OnProductSelected;
        }

        private void OnDisable()
        {
            StoreController.OnProductSelected -= OnProductSelected;
        }

        private void OnProductSelected(StoreProduct product)
        {
            if (product.Type == StoreProductType.GunSkin)
            {
                InitGun((GunSkinProduct)StoreController.GetSelectedProduct(StoreProductType.GunSkin));
            }
        }

        public void InitGun(GunSkinProduct gunProduct)
        {
            // Destroy previous gun
            if (gunBehaviour != null)
            {
                Destroy(gunBehaviour.gameObject);
            }

            gunBehaviour = Instantiate(gunProduct.GunData.GunPrefab).GetComponent<GunBehaviour>();
            gunBehaviour.transform.SetParent(gunHolderContainer);
            gunBehaviour.transform.ResetLocal();

            gunBehaviour.Init(gunProduct.GunData, PhysicsHelper.LAYER_INTERACTABLE);
        }

        private void Update()
        {
            if (isShootingActive)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 hitPoint;
                    RaycastHit hit;
                    Ray ray = fpsCamera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.SphereCast(ray, 1.0f, out hit, 200))
                    {
                        if (hit.collider.CompareTag(PhysicsHelper.TAG_ENEMY) && hit.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                        {
                            BodyHitPoint bodyPart = hit.collider.GetComponent<BodyHitPoint>();
                            if (bodyPart != null)
                                bodyPart.CharacterEnemy.OnEnemyClicked();
                        }

                        hitPoint = hit.point;

                        targetHandRotation = Quaternion.LookRotation(hitPoint - Camera.main.transform.position);

                        gunBehaviour.Shoot(hitPoint - gunBehaviour.ShootPoint.position, true);

                        // Reset gun movement tween
                        if (gunMovementTweenCase != null && !gunMovementTweenCase.isCompleted)
                        {
                            gunMovementTweenCase.Kill();
                        }

                        gunMovementTweenCase = handContainer.DOLocalMoveZ(-0.13f, 0.1f).OnComplete(delegate
                        {
                            gunMovementTweenCase = handContainer.DOLocalMoveZ(0.03f, 0.05f);
                        });
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (isShootingActive)
                handContainer.rotation = targetHandRotation;
        }

        public static void SetShootingState(bool state)
        {
            isShootingActive = state;

            if (state)
            {
                StoreController.OnProductSelected -= shootingController.OnProductSelected;
            }
        }

        public static void ResetGunRotation()
        {
            shootingController.targetHandRotation = shootingController.transform.rotation;
        }
    }
}