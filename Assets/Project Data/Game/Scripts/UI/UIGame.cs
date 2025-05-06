using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class UIGame : UIPage
    {
        private readonly string[] CONGRATULATION_TEXTS = new string[] { "ОТЛИЧНО!", "НЕПЛОХО!", "А ТЫ КРУТОЙ", "ЖЕСТКО!" };

        [Header("Progressbar")]
        [SerializeField] RectTransform progressbarRectTrasform;
        [SerializeField] Image progressbarImage;
        [SerializeField] Text currentLevelText;
        [SerializeField] Text nextLevelText;
        [SerializeField] Text levelNumberText;

        [Header("Congratulation")]
        [SerializeField] Text congratulationText;
        [SerializeField] ParticleSystem congratulationParticle;

        [Header("Tap to shoot")]
        [SerializeField] Image tapToShootImage;

        private Vector2 progressbarDefaultPosition;

        private bool tapToShootImageIsEnabled;

        public override void Init()
        {
            base.Init();

            progressbarDefaultPosition = progressbarRectTrasform.anchoredPosition;
        }

        public override void Hide()
        {
            if (!isPageDisplayed)
                return;

            progressbarRectTrasform.DOAnchoredPosition(new Vector2(0, 300), 0.6f).SetEasing(Ease.Type.BackOut).OnComplete(delegate
            {
                canvas.enabled = false;
            });

            isPageDisplayed = false;
        }

        public override void HideImmediately()
        {
            progressbarRectTrasform.anchoredPosition = new Vector2(0, 300);

            canvas.enabled = false;
            isPageDisplayed = false;
        }

        public override void Show()
        {
            if (isPageDisplayed)
                return;

            levelNumberText.text = "УРОВЕНЬ " + GameController.LevelNumber;

            progressbarRectTrasform.anchoredPosition = new Vector2(0, 300);
            progressbarRectTrasform.DOAnchoredPosition(progressbarDefaultPosition, 0.6f).SetEasing(Ease.Type.BackOut);

            isPageDisplayed = true;
            canvas.enabled = true;
        }

        public override void ShowImmediately()
        {
            if (isPageDisplayed)
                return;

            levelNumberText.text = "УРОВЕНЬ " + GameController.LevelNumber;
            progressbarRectTrasform.anchoredPosition = progressbarDefaultPosition;

            isPageDisplayed = true;
            canvas.enabled = true;
        }

        #region Congratulation
        public void ShowCongratulationText()
        {
            if (congratulationText.gameObject.activeSelf)
                return;

            congratulationParticle.Play();

            congratulationText.text = CONGRATULATION_TEXTS.GetRandomItem();
            congratulationText.rectTransform.anchoredPosition = new Vector2(0, -280);
            congratulationText.gameObject.SetActive(true);

            congratulationText.rectTransform.DOAnchoredPosition(new Vector3(0, -230), 0.4f).SetEasing(Ease.Type.BackInOut).OnComplete(delegate
            {
                Tween.DelayedCall(1.5f, delegate
                {
                    congratulationText.gameObject.SetActive(false);
                });
            });

            congratulationText.rectTransform.localScale = Vector3.one * 0.8f;
            congratulationText.rectTransform.DOScale(1.0f, 0.4f).SetEasing(Ease.Type.BackOut);
        }
        #endregion

        #region Progressbar
        public void SetLevel(int levelIndex)
        {
            currentLevelText.text = levelIndex.ToString();
            nextLevelText.text = (levelIndex + 1).ToString();
        }

        public void SetProgress(float progress)
        {
            progressbarImage.fillAmount = progress;
        }

        public void SetProgressWithAnimation(float progress)
        {
            Tween.DoFloat(progressbarImage.fillAmount, progress, 0.4f, (value) =>
            {
                progressbarImage.fillAmount = value;
            }).SetEasing(Ease.Type.CubicOut);
        }
        #endregion


    }
}