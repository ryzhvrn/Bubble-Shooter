using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class ScenesMenu
{
    private static void OpenScene(string path)
    {
        int option = EditorUtility.DisplayDialogComplex("Select Scene Loading Mode", "Select Single mode if you want to close all previous scenes and Additive if you want to add selected scene to current opened scene.", "Single", "Additive", "Cancel");
        switch(option)
        {
             case 0:
                 Scene[] scenes = new Scene[SceneManager.sceneCount];
                 for (int i = 0; i < scenes.Length; i++)
                 {
                     scenes[i] = SceneManager.GetSceneAt(i);
                 }
                 EditorSceneManager.SaveModifiedScenesIfUserWantsTo(scenes);
                 EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                 
                 break;
             case 1:
                 EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                 break;
        }
    }

    [MenuItem("Scenes/Game")]
    public static void Scene0()
    {
        if(Application.isPlaying)
        {
             SceneManager.LoadScene(0);
        }
        else
        {
            OpenScene("Assets/Project Data/Game/Scenes/Game.unity");
        }
    }

}
