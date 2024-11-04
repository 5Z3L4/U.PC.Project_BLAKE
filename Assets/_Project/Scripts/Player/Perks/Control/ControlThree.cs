using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.PointsSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ControlThreePerk", menuName = "Abilities/Perks/Control Three")]

public class ControlThree : PerkScriptableObject
{
    PlayerPerkManager player;
    public override void OnPickup(PlayerPerkManager player)
    {
        this.player = player;
        foreach(Room room in ReferenceManager.RoomManager.GetAllRooms())
        {
            room.SetControlPerkThree(true);
        }
        EnemyDeathMediator.Instance.ComboController.AddMinCombo(-0.3f);
        ReferenceManager.Instance.OnFloorLoad += Refresh;
    }
    private void Refresh()
    {
        foreach (Room room in ReferenceManager.RoomManager.GetAllRooms())
        {
            room.SetControlPerkThree(true);
        }
    }


    public override void OnRemove()
    {
        EnemyDeathMediator.Instance.ComboController.AddMinCombo(0.3f);
        foreach (Room room in ReferenceManager.RoomManager.GetAllRooms())
        {
            room.SetControlPerkThree(false);
        }
    }
}
