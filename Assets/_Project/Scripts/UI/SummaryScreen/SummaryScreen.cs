using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Weapons;
using _Project.Scripts.Floor_Generation;
using _Project.Scripts.SceneHandlers;

public class SummaryScreen : MonoBehaviour
{
    [SerializeField] private Image winLoseImage;
    [SerializeField] private Sprite winImage;
    [SerializeField] private Sprite loseImage;

    [SerializeField] private GameObject killedByPanel;
    [SerializeField] private TMP_Text killedByText;

    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text floorText;
    [SerializeField] private TMP_Text playTimeText;
    [SerializeField] private TMP_Text roomsText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text weaponText;

    [SerializeField] private Image newRunButton;
    [SerializeField] private Image mainMenuButton;

    public void SetupSummary(bool win)
    {
        gameObject.SetActive(true);
        if(win)
        {
            killedByPanel.SetActive(false);
            winLoseImage.sprite = winImage;

            newRunButton.color = Color.green;
            mainMenuButton.color = Color.green;
        }
        else
        {
            killedByPanel.SetActive(true);
            winLoseImage.sprite = loseImage;

            newRunButton.color = Color.red;
            mainMenuButton.color = Color.red;
        }
        SetupRepeatingSummary();
    }

    private void SetupRepeatingSummary()
    {
        var time = SummaryHandler.Instance.GetTimeSpent();
        pointsText.text = string.Format($"Total Points: {Mathf.FloorToInt(SummaryHandler.Instance.points)}");
        killsText.text = string.Format($"Total Kills: {SummaryHandler.Instance.kills}");
        playTimeText.text = string.Format("Play Time: {0:D2}:{1:D2}", Mathf.Floor(time / 60).ToString(), Mathf.FloorToInt(time % 60).ToString());
        weaponText.text = string.Format($"Most efficient weapon: {ReferenceManager.BlakeHeroCharacter.GetComponent<WeaponsManager>().GetBestWeaponName()}");
        var counter = FloorManager.Instance.roomsDoneCounter;
        roomsText.text = string.Format($"Rooms: {counter.RoomsBeaten}/{counter.RoomsInitialized}");
        var floorCounter = LevelHandler.Instance.GetLevelList();
        floorText.text = string.Format($"Floor: {LevelHandler.Instance.GetIndex() + 1}/{floorCounter.Length}");
    }

    public void SetKiller(string killerName)
    {
        killedByText.text = string.Format($"Killed By: {killerName}");
    }
}
