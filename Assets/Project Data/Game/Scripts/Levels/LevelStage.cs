using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class LevelStage : MonoBehaviour
    {
        [SerializeField] Transform playerStartPosition;
        public Transform PlayerStartPosition => playerStartPosition;

        [SerializeField] Transform nextStagePosition;
        public Transform NextStagePosition => nextStagePosition;

        [SerializeField] List<Transform> extraPathPoints;
        public List<Transform> ExtraPathPoints => extraPathPoints;

        [Space()]
        [SerializeField] BaseEnemy[] enemies;
        public BaseEnemy[] Enemies => enemies;

        private int enemiesDiedAmount = 0;


        private void Awake()
        {
            if (nextStagePosition.childCount > 0)
            {
                nextStagePosition.GetChild(0).gameObject.SetActive(false);
            }
        }

        public void OnStageReached()
        {
            if (enemies.Length > 0)
            {
                enemiesDiedAmount = 0;

                for (int i = 0; i < enemies.Length; i++)
                {
                    enemies[i].OnCombatStart();

                    if (enemies[i].IsDead)
                    {
                        enemiesDiedAmount++;
                    }
                    else
                    {
                        enemies[i].OnEnemyDied += OnEnemyDied;
                    }
                }
            }

            // for the case if user already killed enemies before arriving
            if (enemiesDiedAmount >= enemies.Length)
            {
                OnAllEnemiesDied();
                return;
            }
        }

        private void OnEnemyDied()
        {
            enemiesDiedAmount++;

            if (enemiesDiedAmount == enemies.Length)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    enemies[i].OnEnemyDied -= OnEnemyDied;
                }

                OnAllEnemiesDied();
            }
        }

        private void OnAllEnemiesDied()
        {
            LevelController.OnStageCompleted();
        }

        public List<Vector3> GetCameraPath()
        {
            List<Vector3> path = new List<Vector3>();
            path.Add(playerStartPosition.position);

            for (int i = 0; i < extraPathPoints.Count; i++)
            {
                path.Add(extraPathPoints[i].position.AddToY(playerStartPosition.position.y));
            }

            return path;
        }

#if UNITY_EDITOR

        public void SetEnemies(List<BaseEnemy> newEnemies)
        {
            enemies = newEnemies.ToArray();
        }

        [Button("Preview Player Position")]
        public void PreviewCamera()
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();

            if (player != null)
            {
                player.transform.position = playerStartPosition.transform.position;
                player.transform.rotation = playerStartPosition.transform.rotation;
            }
            else
            {
                Debug.LogError("Player is not found on this scene");
            }
        }

#endif
    }
}