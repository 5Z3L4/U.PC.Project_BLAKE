using _Project.Scripts.Floor_Generation;
using _Project.Scripts.GlobalHandlers;
using UnityEngine;

namespace _Project.Scripts.UI.Gameplay
{
    public class PausedGameCanvasController : MonoBehaviour
    {

        [SerializeField]
        private FloorManager floorManager;
    
        private void Start()
        {
            floorManager.FloorGeneratorEnd += FloorManagerOnFloorGeneratorEnd;
        }

        public void SwitchPauseGameCanvas()
        {
            if (GameHandler.Instance.IsGamePaused)
            {
                GameHandler.Instance.CloseAllCanvasAndUnpause();
            }
            else
            {
                GameHandler.Instance.OpenPauseGameCanvas();
            }
        }

        public void CloseAllCanvasAndUnpause()
        {
            GameHandler.Instance.CloseAllCanvasAndUnpause();
        }
        
        public void LeaveToMainMenu()
        {
            ReferenceManager.SceneHandler.LoadMainMenu();
        }

        public void PlayAgain()
        {
            ReferenceManager.SceneHandler.StartNewGame();
        }

        private void FloorManagerOnFloorGeneratorEnd(Transform playerTransform, Transform cameraFollowTransform)
        {
            ReferenceManager.PlayerInputController.escapeButtonEvent += SwitchPauseGameCanvas;
        }

        private void OnDestroy()
        {
            if(ReferenceManager.PlayerInputController != null)
            {
                ReferenceManager.PlayerInputController.escapeButtonEvent -= SwitchPauseGameCanvas;
            }
        }

    }
}
