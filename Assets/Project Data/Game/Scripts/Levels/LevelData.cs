using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BubbleShooter
{
    [System.Serializable]
    public class LevelData
    {
        [SerializeField] List<LevelStage> stages = new List<LevelStage>();
        public List<LevelStage> Stages => stages;
    }
}