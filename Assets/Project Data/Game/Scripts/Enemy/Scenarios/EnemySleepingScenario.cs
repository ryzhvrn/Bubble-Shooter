using UnityEngine;

namespace Watermelon.BubbleShooter
{
    [CreateAssetMenu(fileName = "Sleeping Scenario", menuName = "Content/Enemies/Scenario/Sleeping Scenario")]
    public sealed class EnemySleepingScenario : EnemyBaseScenario
    {
        private static readonly int ANIMATOR_WAKEUP_TRIGGER_HASH = Animator.StringToHash("WakeUp");

        public override ScenarioCase Init(CharacterEnemy characterEnemy)
        {
            ScenarioCase scenarioCase = new ScenarioCase(characterEnemy);

            // Change animator controller
            characterEnemy.SetRuntimeAnimatorController(enemyAnimatorController);

            return scenarioCase;
        }

        public override void InvokeScenario(ScenarioCase scenarioCase)
        {
            scenarioCase.CharacterEnemy.CharacterAnimator.SetTrigger(ANIMATOR_WAKEUP_TRIGGER_HASH);
        }
    }
}