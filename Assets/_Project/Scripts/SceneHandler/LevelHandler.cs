using System;
using System.Linq;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.SceneHandler
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

        public void GoToNextLevel()
        {
            if (levelIndex == levelNames.levelNames.Length - 1)
            {
                EndRun();
                return;
            }
            onNextLevel?.Invoke();
            levelIndex++;
            ReferenceManager.RoomManager.ClearRooms();
            sceneHandler.LoadNewLevel(levelNames.levelNames[levelIndex]);
        }

        public void EndRun()
        {
            levelIndex = 0;
            if (ReferenceManager.PlayerInputController != null)
            {
                Destroy(ReferenceManager.PlayerInputController.gameObject);
            }
            ReferenceManager.RoomManager.ClearRooms();

            sceneHandler.LoadMainMenu();
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

        }
    }
}
