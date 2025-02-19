using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Weapons.Upgrades.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Weapons.Upgrades.UI
{
    public class WeaponUpgradeCardUI : MonoBehaviour
    {
        private const string BUY_BUTTON_PREFIX = "Buy:";
        
        private readonly Color COLOR_GRAY = Color.gray;
        private readonly Color COLOR_BLUE = Color.blue;
        private readonly Color COLOR_PURPLE = new Color(0.5f, 0f, 0.5f, 1f);
        private readonly Color COLOR_GOLD = new Color(0.831f, 0.686f, 0.216f, 1f);
        
        [SerializeField] private TextMeshProUGUI weaponName;
        [SerializeField] private Transform statisticParent;
        [SerializeField] private Image borderImage;
        [SerializeField] private Button buyButton;
        [SerializeField] private TextMeshProUGUI buyButtonText;

        private float upgradeCost = float.MaxValue;

        public Button BuyButton => buyButton;
        public TextMeshProUGUI WeaponName => weaponName;
        public WeaponUpgradeData WeaponUpgradeData { get; private set; }

        private void OnEnable()
        {
            RefreshBuyButton();
        }

        public WeaponStatisticUpgradeUI CreateNewUpgradeStatistic(WeaponStatisticUpgradeUI weaponStatisticUpgradeUI)
        {
            return Instantiate(weaponStatisticUpgradeUI, statisticParent, false);
        }

        public void SetupCard(WeaponUpgradeData upgradeData, WeaponUpgradeRarityEnum weaponRarity)
        {
            WeaponUpgradeData = upgradeData;
            borderImage.color = GetColorByRarity(weaponRarity);
            weaponName.text = upgradeData.WeaponDefinition.WeaponName;
            buyButtonText.text = $"{BUY_BUTTON_PREFIX} {upgradeData.UpgradeCost}";
            
            if(upgradeData.UpgradeCost >= 0)
            {
                upgradeCost = upgradeData.UpgradeCost;
            }
            
            RefreshBuyButton();
        }

        public void RefreshBuyButton()
        {
            var isEnoughPoints =
                ReferenceManager.PlayerCurrencyController.HasPlayerEnoughPoints(upgradeCost);
            buyButton.interactable = isEnoughPoints;
        }
        
        private Color GetColorByRarity(WeaponUpgradeRarityEnum weaponRarity)
        {
            switch (weaponRarity)
            {
                case WeaponUpgradeRarityEnum.Common:
                    return COLOR_GRAY;
                case WeaponUpgradeRarityEnum.Rare:
                    return COLOR_BLUE;
                case WeaponUpgradeRarityEnum.Epic:
                    return COLOR_PURPLE;
                case WeaponUpgradeRarityEnum.Legendary:
                    return COLOR_GOLD;
                case WeaponUpgradeRarityEnum.Undefined:
                default:
                    break;
            }
            
            Debug.LogError("Weapon rarity not assigned!");
            return Color.black;
        }
    }
}
