﻿using UnityEngine;

#if MODULE_UNITYADS
using UnityEngine.Advertisements;
#endif

namespace Watermelon
{
#if MODULE_UNITYADS
    public class UnityAdsLegacyHandler : AdvertisingHandler
    {
        private const int INIT_CHECK_MAX_ATTEMPT_AMOUNT = 5;

        private static string placementBannerID;
        private static string placementInterstitialID;
        private static string placementRewardedVideoID;
        private static string appId;

        private bool interstitialIsLoaded;
        private bool rewardVideoIsLoaded;
        private int initializationAttemptCount = 0;

        private UnityAdvertismentListner unityAdvertisment;

        public UnityAdsLegacyHandler(AdvertisingModules moduleType) : base(moduleType) { }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;

            placementBannerID = GetBannerID();
            placementInterstitialID = GetInterstitialID();
            placementRewardedVideoID = GetRewardedVideoID();
            appId = GetUnityAdsAppID();

            if(isInitialized || Advertisement.isInitialized)
            {
                Debug.LogError("[AdsManager]: Unity Ads Legacy is already initialized!");

                return;
            }

            if(!Advertisement.isSupported)
            {
                Debug.LogError("[AdsManager]: Unity Ads Legacy isn't supported!");

                return;
            }

            unityAdvertisment = Initialiser.InitialiserGameObject.AddComponent<UnityAdvertismentListner>();
            unityAdvertisment.Init(adsSettings, this);

            Advertisement.Initialize(appId, adsSettings.TestMode, unityAdvertisment);

            Advertisement.Banner.SetPosition((UnityEngine.Advertisements.BannerPosition)adsSettings.UnityAdsSettings.BannerPosition);

            InitGDPR(AdsManager.GetGDPRState());

            if (adsSettings.SystemLogs)
            {
                Debug.Log("[AdsManager]: Unity Ads initialized: " + Advertisement.isInitialized);
                Debug.Log("[AdsManager]: Unity Ads is supported: " + Advertisement.isSupported);
                Debug.Log("[AdsManager]: Unity Ads test mode enabled: " + Advertisement.debugMode);
                Debug.Log("[AdsManager]: Unity Ads version: " + Advertisement.version);
            }
        }

        public override void DestroyBanner()
        {
            if (!isInitialized)
                return;

            Advertisement.Banner.Hide(true);
        }

        public override void HideBanner()
        {
            if (!isInitialized)
                return;

            Advertisement.Banner.Hide(false);
        }

        public override void RequestInterstitial()
        {
            if (!isInitialized)
                return;

            Advertisement.Load(placementInterstitialID, unityAdvertisment);
        }

        public override void RequestRewardedVideo()
        {
            if (!isInitialized)
                return;

            Advertisement.Load(placementRewardedVideoID, unityAdvertisment);
        }

        public override void ShowBanner()
        {
            if (!isInitialized)
                return;

            Advertisement.Banner.Show(placementBannerID);
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            if (!isInitialized)
            {
                AdsManager.ExecuteInterstitialCallback(false);

                return;
            }

            Advertisement.Show(placementInterstitialID, unityAdvertisment);
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (!isInitialized)
                return;

            Advertisement.Show(placementRewardedVideoID, unityAdvertisment);
        }

        public override bool IsInterstitialLoaded()
        {
#if UNITY_EDITOR
            // Requires to show Unity Ads dummy
            return true;
#else
            return interstitialIsLoaded;
#endif
        }

        public override bool IsRewardedVideoLoaded()
        {
#if UNITY_EDITOR
            // Requires to show Unity Ads dummy
            return true;
#else
            return rewardVideoIsLoaded;
#endif
        }

        public string GetUnityAdsAppID()
        {
#if UNITY_ANDROID
            return adsSettings.UnityAdsSettings.AndroidAppID;
#elif UNITY_IOS
            return adsSettings.UnityAdsSettings.IOSAppID;
#else
            return string.Empty;
#endif
        }

        public override void SetGDPR(bool state)
        {
            InitGDPR(state);
        }

        private void InitGDPR(bool state)
        {
            string gdprState = state ? "true" : "false";

            MetaData gdprMetaData = new MetaData("gdpr");
            gdprMetaData.Set("consent", gdprState);
            Advertisement.SetMetaData(gdprMetaData);

            MetaData privacyMetaData = new MetaData("privacy");
            privacyMetaData.Set("consent", gdprState);
            Advertisement.SetMetaData(privacyMetaData);
        }

        public override bool IsGDPRRequired()
        {
            return true;
        }

        public override string GetBannerID()
        {
#if UNITY_ANDROID
            return adsSettings.UnityAdsSettings.AndroidBannerID;
#elif UNITY_IOS
            return adsSettings.UnityAdsSettings.IOSBannerID;
#else
            return string.Empty;
#endif
        }

        public override string GetInterstitialID()
        {
#if UNITY_ANDROID
            return adsSettings.UnityAdsSettings.AndroidInterstitialID;
#elif UNITY_IOS
            return adsSettings.UnityAdsSettings.IOSInterstitialID;
#else
            return string.Empty;
#endif
        }

        public override string GetRewardedVideoID()
        {
#if UNITY_ANDROID
            return adsSettings.UnityAdsSettings.AndroidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.UnityAdsSettings.IOSRewardedVideoID;
#else
            return string.Empty;
#endif
        }

        private class UnityAdvertismentListner : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
        {
            private UnityAdsLegacyHandler adsHandler;
            private AdsData adsSettings;

            public void Init(AdsData adsSettings, UnityAdsLegacyHandler adsHandler)
            {
                this.adsSettings = adsSettings;
                this.adsHandler = adsHandler;
            }

            public void OnInitializationComplete()
            {
                adsHandler.isInitialized = true;

                if (AdsManager.OnAdsModuleInitializedEvent != null)
                    AdsManager.OnAdsModuleInitializedEvent.Invoke(adsHandler.ModuleType);

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: Unity Ads is initialized!");
            }

            public void OnInitializationFailed(UnityAdsInitializationError error, string message)
            {
                adsHandler.initializationAttemptCount++;

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnInitializationFailed event error:" + error.ToString() + "    message: " + message);

                if (adsHandler.initializationAttemptCount <= INIT_CHECK_MAX_ATTEMPT_AMOUNT)
                {
                    Advertisement.Initialize(appId, adsSettings.TestMode, this);
                }
                else
                {
                    if (adsSettings.SystemLogs)
                        Debug.Log("[AdsManager]: OnInitializationFailed in every attempt");
                }
            }

            public void OnUnityAdsAdLoaded(string placementId)
            {
                if (placementId.Equals(placementBannerID))
                {
                    Advertisement.Banner.Show(placementBannerID);

                    if (adsSettings.SystemLogs)
                        Debug.Log("[AdsManager]: OnUnityAdsAdLoaded - banner loaded");
                }
                else if (placementId.Equals(placementInterstitialID))
                {
                    adsHandler.interstitialIsLoaded = true;

                    if (adsSettings.SystemLogs)
                        Debug.Log("[AdsManager]: OnUnityAdsAdLoaded - interstitial loaded");
                }
                else if (placementId.Equals(placementRewardedVideoID))
                {
                    adsHandler.rewardVideoIsLoaded = true;

                    if (adsSettings.SystemLogs)
                        Debug.Log("[AdsManager]: OnUnityAdsAdLoaded - rewardVideo loaded");
                }
            }

            public void OnUnityAdsDidError(string message)
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsDidError - " + message);
            }

            public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
            {
                if (placementId == placementInterstitialID)
                {
                    adsHandler.interstitialIsLoaded = false;

                    if (AdsManager.OnInterstitialHiddenEvent != null)
                        AdsManager.OnInterstitialHiddenEvent.Invoke();

                    AdsManager.ExecuteInterstitialCallback(showResult == ShowResult.Finished);

                    AdsManager.ResetInterstitialDelayTime();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    adsHandler.rewardVideoIsLoaded = false;

                    bool state = showResult == ShowResult.Finished;

                    // Reward the player
                    AdsManager.ExecuteRewardVideoCallback(state);

                    if (state && AdsManager.OnRewardedAdReceivedRewardEvent != null)
                        AdsManager.OnRewardedAdReceivedRewardEvent.Invoke();

                    if (AdsManager.OnRewardedAdHiddenEvent != null)
                        AdsManager.OnRewardedAdHiddenEvent.Invoke();

                    AdsManager.ResetInterstitialDelayTime();
                }

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsDidFinish - " + placementId + ". Result - " + showResult);
            }

            public void OnUnityAdsDidStart(string placementId)
            {
                if (placementId == placementInterstitialID)
                {
                    adsHandler.interstitialIsLoaded = false;

                    if (AdsManager.OnInterstitialLoadedEvent != null)
                        AdsManager.OnInterstitialLoadedEvent.Invoke();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    adsHandler.rewardVideoIsLoaded = false;

                    if (AdsManager.OnRewardedAdLoadedEvent != null)
                        AdsManager.OnRewardedAdLoadedEvent.Invoke();
                }

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsDidStart - " + placementId);
            }

            public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
            {
                if (placementId == placementInterstitialID)
                {
                    adsHandler.interstitialIsLoaded = false;

                    if (AdsManager.OnInterstitialLoadFailedEvent != null)
                        AdsManager.OnInterstitialLoadFailedEvent.Invoke();

                    AdsManager.RequestInterstitial();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    adsHandler.rewardVideoIsLoaded = false;

                    if (AdsManager.OnRewardedAdLoadFailedEvent != null)
                        AdsManager.OnRewardedAdLoadFailedEvent.Invoke();

                    AdsManager.RequestRewardBasedVideo();
                }

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsFailedToLoad - " + placementId + ". Error - " + error + " .Message: " + message );
            }

            public void OnUnityAdsReady(string placementId)
            {
                if (placementId == placementBannerID)
                {
                    if (AdsManager.OnBannerAdLoadedEvent != null)
                        AdsManager.OnBannerAdLoadedEvent.Invoke();
                }
                else if (placementId == placementInterstitialID)
                {
                    if (AdsManager.OnInterstitialLoadedEvent != null)
                        AdsManager.OnInterstitialLoadedEvent.Invoke();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    if (AdsManager.OnRewardedAdLoadedEvent != null)
                        AdsManager.OnRewardedAdLoadedEvent.Invoke();
                }

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsReady - " + placementId);
            }

            public void OnUnityAdsShowClick(string placementId)
            {
                if (placementId == placementInterstitialID)
                {
                    if (AdsManager.OnInterstitialClickedEvent != null)
                        AdsManager.OnInterstitialClickedEvent.Invoke();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    if (AdsManager.OnRewardedAdClickedEvent != null)
                        AdsManager.OnRewardedAdClickedEvent.Invoke();
                }

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsFailedToLoad - " + placementId);
            }

            public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
            {
                if (placementId == placementInterstitialID)
                {
                    adsHandler.interstitialIsLoaded = false;

                    if (AdsManager.OnInterstitialHiddenEvent != null)
                        AdsManager.OnInterstitialHiddenEvent.Invoke();

                    AdsManager.ExecuteInterstitialCallback(showCompletionState == UnityAdsShowCompletionState.COMPLETED);

                    AdsManager.ResetInterstitialDelayTime();

                    AdsManager.RequestInterstitial();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    adsHandler.rewardVideoIsLoaded = false;

                    bool state = showCompletionState == UnityAdsShowCompletionState.COMPLETED;

                    // Reward the player
                    AdsManager.ExecuteRewardVideoCallback(state);

                    if (state && AdsManager.OnRewardedAdReceivedRewardEvent != null)
                        AdsManager.OnRewardedAdReceivedRewardEvent.Invoke();

                    if (AdsManager.OnRewardedAdHiddenEvent != null)
                        AdsManager.OnRewardedAdHiddenEvent.Invoke();

                    AdsManager.ResetInterstitialDelayTime();

                    AdsManager.RequestRewardBasedVideo();
                }

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsDidFinish - " + placementId + ". Result - " + showCompletionState);
            }

            public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
            {
                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsShowFailure - " + placementId + " - " + message);

                if(error == UnityAdsShowError.NOT_READY)
                {
                    if (placementId == placementInterstitialID)
                    {
                        adsHandler.interstitialIsLoaded = false;

                        AdsManager.RequestInterstitial();
                    }
                    else if (placementId == placementRewardedVideoID)
                    {
                        adsHandler.rewardVideoIsLoaded = false;

                        AdsManager.RequestRewardBasedVideo();
                    }
                }
            }

            public void OnUnityAdsShowStart(string placementId)
            {
                if (placementId == placementInterstitialID)
                {
                    if (AdsManager.OnInterstitialLoadedEvent != null)
                        AdsManager.OnInterstitialLoadedEvent.Invoke();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    if (AdsManager.OnRewardedAdLoadedEvent != null)
                        AdsManager.OnRewardedAdLoadedEvent.Invoke();
                }

                if (adsSettings.SystemLogs)
                    Debug.Log("[AdsManager]: OnUnityAdsShowStart - " + placementId);
            }
        }
    }
#endif
        }

// -----------------
// Advertisement v 1.3
// -----------------