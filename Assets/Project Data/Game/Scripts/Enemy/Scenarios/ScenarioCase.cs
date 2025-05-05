using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public class ScenarioCase
    {
        protected bool isScenarioActive;
        public bool IsScenarioActive => isScenarioActive;

        protected bool isScenarioFinished;
        public bool IsScenarioFinished => isScenarioFinished;

        protected CharacterEnemy characterEnemy;
        public CharacterEnemy CharacterEnemy => characterEnemy;

        public ScenarioCase(CharacterEnemy characterEnemy)
        {
            this.characterEnemy = characterEnemy;
        }

        public virtual void Reset()
        {

        }
    }
}
    
