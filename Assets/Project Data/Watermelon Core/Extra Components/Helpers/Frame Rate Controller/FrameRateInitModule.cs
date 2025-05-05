#pragma warning disable 0414,0618

using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Helpers/Frame Rate Manager")]
    public class FrameRateInitModule : InitModule
    {
        [SerializeField] bool setFrameRateAutomatically = false;

        [Space]
        [SerializeField, HideIf("setFrameRateAutomatically")] AllowedFrameRates defaultFrameRate = AllowedFrameRates.Rate60;
        [SerializeField, HideIf("setFrameRateAutomatically")] AllowedFrameRates batterySaveFrameRate = AllowedFrameRates.Rate30;

        public override void CreateComponent(Initialiser Initialiser)
        {
            if(setFrameRateAutomatically)
            {
                
                Application.targetFrameRate = Screen.currentResolution.refreshRate;
            }
            else
            {
#if UNITY_IOS
                if(UnityEngine.iOS.Device.lowPowerModeEnabled)
                {
                    Application.targetFrameRate = (int)batterySaveFrameRate;
                }
                else
                {
                    Application.targetFrameRate = (int)defaultFrameRate;
                }    
#else
                Application.targetFrameRate = (int)defaultFrameRate;
#endif
            }
        }

        public FrameRateInitModule()
        {
            moduleName = "Frame Rate Manager";
        }

        private enum AllowedFrameRates
        {
            Rate30 = 30,
            Rate60 = 60,
            Rate90 = 90,
            Rate120 = 120,
        }
    }
}

// -----------------
// Frame Rate Manager v1.0
// -----------------

// Changelog
// v 1.0
// • Base functionality