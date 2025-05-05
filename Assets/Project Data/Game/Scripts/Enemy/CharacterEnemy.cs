using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using Watermelon;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon.BubbleShooter
{


    [RequireComponent(typeof(Animator))]
    public class CharacterEnemy : BaseEnemy
    {
        // Consts
        private static readonly int ANIMATION_SHOOT_HASH = Animator.StringToHash("Shoot");
        private static readonly int ANIMATION_AIM_HASH = Animator.StringToHash("Aim");
        private static readonly int ANIMATION_RUN_HASH = Animator.StringToHash("Run");

        private static readonly int[] ANIMATION_HIT_HASHES = new int[] { Animator.StringToHash("Hit_01"), Animator.StringToHash("Hit_02"), Animator.StringToHash("Hit_03") };

        private static readonly Emotion.Type[] HIT_EMOTIONS = new Emotion.Type[] { Emotion.Type.Angry, Emotion.Type.Cry, Emotion.Type.Sad };

        // Inspector variables
        [Header("Health")]
        [SerializeField] int hitsToKill = 1;
        public int HitsToKill => hitsToKill;

        [Header("Behavior")]
        [SerializeField] BehaviorType behavior;
        public BehaviorType Behavior => behavior;

        [SerializeField] bool isRunning = false;
        public bool IsRunning => isRunning;

        [SerializeField, HideIf("HideRunningSettings")] float runningSpeed = 2.0f;
        public float RunningSpeed => runningSpeed;

        [Header("Aiming")]
        [SerializeField]
        protected bool enableAiming;
        public bool EnableAimingValue => enableAiming;

        [SerializeField, HideIf("HideAimingSettings")]
        protected DuoFloat minMaxAimDelay = new DuoFloat(0.1f, 1f);
        public DuoFloat MinMaxAimDelay => minMaxAimDelay;

        [Header("Warning")]
        [SerializeField] bool enableWarning = true;
        public bool EnableWarningValue => enableWarning;

        [Header("Physics")]
        [SerializeField] bool moveOnHit = true;
        public bool MoveOnHit => moveOnHit;

        [SerializeField] bool spawnFoamOnHit = true;
        public bool SpawnFoamOnHit => spawnFoamOnHit;

        [SerializeField] bool useRig = true;
        public bool UseRig => useRig;

        [Header("Emotions")]
        [SerializeField] bool showEmotionOnHit = true;
        public bool ShowEmotionOnHit => showEmotionOnHit;

        [SerializeField] bool showEmotionOnDead = false;
        public bool ShowEmotionOnDead => showEmotionOnDead;

        [Header("Target")]
        [SerializeField]
        protected bool enableTarget;
        public bool EnableTargetValue => enableTarget;

        [SerializeField, HideIf("HideTargetSettings")]
        protected bool showHandClickingTarget;
        public bool ShowHandClickingTarget => showHandClickingTarget;

        [SerializeField, HideIf("HideTargetSettings")]
        protected Transform targetTransform;

        [SerializeField, HideIf("HideTargetSettings")]
        protected float targetScale = 0.1f;
        public float TargetScale => targetScale;

        [Space(), LineSpacer("References")]
        [SerializeField] BodyHitPoint[] bodyParts;
        [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
        [SerializeField] Transform warningTransform;

        [Header("Rig")]
        [SerializeField] Rig mainRig;

        [SerializeField] TwoBoneIKConstraint rightHandIKConstraint;
        [SerializeField] Transform rightHandConstraintController;

        [SerializeField] TwoBoneIKConstraint leftHandIKConstraint;
        [SerializeField] Transform leftHandConstraintController;

        [SerializeField] TwoBoneIKConstraint rightLegIKConstraint;
        [SerializeField] Transform rightLegConstraintController;

        [SerializeField] TwoBoneIKConstraint leftLegIKConstraint;
        [SerializeField] Transform leftLegConstraintController;

        [Space()]
        [SerializeField] EnemyBaseScenario[] scenarios;
        [SerializeField] GunData selectedGun;
        [SerializeField] Transform gunHolderTransfrom;
        [SerializeField] Transform emotionTransform;
        [SerializeField] HatBehaviour hatBehaviour;


        // Variables
        private Animator characterAnimator;
        public Animator CharacterAnimator => characterAnimator;

        private TweenCase shootingDelayTweenCase;
        private Transform playerTransform;

        private GunBehaviour gunBehaviour;
        private ScenarioCase scenarioCase;
        private EnemyBaseScenario currentScenario;

        private MaterialPropertyBlock materialPropertyBlock;

        private int hitAnimationIndex = 0;
        private int health = 3;

        private bool isWarningEnabled;
        private bool isFirstEmotionsShown;
        private bool isCombatActive;
        private bool isAiming;
        private bool isCharacterSticked = false;
        private bool isGunKnockedOut = false;

        private TargetBehaviour activeTargetBehaviour;
        private Transform warningActiveTransform;
        private TweenCase hitMovementTweenCase;

        private Vector3 lastHitPosition;
        private BodyHitPoint lastBodyPart;

        public BodyHitPoint[] BodyParts => bodyParts;
        protected Dictionary<BodyHitPoint.Type, BodyHitPoint> bodyPartsLink;

        protected void Awake()
        {
            // Create instances
            materialPropertyBlock = new MaterialPropertyBlock();
            hitAnimationIndex = Random.Range(0, ANIMATION_HIT_HASHES.Length);
            health = hitsToKill;

            // Get components
            characterAnimator = GetComponent<Animator>();

            // Init body parts
            bodyPartsLink = new Dictionary<BodyHitPoint.Type, BodyHitPoint>();
            for (int i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].Init(this);

                if (bodyPartsLink.ContainsKey(bodyParts[i].BodyPartType))
                    bodyPartsLink.Add(bodyParts[i].BodyPartType, bodyParts[i]);
            }

            // Init gun
            if (selectedGun != null)
            {
                selectedGun.Init();
                gunBehaviour = Instantiate(selectedGun.GunPrefab).GetComponent<GunBehaviour>();
                gunBehaviour.transform.SetParent(gunHolderTransfrom);
                gunBehaviour.transform.ResetLocal();

                gunBehaviour.Init(selectedGun, PhysicsHelper.LAYER_INTERACTABLE);

                isGunKnockedOut = false;
            }
            else
            {
                isGunKnockedOut = true;
            }

            // Init hat
            hatBehaviour.Init((HatSkinProduct)StoreController.GetSelectedProduct(StoreProductType.HatSkin));

            StoreController.OnProductSelected += OnProductSelected;

            // Init scenario
            currentScenario = GetCurrentScenario();

            if (currentScenario != null)
                scenarioCase = currentScenario.Init(this);

            // Change physics state
            SetRegdollState(false);

            // Disable rig constraints
            rightHandIKConstraint.weight = 0.0f;
            leftHandIKConstraint.weight = 0.0f;
            rightLegIKConstraint.weight = 0.0f;
            leftLegIKConstraint.weight = 0.0f;

            isCombatActive = false;
            isDead = false;
        }

        private void OnProductSelected(StoreProduct product)
        {
            if (product.Type == StoreProductType.HatSkin)
            {
                hatBehaviour.Init((HatSkinProduct)StoreController.GetSelectedProduct(StoreProductType.HatSkin));
            }
        }

        private void OnDisable()
        {
            StoreController.OnProductSelected -= OnProductSelected;
        }



        #region Behavior

        public override void OnCombatStart()
        {
            StoreController.OnProductSelected -= OnProductSelected;

            if (isDead)
                return;

            if (currentScenario != null)
                currentScenario.InvokeScenario(scenarioCase);

            if (enableTarget)
            {
                if (showHandClickingTarget)
                {
                    EnableTargetWithHand();
                }
                else
                {
                    EnableTarget();
                }
            }

            if (enableAiming)
            {
                EnableAiming();
            }

            if (isRunning)
            {
                characterAnimator.SetBool(ANIMATION_RUN_HASH, true);
                characterAnimator.Play("Walking", 0, Random.Range(0.0f, 1.0f));
            }

            isCombatActive = true;
        }

        private void EnableAiming()
        {
            if (isAiming || isDead || isGunKnockedOut || isRunning)
                return;

            isAiming = true;
            float shootTime = 2f;

            playerTransform = PlayerController.PlayerTransform;

            Tween.DelayedCall(Random.Range(minMaxAimDelay.firstValue, minMaxAimDelay.secondValue), delegate
            {
                characterAnimator.SetBool(ANIMATION_AIM_HASH, true);

                shootingDelayTweenCase = Tween.DelayedCall(shootTime * 0.6f, delegate
                {
                    if (IsDead)
                        return;

                    EnableWarning();

                    shootingDelayTweenCase = Tween.DelayedCall(shootTime * 0.4f, delegate
                    {
                        Shoot();
                    });
                });
            });
        }

        private void DisableAiming()
        {
            if (isAiming)
            {
                if (shootingDelayTweenCase != null && !shootingDelayTweenCase.isCompleted)
                    shootingDelayTweenCase.Kill();

                if (isWarningEnabled)
                    DisableWarning();

                isAiming = false;
            }
        }

        private void FixedUpdate()
        {
            if (isWarningEnabled)
            {
                warningTransform.LookAt(playerTransform);
            }

            if (isRunning && !isDead && isCombatActive)
            {
                if (playerTransform == null)
                    playerTransform = PlayerController.PlayerTransform;

                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position.SetY(transform.position.y), Time.fixedDeltaTime * runningSpeed);
                transform.LookAt(playerTransform.position.SetY(transform.position.y));

                if (Vector3.Distance(transform.position, playerTransform.position) < 8f)
                {
                    PlayerController.OnEnemyTooClose();
                }
            }
        }

        private void OnDestroy()
        {
            if (shootingDelayTweenCase != null)
                shootingDelayTweenCase.Kill();
        }

        private void DropGun()
        {
            if (gunBehaviour != null)
            {
                isGunKnockedOut = true;

                gunBehaviour.transform.SetParent(null);
                gunBehaviour.SetPhysicsState(true);
                gunBehaviour.ObjectRigidbody.AddExplosionForce(1000f, lastHitPosition, 200);
            }
        }

        public override void Die()
        {
            if (isDead)
                return;

            isDead = true;

            DisableAiming();

            skinnedMeshRenderer.DOPropertyBlockFloat(Shader.PropertyToID("_Grayscale"), materialPropertyBlock, 1.0f, 0.3f).SetEasing(Ease.Type.CircIn);

            DisableTarget();

            DisableBodyPartsSickParents();
            SetRegdollState(true);

            DropGun();

            hatBehaviour.DetachHat();
            OnEnemyDied?.Invoke();

            AudioController.PlaySound(Random.Range(0, 2) == 0 ? AudioController.Sounds.scream1 : AudioController.Sounds.scream2);
        }

        private void DisableBodyPartsSickParents()
        {
            for (int i = 0; i < bodyParts.Length; i++)
            {
                if (bodyParts[i].StickBehaviour != null)
                    bodyParts[i].StickBehaviour.ResetParent();
            }
        }

        public void OnEnemyClicked()
        {
            DisableTargetImmediately();
        }

        public void OnBodyPartHitted(BodyHitPoint bodyPart, Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
            if (isDead)
                return;

            lastHitPosition = hitPosition;
            lastBodyPart = bodyPart;

            health--;
            if (bodyPart.BodyPartType == BodyHitPoint.Type.Head)
            {
                health = 0;
            }

            if (health > 0)
            {
                if (!isFirstEmotionsShown)
                {
                    if (showEmotionOnHit)
                        EmotionsController.SpawnEmotion(HIT_EMOTIONS.GetRandomItem(), emotionTransform.position, 2.2f);

                    isFirstEmotionsShown = true;
                }

                characterAnimator.Play(ANIMATION_HIT_HASHES[hitAnimationIndex], 0, 0.02f);

                hitAnimationIndex++;
                if (hitAnimationIndex >= ANIMATION_HIT_HASHES.Length)
                    hitAnimationIndex = 0;

                // New stick behaviour
                if (spawnFoamOnHit)
                    bodyPart.SpawnFoam();

                if (moveOnHit && !isCharacterSticked)
                    hitMovementTweenCase = transform.DOMove(transform.position + (direction * 5).SetY(0), 0.4f, tweenType: TweenType.FixedUpdate);

                if (shootingDelayTweenCase != null && !shootingDelayTweenCase.isCompleted)
                    shootingDelayTweenCase.state = 0.0f;

                if (useRig)
                {
                    if (bodyPart.BodyPartType == BodyHitPoint.Type.RightArm || bodyPart.BodyPartType == BodyHitPoint.Type.RightForeArm)
                    {
                        Tween.DoFloat(0.0f, 1.0f, 0.4f, (value) =>
                        {
                            rightHandIKConstraint.weight = value;
                        }).SetEasing(Ease.Type.ExpoOut);

                        rightHandConstraintController.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
                        rightHandConstraintController.transform.DOLocalMove(new Vector3(2.4000001f, 4.51f, -0.769999981f), 0.4f).SetEasing(Ease.Type.ExpoOut);
                    }
                    else if (bodyPart.BodyPartType == BodyHitPoint.Type.LeftArm || bodyPart.BodyPartType == BodyHitPoint.Type.LeftForeArm)
                    {
                        DisableAiming();

                        DropGun();

                        Tween.DoFloat(0.0f, 1.0f, 0.4f, (value) =>
                        {
                            leftHandIKConstraint.weight = value;
                        }).SetEasing(Ease.Type.ExpoOut);

                        leftHandConstraintController.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
                        leftHandConstraintController.transform.DOLocalMove(new Vector3(-2.4f, 4.51f, -0.769999981f), 0.4f).SetEasing(Ease.Type.ExpoOut);
                    }
                    else if (bodyPart.BodyPartType == BodyHitPoint.Type.RightLeg || bodyPart.BodyPartType == BodyHitPoint.Type.RightUpLeg)
                    {
                        Tween.DoFloat(0.0f, 1.0f, 0.4f, (value) =>
                        {
                            rightLegIKConstraint.weight = value;
                        }).SetEasing(Ease.Type.ExpoOut);

                        rightLegConstraintController.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg).position;
                        rightLegConstraintController.transform.DOLocalMove(new Vector3(0.332076937f, 0.138600677f, -0.0645424128f), 0.4f).SetEasing(Ease.Type.ExpoOut);
                    }
                    else if (bodyPart.BodyPartType == BodyHitPoint.Type.LeftLeg || bodyPart.BodyPartType == BodyHitPoint.Type.LeftUpLeg)
                    {
                        Tween.DoFloat(0.0f, 1.0f, 0.4f, (value) =>
                        {
                            leftLegIKConstraint.weight = value;
                        }).SetEasing(Ease.Type.ExpoOut);

                        leftLegConstraintController.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position;
                        leftLegConstraintController.transform.DOLocalMove(new Vector3(-0.332079351f, 0.138600037f, -0.0645318255f), 0.4f).SetEasing(Ease.Type.ExpoOut);
                    }
                }

                EnableAiming();
            }
            else
            {
                bodyPart.SpawnFoam();

                Die();

                Tween.NextFrame(delegate
                {
                    if (!isCharacterSticked)
                    {
                        for (int i = 0; i < bodyParts.Length; i++)
                        {
                            if (bodyParts[i] != lastBodyPart)
                                bodyParts[i].ObjectRigidbody.AddForce(direction * 1500f);
                        }

                        lastBodyPart.ObjectRigidbody.AddForce(direction * 1500f);
                    }
                }, updateMethod: TweenType.FixedUpdate);
            }
        }

        public override void OnHitPointTriggered(Vector3 direction, Vector3 hitPosition, BulletData bulletData)
        {
        }

        private void Shoot()
        {
            if (IsDead)
                return;

            characterAnimator.SetBool(ANIMATION_AIM_HASH, false);
            characterAnimator.SetTrigger(ANIMATION_SHOOT_HASH);

            gunBehaviour.Shoot(PlayerController.PlayerTransform.position - gunBehaviour.ShootPoint.position, false);

            DisableWarning();

            isAiming = false;
        }

        public void PlaySpecialAnimation()
        {
            characterAnimator.Play("Special");
        }

        public void OnBodyPartSticked(BodyHitPoint bodyPart)
        {
            if (isDead && showEmotionOnDead)
                EmotionsController.SpawnEmotion(Emotion.Type.Dead, emotionTransform.position, 2.2f);

            if (isCharacterSticked)
                return;

            isCharacterSticked = true;

            if (hitMovementTweenCase != null && !hitMovementTweenCase.isCompleted)
                hitMovementTweenCase.Kill();
        }

        #endregion

        #region Physics
        private void SetRegdollState(bool state)
        {
            // Turn off/on character animator
            characterAnimator.enabled = !state;

            // Change bones state
            for (int i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].SetPhysicsState(state);
            }
        }

        public void AddForceToBodyParts(Vector3 direction, float force)
        {
            for (int i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].ObjectRigidbody.AddForce(direction * force);
            }
        }
        #endregion

        #region Scenario
        public void SetRuntimeAnimatorController(RuntimeAnimatorController animatorController)
        {
            characterAnimator.runtimeAnimatorController = animatorController;
        }

        private EnemyBaseScenario GetCurrentScenario()
        {
            for (int i = 0; i < scenarios.Length; i++)
            {
                if (scenarios[i].Type.Equals(behavior))
                {
                    return scenarios[i];
                }
            }

            return scenarios[0];
        }

        #endregion

        #region Target
        public void EnableTarget()
        {
            activeTargetBehaviour = LevelController.SpawnTarget(targetTransform, targetScale);
        }

        public void EnableTargetWithHand()
        {
            activeTargetBehaviour = LevelController.SpawnTarget(targetTransform, targetScale);
            LevelController.SpawnTutorialHand(targetTransform);
        }

        public void DisableTarget()
        {
            if (activeTargetBehaviour != null)
            {
                activeTargetBehaviour.Disable();
                activeTargetBehaviour = null;
                DisableTutorialHand();
            }
        }

        

        public void DisableTargetImmediately()
        {
            if (activeTargetBehaviour != null)
            {
                activeTargetBehaviour.DisableImmediately();
                activeTargetBehaviour = null;
                DisableTutorialHand();
            }
        }

        private void DisableTutorialHand()
        {
            if (showHandClickingTarget)
            {
                LevelController.DisableTutorialHand();
            }
        }
        #endregion

        #region Warning
        protected void EnableWarning()
        {
            if (!enableWarning)
                return;

            isWarningEnabled = true;

            warningActiveTransform = LevelController.SpawnWarning(warningTransform).transform;
        }

        protected void DisableWarning()
        {
            if (!enableWarning)
                return;

            warningActiveTransform.DOScale(0.0f, 0.2f, unscaledTime: true).SetEasing(Ease.Type.CircOut).OnComplete(delegate
            {
                warningActiveTransform.SetParent(null);
                warningActiveTransform.gameObject.SetActive(false);
            });

            isWarningEnabled = false;
        }
        #endregion

        #region Set Methods

        public void SetHitsToKill(int newHits)
        {
            hitsToKill = newHits;
        }

        public void SetBehaviorType(BehaviorType newType)
        {
            behavior = newType;
        }

        public void SetIsRunning(bool newState)
        {
            isRunning = newState;
        }

        public void SetRunningSpeed(float newSpeed)
        {
            runningSpeed = newSpeed;
        }

        public void SetEnableAiming(bool newState)
        {
            enableAiming = newState;
        }

        public void SetMinMaxAimDelay(DuoFloat newValue)
        {
            minMaxAimDelay = newValue;
        }

        public void SetEnableWarning(bool newState)
        {
            enableWarning = newState;
        }

        public void SetMoveOnHit(bool newState)
        {
            moveOnHit = newState;
        }

        public void SetSpawnFoamOnHit(bool newState)
        {
            spawnFoamOnHit = newState;
        }

        public void SetUseRig(bool newState)
        {
            useRig = newState;
        }

        public void SetShowEmotionOnHit(bool newState)
        {
            showEmotionOnHit = newState;
        }

        public void SetShowEmotionOnDead(bool newState)
        {
            showEmotionOnDead = newState;
        }

        public void SetEnableTarget(bool newState)
        {
            enableTarget = newState;
        }

        public void SetShowHandClickingTarget(bool newState)
        {
            showHandClickingTarget = newState;
        }

        public void SetTargetScale(float newScale)
        {
            targetScale = newScale;
        }

        #endregion

        #region Editor

        [Button("Print bones names")]
        private void PrintBonesNames()
        {
            Animator characterAnimator = GetComponent<Animator>();

            if (characterAnimator != null)
            {
                Debug.Log("Pelvis : " + characterAnimator.GetBoneTransform(HumanBodyBones.Hips).name);
                Debug.Log("Left Hips : " + characterAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).name);
                Debug.Log("Left Knee : " + characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).name);
                Debug.Log("Left Foot : " + characterAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).name);
                Debug.Log("Right Hips : " + characterAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg).name);
                Debug.Log("Right Knee : " + characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg).name);
                Debug.Log("Right Foot : " + characterAnimator.GetBoneTransform(HumanBodyBones.RightFoot).name);
                Debug.Log("Left Arm : " + characterAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm).name);
                Debug.Log("Left Elbow : " + characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm).name);
                Debug.Log("Right Arm : " + characterAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm).name);
                Debug.Log("Right Elbow : " + characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm).name);
                Debug.Log("Middle Spine : " + characterAnimator.GetBoneTransform(HumanBodyBones.Spine).name);
                Debug.Log("Head : " + characterAnimator.GetBoneTransform(HumanBodyBones.Head).name);
            }
            else
            {
                Debug.LogError("Add animator component first!");
            }
        }

        [Button("Create and Init")]
        private void CreateAndInitRequiredObjects()
        {
            Animator characterAnimator = GetComponent<Animator>();

            if (characterAnimator == null)
            {
                Debug.LogError("Add animator component first!");
                return;
            }

            if (characterAnimator.avatar == null || !characterAnimator.avatar.isHuman)
            {
                Debug.LogError("Avatar is missing or type isn't humanoid!");
                return;
            }

            // Mesh
            skinnedMeshRenderer = transform.GetComponentInChildren<SkinnedMeshRenderer>();

            // Warning
            if (warningTransform == null)
            {
                GameObject warningObject = new GameObject("Warning Holder");

                warningObject.transform.ResetGlobal();
                warningObject.transform.SetParent(transform);
                warningObject.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.Head).position + new Vector3(0, 2.0f, 0);

                warningTransform = warningObject.transform;
            }

            // Target
            if (targetTransform == null)
            {
                GameObject targetObject = new GameObject("Target Holder");

                targetObject.transform.ResetGlobal();
                targetObject.transform.SetParent(transform);
                targetObject.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.Head).position;

                targetTransform = targetObject.transform;
            }

            // Gun holder
            if (gunHolderTransfrom == null)
            {
                GameObject gunHolderObject = new GameObject("Gun Holder");

                gunHolderObject.transform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand));
                gunHolderObject.transform.localPosition = new Vector3(-0.37f, 0.21f, 0.28f);
                gunHolderObject.transform.localRotation = Quaternion.Euler(-109f, -275f, 220);
                gunHolderObject.transform.localScale = new Vector3(3f, 3f, 3f);

                gunHolderTransfrom = gunHolderObject.transform;
            }

            // Emotion holder
            if (emotionTransform == null)
            {
                GameObject emotionObject = new GameObject("Emotion Holder");

                emotionObject.transform.ResetGlobal();
                emotionObject.transform.SetParent(transform);
                emotionObject.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.Head).position + new Vector3(0, 2.0f, 0);

                emotionTransform = emotionObject.transform;
            }

            // Hat holder
            if (hatBehaviour.HolderTrasform == null)
            {
                GameObject hatHolder = new GameObject("Hat Holder");

                hatHolder.transform.ResetGlobal();
                hatHolder.transform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.Head).GetChild(0));
                hatHolder.transform.localPosition = new Vector3(0f, -0.048f, 0.118f);

                hatBehaviour.InitHatHolderTransform(hatHolder.transform);
            }

            // Scenarios
            List<ScenarioCase> secenariosList = new List<ScenarioCase>();
#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif


            // Rig
            if (mainRig != null)
                DestroyImmediate(mainRig.gameObject);

            GameObject mainRigObject = new GameObject("Main Rig");

            mainRigObject.transform.SetParent(transform);
            mainRigObject.transform.ResetLocal();

            mainRig = mainRigObject.AddComponent<Rig>();

            // Create rig builder
            RigBuilder rigBuilder = GetComponent<RigBuilder>();

            if (rigBuilder == null)
                rigBuilder = gameObject.AddComponent<RigBuilder>();

            rigBuilder.layers = new List<RigLayer>() { new RigLayer(mainRig, true) };

            // Right hand rig
            GameObject rightHandTargetObject = new GameObject("Right Hand IK");
            rightHandTargetObject.transform.SetParent(mainRigObject.transform);
            rightHandTargetObject.transform.ResetLocal();

            rightHandIKConstraint = rightHandTargetObject.AddComponent<TwoBoneIKConstraint>();
            rightHandIKConstraint.data.root = characterAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rightHandIKConstraint.data.mid = characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            rightHandIKConstraint.data.tip = characterAnimator.GetBoneTransform(HumanBodyBones.RightHand);

            GameObject rightHandController = new GameObject("Right Hand Controller");
            rightHandController.transform.SetParent(rightHandTargetObject.transform);
            rightHandController.transform.ResetLocal();
            rightHandController.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.RightHand).position;

            rightHandIKConstraint.data.target = rightHandController.transform;

            rightHandConstraintController = rightHandController.transform;

            // Left hand rig
            GameObject leftHandTargetObject = new GameObject("Left Hand IK");
            leftHandTargetObject.transform.SetParent(mainRigObject.transform);
            leftHandTargetObject.transform.ResetLocal();

            leftHandIKConstraint = leftHandTargetObject.AddComponent<TwoBoneIKConstraint>();
            leftHandIKConstraint.data.root = characterAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            leftHandIKConstraint.data.mid = characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            leftHandIKConstraint.data.tip = characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand);

            GameObject leftHandController = new GameObject("Left Hand Controller");
            leftHandController.transform.SetParent(leftHandTargetObject.transform);
            leftHandController.transform.ResetLocal();
            leftHandController.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand).position;

            leftHandIKConstraint.data.target = leftHandController.transform;

            leftHandConstraintController = leftHandController.transform;

            // Right leg rig
            GameObject rightLegTargetObject = new GameObject("Right Leg IK");
            rightLegTargetObject.transform.SetParent(mainRigObject.transform);
            rightLegTargetObject.transform.ResetLocal();

            rightLegIKConstraint = rightLegTargetObject.AddComponent<TwoBoneIKConstraint>();
            rightLegIKConstraint.data.root = characterAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            rightLegIKConstraint.data.mid = characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            rightLegIKConstraint.data.tip = characterAnimator.GetBoneTransform(HumanBodyBones.RightFoot);

            GameObject rightLegController = new GameObject("Right Leg Controller");
            rightLegController.transform.SetParent(rightLegTargetObject.transform);
            rightLegController.transform.ResetLocal();
            rightLegController.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.RightFoot).position;

            rightLegIKConstraint.data.target = rightLegController.transform;

            rightLegConstraintController = rightLegController.transform;

            // Left leg rig
            GameObject leftLegTargetObject = new GameObject("Left Leg IK");
            leftLegTargetObject.transform.SetParent(mainRigObject.transform);
            leftLegTargetObject.transform.ResetLocal();

            leftLegIKConstraint = leftLegTargetObject.AddComponent<TwoBoneIKConstraint>();
            leftLegIKConstraint.data.root = characterAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            leftLegIKConstraint.data.mid = characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            leftLegIKConstraint.data.tip = characterAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);

            GameObject leftLegController = new GameObject("Left Leg Controller");
            leftLegController.transform.SetParent(leftLegTargetObject.transform);
            leftLegController.transform.ResetLocal();
            leftLegController.transform.position = characterAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position;

            leftLegIKConstraint.data.target = leftLegController.transform;

            leftLegConstraintController = leftLegController.transform;


            gameObject.tag = PhysicsHelper.TAG_ENEMY;
            gameObject.layer = PhysicsHelper.LAYER_ENEMY;


            // Hit Points setup
            List<BodyHitPoint> hitPointsList = new List<BodyHitPoint>();

            // Hips
            BodyHitPoint hipsBodyPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.Hips), BodyHitPoint.Type.Hips);
            hipsBodyPart.SetFoamSpawnPoint(hipsBodyPart.transform);

            hitPointsList.Add(hipsBodyPart);

            // Spine
            BodyHitPoint spineBodyPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.Spine), BodyHitPoint.Type.Spine);
            spineBodyPart.SetFoamSpawnPoint(spineBodyPart.transform);

            hitPointsList.Add(spineBodyPart);

            // Head
            BodyHitPoint headBodyPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.Head), BodyHitPoint.Type.Head);
            headBodyPart.SetFoamSpawnPoint(headBodyPart.transform);

            hitPointsList.Add(headBodyPart);

            // Left leg
            BodyHitPoint leftUpLegPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg), BodyHitPoint.Type.LeftUpLeg);
            BodyHitPoint leftLegPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), BodyHitPoint.Type.LeftLeg);

            leftUpLegPart.SetConnectedBodyPart(leftLegPart);
            leftLegPart.SetConnectedBodyPart(leftUpLegPart);

            leftUpLegPart.SetFoamSpawnPoint(leftLegPart.transform);
            leftLegPart.SetFoamSpawnPoint(leftLegPart.transform);

            hitPointsList.Add(leftUpLegPart);
            hitPointsList.Add(leftLegPart);

            // Right leg
            BodyHitPoint rightUpLegPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg), BodyHitPoint.Type.RightUpLeg);
            BodyHitPoint rightLegPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg), BodyHitPoint.Type.RightLeg);

            rightUpLegPart.SetConnectedBodyPart(rightLegPart);
            rightLegPart.SetConnectedBodyPart(rightUpLegPart);

            rightUpLegPart.SetFoamSpawnPoint(rightLegPart.transform);
            rightLegPart.SetFoamSpawnPoint(rightLegPart.transform);

            hitPointsList.Add(rightUpLegPart);
            hitPointsList.Add(rightLegPart);

            // Left hand
            BodyHitPoint leftUpHandPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm), BodyHitPoint.Type.LeftArm);
            BodyHitPoint leftHandPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm), BodyHitPoint.Type.LeftForeArm);

            leftUpHandPart.SetConnectedBodyPart(leftHandPart);
            leftHandPart.SetConnectedBodyPart(leftUpHandPart);

            leftUpHandPart.SetFoamSpawnPoint(leftHandPart.transform);
            leftHandPart.SetFoamSpawnPoint(leftHandPart.transform);

            hitPointsList.Add(leftUpHandPart);
            hitPointsList.Add(leftHandPart);

            // Right hand
            BodyHitPoint rightUpHandPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm), BodyHitPoint.Type.RightArm);
            BodyHitPoint rightHandPart = AddBodyPartAndCollider(characterAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm), BodyHitPoint.Type.RightForeArm);

            rightUpHandPart.SetConnectedBodyPart(rightHandPart);
            rightHandPart.SetConnectedBodyPart(rightUpHandPart);

            rightUpHandPart.SetFoamSpawnPoint(rightHandPart.transform);
            rightHandPart.SetFoamSpawnPoint(rightHandPart.transform);

            hitPointsList.Add(rightUpHandPart);
            hitPointsList.Add(rightHandPart);

            bodyParts = hitPointsList.ToArray();

            Debug.Log("Setup Completed");
        }

        private BodyHitPoint AddBodyPartAndCollider(Transform partTransform, BodyHitPoint.Type type)
        {
            BodyHitPoint tempBodyPart = partTransform.GetComponent<BodyHitPoint>();
            if (tempBodyPart == null)
            {
                tempBodyPart = partTransform.gameObject.AddComponent<BodyHitPoint>();
                tempBodyPart.SetBodyPartType(type);

                BoxCollider boxCollider = partTransform.gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;

                partTransform.gameObject.tag = PhysicsHelper.TAG_ENEMY;
                partTransform.gameObject.layer = PhysicsHelper.LAYER_ENEMY;
            }

            return tempBodyPart;
        }

        protected bool HideAimingSettings()
        {
            return !enableAiming;
        }

        protected bool HideTargetSettings()
        {
            return !enableTarget;
        }

        protected bool HideWarningSettings()
        {
            return !enableWarning;
        }

        protected bool HideRunningSettings()
        {
            return !isRunning;
        }

        #endregion
    }
}