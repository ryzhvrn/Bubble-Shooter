using UnityEngine;

namespace Watermelon.BubbleShooter
{
    public abstract class EnemyBaseScenario : ScriptableObject
    {
        [SerializeField] BehaviorType type;
        public BehaviorType Type => type;

        [SerializeField]
        protected RuntimeAnimatorController enemyAnimatorController;
        public RuntimeAnimatorController EnemyAnimatorController => enemyAnimatorController;

        public abstract ScenarioCase Init(CharacterEnemy characterEnemy);
        public abstract void InvokeScenario(ScenarioCase scenarioCase);
    }
}