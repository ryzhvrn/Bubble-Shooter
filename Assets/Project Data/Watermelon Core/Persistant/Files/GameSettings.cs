using UnityEngine;

namespace Watermelon
{
    [SetupTab("Settings", texture = "icon_settings", priority = 0)]
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Settings/Game Settings")]
    public class GameSettings : ScriptableObject
    {
    }
}
