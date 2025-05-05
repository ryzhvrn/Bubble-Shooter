using UnityEngine;

namespace Watermelon
{
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] RectTransform[] panels;

        [Space]
        [SerializeField] bool ConformX = true;  // Conform to screen safe area on X-axis (default true, disable to ignore)
        [SerializeField] bool ConformY = true;  // Conform to screen safe area on Y-axis (default true, disable to ignore)

        private Rect LastSafeArea = new Rect (0, 0, 0, 0);
        private Vector2Int LastScreenSize = new Vector2Int (0, 0);
        private ScreenOrientation LastOrientation = ScreenOrientation.AutoRotation;

        private void Awake ()
        {
            Refresh ();
        }

        [Button("Refresh")]
        private void Refresh ()
        {
            Rect safeArea = Screen.safeArea;

            if (safeArea != LastSafeArea || Screen.width != LastScreenSize.x || Screen.height != LastScreenSize.y || Screen.orientation != LastOrientation)
            {
                LastScreenSize.x = Screen.width;
                LastScreenSize.y = Screen.height;
                LastOrientation = Screen.orientation;

                ApplySafeArea (safeArea);
            }
        }

        private void ApplySafeArea (Rect r)
        {
            LastSafeArea = r;

            // Ignore x-axis?
            if (!ConformX)
            {
                r.x = 0;
                r.width = Screen.width;
            }

            // Ignore y-axis?
            if (!ConformY)
            {
                r.y = 0;
                r.height = Screen.height;
            }

            // Check for invalid screen startup state on some Samsung devices (see below)
            if (Screen.width > 0 && Screen.height > 0)
            {
                // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
                Vector2 anchorMin = r.position;
                Vector2 anchorMax = r.position + r.size;

                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                // Fix for some Samsung devices (e.g. Note 10+, A71, S20) where Refresh gets called twice and the first time returns NaN anchor coordinates
                // See https://forum.unity.com/threads/569236/page-2#post-6199352
                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
                {
                    for(int i = 0; i < panels.Length; i++)
                    {
                        panels[i].anchorMin = anchorMin;
                        panels[i].anchorMax = anchorMax;
                    }
                }
            }
        }
    }
}
