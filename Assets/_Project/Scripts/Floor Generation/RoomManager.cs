using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.GlobalHandlers;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private Room activeRoom;
    private Room previousRoom;
    public delegate void EnteredRoom(Room room);
    public event EnteredRoom onRoomEnter;
    public event EnteredRoom onRoomLeave;
    public bool isControlOneActivated = false;

    private List<Room> allRooms = new List<Room>();

    [SerializeField]
    private Transform minimapFloor;

    private void Awake()
    {
        ReferenceManager.RoomManager = this;
    }


    public void AddRoom(Room room)
    {
        allRooms.Add(room);
    }

    public void ClearRooms()
    {
        allRooms.Clear();
    }

    public List<Room> GetAllRooms()
    {
        return allRooms;
    }

    public void SetActiveRoom(Room newRoom)
    {
        activeRoom = newRoom;
        onRoomEnter?.Invoke(activeRoom);
    }

    public Room GetActiveRoom()
    {
        return activeRoom;
    }

    public void SetPreviousRoom(Room newRoom)
    {
        if (newRoom == null) return;
        previousRoom = newRoom;
        onRoomLeave?.Invoke(previousRoom);
    }

    public Room GetPreviousRoom()
    {
        return previousRoom;
    }

    public Transform GetMinimapFloor()
    {
        return minimapFloor;
    }
    /*
    public void OnDestroy()
    {
        Delegate[] delegateList = onRoomEnter.GetInvocationList();
        foreach(var d in delegateList)
        {
            onRoomEnter -= (d as EnteredRoom);
        }

        delegateList = onRoomLeave.GetInvocationList();
        foreach (var d in delegateList)
        {
            onRoomLeave -= (d as EnteredRoom);
        }
    }*/
}
