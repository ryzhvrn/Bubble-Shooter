#pragma warning disable 649

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Watermelon.BubbleShooter;

namespace Watermelon.LevelSystem
{
    public class EditorSceneController : MonoBehaviour
    {

#if UNITY_EDITOR
        private static EditorSceneController instance;
        public static EditorSceneController Instance { get => instance; }

        [SerializeField] private GameObject container;
        public GameObject Container { set => container = value; }

        List<LevelStage> levelStages;

        public EditorSceneController()
        {
            instance = this;
            levelStages = new List<LevelStage>();
        }

        //used when user spawns objects by clicking on object name in level editor
        public void Spawn(GameObject prefab, Vector3 defaultPosition)
        {
            GameObject gameObject = Instantiate(prefab, defaultPosition, Quaternion.identity, container.transform);
            gameObject.name = prefab.name + " ( Child # " + container.transform.childCount + ")";
            SelectGameObject(gameObject);
        }

        public void SpawnStages(LevelStage[] stages)
        {
            Transform nextStagePos = container.transform;
            GameObject gameObject;

            for (int i = 0; i < stages.Length; i++)
            {
                if(i > 0)
                {
                    nextStagePos = levelStages[i - 1].NextStagePosition;
                }

                gameObject = Instantiate(stages[i].gameObject, nextStagePos.transform.position, nextStagePos.transform.rotation, container.transform);
                gameObject.hideFlags = HideFlags.DontSave;
                Selection.activeGameObject = gameObject;
                levelStages.Add(gameObject.GetComponent<LevelStage>());
            }
        }

        public void SelectGameObject(GameObject selectedGameObject, bool frame = false)
        {
            Selection.activeGameObject = selectedGameObject;

            if (frame)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        public void SelectStage(int index)
        {
            Selection.activeGameObject = container.transform.GetChild(index).gameObject;
        }


        public void Clear()
        {
            levelStages.Clear();

            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }
        }

        


#endif
    }
}
