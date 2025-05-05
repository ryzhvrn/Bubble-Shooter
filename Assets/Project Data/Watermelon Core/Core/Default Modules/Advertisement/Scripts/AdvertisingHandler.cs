namespace Watermelon
{
    public abstract class AdvertisingHandler
    {
        private AdvertisingModules moduleType;
        public AdvertisingModules ModuleType
        {
            get { return moduleType; }
        }

        protected AdsData adsSettings;

        protected bool isInitialized = false;

        public AdvertisingHandler(AdvertisingModules moduleType)
        {
            this.moduleType = moduleType;
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public abstract void Init(AdsData adsSettings);

        public abstract bool IsGDPRRequired();
        public abstract void SetGDPR(bool state);

        public abstract void ShowBanner();
        public abstract void HideBanner();
        public abstract void DestroyBanner();

        public abstract void RequestInterstitial();
        public abstract void ShowInterstitial(InterstitialCallback callback);
        public abstract bool IsInterstitialLoaded();

        public abstract void RequestRewardedVideo();
        public abstract void ShowRewardedVideo(RewardedVideoCallback callback);
        public abstract bool IsRewardedVideoLoaded();

        public abstract string GetBannerID();
        public abstract string GetInterstitialID();
        public abstract string GetRewardedVideoID();

        public delegate void RewardedVideoCallback(bool hasReward);
        public delegate void InterstitialCallback(bool isDisplayed);
    }
}

// -----------------
// Advertisement v 1.3
// -----------------