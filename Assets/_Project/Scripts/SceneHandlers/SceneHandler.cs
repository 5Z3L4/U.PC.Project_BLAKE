using System;
using System.Collections;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.SceneHandlers
{
    public class SceneHandler : Singleton<SceneHandler>
    {
        private const string mainMenu = "MainMenu";
        private const string loadingScene = "LoadingScene";
        private const string firstFloor = "Floor1";

        [HideInInspector]
        public float loadingProgress;
    
        [HideInInspector] 
        public bool isSceneLoadedProperly = true;

        [HideInInspector] 
        public float roomsGenerated = 0;

        [HideInInspector]
        public float roomsToGenerate = 0;

        [HideInInspector]
        public bool isNormalDifficulty = true; //only for DD purposes

        public event Action OnMainMenuLoad;

        public bool IsAnySceneLoading => !isSceneLoadedProperly;

        protected override void Awake()
        {
            base.Awake();
            ReferenceManager.SceneHandler = this;
        }

        public void StartNewGame()
        {
            GetComponent<LevelHandler>().ResetValues();
            StartCoroutine(LoadNewSceneAdditive(firstFloor));
            PerkShop.ResetShopValues();
        }
        
        public void LoadMainMenu()
        {
            StartCoroutine(LoadNewSceneAdditive(mainMenu));
            OnMainMenuLoad?.Invoke();
        }

        public void LoadNewLevel(string sceneToLoadString)
        {
            StartCoroutine(LoadNewSceneAdditive(sceneToLoadString));
        }

        public void QuitGame()
        {
            Application.Quit();
        
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private IEnumerator LoadNewSceneAdditive(string sceneToLoadString)
        {
            isSceneLoadedProperly = false;
            var loadingSceneAsync = SceneManager.LoadSceneAsync(loadingScene);
            yield return loadingSceneAsync;

            var asyncOperation = SceneManager.LoadSceneAsync(sceneToLoadString, LoadSceneMode.Additive);
        
            while (!asyncOperation.isDone)
            {
                loadingProgress = Mathf.Clamp01(asyncOperation.progress / .9f);
                yield return null;
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoadString));
            isSceneLoadedProperly = true;

            while (roomsGenerated < roomsToGenerate)
            {
                loadingProgress = Mathf.Clamp01((roomsGenerated / roomsToGenerate) / .9f);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.3f);
            SceneManager.UnloadSceneAsync(loadingScene);
            ResetValues();
        }

        private void ResetValues()
        {
            loadingProgress = 0;
            roomsGenerated = 0;
            roomsToGenerate = 0;
            Time.timeScale = 1;
        }
    }
}
