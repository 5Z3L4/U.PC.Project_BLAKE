using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkShopButton : MonoBehaviour
{

    [SerializeField] private string perkName;
    [SerializeField, TextArea] private string perkDesc;
    [SerializeField] private PerkShop shop;
    [SerializeField] private PerkScriptableObject perk;
    

    private bool bought = false;
    public bool PerkBought => bought;
    public PerkShop.PerkRarity perkRarity;
    public void OnClick()
    {
        shop.SetPerk(perkName, perkDesc, perk, this);
    }

    public void Buy()
    {
        bought = true;
    }
}
