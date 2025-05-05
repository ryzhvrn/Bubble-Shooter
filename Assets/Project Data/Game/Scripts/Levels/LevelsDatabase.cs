using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Content/Levels Database")]
    public class LevelsDatabase : ScriptableObject
    {
        [SerializeField] List<LevelData> levels;
        public List<LevelData> Levels => levels;

        [SerializeField] List<LevelStage> stagesForGeneration;
        public List<LevelStage> StagesForGeneration => stagesForGeneration;

        public LevelData GetLevel(int levelNumber)
        {
            return levels[(levelNumber - 1) % levels.Count];
        }
    }
}