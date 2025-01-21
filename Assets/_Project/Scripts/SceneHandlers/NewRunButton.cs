using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.SceneHandlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewRunButton : MonoBehaviour
{
    public void OnClick()
    {
        GameHandler.Instance.IsGamePaused = true;
        SceneHandler.Instance.StartNewGame();
    }
}