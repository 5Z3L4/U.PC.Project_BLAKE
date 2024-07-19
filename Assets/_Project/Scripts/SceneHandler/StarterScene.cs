using System;
using System.Linq;
using _Project.Scripts.GlobalHandlers;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace _Project.Scripts.SceneHandler
{
    public class StarterScene : MonoBehaviour
    {
        [SerializeField] private bool instantLoadGame = true;
        private float delayTimeBetweenLogs = .5f;

        private void Start()
        {
            _ = StartLoading();
        }

        private async UniTaskVoid StartLoading()
        {
            var objectsInActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
            objectsInActiveScene.RemoveAll(go => go == gameObject);
            
            while (objectsInActiveScene.Count > 0)
            {
                var log = $"There are still {objectsInActiveScene.Count} objects not loaded from StarterScene!";

                foreach (var objectToLoad in objectsInActiveScene)
                {
                    log += $"\n {objectToLoad}";
                }
                
                Debug.Log(log);
                await UniTask.Delay(TimeSpan.FromSeconds(delayTimeBetweenLogs));
            }

            if (instantLoadGame)
            {
                ReferenceManager.SceneHandler.StartNewGame();
            }
            else
            {
                ReferenceManager.SceneHandler.LoadMainMenu();
            }
        }
    }
}
