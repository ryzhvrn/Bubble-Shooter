using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class ValidatorViewer : EditorWindow
    {
        private static ValidatorViewer window;

        private Dictionary<Type, List<ValidatorContainer>> allowedTypes = new Dictionary<Type, List<ValidatorContainer>>();
        private Dictionary<UnityEngine.Object, List<ValidatorComponent>> validatorObjects = new Dictionary<UnityEngine.Object, List<ValidatorComponent>>();

        private int resultSuccessCount;
        private int resultWarningCount;
        private int resultErrorCount;

        private Texture2D iconWarnSmall;
        private Texture2D iconErrorSmall;
        private Texture2D iconSuccess;

        private GUIStyle errorBackground;
        private GUIStyle warningBackground;
        private GUIStyle successBackground;

        private GUIStyle transparentBackground;

        private GUIStyle titleStyle;
        private GUIStyle descriptionStyle;

        private bool isLoadedIcons = false;

        private Vector2 scrollViewMessages = Vector2.zero;

        private ValidatorAttribute.ValidateType filter = ValidatorAttribute.ValidateType.Success | ValidatorAttribute.ValidateType.Warning | ValidatorAttribute.ValidateType.Error;
        
        private class Constants
        {
            public static GUIStyle WarningStyle;
            public static GUIStyle ErrorStyle;

            public static void Init()
            {
                WarningStyle = new GUIStyle("CN EntryWarn");
                ErrorStyle = new GUIStyle("CN EntryError");
            }
        }

        [MenuItem("Tools/Editor/Validator")]
        public static void Init()
        {
            ValidatorViewer tempWindow = (ValidatorViewer)EditorWindow.GetWindow(typeof(ValidatorViewer), true);

            Vector2 windowSize = new Vector2(450, 310);
            tempWindow.minSize = windowSize;
            tempWindow.maxSize = windowSize;

            tempWindow.InitTitle();

            window = tempWindow;
        }

        private void InitIcons()
        {
            //Init backgrounds
            if (errorBackground == null)
                errorBackground = EditorStylesExtended.GetBoxWithColor(new Color(1.0f, 0.38f, 0.46f));
            if (successBackground == null)
                successBackground = EditorStylesExtended.GetBoxWithColor(new Color(0.5f, 0.77f, 0.57f));
            if (warningBackground == null)
                warningBackground = EditorStylesExtended.GetBoxWithColor(new Color(1.0f, 0.78f, 0.18f));
            if (transparentBackground == null)
                transparentBackground = EditorStylesExtended.GetBoxWithColor(new Color(0.0f, 0.0f, 0.0f, 0.1f));
            
            if (isLoadedIcons)
                return;

            Constants.Init();

            iconWarnSmall = EditorGUIUtility.FindTexture("console.warnicon.sml");
            iconErrorSmall = EditorGUIUtility.FindTexture("console.erroricon.sml");
            iconSuccess = EditorStylesExtended.GetTexture("icon_check");

            //Init backgrounds
            errorBackground = EditorStylesExtended.GetBoxWithColor(new Color(1.0f, 0.38f, 0.46f));
            successBackground = EditorStylesExtended.GetBoxWithColor(new Color(0.5f, 0.77f, 0.57f));
            warningBackground = EditorStylesExtended.GetBoxWithColor(new Color(1.0f, 0.78f, 0.18f));

            transparentBackground = EditorStylesExtended.GetBoxWithColor(new Color(0.0f, 0.0f, 0.0f, 0.1f));

            titleStyle = EditorStylesExtended.label_medium_bold;
            titleStyle.normal.textColor = Color.white;

            descriptionStyle = EditorStylesExtended.label_small_bold;
            descriptionStyle.normal.textColor = Color.white;

            isLoadedIcons = true;
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneUnloaded += OnSceneUnloaded;
            EditorSceneManager.activeSceneChanged += OnSceneChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;

            window = this;

            ScanScene();

            LoadMessages();
        }

        private void OnDestroy()
        {
            errorBackground = null;
            successBackground = null;
            warningBackground = null;

            transparentBackground = null;
        }

        private void OnSceneClosed(Scene scene)
        {
            ScanScene();

            LoadMessages();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ScanScene();

            LoadMessages();
        }

        private void OnSceneChanged(Scene prevScene, Scene newScene)
        {
            ScanScene();

            LoadMessages();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            ScanScene();

            LoadMessages();
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneUnloaded -= OnSceneUnloaded;
            EditorSceneManager.activeSceneChanged -= OnSceneChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
        }

        private void ToolbarGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            SetFlag(ValidatorAttribute.ValidateType.Success, GUILayout.Toggle(HasFlag(ValidatorAttribute.ValidateType.Success), new GUIContent(resultSuccessCount.ToString(), iconSuccess), EditorStyles.toolbarButton));
            SetFlag(ValidatorAttribute.ValidateType.Warning, GUILayout.Toggle(HasFlag(ValidatorAttribute.ValidateType.Warning), new GUIContent(resultWarningCount.ToString(), iconWarnSmall), EditorStyles.toolbarButton));
            SetFlag(ValidatorAttribute.ValidateType.Error, GUILayout.Toggle(HasFlag(ValidatorAttribute.ValidateType.Error), new GUIContent(resultErrorCount.ToString(), iconErrorSmall), EditorStyles.toolbarButton));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                ScanScene();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            InitIcons();

            ToolbarGUI();

            if (validatorObjects.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();

                scrollViewMessages = EditorGUILayout.BeginScrollView(scrollViewMessages, false, false);

                EditorGUILayout.BeginVertical();

                int index = 0;
                foreach (var validatorObject in validatorObjects)
                {
                    foreach (var component in validatorObject.Value)
                    {
                        for (int i = 0; i < component.messages.Count; i++)
                        {
                            if (HasFlag(component.messages[i].ValidateType))
                            {
                                EditorGUILayout.BeginHorizontal(GetStyle(component.messages[i].ValidateType), GUILayout.Height(45));
                                
                                EditorGUILayout.BeginVertical(transparentBackground, GUILayout.Width(15), GUILayout.Height(45));
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.LabelField(new GUIContent(GetIcon(component.messages[i].ValidateType)), GUILayout.Width(15));
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.BeginVertical();
                                GUILayout.FlexibleSpace();

                                Rect buttonRect = EditorGUILayout.BeginHorizontal();

                                EditorGUILayout.BeginVertical();
                                GUILayout.FlexibleSpace();
                                GUILayout.Space(2);
                                EditorGUILayout.LabelField(new GUIContent(!string.IsNullOrEmpty(validatorObject.Key.name) ? validatorObject.Key.name : validatorObject.Key.GetType().Name), titleStyle);
                                GUILayout.Space(-7);
                                EditorGUILayout.LabelField(new GUIContent(component.messages[i].Message), descriptionStyle);
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndVertical();

                                GUILayout.FlexibleSpace();

                                if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                                {
                                    Selection.activeObject = validatorObject.Key;
                                    EditorGUIUtility.PingObject(validatorObject.Key);
                                }

                                EditorGUILayout.EndHorizontal();
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndHorizontal();

                                GUILayout.Space(5);
                            }

                            index++;
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("There is nothing to validate :)");
            }
        }

        private void OnFocus()
        {
            LoadMessages();
        }

        private void LoadMessages()
        {
            resultSuccessCount = 0;
            resultWarningCount = 0;
            resultErrorCount = 0;

            validatorObjects = new Dictionary<UnityEngine.Object, List<ValidatorComponent>>();
            foreach (var allowedType in allowedTypes.Keys)
            {
                UnityEngine.Object[] typeObjects = Resources.FindObjectsOfTypeAll(allowedType);
                for (int i = 0; i < typeObjects.Length; i++)
                {
                    List<ValidatorAttribute.ValidateResult> messages = new List<ValidatorAttribute.ValidateResult>();

                    for (int x = 0; x < allowedTypes[allowedType].Count; x++)
                    {
                        for (int a = 0; a < allowedTypes[allowedType][x].attributes.Length; a++)
                        {
                            ValidatorAttribute.ValidateResult result = allowedTypes[allowedType][x].attributes[a].Validate(allowedTypes[allowedType][x].fieldInfo.GetValue(typeObjects[i]), typeObjects[i]);

                            if (result != null)
                            {
                                messages.Add(result);

                                switch (result.ValidateType)
                                {
                                    case ValidatorAttribute.ValidateType.Success:
                                        resultSuccessCount++;
                                        break;
                                    case ValidatorAttribute.ValidateType.Warning:
                                        resultWarningCount++;
                                        break;
                                    case ValidatorAttribute.ValidateType.Error:
                                        resultErrorCount++;
                                        break;
                                }
                            }
                        }
                    }

                    if (messages.Count > 0)
                    {
                        if (typeObjects[i] != null)
                        {
                            if (validatorObjects.ContainsKey(typeObjects[i]))
                            {
                                validatorObjects[typeObjects[i]].Add(new ValidatorComponent(typeObjects[i], messages));
                            }
                            else
                            {
                                validatorObjects.Add(typeObjects[i], new List<ValidatorComponent>() { new ValidatorComponent(typeObjects[i], messages) });
                            }
                        }
                    }
                }
            }
        }

        private void InitTitle()
        {
            string scenesLine = "";
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isLoaded && !string.IsNullOrEmpty(scene.name))
                {
                    if (i != 0)
                        scenesLine += ", ";

                    scenesLine += scene.name;
                }
            }

            if (string.IsNullOrEmpty(scenesLine))
            {
                window.titleContent = new GUIContent("Validator");
            }
            else
            {
                window.titleContent = new GUIContent("Validator - " + scenesLine);
            }
        }

        private void ScanScene()
        {
            allowedTypes = new Dictionary<Type, List<ValidatorContainer>>();
            validatorObjects = new Dictionary<UnityEngine.Object, List<ValidatorComponent>>();

            resultSuccessCount = 0;
            resultWarningCount = 0;
            resultErrorCount = 0;
            
            Assembly assembly = Assembly.GetAssembly(typeof(ValidatorAttribute));
            Type[] types = assembly.GetTypes();

            var attributeTypes = types.Where(t => t.IsSubclassOf(typeof(ValidatorAttribute)));

            int index = 0;
            int count = types.Length;

            InitTitle();

            foreach (Type type in types)
            {
                EditorUtility.DisplayProgressBar("Validating", "Validating fields.. (" + index + "/" + count + ")", (float)index / count);

                FieldInfo[] fieldsInfo = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                for (int i = 0; i < fieldsInfo.Length; i++)
                {
                    foreach (var attributeType in attributeTypes)
                    {
                        object[] attributes = fieldsInfo[i].GetCustomAttributes(attributeType, false);
                        if (attributes.Length > 0)
                        {
                            if (!allowedTypes.ContainsKey(type))
                            {
                                allowedTypes.Add(type, new List<ValidatorContainer>() { new ValidatorContainer(fieldsInfo[i], attributes) });
                            }
                            else
                            {
                                allowedTypes[type].Add(new ValidatorContainer(fieldsInfo[i], attributes));
                            }
                        }
                    }
                }

                index++;
            }

            EditorUtility.ClearProgressBar();
        }

        private void SetFlag(ValidateInputAttribute.ValidateType validateType, bool state)
        {
            if (state)
                AddFlag(validateType);
            else
                RemoveFlag(validateType);
        }

        private bool HasFlag(ValidatorAttribute.ValidateType validateType)
        {
            return (filter & validateType) == validateType;
        }

        private void AddFlag(ValidatorAttribute.ValidateType validateType)
        {
            filter |= validateType;
        }

        private void RemoveFlag(ValidatorAttribute.ValidateType validateType)
        {
            filter &= ~validateType;
        }

        private Texture2D GetIcon(ValidatorAttribute.ValidateType type)
        {
            switch (type)
            {
                case ValidatorAttribute.ValidateType.Success:
                    return iconSuccess;
                case ValidatorAttribute.ValidateType.Warning:
                    return iconWarnSmall;
                case ValidatorAttribute.ValidateType.Error:
                    return iconErrorSmall;
            }

            return null;
        }

        private GUIStyle GetStyle(ValidatorAttribute.ValidateType type)
        {
            switch (type)
            {
                case ValidatorAttribute.ValidateType.Success:
                    return successBackground;
                case ValidatorAttribute.ValidateType.Warning:
                    return warningBackground;
                case ValidatorAttribute.ValidateType.Error:
                    return errorBackground;
            }

            return null;
        }

        private class ValidatorContainer
        {
            public FieldInfo fieldInfo;
            public ValidatorAttribute[] attributes;

            public ValidatorContainer(FieldInfo fieldInfo, object[] attributeObjects)
            {
                this.fieldInfo = fieldInfo;

                attributes = new ValidatorAttribute[attributeObjects.Length];
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributes[i] = attributeObjects[i] as ValidatorAttribute;
                }
            }
        }

        private class ValidatorComponent
        {
            public UnityEngine.Object component;
            public List<ValidatorAttribute.ValidateResult> messages = new List<ValidatorAttribute.ValidateResult>();

            public ValidatorComponent(UnityEngine.Object component, List<ValidatorAttribute.ValidateResult> messages)
            {
                this.component = component;
                this.messages = messages;
            }
        }
    }
}