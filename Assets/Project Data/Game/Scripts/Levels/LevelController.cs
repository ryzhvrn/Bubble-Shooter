using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class LevelController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] int levelCompleteReward;

        [Header("Target")]
        [SerializeField] GameObject targetPrefab;

        [Header("Warning")]
        [SerializeField] GameObject warningPrefab;

        [Header("Foam")]
        [SerializeField] GameObject foamPrefab;
        [SerializeField] GameObject foamSplashPrefab;

        [Header("Tutorial")]
        [SerializeField] GameObject tutorialHandPrefab;

        private static Pool targetPool;
        private static Pool warningPool;
        private static Pool foamPool;
        private static Pool foamSplashPool;

        private LevelsDatabase levelsDatabase;
        private GameObject levelGameObject;
        private PlayerController player;
        private List<LevelStage> levelStages = new List<LevelStage>();
        public static LevelStage CurrentStage => instance.levelStages[instance.CurrentStageIndex];
        public static LevelStage NextStage => instance.levelStages[instance.CurrentStageIndex + 1];

        private static TutorialHandBehaviour tutorialHandBehaviour;
        public static bool IsTutorialHandActive => tutorialHandBehaviour.gameObject.activeInHierarchy;

        private static int activeLevelNumber = -1;
        public static int ActiveLevelNumber => activeLevelNumber;

        private static LevelController instance;
        public static int LevelCompleteReward => instance.levelCompleteReward;

        private int CurrentStageIndex { get; set; }

        private void Awake()
        {
            instance = this;
            // Create target pool
            targetPool = new Pool(new PoolSettings("Target", targetPrefab, 1, true));

            // Create warning pool
            warningPool = new Pool(new PoolSettings("Warning", warningPrefab, 1, true));

            // Create foam pool
            foamPool = new Pool(new PoolSettings("Foam", foamPrefab, 1, true));
            foamSplashPool = new Pool(new PoolSettings("FoamSplash", foamSplashPrefab, 3, true));

            // Create tutorial object
            GameObject tutorialHand = Instantiate(tutorialHandPrefab);
            tutorialHand.SetActive(false);

            tutorialHandBehaviour = tutorialHand.GetComponent<TutorialHandBehaviour>();
        }

        public void Init(LevelsDatabase levelsDatabase)
        {
            this.levelsDatabase = levelsDatabase;
            player = PlayerController.GetPlayer();
        }

        public void LoadLevel(int levelNumber)
        {
            // Unload level
            if (activeLevelNumber != -1)
                UnloadLevel();

            // Get level from database
            LevelData levelData = levelsDatabase.GetLevel(levelNumber);

            // Spawn a level
            levelStages.Clear();
            levelGameObject = new GameObject("[Level]");
            CurrentStageIndex = 0;

            for (int i = 0; i < levelData.Stages.Count; i++)
            {
                Transform nextStagePos = GetNextStagePosition();
                LevelStage stage = Instantiate(levelData.Stages[i].gameObject, nextStagePos.transform.position, nextStagePos.transform.rotation, levelGameObject.transform).GetComponent<LevelStage>();
                levelStages.Add(stage);
            }

            // Get component from cloned object
            activeLevelNumber = levelNumber;

            // Reset UI
            UIGame gamePage = UIController.GetPage<UIGame>();
            gamePage.SetProgress(0.0f);
            gamePage.SetLevel(GameController.LevelNumber);

            DisableShooting();

            // Init player on the first stage
            player.OnStageReached(levelStages[CurrentStageIndex]);

        }

        private Transform GetNextStagePosition()
        {
            if (levelStages.Count == 0)
            {
                return levelGameObject.transform;
            }
            else
            {
                return levelStages[levelStages.Count - 1].NextStagePosition;
            }
        }

        public void UnloadLevel()
        {
            if (levelGameObject != null)
                Destroy(levelGameObject);

            activeLevelNumber = -1;
        }

        #region Logic

        public void StartGameplay()
        {
            player.Activate();
            OnStageReached();
        }

        private void OnStageReached()
        {
            levelStages[CurrentStageIndex].OnStageReached();

            EnableShooting();
        }

        public static void OnStageCompleted()
        {
            instance.OnStageComplete();
        }

        private void OnStageComplete()
        {
            UIGame gamePage = UIController.GetPage<UIGame>();
            gamePage.SetProgressWithAnimation((float)(CurrentStageIndex + 1) / levelStages.Count);

            gamePage.ShowCongratulationText();

            if (CurrentStageIndex >= levelStages.Count - 1)
            {
                OnLevelComplete();
                return;
            }
            else
            {
                AudioController.PlaySound(AudioController.Sounds.win);
            }

            StartTransitionToTheNextStage();
        }

        private void StartTransitionToTheNextStage()
        {
            DisableShooting();
            List<Vector3> path = CurrentStage.GetCameraPath();
            path.Add(NextStage.PlayerStartPosition.position);

            player.MoveAlongPath(PathSmoothTool.Smooth(path.ToArray(), 10f));
        }

        public static void OnPlayerReachedNextStage()
        {
            instance.CurrentStageIndex++;
            instance.player.OnStageReached(CurrentStage);
            instance.OnStageReached();
        }

        public void EnableShooting()
        {
            ShootingController.SetShootingState(true);
        }

        public void DisableShooting()
        {
            ShootingController.SetShootingState(false);
        }

        public void OnLevelComplete()
        {
            GameController.LevelComplete();
        }

        #endregion

        #region Spawning

        public static FoamBehaviour SpawnFoam(Vector3 position, float minSize, float maxSize, Transform parent = null)
        {
            GameObject foamObject = foamPool.GetPooledObject();
            foamObject.transform.SetParent(parent);
            foamObject.transform.position = position;
            foamObject.SetActive(true);

            FoamBehaviour targetBehaviour = foamObject.GetComponent<FoamBehaviour>();
            targetBehaviour.Spawn(Random.Range(minSize, maxSize));

            return targetBehaviour;
        }

        public static ParticleSystem SpawnFoamSplash(Vector3 position, float scale = 1.0f)
        {
            GameObject foamObject = foamSplashPool.GetPooledObject();
            foamObject.transform.position = position;
            foamObject.transform.localScale = scale.ToVector3();
            foamObject.SetActive(true);

            ParticleSystem foamSplashParticle = foamObject.GetComponent<ParticleSystem>();
            foamSplashParticle.Play();

            return foamSplashParticle;
        }

        public static TargetBehaviour SpawnTarget(Transform target, float scale = 1.0f)
        {
            GameObject targetObject = targetPool.GetPooledObject();
            targetObject.SetActive(true);
            targetObject.transform.localScale = scale.ToVector3();

            TargetBehaviour targetBehaviour = targetObject.GetComponent<TargetBehaviour>();
            targetBehaviour.SetTarget(target);
            targetBehaviour.Enable();

            return targetBehaviour;
        }

        public static GameObject SpawnWarning(Transform target)
        {
            GameObject warningObject = warningPool.GetPooledObject();
            warningObject.SetActive(true);

            WarningBehaviour warningBehaviour = warningObject.GetComponent<WarningBehaviour>();
            warningBehaviour.Init();

            warningObject.transform.SetParent(target);
            warningObject.transform.localPosition = Vector3.zero;
            warningObject.transform.localScale = Vector3.zero;
            warningObject.transform.DOScale(1.1f, 0.3f, unscaledTime: true).SetEasing(Ease.Type.CircOut).OnComplete(delegate
            {
                warningObject.transform.DOScale(1.0f, 0.05f, unscaledTime: true).SetEasing(Ease.Type.CircOut);
            });

            return warningObject;
        }

        public static void SpawnTutorialHand(Transform target)
        {
            tutorialHandBehaviour.Init();
            tutorialHandBehaviour.transform.position = target.transform.position;
            tutorialHandBehaviour.gameObject.SetActive(true);
        }

        public static void DisableTutorialHand()
        {
            tutorialHandBehaviour.transform.SetParent(null);
            tutorialHandBehaviour.gameObject.SetActive(false);
        }

        #endregion
    }
}