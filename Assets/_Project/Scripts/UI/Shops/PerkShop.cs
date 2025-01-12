using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.PointsSystem;
using System;
using _Project.Scripts.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private TMP_Text pointsText;
    private PerkShopButton lastButton;
    private int perkCost = 0;
    private PerkScriptableObject perkToBuy;

    private static int perkRarityOneCost = 1000;
    private static int perkRarityTwoCost = 1500;
    private static int perkRarityThreeCost = 2000;

    public static void ResetShopValues()
    {
        perkRarityOneCost = 1000;
        perkRarityTwoCost = 1500;
        perkRarityThreeCost = 2000;
    }

    public void SetPerk(string name, string desc, PerkScriptableObject perk, PerkShopButton button, Color perkColor)
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

        buyButton.GetComponentInParent<Image>().color = perkColor;
        perkName.color = perkColor;

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
        this.TrySendAnalytics(perkToBuy);
        EnemyDeathMediator.Instance.PlayerCurrencyController.RemovePoints(perkCost);
        EnemyDeathMediator.Instance.Refresh();
        switch (lastButton.perkRarity)
        {
            case PerkRarity.One:
                perkRarityOneCost += 250;
                break;
            case PerkRarity.Two:
                perkRarityTwoCost += 375;
                break;
            case PerkRarity.Three:
                perkRarityThreeCost += 500;
                break;
        }
        
        lastButton.Buy();
        lastButton.OnClick();
    }

    private void RefreshPoints(float points)
    {
        pointsText.text = $"Points: {points}";
    }

    private void OnEnable()
    {
        ReferenceManager.PlayerCurrencyController.OnPointsChanged += RefreshPoints;
        ReferenceManager.PlayerCurrencyController.RefreshPoints();
    }

    private void OnDisable()
    {
        ReferenceManager.PlayerCurrencyController.OnPointsChanged += RefreshPoints;
    }
}
