#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Watermelon
{
    //Current version v1.0.2
    [DefaultExecutionOrder(5)]
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;
                
        private static string currentScene;
        public static string CurrentScene
        {
            get { return currentScene; }
        }

        private string prevScene;
        public static string PrevScene
        {
            get { return instance.prevScene; }
        }
        
        private List<SceneEvent> sceneOpenEvents = new List<SceneEvent>();
        private List<SceneEvent> sceneLeaveEvents = new List<SceneEvent>();

        private Image fadePanel;

        private SceneLoaderCallback onSceneChanged;
        public static SceneLoaderCallback OnSceneChanged
        {
            get { return instance.onSceneChanged; }
            set { instance.onSceneChanged = value; }
        }

        public void InitModule(Initialiser initialiser, string firstScene = "")
        {
            if(instance != null)
            {
                Debug.LogWarning("[SceneLoader]: Module already exists!");

                Destroy(this);

                return;
            }

            instance = this;

            // Create panel
            CreateFadePanel(Initialiser.SystemCanvas.transform);

            currentScene = SceneManager.GetActiveScene().name;

            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            if(!string.IsNullOrEmpty(firstScene))
                LoadScene(firstScene);
        }

        private void CreateFadePanel(Transform parent)
        {
            GameObject panelGameObject = new GameObject("FadePanel", typeof(RectTransform));
            panelGameObject.transform.SetParent(parent);

            RectTransform panelRectTransform = panelGameObject.GetComponent<RectTransform>();
            panelRectTransform.localScale = Vector3.one;
            panelRectTransform.localPosition = Vector3.zero;

            panelRectTransform.anchorMin = Vector3.zero;
            panelRectTransform.anchorMax = Vector3.one;

            panelRectTransform.sizeDelta = Vector2.zero;

            Image panelImage = panelGameObject.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0);
            panelImage.raycastTarget = true;

            panelGameObject.SetActive(false);

            fadePanel = panelImage;
        }

        public static void OnSceneOpened(string scene, SceneCallback callback, bool callOnce = false)
        {
            instance.sceneOpenEvents.Add(new SceneEvent(scene, callback, callOnce));
        }

        public static void OnSceneLeave(string scene, SceneCallback callback, bool callOnce = false)
        {
            instance.sceneLeaveEvents.Add(new SceneEvent(scene, callback, callOnce));
        }

        private void OnActiveSceneChanged(Scene prevScene, Scene currentScene)
        {
            int eventsCount = sceneOpenEvents.Count;
            for (int i = eventsCount - 1; i >= 0; i--)
            {
                if (sceneOpenEvents[i].scene == currentScene.name)
                {
                    sceneOpenEvents[i].callback.Invoke();

                    if (sceneOpenEvents[i].callOnce)
                        sceneOpenEvents.RemoveAt(i);
                }
            }
        }

        public static void ReloadScene(SceneTransition transition = SceneTransition.Fade)
        {
            LoadScene(currentScene, transition);
        }

        public static void LoadScene(string sceneName, SceneTransition transition = SceneTransition.Fade)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                string currentSceneName = currentScene;

                int eventsCount = instance.sceneLeaveEvents.Count;
                for (int i = eventsCount - 1; i >= 0; i--)
                {
                    if (instance.sceneLeaveEvents[i].scene == currentSceneName)
                    {
                        instance.sceneLeaveEvents[i].callback.Invoke();

                        if (instance.sceneLeaveEvents[i].callOnce)
                            instance.sceneLeaveEvents.RemoveAt(i);
                    }
                }

                Debug.Log("[SceneLoader]: Loading scene: " + sceneName);

                if (transition == SceneTransition.Fade)
                {
                    FadePanel(delegate
                    {
                        Tween.RemoveAll();

                        if (instance.onSceneChanged != null)
                        {
                            instance.onSceneChanged();
                        }

                        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                    });
                }
                else
                {
                    if (instance.onSceneChanged != null)
                    {
                        instance.onSceneChanged();
                    }

                    SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                }

                instance.prevScene = currentScene;
                currentScene = sceneName;
            }
            else
            {
                Debug.LogError("[SceneLoader]: Scene " + sceneName + " can't be found!");
            }
        }
        
        public static void HideFadePanel()
        {
            instance.fadePanel.color.SetAlpha(0);
            instance.fadePanel.raycastTarget = false;
            instance.fadePanel.gameObject.SetActive(false);
        }

        public static void FadePanel(Tween.TweenCallback callback)
        {
            instance.fadePanel.raycastTarget = true;
            instance.fadePanel.color.SetAlpha(0);
            instance.fadePanel.gameObject.SetActive(true);
            instance.fadePanel.DOFade(1, 0.5f, unscaledTime: true).OnComplete(delegate
            {
                callback.Invoke();

                instance.fadePanel.DOFade(0, 0.5f, unscaledTime: true).OnComplete(delegate
                {
                    instance.fadePanel.color.SetAlpha(0);
                    instance.fadePanel.raycastTarget = false;
                    instance.fadePanel.gameObject.SetActive(false);
                });
            });
        }

        public delegate void SceneCallback();
        public delegate void SceneLoaderCallback();

        public enum SceneTransition
        {
            None,
            Fade,
        }

        private class SceneEvent
        {
            public string scene;
            public SceneCallback callback;

            public bool callOnce;

            public SceneEvent(string scene, SceneCallback callback, bool callOnce = false)
            {
                this.scene = scene;
                this.callback = callback;
                this.callOnce = callOnce;
            }
        }
    }
}

//Changelog
//v1.0.0 - Base version
//v1.0.1 - Custom events, transition
//v1.0.2 - Fixed scene opening