using UnityEditor;

namespace Watermelon
{
    public class CustomProjectPreferences : SettingsProvider
    {
        private const string AUDIO_IMPORT_PREF = "audioImportWindow";
        private const string TEXTURES_IMPORT_PREF = "texturesImportWindow";

        private static bool showImportWindowForAudio = false;
        private static bool showImportWindowForTextures = false;

        public static bool ShowAudioImportWindow => EditorPrefs.GetBool(AUDIO_IMPORT_PREF, false);
        public static bool ShowTexturesImportWindow => EditorPrefs.GetBool(TEXTURES_IMPORT_PREF, false);

        private bool prefsLoaded = false;

        public CustomProjectPreferences(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {
            EditorStylesExtended.InitializeStyles();
        }

        private void LoadValues()
        {
            if (prefsLoaded)
                return;

            showImportWindowForAudio = EditorPrefs.GetBool(AUDIO_IMPORT_PREF, false);
            showImportWindowForTextures = EditorPrefs.GetBool(TEXTURES_IMPORT_PREF, false);

            prefsLoaded = true;
        }

        public override void OnGUI(string searchContext)
        {
            LoadValues();

            EditorGUILayout.BeginVertical(EditorStylesExtended.padding05);

            EditorGUI.BeginChangeCheck();
            showImportWindowForAudio = EditorGUILayout.Toggle("Audio Window", showImportWindowForAudio);
            if(EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(AUDIO_IMPORT_PREF, showImportWindowForAudio);
            }

            EditorGUI.BeginChangeCheck();
            showImportWindowForTextures = EditorGUILayout.Toggle("Textures Window", showImportWindowForTextures);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(TEXTURES_IMPORT_PREF, showImportWindowForTextures);
            }

            EditorGUILayout.EndVertical();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            CustomProjectPreferences provider = new CustomProjectPreferences("Project/Import Settings", SettingsScope.Project);

            return provider;
        }
    }
}
