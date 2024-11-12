using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.PointsSystem;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PerkShop : MonoBehaviour
{
    public enum PerkRarity
    {
        One,
        Two,
        Three
    }

    [SerializeField] private TMP_Text perkName;
    [SerializeField] private TMP_Text perkDesc;
    [SerializeField] private TMP_Text buyButton;
    private PerkShopButton lastButton;
    private int perkCost = 0;
    private PerkScriptableObject perkToBuy;

    private int perkRarityOneCost = 1000;
    private int perkRarityTwoCost = 2000;
    private int perkRarityThreeCost = 3000;
    public void SetPerk(string name, string desc, PerkScriptableObject perk, PerkShopButton button)
    {
        perkName.text = name;
        perkDesc.text = desc;
        perkToBuy = perk;
        lastButton = button;

        switch (button.perkRarity)
        {
            case PerkRarity.One:
                perkCost = perkRarityOneCost;
                break;
            case PerkRarity.Two:
                perkCost = perkRarityTwoCost;
                break;
            case PerkRarity.Three:
                perkCost = perkRarityThreeCost;
                break;
            default:
                perkCost = 1000;
                break;
        }

        if(!button.PerkBought)
        {
            buyButton.text = String.Format("Buy: {0} points", perkCost);
        } else
        {
            buyButton.text = "PERK BOUGHT";
        }
    }

    public void BuyPerk()
    {
        if (perkToBuy == null) return;
        if (lastButton == null) return;
        if (lastButton.PerkBought) return;

        var player = ReferenceManager.BlakeHeroCharacter?.GetComponent<PlayerPerkManager>();
        if (player == null) return;
        
        if (EnemyDeathMediator.Instance.PlayerCurrencyController.Points < perkCost) return;


        player.AddPerk(perkToBuy);
        EnemyDeathMediator.Instance.PlayerCurrencyController.RemovePoints(perkCost);
        EnemyDeathMediator.Instance.Refresh();
        switch (lastButton.perkRarity)
        {
            case PerkRarity.One:
                perkRarityOneCost += 250;
                break;
            case PerkRarity.Two:
                perkRarityTwoCost += 500;
                break;
            case PerkRarity.Three:
                perkRarityThreeCost += 750;
                break;
        }
        lastButton.Buy();
        lastButton.OnClick();
    }
}
