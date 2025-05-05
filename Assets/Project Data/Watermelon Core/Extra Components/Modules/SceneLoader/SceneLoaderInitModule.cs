#pragma warning disable 649

using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Modules/Scene Loader")]
    public class SceneLoaderInitModule : InitModule
    {
        [SerializeField] bool loadSceneOnStart = false;

        [ShowIf("loadSceneOnStart")]
        [Scenes]
        [SerializeField] string firstScene;

        public override void CreateComponent(Initialiser initialiser)
        {
            SceneLoader sceneLoader = initialiser.gameObject.AddComponent<SceneLoader>();

            sceneLoader.InitModule(initialiser, loadSceneOnStart ? firstScene : string.Empty);
        }

        public SceneLoaderInitModule()
        {
            moduleName = "Scene Loader";
        }
    }
}

// -----------------
// Scene Loader v 0.1
// -----------------