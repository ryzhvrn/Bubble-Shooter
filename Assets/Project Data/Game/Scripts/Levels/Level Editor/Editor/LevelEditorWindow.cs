#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Text;
using Watermelon.LevelSystem;
using UnityEditorInternal;
using System.Collections.Generic;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    public class LevelEditorWindow : LevelEditorBase
    {

        //Window configuration
        private const float WINDOW_MIN_WIDTH = 600;
        private const float WINDOW_MIN_HEIGHT = 560;
        private const float WINDOW_MAX_WIDTH = 800;
        private const float WINDOW_MAX_HEIGHT = 1200;

        //Level database fields
        private const string LEVELS_PROPERTY_NAME = "levels";
        private const string STAGES_PROPERTY_NAME = "stagesForGeneration";
        private SerializedProperty levelsProperty;
        private SerializedProperty stagesProperty;


        //sidebar
        private LevelRepresentation selectedLevelRepresentation;
        private const int SIDEBAR_WIDTH = 240;

        private const string REMOVE_SELECTION = "Remove selection";
        private const string SELECT_LEVEL = "Set this level as current level";

        //new stuff
        private GameObject editorGameobject;
        private ReorderableList levelsList;
        private int stagesCount = 5;
        private Vector2 levelScrollVector;
        private bool[] isUsed;
        private List<int> randomIndexesList;

        protected override string LEVELS_FOLDER_NAME => "Level Stages";

        protected override string LEVELS_DATABASE_FOLDER_PATH => "Assets/Project Data/Content/Level System";

        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            builder.KeepWindowOpenOnScriptReload(true);
            builder.SetWindowMinSize(new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT));
            builder.SetContentMaxSize(new Vector2(WINDOW_MAX_WIDTH, WINDOW_MAX_HEIGHT));
            builder.SetWindowMaxSize(new Vector2(WINDOW_MAX_WIDTH, WINDOW_MAX_HEIGHT));
            return builder.Build();
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelsDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(LevelData);
        }

        protected override void ReadLevelDatabaseFields()
        {
            levelsProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
            stagesProperty = levelsDatabaseSerializedObject.FindProperty(STAGES_PROPERTY_NAME);
            isUsed = new bool[stagesProperty.arraySize];
            SetUsage(true);
        }

        protected override void InitialiseVariables()
        {
            SetUpEditorSceneController();
            SetUpLevelsList();
            PrefsSettings.InitEditor();
            randomIndexesList = new List<int>();
        }



        private void SetUpEditorSceneController()
        {
            editorGameobject = new GameObject("[Level editor]");
            editorGameobject.hideFlags = HideFlags.DontSave;
            EditorSceneController editorSceneController = editorGameobject.AddComponent<EditorSceneController>();
            editorSceneController.Container = editorGameobject;

        }

        private void OnDestroy()
        {
            DestroyImmediate(editorGameobject);
        }

        private void SetUpLevelsList()
        {
            levelsList = new ReorderableList(levelsDatabaseSerializedObject, levelsProperty, true, true, true, true);
            levelsList.onAddCallback = AddCallback;
            levelsList.drawElementCallback = ElementCallback;
            levelsList.onRemoveCallback = RemoveCallback;
            levelsList.drawHeaderCallback = HeaderCallback;
            levelsList.onSelectCallback = LevelSelectedCallback;
        }

        private void AddCallback(ReorderableList list)
        {
            levelsProperty.arraySize++;

            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(levelsProperty.arraySize - 1));
            selectedLevelRepresentation.Clear();
            levelsDatabaseSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            levelsList.Select(levelsProperty.arraySize - 1);
        }

        private void ElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            GUI.Label(rect, "Level #" + (index + 1));
        }

        private void RemoveCallback(ReorderableList list)
        {
            if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to remove level #" + (list.index + 1) + "?", "Yes", "Cancel"))
            {
                int temp = levelsList.index;
                selectedLevelRepresentation = null;
                levelsList.index = -1;
                levelsProperty.DeleteArrayElementAtIndex(temp);
                levelsDatabaseSerializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

        private void HeaderCallback(Rect rect)
        {
            GUI.Label(rect, "Levels amount: " + levelsProperty.arraySize);
        }


        private void LevelSelectedCallback(ReorderableList list)
        {
            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(list.index));
            LoadLevel();
        }

        protected override void Styles()
        {
        }

        #region unusedStuff
        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            return string.Empty;
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
        }

        #endregion




        protected override void DrawContent()
        {
            DisplayLevelsTab();
        }




        private void DisplayLevelsTab()
        {

            EditorGUILayout.BeginHorizontal();
            //sidebar 
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(SIDEBAR_WIDTH), GUILayout.ExpandHeight(true));
            levelsList.DoLayoutList();
            DisplaySidebarButtons();
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            //level content
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DisplaySelectedLevel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DisplaySidebarButtons()
        {

            if (GUILayout.Button(REMOVE_SELECTION, EditorStylesExtended.button_01))
            {
                levelsList.ClearSelection();
                ClearScene();
            }
        }

        private static void ClearScene()
        {
            EditorSceneController.Instance.Clear();
        }


        private void DisplaySelectedLevel()
        {
            if (selectedLevelRepresentation == null)
            {
                return;
            }

            levelScrollVector = EditorGUILayout.BeginScrollView(levelScrollVector);

            EditorGUILayout.BeginVertical();
            CurrentLevelSection();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            GenerateSection();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();


        }



        private void CurrentLevelSection()
        {
            EditorGUILayout.LabelField("Level " + (levelsList.index + 1).ToString(), EditorStylesExtended.label_large_bold);

            for (int i = 0; i < selectedLevelRepresentation.stagesProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(selectedLevelRepresentation.stagesProperty.GetArrayElementAtIndex(i));
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Change"))
                {
                    GenericMenu menu = new GenericMenu();
                    UnityEngine.Object currentObjectRef = selectedLevelRepresentation.stagesProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                    UnityEngine.Object potentialObjectRef;

                    for (int j = 0; j < stagesProperty.arraySize; j++)
                    {
                        potentialObjectRef = stagesProperty.GetArrayElementAtIndex(j).objectReferenceValue;

                        if (currentObjectRef == potentialObjectRef)
                        {
                            menu.AddDisabledItem(new GUIContent(currentObjectRef.name));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent(potentialObjectRef.name), false, MenuFunction, new Vector2Int(i, j));
                        }
                    }

                    menu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();
            }



            if (GUILayout.Button(SELECT_LEVEL, EditorStylesExtended.button_01))
            {
                PrefsSettings.SetInt(PrefsSettings.Key.LevelNumber, levelsList.index + 1);
            }
        }

        private void MenuFunction(object data)
        {
            Vector2Int indexes = (Vector2Int)data;
            selectedLevelRepresentation.stagesProperty.GetArrayElementAtIndex(indexes.x).objectReferenceValue = stagesProperty.GetArrayElementAtIndex(indexes.y).objectReferenceValue;
            SaveLevel();
            EditorSceneController.Instance.SelectStage(indexes.x);
        }

        private void GenerateSection()
        {
            EditorGUILayout.LabelField("Level Generation", EditorStylesExtended.label_large_bold);


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("isUsed", GUILayout.MaxWidth(40));

            if (GUILayout.Button("All", GUILayout.MaxWidth(40)))
            {
                SetUsage(true);
            }

            if (GUILayout.Button("None", GUILayout.MaxWidth(40)))
            {
                SetUsage(false);
            }

            EditorGUILayout.Space();

            stagesProperty.arraySize = EditorGUILayout.IntField("Stages For Generation", stagesProperty.arraySize);

            if (stagesProperty.arraySize != isUsed.Length)
            {
                bool[] newArray = new bool[stagesProperty.arraySize];
                int minLength = Mathf.Min(newArray.Length, isUsed.Length);

                for (int i = 0; i < minLength; i++)
                {
                    newArray[i] = isUsed[i];
                }

                isUsed = newArray;
            }

            EditorGUILayout.EndHorizontal();



            for (int i = 0; i < stagesProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                isUsed[i] = EditorGUILayout.ToggleLeft("Element " + i, isUsed[i], GUILayout.MaxWidth(100));
                EditorGUILayout.PropertyField(stagesProperty.GetArrayElementAtIndex(i), GUIContent.none);

                EditorGUILayout.EndHorizontal();
            }



            stagesCount = EditorGUILayout.IntField("Stages count:", stagesCount);

            if (GUILayout.Button("Generate", EditorStylesExtended.button_01))
            {
                GenerateLevel();
            }

        }

        private void SetUsage(bool value)
        {
            for (int i = 0; i < isUsed.Length; i++)
            {
                isUsed[i] = value;
            }
        }

        private void GenerateLevel()
        {
            selectedLevelRepresentation.Clear();
            selectedLevelRepresentation.stagesProperty.arraySize = stagesCount;

            int excludedElementIndex = -1; // we avoid situation with two exact  same elements in a row
            int randomIndex;
            randomIndexesList.Clear();

            for (int i = 0; i < stagesProperty.arraySize; i++)
            {
                if (isUsed[i])
                {
                    randomIndexesList.Add(i);
                }
            }

            for (int i = 0; i < stagesCount; i++)
            {
                if (i > 0)
                {
                    randomIndexesList.Remove(excludedElementIndex);
                }

                randomIndex = randomIndexesList[UnityEngine.Random.Range(0, randomIndexesList.Count)];
                selectedLevelRepresentation.stagesProperty.GetArrayElementAtIndex(i).objectReferenceValue = stagesProperty.GetArrayElementAtIndex(randomIndex).objectReferenceValue;


                if (i > 0)
                {
                    randomIndexesList.Add(excludedElementIndex);
                }

                excludedElementIndex = randomIndex;
            }

            SaveLevel();

        }


        private void LoadLevel()
        {
            EditorSceneController.Instance.Clear();

            LevelStage[] stages = new LevelStage[selectedLevelRepresentation.stagesProperty.arraySize];

            for (int i = 0; i < selectedLevelRepresentation.stagesProperty.arraySize; i++)
            {
                stages[i] = selectedLevelRepresentation.stagesProperty.GetArrayElementAtIndex(i).objectReferenceValue as LevelStage;

            }

            EditorSceneController.Instance.SpawnStages(stages);
        }

        private void SaveLevel()
        {

            levelsDatabaseSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            LoadLevel();
        }

        protected class LevelRepresentation
        {
            public SerializedProperty levelProperty;

            private const string STAGES_PROPERTY_NAME = "stages";
            public SerializedProperty stagesProperty;


            public LevelRepresentation(SerializedProperty levelProperty)
            {
                this.levelProperty = levelProperty;
                stagesProperty = levelProperty.FindPropertyRelative(STAGES_PROPERTY_NAME);
            }


            public void Clear()
            {
                stagesProperty.arraySize = 0;
            }
        }
    }
}

// -----------------
// Scene interraction level editor V1.5
// -----------------

// Changelog
// v 1.4
// • Updated EnumObjectlist
// • Updated object preview
// v 1.4
// • Updated EnumObjectlist
// • Fixed bug with window size
// v 1.3
// • Updated EnumObjectlist
// • Added StartPointHandles script that can be added to gameobjects
// v 1.2
// • Reordered some methods
// v 1.1
// • Added spawner tool
// v 1 basic version works
