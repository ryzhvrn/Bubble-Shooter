using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public static class ScenesMenuGenerator
    {
        [MenuItem("Tools/Editor/Generate Scenes Tab")]
        public static void GenerateMenu()
        {
            //Class strings
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEditor;");
            sb.AppendLine("using UnityEngine.SceneManagement;");
            sb.AppendLine("using UnityEditor.SceneManagement;");
            sb.AppendLine("");
            sb.AppendLine("public static class ScenesMenu");
            sb.AppendLine("{");
            sb.AppendLine("    private static void OpenScene(string path)");
            sb.AppendLine("    {");
            sb.AppendLine("        int option = EditorUtility.DisplayDialogComplex(\"Select Scene Loading Mode\", \"Select Single mode if you want to close all previous scenes and Additive if you want to add selected scene to current opened scene.\", \"Single\", \"Additive\", \"Cancel\");");
            sb.AppendLine("        switch(option)");
            sb.AppendLine("        {");
            sb.AppendLine("             case 0:");
            sb.AppendLine("                 Scene[] scenes = new Scene[SceneManager.sceneCount];");
            sb.AppendLine("                 for (int i = 0; i < scenes.Length; i++)");
            sb.AppendLine("                 {");
            sb.AppendLine("                     scenes[i] = SceneManager.GetSceneAt(i);");
            sb.AppendLine("                 }");
            sb.AppendLine("                 EditorSceneManager.SaveModifiedScenesIfUserWantsTo(scenes);");
            sb.AppendLine("                 EditorSceneManager.OpenScene(path, OpenSceneMode.Single);");
            sb.AppendLine("                 ");
            sb.AppendLine("                 break;");
            sb.AppendLine("             case 1:");
            sb.AppendLine("                 EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);");
            sb.AppendLine("                 break;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("");

            int sceneIndex = 0;
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                string sceneName = scene.path.Substring(scene.path.LastIndexOf('/') + 1);

                sb.AppendLine("    [MenuItem(\"Scenes/" + sceneName.Replace(".unity", "") + "\")]");
                sb.AppendLine("    public static void Scene" + sceneIndex + "()");
                sb.AppendLine("    {");
                sb.AppendLine("        if(Application.isPlaying)");
                sb.AppendLine("        {");
                sb.AppendLine("             SceneManager.LoadScene(" + sceneIndex + ");");
                sb.AppendLine("        }");
                sb.AppendLine("        else");
                sb.AppendLine("        {");
                sb.AppendLine("            OpenScene(\"" + scene.path + "\");");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                sb.AppendLine("");

                sceneIndex++;
            }

            sb.AppendLine("}");
            
            string menuPath = ApplicationConsts.PROJECT_FOLDER + "Watermelon Core/Scripts/Editor/";

            string fileName = "TempScenesMenu.cs";
            string fullPath = menuPath + fileName;

            //Check if path exists
            string[] pathFolders = menuPath.Split('/');
            for (int i = 0; i < pathFolders.Length; i++)
            {
                string tempPath = "";

                for (int j = 0; j < i; j++)
                {
                    tempPath += pathFolders[j] + "/";
                }
                
                if (!System.IO.Directory.Exists(tempPath + pathFolders[i]))
                {
                    System.IO.Directory.CreateDirectory(tempPath + pathFolders[i]);
                }
            }

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            System.IO.File.WriteAllText(fullPath, sb.ToString(), System.Text.Encoding.UTF8);

            AssetDatabase.Refresh();
        }
    }
}