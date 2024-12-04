using _Project.Scripts.GlobalHandlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerkShopButton : MonoBehaviour
{

    [SerializeField] private string perkName;
    [SerializeField, TextArea] private string perkDesc;
    [SerializeField] private PerkShop shop;
    [SerializeField] private PerkScriptableObject perk;
    [SerializeField] private Color perkColor;
    [SerializeField] private Sprite bouthSprite;
    [SerializeField] private Sprite bouthSelectedSprite;


    private bool bought = false;
    public bool PerkBought => bought;
    public PerkShop.PerkRarity perkRarity;

    private void OnEnable()
    {
        if(ReferenceManager.PlayerInputController.GetComponent<PlayerPerkManager>().GetPerkList().Contains(perk))
        {
            Buy();
        }
    }
    public void OnClick()
    {
        shop.SetPerk(perkName, perkDesc, perk, this, perkColor);
    }

    public void Buy()
    {
        bought = true;
        GetComponent<Image>().sprite = bouthSprite;
        SpriteState spriteState = new SpriteState();
        spriteState.selectedSprite = bouthSelectedSprite;
        GetComponent<Button>().spriteState = spriteState;
    }
}
