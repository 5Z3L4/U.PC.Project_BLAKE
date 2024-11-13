using _Project.Scripts.GlobalHandlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ControlOnePerk", menuName = "Abilities/Perks/Control One")]

public class ControlOne : PerkScriptableObject
{
    PlayerPerkManager player;
    public override void OnPickup(PlayerPerkManager player)
    {
        this.player = player;
        ReferenceManager.RoomManager.isControlOneActivated = true;
        foreach(Room room in ReferenceManager.RoomManager.GetAllRooms())
        {
            room.SetControlPerkOne(true);
        }
        ReferenceManager.Instance.OnFloorLoad += Refresh;
    }
    private void Refresh()
    {
        ReferenceManager.RoomManager.isControlOneActivated = true;
        foreach (Room room in ReferenceManager.RoomManager.GetAllRooms())
        {
            room.SetControlPerkOne(true);
            room.SetupFogBlockers();
        }
    }


    public override void OnRemove()
    {
        ReferenceManager.RoomManager.isControlOneActivated = false;
    }
}
