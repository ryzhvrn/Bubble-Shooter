using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomEditor(typeof(ScriptableObjectInitModule))]
    public class ScriptableObjectInitModuleEditor : InitModuleEditor
    {
        private readonly string INIT_OBJECTS_PROPERTY_NAME = "initObjects";

        private SerializedProperty initObjectsProperty;

        private MonoScript script;

        private void OnEnable()
        {
            try
            {
                initObjectsProperty = serializedObject.FindProperty(INIT_OBJECTS_PROPERTY_NAME);

                script = MonoScript.FromScriptableObject((ScriptableObject)target);
            }
            catch (System.Exception)
            {
                return;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            GUI.enabled = true;

            int initObjectsArraySize = initObjectsProperty.arraySize;
            if(initObjectsArraySize > 0)
            {
                for (int i = 0; i < initObjectsArraySize; i++)
                {
                    SerializedProperty arrayElement = initObjectsProperty.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel("Element " + i);
                    EditorGUILayout.PropertyField(arrayElement, GUIContent.none);

                    if (GUILayout.Button("X", EditorStylesExtended.button_04_mini, GUILayout.Height(18), GUILayout.Width(18)))
                    {
                        if (EditorUtility.DisplayDialog("Remove element", "Are you sure you want to remove scriptable object?", "Remove", "Cancel"))
                        {
                            initObjectsProperty.RemoveFromObjectArrayAt(i);

                            return;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("List is empty!");
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override void Buttons()
        {
            if (GUILayout.Button("Add", GUILayout.Width(90)))
            {
                initObjectsProperty.serializedObject.Update();

                int arraySize = initObjectsProperty.arraySize;
                initObjectsProperty.arraySize++;
                initObjectsProperty.GetArrayElementAtIndex(arraySize).objectReferenceValue = null;

                initObjectsProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

// -----------------
// Initialiser v 0.4.1
// -----------------