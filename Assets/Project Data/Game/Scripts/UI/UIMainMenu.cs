using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        public readonly float HIDDEN_PAGE_DELAY = 0.55F;
        public readonly float STORE_AD_RIGHT_OFFSET_X = 300F;

        [Space]
        [SerializeField] RectTransform tapToPlayRect;
        [SerializeField] GameObject tapToPlayRaycastObject;

        [Header("Coins Label")]
        [SerializeField] UIScalableObject coinsLabel;
        [SerializeField] Text coinsAmountsText;

        [SerializeField] UIMainMenuButton storeButtonRect;
        [SerializeField] UIMainMenuButton noAdsButtonRect;

        private TweenCase tapToPlayPingPong;
        private TweenCase showHideStoreAdButtonDelayTweenCase;

        private void OnEnable()
        {
            IAPManager.OnPurchaseComplete += OnAdPurchased;
        }

        private void OnDisable()
        {
            IAPManager.OnPurchaseComplete -= OnAdPurchased;
        }

        public override void Init() // Called in the Start method
        {
            base.Init();

            UpdateCashLabel();

            storeButtonRect.Init(STORE_AD_RIGHT_OFFSET_X);
            noAdsButtonRect.Init(STORE_AD_RIGHT_OFFSET_X);
        }

        #region Show/Hide

        public override void Show()
        {
            if (isPageDisplayed)
                return;

            // KILL, RESET ANIMATED OBJECT

            showHideStoreAdButtonDelayTweenCase?.Kill();

            HideAdButton(true);

            isPageDisplayed = true;
            canvas.enabled = true;
            tapToPlayRaycastObject.SetActive(true);

            ShowTapToPlay(false);

            coinsLabel.Show(false);
            storeButtonRect.Show(false);

#if MODULE_IAP
            if (AdsManager.IsForcedAdEnabled())
            {
                showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.12f, delegate
                {
                    noAdsButtonRect.Show(false);
                });
            }
#endif

            SettingsPanel.ShowPanel(false);
        }

        public override void ShowImmediately()
        {
            if (isPageDisplayed)
                return;

            isPageDisplayed = true;
            canvas.enabled = true;
            tapToPlayRaycastObject.SetActive(true);

            ShowTapToPlay(true);

            coinsLabel.Show(true);
            storeButtonRect.Show(true);

            HideAdButton(true);

#if MODULE_IAP
            if (AdsManager.IsForcedAdEnabled())
            {
                noAdsButtonRect.Show(true);
            }
#endif

            SettingsPanel.ShowPanel(true);
            UILevelNumberText.Show(true);
        }

        public override void Hide()
        {
            if (!isPageDisplayed)
                return;

            // KILL, RESET

            showHideStoreAdButtonDelayTweenCase?.Kill();
            tapToPlayRaycastObject.SetActive(false);

            //

            isPageDisplayed = false;

            HideTapToPlayText(false);

            coinsLabel.Hide(false);

#if MODULE_IAP
            HideAdButton(immediately: false);
#endif

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.1f, delegate
            {
                storeButtonRect.Hide(immediately: false);
            });

            SettingsPanel.HidePanel(false);

            Tween.DelayedCall(HIDDEN_PAGE_DELAY, delegate
            {
                canvas.enabled = false;
            });
        }

        public override void HideImmediately()
        {
            if (!isPageDisplayed)
                return;

            tapToPlayRaycastObject.SetActive(false);

            canvas.enabled = false;
            isPageDisplayed = false;

            SettingsPanel.HidePanel(true);
        }

#endregion

#region Tap To Play Label

        public void ShowTapToPlay(bool immediately = true)
        {
            if (tapToPlayPingPong != null && tapToPlayPingPong.isActive)
                tapToPlayPingPong.Kill();

            if (immediately)
            {
                tapToPlayRect.localScale = Vector3.one;

                tapToPlayPingPong = tapToPlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                return;
            }

            // RESET
            tapToPlayRect.localScale = Vector3.zero;

            tapToPlayRect.DOPushScale(Vector3.one * 1.2f, Vector3.one, 0.35f, 0.2f, Ease.Type.CubicOut, Ease.Type.CubicIn).OnComplete(delegate
            {

                tapToPlayPingPong = tapToPlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

            });

        }

        public void HideTapToPlayText(bool immediately = true)
        {
            if (tapToPlayPingPong != null && tapToPlayPingPong.isActive)
                tapToPlayPingPong.Kill();

            if (immediately)
            {
                tapToPlayRect.localScale = Vector3.zero;

                return;
            }

            tapToPlayRect.DOPushScale(Vector3.one * 1.2f, Vector3.zero, 0.2f, 0.35f, Ease.Type.CubicOut, Ease.Type.CubicIn);
        }

#endregion

#region Coins Label      

        public void UpdateCashLabel()
        {
            coinsAmountsText.text = GameController.MoneyAmount.ToString();
        }

#endregion

#region Ad Button Label

        private void HideAdButton(bool immediately = false)
        {
            noAdsButtonRect.Hide(immediately);
        }

        private void OnAdPurchased(ProductKeyType productKeyType)
        {
            if (productKeyType == ProductKeyType.NoAds)
            {
                HideAdButton(immediately: true);
            }
        }

#endregion

#region Buttons

        public void TapToPlayButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        public void StoreButton()
        {
            storeButtonRect.SetInteractable(false);
            UIController.HidePage(typeof(UIMainMenu));

            UIMainMenu uiMainMenu = UIController.GetPage<UIMainMenu>();
            UILevelNumberText.Hide(false);

            Tween.DelayedCall(uiMainMenu.HIDDEN_PAGE_DELAY, delegate
            {
                storeButtonRect.SetInteractable(true);
                UIController.GetPage<UIStore>().Show();
            });

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }


        public void NoAdButton()
        {
            IAPManager.BuyProduct(ProductKeyType.NoAds);
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

#endregion

    }


}
