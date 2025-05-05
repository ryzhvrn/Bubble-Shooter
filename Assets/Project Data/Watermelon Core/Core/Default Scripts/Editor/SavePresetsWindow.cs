using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Watermelon
{
    public class SavePresetsWindow : WatermelonWindow
    {
        private const string PRESET_PREFIX = "savePreset_";
        private const string SAVE_FILE_NAME = "save";

        private static readonly Vector2 WINDOW_SIZE = new Vector2(490, 495);
        private static readonly string WINDOW_TITLE = "Save Presets";

        private static SavePresetsWindow setupWindow;

        private Vector2 scrollView;

        private List<SavePreset> savePresets;
        private string tempPresetName;

        [MenuItem("Tools/Save Presets")]
        [MenuItem("Window/Save Presets")]
        static void ShowWindow()
        {
            SavePresetsWindow tempWindow = (SavePresetsWindow)GetWindow(typeof(SavePresetsWindow), false, WINDOW_TITLE);
            tempWindow.minSize = WINDOW_SIZE;
            tempWindow.titleContent = new GUIContent(WINDOW_TITLE, EditorStylesExtended.GetTexture("icon_title", EditorStylesExtended.IconColor));

            setupWindow = tempWindow;

            EditorStylesExtended.InitializeStyles();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            setupWindow = this;

            savePresets = new List<SavePreset>();

            string[] fileEntries = Directory.GetFiles(Application.persistentDataPath);
            for (int i = 0; i < fileEntries.Length; i++)
            {
                if (fileEntries[i].Contains(PRESET_PREFIX))
                {
                    savePresets.Add(new SavePreset(File.GetCreationTimeUtc(fileEntries[i]), fileEntries[i]));
                }
            }

            ForceInitStyles();
        }

        private void OnDisable()
        {

        }

        protected override void Styles()
        {

        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("PRESETS");

            scrollView = EditorGUILayoutCustom.BeginScrollView(scrollView);
            for (int i = 0; i < savePresets.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStylesExtended.editorSkin.box);
                EditorGUILayout.LabelField(savePresets[i].name);
                EditorGUILayout.LabelField(savePresets[i].creationDate.ToString("dd.MM"), GUILayout.MaxWidth(50));

                if (GUILayout.Button("Activate", EditorStylesExtended.button_03, GUILayout.Height(18)))
                {
                    ActivatePreset(savePresets[i].name);
                }

                if (GUILayout.Button("Update", GUILayout.Height(18)))
                {
                    if (EditorUtility.DisplayDialog("This preset will rewrited!", "Are you sure?", "Rewrite", "Cancel"))
                    {
                        UpdatePreset(savePresets[i]);
                    }
                }

                if (GUILayout.Button("X", EditorStylesExtended.button_04, GUILayout.Height(18)))
                {
                    if (EditorUtility.DisplayDialog("This preset will be removed!", "Are you sure?", "Remove", "Cancel"))
                    {
                        RemovePreset(i);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal(EditorStylesExtended.editorSkin.box);
            tempPresetName = EditorGUILayout.TextField(tempPresetName);

            if (GUILayout.Button("Add"))
            {
                AddNewPreset(tempPresetName);

                tempPresetName = "";

                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ActivatePreset(string name)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[Save Presets]: Preset can't be activated in playmode!");

                return;
            }

            if (EditorApplication.isCompiling)
            {
                Debug.LogError("[Save Presets]: Preset can't be activated during compiling!");

                return;
            }

            string presetPath = Path.Combine(Application.persistentDataPath, PRESET_PREFIX + name);
            if (!File.Exists(presetPath))
            {
                Debug.LogError(string.Format("[Save Presets]: Preset with name {0} isn't exist!", name));

                return;
            }

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game")
            {
                EditorSceneManager.OpenScene(@"Assets\Project Data\Game\Scenes\Game.unity");
            }

            // Replace current save file with the preset
            File.Copy(presetPath, GetSavePath(), true);

            // Start game
            EditorApplication.isPlaying = true;
        }

        private void RemovePreset(int index)
        {
            if (savePresets.IsInRange(index))
            {
                // Delete preset file
                File.Delete(savePresets[index].path);

                // Remove preset from the list
                savePresets.RemoveAt(index);
            }
        }

        private void UpdatePreset(SavePreset savePreset)
        {
            if (EditorApplication.isPlaying)
                SaveController.ForceSave();

            string savePath = GetSavePath();
            if (!File.Exists(savePath))
            {
                Debug.LogError("[Save Presets]: Save file isn't exist!");

                return;
            }

            if (savePreset != null)
            {
                savePreset.creationDate = DateTime.Now;

                if (EditorApplication.isPlaying)
                {
                    File.SetCreationTime(savePreset.path, DateTime.Now);
                    SaveController.PresetsSave(PRESET_PREFIX + savePreset.name);
                }
                else
                {
                    File.Copy(savePath, savePreset.path, true);
                    File.SetCreationTime(savePreset.path, DateTime.Now);
                }

            }

        }

        private void AddNewPreset(string name)
        {
            if (EditorApplication.isPlaying)
                SaveController.ForceSave();

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[Save Presets]: Preset name can't be empty!");

                return;
            }

            if (savePresets.FindIndex(x => x.name == name) != -1)
            {
                Debug.LogError(string.Format("[Save Presets]: Preset with name {0} already exist!", name));

                return;
            }

            string savePath = GetSavePath();
            if (!File.Exists(savePath))
            {
                Debug.LogError("[Save Presets]: Save file isn't exist!");

                return;
            }

            string presetPath = Path.Combine(Application.persistentDataPath, PRESET_PREFIX + name);


            if (EditorApplication.isPlaying)
            {
                SaveController.PresetsSave(PRESET_PREFIX + name);
            }
            else
            {
                File.Copy(savePath, presetPath, true);
            }

            savePresets.Add(new SavePreset(DateTime.Now, presetPath));
        }

        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        private class SavePreset
        {
            public string name;
            public DateTime creationDate;
            public string path;

            public SavePreset(DateTime lastModifiedDate, string path)
            {
                creationDate = lastModifiedDate;
                this.path = path;
                name = Path.GetFileName(path).Replace(PRESET_PREFIX, "");
            }
        }
    }
}