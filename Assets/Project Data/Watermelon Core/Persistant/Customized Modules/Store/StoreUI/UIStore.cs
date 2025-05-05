using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    public class UIStore : UIPage, IDragHandler, IEndDragHandler
    {
        public readonly float HIDDED_PAGE_DELAY = 0.52F;

        public readonly string STORE_ITEM_POOL_NAME = "StoreItem";
        public readonly string PAGES_POOL_NAME = "StorePage";
        public readonly string PAGE_DOT_POOL_NAME = "PageDot";
        private readonly string GET_COINS_FOR_AD_LABEL = "GET\n";
        private readonly float PANEL_BOTTOM_OFFSET_Y = -2000f;

        [Header("Settings")]
        [SerializeField] float swipeThereshold = 0.2f;
        [SerializeField] StoreProductType defaultStoreProductType;

        [Header("Background")]
        [SerializeField] Canvas backgroundCanvas;
        [SerializeField] UIFade backgroundFade;
        [SerializeField] Text coinsText;
        [SerializeField] UIScalableObject coinsLabel;
        [SerializeField] UIScalableObject closeButton;

        [Header("Prefabs")]
        [SerializeField] GameObject storeItemPrefab;
        [SerializeField] GameObject pageDotPrefab;
        [SerializeField] GameObject pagePrefab;

        [Header("Prefabs Holder")]
        [SerializeField] Transform pagesHolderTransform;
        [SerializeField] Transform pageDotsHolderTransform;
        [SerializeField] ProductPreviewController productPreview;

        [Space(5f)]
        [SerializeField] RectTransform storeAnimatedPanelRect;

        [Header("Unlock Random Label")]
        [SerializeField] Button unlockRandomButton;
        [SerializeField] Text unlockRandomPriceText;

        [Header("Get Coins Label")]
        [SerializeField] Button getCoinsButton;
        [SerializeField] Text coinsForAdsText;

        [Space(5f)]
        [SerializeField] List<StoreTab> tabsList = new List<StoreTab>();

        private static Dictionary<StorePageName, List<StoreProduct>> productsByGroupDictionary;
        private static List<Image> pageDotsImagesList = new List<Image>();
        private static List<StorePage> pagesList = new List<StorePage>();
        private static List<StoreProduct> skinsOnCurrentPageList = new List<StoreProduct>();
        private static List<int> closedSkinsOnCurrentPageIndexes = new List<int>();
        private static List<int> pagesAmountForEachProductType = new List<int>();

        private static HorizontalLayoutGroup pageDotsLayout;
        private static Pool storeItemPool;
        private static Pool pagesPool;
        private static Pool pageDotsPool;

        public static StoreProductType CurrentProductType { get; private set; }
        private static Vector3 pagesHolderLocalPosition;

        private static int Coins
        {
            get { return GameController.MoneyAmount; }
            set { GameController.MoneyAmount = value; }
        }

        private static bool isAnimationRunning;
        private static float canvasWidth;
        private static float startedStorePanelRectPositionY;
        private static int currentPageIndex;
        private static int pagesAmount;


        private void OnEnable()
        {
            StoreController.OnProductSelected += OnNewSkinSelected;
            GameController.OnMoneyAmountChanged += OnCoinsAmountChanged;
            AdsManager.OnRewardedAdLoadedEvent += UpdateGetCoinsButton;
        }

        private void OnDisable()
        {
            StoreController.OnProductSelected -= OnNewSkinSelected;
            GameController.OnMoneyAmountChanged -= OnCoinsAmountChanged;
            AdsManager.OnRewardedAdLoadedEvent -= UpdateGetCoinsButton;
        }

        public override void Init()
        {
            base.Init();

            canvasWidth = UIController.CanvasScaler.referenceResolution.x;
            pageDotsLayout = pageDotsHolderTransform.GetComponent<HorizontalLayoutGroup>();

            startedStorePanelRectPositionY = storeAnimatedPanelRect.anchoredPosition.y;

            // Init store pools
            storeItemPool = PoolManager.AddPool(new PoolSettings(STORE_ITEM_POOL_NAME, storeItemPrefab, 10, true));
            pagesPool = PoolManager.AddPool(new PoolSettings(PAGES_POOL_NAME, pagePrefab, 3, true, pagesHolderTransform));
            pageDotsPool = PoolManager.AddPool(new PoolSettings(PAGE_DOT_POOL_NAME, pageDotPrefab, 3, true, pageDotsHolderTransform));

            pagesAmountForEachProductType = StoreController.Database.GetPagesAmountPerProducts();

            productPreview.Init();
            productPreview.HidePreview(true);
        }

        public override void Show()
        {
            if (IsPageDisplayed)
                return;

            isPageDisplayed = true;
            Canvas.enabled = true;

            backgroundCanvas.enabled = true;
            backgroundFade.Show(duration: 0.25f);

            coinsLabel.Show(false);
            closeButton.Show(false);

            OnCoinsAmountChanged();

            InitStoreUI(defaultStoreProductType);

            storeAnimatedPanelRect.anchoredPosition = storeAnimatedPanelRect.anchoredPosition.SetY(PANEL_BOTTOM_OFFSET_Y);

            storeAnimatedPanelRect.DOAnchoredPosition(new Vector3(storeAnimatedPanelRect.anchoredPosition.x, startedStorePanelRectPositionY + 100f, 0f), 0.4f).SetEasing(Ease.Type.SineInOut).OnComplete(delegate
            {
                storeAnimatedPanelRect.DOAnchoredPosition(new Vector3(storeAnimatedPanelRect.anchoredPosition.x, startedStorePanelRectPositionY, 0f), 0.2f).SetEasing(Ease.Type.SineInOut);
            });

            productPreview.ShowPreview();
        }
        public override void ShowImmediately()
        {
            if (IsPageDisplayed)
                return;

            isPageDisplayed = true;
            Canvas.enabled = true;

            backgroundCanvas.enabled = true;
            backgroundFade.Show(immediately: true);

            coinsLabel.Show(true);
            closeButton.Show(true);

            OnCoinsAmountChanged();

            InitStoreUI(defaultStoreProductType);

            storeAnimatedPanelRect.anchoredPosition = storeAnimatedPanelRect.anchoredPosition.SetY(startedStorePanelRectPositionY);

            productPreview.ShowPreview();
        }

        public override void Hide()
        {
            if (!IsPageDisplayed)
                return;

            coinsLabel.Hide(false);
            closeButton.Hide(false);

            backgroundFade.Hide(duration: 0.55f);

            storeAnimatedPanelRect.DOAnchoredPosition(new Vector3(storeAnimatedPanelRect.anchoredPosition.x, startedStorePanelRectPositionY + 100f, 0f), 0.2f).SetEasing(Ease.Type.SineInOut).OnComplete(delegate
            {
                storeAnimatedPanelRect.DOAnchoredPosition(new Vector3(storeAnimatedPanelRect.anchoredPosition.x, PANEL_BOTTOM_OFFSET_Y, 0f), 0.4f).SetEasing(Ease.Type.SineInOut).OnComplete(delegate
                {

                    isPageDisplayed = false;
                    Canvas.enabled = false;

                    backgroundCanvas.enabled = false;
                });

            });

            productPreview.HidePreview(false);
        }

        public override void HideImmediately()
        {
            if (!IsPageDisplayed)
                return;

            isPageDisplayed = false;
            Canvas.enabled = false;

            backgroundCanvas.enabled = false;

            productPreview.HidePreview(true);
        }

        private void InitStoreUI(StoreProductType type)
        {
            CurrentProductType = type;

            storeItemPool?.ReturnToPoolEverything(true);
            pageDotsImagesList.Clear();

            isAnimationRunning = false;

            InitGetCoinsButton(GET_COINS_FOR_AD_LABEL + StoreController.Database.CoinsForAdsAmount);

            currentPageIndex = (int)StoreController.GetProduct(StoreController.GetSelectedProductSkinID(CurrentProductType)).Page;

            InitPages();

            UpdateCurrentPage(false);
            UpdateTabsState();

            InitUnlockRandomButton(StoreController.Database.GetProductPrice(CurrentProductType));

            productPreview.VisualizeProduct(StoreController.GetProduct(StoreController.GetSelectedProductSkinID(CurrentProductType)));

            Tween.DelayedCall(0.1f, () => pageDotsLayout.enabled = true);
        }

        private void InitPages()
        {
            pagesPool?.ReturnToPoolEverything();
            pageDotsPool?.ReturnToPoolEverything();

            pagesList = new List<StorePage>();

            pagesAmount = pagesAmountForEachProductType[(int)CurrentProductType];
            productsByGroupDictionary = StoreController.Database.GetProductsByPageDictionary(CurrentProductType);

            pagesHolderLocalPosition = Vector3.zero.SetX(currentPageIndex * -Mathf.Clamp(Screen.width, canvasWidth, Screen.width));
            pagesHolderTransform.localPosition = pagesHolderLocalPosition;

            for (int i = 0; i < pagesAmount; i++)
            {
                Transform page = pagesPool.GetPooledObject().GetComponent<Transform>();
                page.localPosition = page.localPosition.SetX(Mathf.Clamp(Screen.width, canvasWidth, Screen.width) * i);
                pagesList.Add(page.GetComponent<StorePage>());

                if (pagesAmount > 1)
                {
                    pageDotsLayout.enabled = true;
                    pageDotsImagesList.Add(pageDotsPool.GetPooledObject().GetComponent<Image>());
                    pageDotsImagesList[i].color = Color.white.SetAlpha(currentPageIndex == i ? 1f : 0.4f);
                }

                InitPage(i);
            }
        }

        private void InitPage(int pageIndex)
        {
            pagesList[pageIndex].Init(productsByGroupDictionary[(StorePageName)pageIndex]);
        }

        private void UpdateCurrentPage(bool redrawStorePage)
        {
            UpdatePagePoints();

            skinsOnCurrentPageList = productsByGroupDictionary[(StorePageName)currentPageIndex];

            closedSkinsOnCurrentPageIndexes.Clear();

            for (int i = 0; i < skinsOnCurrentPageList.Count; i++)
            {
                if (!skinsOnCurrentPageList[i].IsUnlocked() && skinsOnCurrentPageList[i].BehaviourType != BehaviourType.Dummy)
                {
                    closedSkinsOnCurrentPageIndexes.Add(i);
                }
            }

            int randomPrice = StoreController.Database.GetProductPrice(CurrentProductType);
            UpdateUnlockRandomButton(isEnoughtMoney: Coins >= randomPrice, isActive: closedSkinsOnCurrentPageIndexes.Count > 0);

            if (redrawStorePage)
            {
                pagesList[currentPageIndex].UpdatePage();
            }
        }

        private void UpdatePagePoints()
        {
            for (int i = 0; i < pageDotsImagesList.Count; i++)
            {
                pageDotsImagesList[i].color = Color.white.SetAlpha(currentPageIndex == i ? 1f : 0.4f);
            }
        }

        private IEnumerator RandomUnlockAnimation()
        {
            isAnimationRunning = true;
            getCoinsButton.interactable = false;
            unlockRandomButton.interactable = false;

            float delay = 0.05f;
            int itemToUnlockIndex = closedSkinsOnCurrentPageIndexes[Random.Range(0, closedSkinsOnCurrentPageIndexes.Count)];
            List<StoreItemUI> storeItemsList = pagesList[currentPageIndex].StoreItemsList;

            if (closedSkinsOnCurrentPageIndexes.Count > 1)
            {
                for (int i = 0; i < storeItemsList.Count; i++)
                {
                    storeItemsList[i].SetHighlightState(false);
                }

                Tween.DoFloat(0.05f, 0.5f, 3f, (float newValue) => delay = newValue);

                while (delay < 0.5f)
                {
                    closedSkinsOnCurrentPageIndexes.Remove(itemToUnlockIndex);
                    int newIndex = closedSkinsOnCurrentPageIndexes[Random.Range(0, closedSkinsOnCurrentPageIndexes.Count)];
                    closedSkinsOnCurrentPageIndexes.Add(itemToUnlockIndex);
                    itemToUnlockIndex = newIndex;

                    for (int i = 0; i < storeItemsList.Count; i++)
                    {
                        storeItemsList[i].SetHighlightState(i == itemToUnlockIndex);
                    }

                    yield return new WaitForSeconds(delay);
                }

                yield return new WaitForSeconds(delay * 0.5f);
            }

            if (StoreController.TryToBuyProduct(skinsOnCurrentPageList[itemToUnlockIndex]))
            {
                UpdateCurrentPage(true);
            }

            isAnimationRunning = false;
            getCoinsButton.interactable = true;

            UpdateUnlockRandomButton(isEnoughtMoney: Coins >= StoreController.Database.GetProductPrice(CurrentProductType), isActive: closedSkinsOnCurrentPageIndexes.Count > 0);
        }

        public void OnNewSkinSelected(StoreProduct product)
        {
            for (int i = 0; i < pagesList.Count; i++)
            {
                pagesList[i].UpdatePage();
            }
        }

        public void OnTabPressed(StoreProductType productType)
        {
            if (isAnimationRunning)
                return;

            if (CurrentProductType != productType)
            {
                InitStoreUI(productType);
            }
        }

        private void UpdateTabsState()
        {
            for (int i = 0; i < tabsList.Count; i++)
            {
                tabsList[i].SetActiveState(tabsList[i].Type == CurrentProductType);
            }
        }

        private void OnCoinsAmountChanged()
        {
            coinsText.text = CurrenciesHelper.Format(Coins);
        }

        #region Unlock Random Label

        private void InitUnlockRandomButton(int price)
        {
            unlockRandomPriceText.text = price.ToString();
        }

        public void UpdateUnlockRandomButton(bool isEnoughtMoney, bool isActive)
        {
            unlockRandomButton.interactable = isEnoughtMoney;
            unlockRandomButton.gameObject.SetActive(isActive);
        }

        #endregion

        #region Get Coins Label

        private void InitGetCoinsButton(string text)
        {
            coinsForAdsText.text = text;
        }

        private void UpdateGetCoinsButton()
        {
            unlockRandomButton.interactable = AdsManager.IsForcedAdEnabled();
        }

        #endregion

        #region Swipe

        public void OnDrag(PointerEventData data)
        {
            if (isAnimationRunning)
                return;

            if (pagesAmount == 1)
            {
                return;
            }

            float difference = data.pressPosition.x - data.position.x;
            pagesHolderTransform.localPosition = Vector3.Lerp(pagesHolderLocalPosition, pagesHolderLocalPosition - Vector3.zero.SetX(difference), 0.8f);
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (isAnimationRunning)
                return;

            if (pagesAmount == 1)
            {
                return;
            }

            float percentage = (data.pressPosition.x - data.position.x) / canvasWidth;
            float pageDeltaSign = Mathf.Sign(percentage);

            if (Mathf.Abs(percentage) >= swipeThereshold && ((pageDeltaSign < 0 && currentPageIndex > 0) || (pageDeltaSign > 0 && currentPageIndex < pagesAmount - 1)))
            {
                Vector3 newPosition = pagesHolderLocalPosition;

                currentPageIndex += (int)pageDeltaSign;
                UpdateCurrentPage(false);


                newPosition += Vector3.zero.SetX(-Mathf.Clamp(Screen.width, canvasWidth, Screen.width) * pageDeltaSign);
                pagesHolderTransform.DOLocalMove(newPosition, 0.1f).OnComplete(() => pagesHolderLocalPosition = newPosition);
            }
            else
            {
                pagesHolderTransform.DOLocalMove(pagesHolderLocalPosition, 0.1f);
            }
        }


        #endregion

        #region Buttons

        public void UnlockRandomButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (closedSkinsOnCurrentPageIndexes.Count <= 0)
                return;

            if (!isAnimationRunning)
            {
                StartCoroutine(RandomUnlockAnimation());
            }
        }

        public void GetCoinsForAdsButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            AdsManager.ShowRewardBasedVideo(AdsManager.Settings.RewardedVideoType, (bool haveReward) =>
             {
                 if (haveReward)
                 {
                     Coins += StoreController.Database.CoinsForAdsAmount;

                     int productPrice = StoreController.Database.GetProductPrice(CurrentProductType);
                     UpdateUnlockRandomButton(isEnoughtMoney: Coins >= productPrice, isActive: closedSkinsOnCurrentPageIndexes.Count > 0);
                 }
             });
        }

        public void CloseStoreButton()
        {

            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (isAnimationRunning)
                return;

            closeButton.RectTransform.GetComponent<Button>().interactable = false;
            Hide();

            Tween.DelayedCall(HIDDED_PAGE_DELAY, delegate
            {
                closeButton.RectTransform.GetComponent<Button>().interactable = true;
                UIController.ShowPage(typeof(UIMainMenu));
            });
        }

        #endregion

    }
}

// -----------------
// Store Module v 0.9.4
// -----------------
