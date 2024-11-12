using _Project.Scripts.GlobalHandlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ControlTwoPerk", menuName = "Abilities/Perks/Control Two")]

public class ControlTwo : PerkScriptableObject
{
    public float scaleToAdd = 0.5f;
    PlayerPerkManager player;
    public override void OnPickup(PlayerPerkManager player)
    {
        this.player = player;
        foreach(Room room in ReferenceManager.RoomManager.GetAllRooms())
        {
            room.AddToBlockerScale(scaleToAdd);
        }
        ReferenceManager.Instance.OnFloorLoad += Refresh;
    }
    private void Refresh()
    {
        foreach (Room room in ReferenceManager.RoomManager.GetAllRooms())
        {
            room.AddToBlockerScale(scaleToAdd);
        }
    }


    public override void OnRemove()
    {
        foreach (Room room in ReferenceManager.RoomManager.GetAllRooms())
        {
            room.AddToBlockerScale(-scaleToAdd);
        }
    }
}
