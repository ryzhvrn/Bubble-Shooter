using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon.BubbleShooter
{


    public class ReplaceEnemyTool : MonoBehaviour
    {
        [SerializeField] GameObject newEnemyPrefab;

        [Button("Replace")]
        private void Replace()
        {
#if UNITY_EDITOR
            if (newEnemyPrefab == null)
            {
                Debug.LogError("Please assign new enemy prefab");
                return;
            }

            LevelStage stage = gameObject.GetComponent<LevelStage>();

            List<BaseEnemy> enemies = new List<BaseEnemy>();
            enemies.AddRange(stage.Enemies);

            for (int i = 0; i < enemies.Count; i++)
            {
                CharacterEnemy enemy = enemies[i] as CharacterEnemy;

                if (enemy != null)
                {
                    GameObject newObject = PrefabUtility.InstantiatePrefab(newEnemyPrefab) as GameObject;
                    CharacterEnemy newEnemy = newObject.GetComponent<CharacterEnemy>();

                    newEnemy.transform.SetParent(transform);

                    newEnemy.transform.localPosition = enemy.transform.localPosition;
                    newEnemy.transform.localRotation = enemy.transform.localRotation;
                    newEnemy.transform.localScale = enemy.transform.localScale;

                    newEnemy.SetHitsToKill(enemy.HitsToKill);
                    newEnemy.SetBehaviorType(enemy.Behavior);
                    newEnemy.SetIsRunning(enemy.IsRunning);
                    newEnemy.SetRunningSpeed(enemy.RunningSpeed);
                    newEnemy.SetEnableAiming(enemy.EnableAimingValue);
                    newEnemy.SetMinMaxAimDelay(enemy.MinMaxAimDelay);
                    newEnemy.SetEnableWarning(enemy.EnableWarningValue);
                    newEnemy.SetMoveOnHit(enemy.MoveOnHit);
                    newEnemy.SetSpawnFoamOnHit(enemy.SpawnFoamOnHit);
                    newEnemy.SetUseRig(enemy.UseRig);
                    newEnemy.SetShowEmotionOnHit(enemy.ShowEmotionOnHit);
                    newEnemy.SetShowEmotionOnDead(enemy.ShowEmotionOnDead);
                    newEnemy.SetEnableTarget(enemy.EnableTargetValue);
                    newEnemy.SetShowHandClickingTarget(enemy.ShowHandClickingTarget);
                    newEnemy.SetTargetScale(enemy.TargetScale);

                    DestroyImmediate(enemy.gameObject);
                    enemies[i] = newEnemy;

                    stage.SetEnemies(enemies);

                    EditorUtility.SetDirty(gameObject);
                }
            }

            DestroyImmediate(this);
#endif

        }
    }
}