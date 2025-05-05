using UnityEngine;

namespace Watermelon
{
    public static class EditorStyle
    {
        private static GUIStyle previewLabelStyle;
        public static GUIStyle PreviewLabelStyle
        {
            get
            {
                if (previewLabelStyle == null)
                {
                    previewLabelStyle = new GUIStyle("PreOverlayLabel")
                    {
                        richText = true,
                        alignment = TextAnchor.UpperLeft,
                        fontStyle = FontStyle.Normal
                    };
                }

                return previewLabelStyle;
            }
        }
    }
}
