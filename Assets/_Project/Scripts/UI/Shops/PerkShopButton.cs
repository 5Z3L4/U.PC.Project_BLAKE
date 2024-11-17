using _Project.Scripts.GlobalHandlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkShopButton : MonoBehaviour
{

    [SerializeField] private string perkName;
    [SerializeField, TextArea] private string perkDesc;
    [SerializeField] private PerkShop shop;
    [SerializeField] private PerkScriptableObject perk;
    [SerializeField] private Color perkColor;
    

    private bool bought = false;
    public bool PerkBought => bought;
    public PerkShop.PerkRarity perkRarity;

    private void OnEnable()
    {
        if(ReferenceManager.PlayerInputController.GetComponent<PlayerPerkManager>().GetPerkList().Contains(perk))
        {
            bought = true;
        }
    }
    public void OnClick()
    {
        shop.SetPerk(perkName, perkDesc, perk, this, perkColor);
    }

    public void Buy()
    {
        bought = true;
    }
}
