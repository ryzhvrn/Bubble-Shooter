using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [Define("DEBUG_LOGS")]
    [DefaultExecutionOrder(-100)]
    public class GameController : MonoBehaviour
    {
        private static GameController instance;

        [Header("Databases")]
        [SerializeField] HatsDatabase hatsDatabase;
        [SerializeField] LevelsDatabase levelsDatabase;

        [Header("Refference")]
        [SerializeField] PlayerController playerController;
        [SerializeField] LevelController levelController;

        public static event SimpleCallback OnLevelChanged;
        public static event SimpleCallback OnMoneyAmountChanged;

        public static bool IsGameActive { get; private set; }

        public static int LevelNumber
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.LevelNumber);
            private set
            {
                PrefsSettings.SetInt(PrefsSettings.Key.LevelNumber, value);
                OnLevelChanged?.Invoke();
            }
        }

        public static int MoneyAmount
        {
            get => PrefsSettings.GetInt(PrefsSettings.Key.MoneyAmount);
            set
            {
                PrefsSettings.SetInt(PrefsSettings.Key.MoneyAmount, value);
                OnMoneyAmountChanged?.Invoke();
            }
        }

        private void Awake()
        {
            instance = this;

            // Initialise physics helper
            PhysicsHelper.Init();

            // Initialise databases
            hatsDatabase.Init();

            // Init refferences
            playerController.Init();
            levelController.Init(levelsDatabase);

        }
        private void Start()
        {
            LoadGame();
        }

        private void LoadGame()
        {
            // Load level
            IsGameActive = false;
            levelController.LoadLevel(LevelNumber);
        }

        public static void OnTapToShoot()
        {
            IsGameActive = true;
            instance.levelController.StartGameplay();

            UIController.HidePage(typeof(UIMainMenu));
            UIController.ShowPage(typeof(UIGame));
        }

        public static void LevelComplete()
        {
            if (!IsGameActive)
                return;

            IsGameActive = false;

            LevelNumber++;
            AudioController.PlaySound(AudioController.Sounds.complete);

            Tween.DelayedCall(1.0f, delegate
            {
                UIController.HidePage(typeof(UIGame));
                UIController.ShowPage(typeof(UIComplete));
            });
        }

        public static void LevelFailed()
        {
            if (!IsGameActive)
                return;

            IsGameActive = false;
            AudioController.PlaySound(AudioController.Sounds.lose);
            Vibration.Vibrate(AudioController.Vibrations.longVibration);

            UIController.HidePage(typeof(UIGame));
            UIController.ShowPage(typeof(UIGameOver));
        }

        public static void OnCompletePageClosed()
        {
            Tween.RemoveAll();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
            AdsManager.ShowInterstitial(delegate { });
        }

        public static void OnSkipLevelPressed()
        {
            AdsManager.ShowRewardBasedVideo((finished) =>
            {
                if (finished)
                {
                    LevelNumber++;

                    OnGameOverPageClosed();
                }
                else
                {
                    OnGameOverPageClosed();
                }
            });
        }

        public static void OnGameOverPageClosed()
        {
            Tween.RemoveAll();

            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}