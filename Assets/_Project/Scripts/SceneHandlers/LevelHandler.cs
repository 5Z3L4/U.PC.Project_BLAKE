using System;
using System.Linq;
using _Project.Scripts.Analytics;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.SceneHandlers
{
    [RequireComponent(typeof(SceneHandler))]
    public class LevelHandler : Singleton<LevelHandler>
    {
        [SerializeField]
        private int levelIndex = 0;
        [SerializeField]
        private LevelList levelNames;
        private SceneHandler sceneHandler;
        public event Action onNextLevel;
        private void Start()
        {
            ReferenceManager.LevelHandler = this;

            sceneHandler = ReferenceManager.SceneHandler;

            string currentSceneName = SceneManager.GetActiveScene().name;
            if (levelNames.levelNames.Contains(currentSceneName))
            {
                for (int i = 0; i < levelNames.levelNames.Length; i++)
                {
                    if (levelNames.levelNames[i] == currentSceneName)
                    {
                        levelIndex = i;
                        break;
                    }
                }
            }
        }

        public string[] GetLevelList()
        {
            return levelNames.levelNames;
        }

        public int GetIndex()
        {
            return levelIndex;
        }

        public void GoToNextLevel()
        {
            if (levelIndex == levelNames.levelNames.Length - 1)
            {
                EndRun(true);
                return;
            }
            
            this.TrySendAnalytics(levelNames.levelNames[levelIndex]);
            
            onNextLevel?.Invoke();
            levelIndex++;
            ReferenceManager.RoomManager.ClearRooms();
            sceneHandler.LoadNewLevel(levelNames.levelNames[levelIndex]);
        }

        public void EndRun(bool won)
        {
            GameHandler.Instance.IsGamePaused = true;
            FindAnyObjectByType<SummaryScreen>(FindObjectsInactive.Include).SetupSummary(won);

            levelIndex = 0;
            if (ReferenceManager.PlayerInputController != null)
            {
                Destroy(ReferenceManager.PlayerInputController.gameObject);
            }
            ReferenceManager.RoomManager.ClearRooms();
            SummaryHandler.Instance.ResetValues();
        }

        public void ResetValues()
        {
            levelIndex = 0;
            if (ReferenceManager.PlayerInputController != null)
            {
                ReferenceManager.PlayerInputController?.GetComponent<PlayerPerkManager>().RemoveAllPerks();
                Destroy(ReferenceManager.PlayerInputController.gameObject);
            }
            if (ReferenceManager.PlayerCurrencyController!= null)
            {

                ReferenceManager.PlayerCurrencyController?.ResetValues();
            }
            if(ReferenceManager.RoomManager != null){
                ReferenceManager.RoomManager.ClearRooms();
            }
            if(GameHandler.Instance != null)
            {
                GameHandler.Instance._wasShownControls = false;
            }
        }
    }
}
