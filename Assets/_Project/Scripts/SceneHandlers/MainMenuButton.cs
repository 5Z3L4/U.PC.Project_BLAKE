using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.SceneHandlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButton : MonoBehaviour
{
    public void OnClick()
    {
        GameHandler.Instance.IsGamePaused = false;
        SceneHandler.Instance.LoadMainMenu();
    }
}
