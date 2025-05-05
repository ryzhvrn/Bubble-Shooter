using UnityEngine;

namespace Watermelon.BubbleShooter
{
    [CreateAssetMenu(fileName = "Standing Scenario", menuName = "Content/Enemies/Scenario/Standing Scenario")]
    public sealed class EnemyStandingScenario : EnemyBaseScenario
    {

        public override ScenarioCase Init(CharacterEnemy characterEnemy)
        {
            ScenarioCase scenarioCase = new ScenarioCase(characterEnemy);

            // Change animator controller
            characterEnemy.SetRuntimeAnimatorController(enemyAnimatorController);

            return scenarioCase;
        }

        public override void InvokeScenario(ScenarioCase scenarioCase)
        {

        }
    }
}