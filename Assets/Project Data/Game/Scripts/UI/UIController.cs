using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class UIController : MonoBehaviour
    {
        // TODO: Add editor type filter attribute
        private Type defaultPage = typeof(UIMainMenu);

        [SerializeField] UIPage[] pages;

        private static Type currentPage;

        private static Dictionary<Type, UIPage> pagesLink = new Dictionary<Type, UIPage>();

        private static bool isTablet;
        public static bool IsTablet => isTablet;

        private static Canvas mainCanvas;
        public static Canvas MainCanvas => mainCanvas;
        public static CanvasScaler CanvasScaler { get; private set; }

        public static OnPageOpenedCallback OnPageOpenedEvent;
        public static OnPageClosedCallback OnPageClosedEvent;

        public static UIGame GamePage => (UIGame)pagesLink[typeof(UIGame)];

        private void Awake()
        {
            mainCanvas = GetComponent<Canvas>();
            CanvasScaler = GetComponent<CanvasScaler>();

            isTablet = GetTabletStage();

            CanvasScaler.matchWidthOrHeight = isTablet ? 1 : 0;

            pagesLink = new Dictionary<Type, UIPage>();
            for (int i = 0; i < pages.Length; i++)
            {
                pagesLink.Add(pages[i].GetType(), pages[i]);
            }
        }

        private void Start()
        {
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Init();
                pages[i].Canvas.enabled = false;
            }

            ShowPage(defaultPage);
        }

        public static void ShowPage(Type pageType, bool immediately = false)
        {

            UIPage page = pagesLink[pageType];
            page.Canvas.enabled = true;

            if (!immediately)
            {
                page.Show();
            }
            else
            {
                page.ShowImmediately();
            }

            if (OnPageOpenedEvent != null)
                OnPageOpenedEvent.Invoke(pagesLink[pageType], pageType);

            currentPage = pageType;
        }

        public static void HidePage(Type pageType, bool immediately = false)
        {
            if (!immediately)
            {
                pagesLink[pageType].Hide();
            }
            else
            {
                pagesLink[pageType].HideImmediately();
            }

            if (OnPageClosedEvent != null)
                OnPageClosedEvent.Invoke(pagesLink[pageType], pageType);

            // DEV
            currentPage = typeof(UIGame);
        }

        public static T GetPage<T>() where T : UIPage
        {
            return pagesLink[typeof(T)] as T;
        }

        private static bool GetTabletStage()
        {
#if UNITY_IOS
            bool deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
            if (deviceIsIpad)
                return true;

            return false;
#else
            var aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
            return GetDeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f;
#endif
        }

        public static float GetDeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        public delegate void OnPageOpenedCallback(UIPage page, Type pageType);
        public delegate void OnPageClosedCallback(UIPage page, Type pageType);
    }
}