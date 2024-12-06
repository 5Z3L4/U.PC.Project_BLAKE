using _Project.Scripts.Patterns;
using _Project.Scripts.UI.Gameplay;
using UnityEngine;

namespace _Project.Scripts.GlobalHandlers
{
    public class GameHandler : Singleton<GameHandler>
    {
        private const string YOU_LOSE = "YouLose_Canvas";
        private const string PAUSE_GAME = "PauseGame_Canvas";
        private const string WEAPON_UPGRADE = "WeaponUpgrade_Canvas";
        private const string CONTROLS_POPUP = "ControlsPopup_Canvas";
        
        [SerializeField]
        private GameObject pausedGameCanvas;
        [SerializeField]
        private GameObject playerUI;

        private bool _isGamePaused = false;
        
        public bool IsGamePaused
        {
            get => _isGamePaused;

            private set
            {
                _isGamePaused = value;
                Time.timeScale = _isGamePaused ? 0f : 1f;
                ReferenceManager.PlayerInputController?.IsGamePaused(value);
            }
        }

        private GameObject PausedGameCanvas
        {
            get
            {
                if (pausedGameCanvas == null)
                {
                    pausedGameCanvas = Resources.FindObjectsOfTypeAll<PausedGameCanvasController>()[0].gameObject;
                }

                return pausedGameCanvas;
            }
        }

        private GameObject PlayerUI
        {
            get
            {
                if (playerUI == null)
                {
                    playerUI = Resources.FindObjectsOfTypeAll<PlayerGameplayUIManager>()[0].gameObject;
                }

                return playerUI;
            }
        }

        private void Start()
        {
            ShowPlayerControlsPopup();
        }

        public void PlayerWin()
        {
            // Debug.LogError("PlayerWin logic is missing");
        }

        public GameObject PlayerLose()
        {
            return OpenPlayerUICanvas(YOU_LOSE);
        }
        
        public GameObject OpenPauseGameCanvas()
        {
            return OpenPlayerUICanvas(PAUSE_GAME);
        }
        
        public GameObject OpenWeaponUpgradesCanvas()
        {
            return OpenPlayerUICanvas(WEAPON_UPGRADE);
        }

        public GameObject ShowPlayerControlsPopup()
        {
            return OpenPlayerUICanvas(CONTROLS_POPUP, false);
        }

        private GameObject OpenPlayerUICanvas(string canvasName, bool pauseGame = true)
        {
            GameObject openedCanvas = null;
            for (var i = 0; i < PausedGameCanvas.transform.childCount; i++)
            {
                var child = PausedGameCanvas.transform.GetChild(i).gameObject;
                var canvasFound = child.name == canvasName;
                
                if(canvasFound)
                {
                    openedCanvas = child;
                }
                
                child.SetActive(canvasFound);
            }

            PausedGameCanvas.SetActive(true);

            IsGamePaused = pauseGame;
            return openedCanvas;
        }

        public void PauseWithoutUI()
        {
            if(ReferenceManager.PlayerInputController != null)
            {
                ReferenceManager.PlayerInputController.SetPauseState();
            }
            
            PausedGameCanvas.SetActive(true);
            IsGamePaused = true;

            PlayerUI.SetActive(false);
        }

        public void CloseAllCanvasAndUnpause()
        {
            PausedGameCanvas.SetActive(false);
            for(int i = 0; i < PausedGameCanvas.transform.childCount; i++)
            {
                var child = PausedGameCanvas.transform.GetChild(i).gameObject;
                child.SetActive(false);
            }
            PlayerUI.SetActive(true);
            if (ReferenceManager.PlayerInputController != null)
            {
                ReferenceManager.PlayerInputController.EnableInputSystem();
            }
            IsGamePaused = false;
        }

        private void OnDestroy()
        {
            IsGamePaused = false;
        }
    }
}
