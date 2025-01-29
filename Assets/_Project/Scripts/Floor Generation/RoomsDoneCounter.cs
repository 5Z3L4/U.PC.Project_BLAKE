using System;
using _Project.Scripts.GlobalHandlers;
using UnityEngine;

public class RoomsDoneCounter : MonoBehaviour
{
    public event Action OnRoomBeaten;
    private FloorGenerator floorGenerator;
    private int roomsInitialized;
    private int roomsBeaten;
    public int RoomsInitialized => roomsInitialized;
    public int RoomsBeaten => roomsBeaten;

    private void Awake()
    {
        floorGenerator = GetComponent<FloorGenerator>();
    }

    public void Initialize()
    {
        roomsInitialized = floorGenerator.MaxRooms;
    }

    public void AddBeatenRoom()
    {
        roomsBeaten++;
        CheckRoomsCounter();
        Debug.Log($"Beaten rooms: {RoomsBeaten}/{RoomsInitialized}");
        OnRoomBeaten?.Invoke();
    }

    private void CheckRoomsCounter()
    {
        if (RoomsBeaten >= RoomsInitialized)
        {
            GameHandler.Instance.PlayerWin();
        }
    }

    private void ResetValues()
    {
        roomsBeaten = 0;
        roomsInitialized = 0;
    }
    private void OnDestroy()
    {
        ResetValues();
    }

}
