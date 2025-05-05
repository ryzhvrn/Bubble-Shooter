using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace Watermelon
{
    public static class ActionsMenu
    {
#if UNITY_EDITOR

        #region Save Management

        [MenuItem("Actions/Remove Save  [not runtime]", priority = 1)]
        private static void RemoveSave()
        {
            if (!Application.isPlaying)
            {
                Serializer.DeleteFileAtPDP("save");

                PlayerPrefs.DeleteAll();
            }
        }

        #endregion

        #region Currencies

        //[MenuItem("Actions/Lots of Money", priority = 21)]
        //private static void LotsOfMoney()
        //{
        //    if (Application.isPlaying)
        //    {
        //        CurrenciesController.Set(Currency.Type.Coin, 2000000);
        //    }
        //}

        //[MenuItem("Actions/No Money", priority = 22)]
        //private static void NoMoney()
        //{
        //    if (Application.isPlaying)
        //    {
        //        CurrenciesController.Set(Currency.Type.Coin, 0);
        //    }
        //}

        //[MenuItem("Actions/Lots of Gems", priority = 23)]
        //private static void LotsOfGems()
        //{
        //    if (Application.isPlaying)
        //    {
        //        CurrenciesController.Set(Currency.Type.Gems, 100);
        //    }
        //}

        //[MenuItem("Actions/No Gems", priority = 24)]
        //private static void NoGems()
        //{
        //    if (Application.isPlaying)
        //    {
        //        CurrenciesController.Set(Currency.Type.Gems, 0);
        //    }
        //}

        #endregion

        #region Levels and Scenes

        //[MenuItem("Actions/Prev Level (menu) [P]", priority = 71)]
        //public static void PrevLevel()
        //{
        //    LevelController.PrevLevelDev();
        //}

        //[MenuItem("Actions/Next Level (menu) [N]", priority = 72)]
        //public static void NextLevel()
        //{
        //    LevelController.NextLevelDev();
        //}

        [MenuItem("Actions/Game Scene", priority = 100)]
        private static void GameScene()
        {
            EditorSceneManager.OpenScene(@"Assets\Project Data\Game\Scenes\Game.unity");
        }

        #endregion

        #region Other

        //[MenuItem("Actions/Print Shorcuts", priority = 150)]
        //private static void PrintShortcuts()
        //{
        //    Debug.Log("H - heal player \nD - toggle player damage \nN - skip level\nR - skip room\n\n");
        //}

        #endregion

#endif
    }
}