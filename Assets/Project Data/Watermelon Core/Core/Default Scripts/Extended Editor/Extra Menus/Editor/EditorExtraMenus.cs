using UnityEngine;
using UnityEditor;
using System;

namespace Watermelon
{
    public static class EditorExtraMenus
    {
#if UNITY_EDITOR_WIN
        [MenuItem("Help/Open Persistent Folder", priority = 151)]
        public static void OpenPersistentFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.persistentDataPath.Replace("/", @"\"));
        }

        [MenuItem("Help/Open Project Folder", priority = 152)]
        public static void OpenProjectFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.dataPath.Replace("/Assets", "/").Replace("/", @"\"));
        }

        [MenuItem("Help/Open Register Path", priority = 153)]
        public static void OpenRegisterPath()
        {
            //Project location path
            string registryLocation = @"HKEY_CURRENT_USER\Software\Unity\UnityEditor\" + Application.companyName + @"\" + Application.productName + @"\";
            //Last location path
            string registryLastKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";

            try
            {
                //Set LastKey value that regedit will go directly to
                Microsoft.Win32.Registry.SetValue(registryLastKey, "LastKey", registryLocation);
                System.Diagnostics.Process.Start("regedit.exe");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        //[MenuItem("Help/Inititalize JAVA_HOME", priority = 154)]
        //public static void InitJavaHomeVariable()
        //{
        //    if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        //    {
        //        string environmentVariable = Environment.GetEnvironmentVariable("JAVA_HOME");
        //        if(environmentVariable != null)
        //        {
        //            if(!EditorUtility.DisplayDialog("Environment variable already exists", "Variable JAVA_HOME already exists. Do you wan't to rewrite it?", "Rewrite", "Cancel"))
        //            {
        //                return;
        //            }
        //        }

        //        string path = (EditorApplication.applicationPath.Replace("Unity.exe", "") + "Data/PlaybackEngines/AndroidPlayer/Tools/OpenJDK/Windows").Replace("/", @"\");

        //        Environment.SetEnvironmentVariable("JAVA_HOME", path, EnvironmentVariableTarget.User);

        //        Debug.Log("[Watermelon Core]: JAVA_HOME value has been changed - " + path);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("[Watermelon Core]: Method is allowed only for Android platform.");
        //    }
        //}
#endif
        }
}
