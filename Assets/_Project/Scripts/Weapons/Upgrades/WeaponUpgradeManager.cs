using System.Collections.Generic;
using _Project.Scripts.Analytics;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Patterns;
using _Project.Scripts.Weapons.Statistics;
using _Project.Scripts.Weapons.Upgrades.Data;
using _Project.Scripts.Weapons.Upgrades.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Weapons.Upgrades
{
    public class WeaponUpgradeManager : Singleton<WeaponUpgradeManager>
    {
        private const int NUMBER_OF_CARDS = 4;
        private const int UPGRADES_REROLL_MAX_TRIES = 100;
        private const int MAX_TIMES_FOR_ROLL_UPGRADE_BY_RARITY = 100;
        
        
        [SerializeField] private WeaponUpgradeHolder weaponUpgradeHolder;
        [SerializeField] private WeaponUpgradeCardUI upgradeCardPrefab;
        [SerializeField] private WeaponStatisticUpgradeUI statisticUpgradePrefab;
        [SerializeField] private bool onlyOneUpgradePerLevel;
        [SerializeField] private float baseMeleeWeaponDrawChance;
        [SerializeField] private int baseRerollCost;
        [SerializeField] private float baseRerollMultiplier;
        [SerializeField] private float commonRarityDropChance = 0.4f;
        [SerializeField] private float rareRarityDropChance = 0.3f;
        [SerializeField] private float epicRarityDropChance = 0.2f;
        [SerializeField] private float legendaryRarityDropChance = 0.1f;
        [SerializeField] private float rarityDropChance;

        private PlayerWeaponUpgradeManager playerWeaponUpgradeManager;
        private WeaponUpgradeUIManager weaponUpgradeUIManager;
        private Dictionary<WeaponUpgradeData, IWeaponStatistics> dictionaryOfUpgrades = new ();
        private bool isUpgradeAvailable = true;

        private float currentMeleeWeaponDrawChance;
        private int currentRerollCost;
        private float currentRerollMultiplier;
        private float rarityDropChanceSum;

        public bool IsUpgradeAvailable => isUpgradeAvailable;

        protected override void Awake()
        {
            base.Awake();

            ReferenceManager.WeaponUpgradeManager = this;
            currentMeleeWeaponDrawChance = baseMeleeWeaponDrawChance;
            currentRerollCost = baseRerollCost;
            currentRerollMultiplier = baseRerollMultiplier;
            
            ReferenceManager.LevelHandler.onNextLevel += OnNextLevel;
        }

        public void TryShowWeaponUpgrades()
        {
            if (!IsUpgradeAvailable)
            {
                return;
            }

            if (playerWeaponUpgradeManager is null)
            {
                playerWeaponUpgradeManager =
                    ReferenceManager.BlakeHeroCharacter.GetComponent<PlayerWeaponUpgradeManager>();
            }

            if (weaponUpgradeUIManager is null)
            {
                weaponUpgradeUIManager = GameHandler.Instance.OpenWeaponUpgradesCanvas().GetComponent<WeaponUpgradeUIManager>();
                
                RerollUpgrades();
                weaponUpgradeUIManager.RerollButton.onClick.AddListener(() => RerollUpgrades(true));
            }
            else
            {
                GameHandler.Instance.OpenWeaponUpgradesCanvas();
            }
        }
        
        private void RerollUpgrades(bool rerolledByPlayer = false)
        {
            if (rerolledByPlayer)
            {
                ReferenceManager.PlayerCurrencyController.RemovePoints(currentRerollCost);
                currentRerollCost = (int)(currentRerollCost * currentRerollMultiplier);
            }
            
            weaponUpgradeUIManager.UpdateRerollButton(currentRerollCost);

            DestroyCards();

            for (var i = 0; i < NUMBER_OF_CARDS; i++)
            {
                CreateUpgrade();
            }
        }

        private void CreateUpgrade(int siblingIndex = -1)
        {
            for (var i = 0; i < UPGRADES_REROLL_MAX_TRIES; i++)
            {
                var upgradeData = GetUpgradeData();
                
                if (!dictionaryOfUpgrades.TryAdd(upgradeData.Item1, upgradeData.Item2))
                {
                    continue;
                }
                
                CreateCard(upgradeData.Item1, upgradeData.Item2, siblingIndex);
                return;
            }
            
            Debug.LogError($"Cannot find available upgrade!");
        }

        private (WeaponUpgradeData, IWeaponStatistics) GetUpgradeData()
        {
            var upgradeRarityToSearchFor = GetRandomizedRarityForUpgrade();
            WeaponUpgradeSo drawnWeapon;
            
            if(Random.value > currentMeleeWeaponDrawChance)
            {
                var randomWeaponNumber = Random.Range(0, weaponUpgradeHolder.ranged.Count);
                drawnWeapon = weaponUpgradeHolder.ranged[randomWeaponNumber];
                var upgradeData = new RangedWeaponUpgradeData();

                for (var i = 0; i < MAX_TIMES_FOR_ROLL_UPGRADE_BY_RARITY; i++)
                {
                    var randomStatisticNumber = Random.Range(0, drawnWeapon.rangedUpgradeData.Count);
                    upgradeData = drawnWeapon.rangedUpgradeData[randomStatisticNumber];

                    if (upgradeData.WeaponUpgradeRarity != upgradeRarityToSearchFor) continue;
                    
                    var weaponStatistics = upgradeData.rangedWeaponStatistics;
                    return (upgradeData, weaponStatistics);
                }
            }
            else
            {
                var randomWeaponNumber = Random.Range(0, weaponUpgradeHolder.melee.Count);
                drawnWeapon = weaponUpgradeHolder.melee[randomWeaponNumber];
                var upgradeData = new MeleeWeaponUpgradeData();

                for (var i = 0; i < MAX_TIMES_FOR_ROLL_UPGRADE_BY_RARITY; i++)
                {
                    var randomStatisticNumber = Random.Range(0, drawnWeapon.meleeUpgradeData.Count);
                    upgradeData = drawnWeapon.meleeUpgradeData[randomStatisticNumber];
                    
                    if (upgradeData.WeaponUpgradeRarity != upgradeRarityToSearchFor) continue;

                    var weaponStatistics = upgradeData.meleeWeaponStatistics;
                    return (upgradeData, weaponStatistics);
                }
            }
            
            Debug.LogError($"Probably {drawnWeapon.weaponDefinition.WeaponName} don't have specified upgrade rarity: {upgradeRarityToSearchFor}");
            return (null, null);
        }

        private WeaponUpgradeRarityEnum GetRandomizedRarityForUpgrade()
        {
            var randomValue = Random.value;
            var baseValue = 0f;
            
            if ((baseValue += commonRarityDropChance) > randomValue)
            {
                return WeaponUpgradeRarityEnum.Common;
            }
            
            if ((baseValue += rareRarityDropChance) > randomValue)
            {
                return WeaponUpgradeRarityEnum.Rare;
            }
            
            if ((baseValue += epicRarityDropChance) > randomValue)
            {
                return WeaponUpgradeRarityEnum.Epic;
            }
            
            if ((baseValue += legendaryRarityDropChance) > randomValue)
            {
                return WeaponUpgradeRarityEnum.Legendary;
            }

            return WeaponUpgradeRarityEnum.Undefined;
        }

        private void CreateCard(WeaponUpgradeData upgradeData, IWeaponStatistics weaponStatistics, int siblingIndex)
        {
            var instantiatedCard = weaponUpgradeUIManager.CreateNewUpgradeCard(upgradeCardPrefab, siblingIndex);
            instantiatedCard.SetupCard(upgradeData, upgradeData.WeaponUpgradeRarity);
            
            instantiatedCard.BuyButton.onClick.AddListener(() => OnUpgradeBought(upgradeData));
            
            foreach (var (statName, upgradeValue) in weaponStatistics.GetNonZeroFields())
            {
                var currentValue = playerWeaponUpgradeManager.GetCurrentWeaponStatistics(upgradeData.WeaponDefinition)
                    .GetValueByName(statName);
                
                CreateStatistic(instantiatedCard, statName, currentValue, upgradeValue);
            }
        }

        private void CreateStatistic(WeaponUpgradeCardUI instantiatedCard, string upgradeName, float currentValue,
            float upgradeValue)
        {
            var instantiatedStatistic = instantiatedCard.CreateNewUpgradeStatistic(statisticUpgradePrefab);
            instantiatedStatistic.SetupStatistic(upgradeName, currentValue, upgradeValue);
        }

        private void RedrawCard(WeaponUpgradeData upgradeData)
        {
            dictionaryOfUpgrades.Remove(upgradeData);
            weaponUpgradeUIManager.DestroyCard(upgradeData, out var siblingIndex);
            CreateUpgrade(siblingIndex);
        }
        
        private void OnUpgradeBought(WeaponUpgradeData upgradeData)
        {
            if (onlyOneUpgradePerLevel)
            {
                weaponUpgradeUIManager.CloseUI();
            }
            
            if (dictionaryOfUpgrades.TryGetValue(upgradeData, out var statistics))
            {
                ReferenceManager.BlakeHeroCharacter.GetComponent<PlayerWeaponUpgradeManager>()
                    .AddWeaponUpgrade(upgradeData.WeaponDefinition.WeaponName, statistics);

                ReferenceManager.PlayerCurrencyController.RemovePoints(upgradeData.UpgradeCost);
                weaponUpgradeUIManager.RefreshCardsStatus();
                
                this.TrySendAnalytics(upgradeData, dictionaryOfUpgrades);
                
                if (onlyOneUpgradePerLevel)
                {
                    ResetAllValues();
                    isUpgradeAvailable = false;
                }
                else
                {
                    RedrawCard(upgradeData);
                }
                
                Debug.Log($"Upgraded: {upgradeData.UpgradeCost} | {upgradeData.WeaponUpgradeRarity} | {upgradeData.WeaponDefinition.WeaponName}");
            }
            else
            {
                Debug.LogError("No statistics found!");
            }
        }
        
        private void OnNextLevel()
        {
            isUpgradeAvailable = true;
            ResetAllValues();
        }

        private void DestroyCards()
        {
            weaponUpgradeUIManager?.DestroyCards();
            dictionaryOfUpgrades.Clear();
        }

        private void ResetAllValues()
        {
            currentMeleeWeaponDrawChance = baseMeleeWeaponDrawChance;
            currentRerollCost = baseRerollCost;
            currentRerollMultiplier = baseRerollMultiplier;
            
            DestroyCards();
            weaponUpgradeUIManager?.RerollButton?.onClick?.RemoveAllListeners();
            weaponUpgradeUIManager = null;
        }

        private void OnValidate()
        {
            rarityDropChance = commonRarityDropChance + rareRarityDropChance + epicRarityDropChance +
                               legendaryRarityDropChance;

            if (!Mathf.Approximately(rarityDropChance, 1))
            {
                Debug.Log($"Sum of rarity drop chances are different than 1 in {name}");
            }
        }
    }
}
