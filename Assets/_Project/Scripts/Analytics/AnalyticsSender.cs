using System.Collections.Generic;
using _Project.Scripts.Floor_Generation;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.SceneHandlers;
using _Project.Scripts.Weapons;
using _Project.Scripts.Weapons.Definition;
using _Project.Scripts.Weapons.Statistics;
using _Project.Scripts.Weapons.Upgrades;
using _Project.Scripts.Weapons.Upgrades.Data;
using UnityEngine;

namespace _Project.Scripts.Analytics
{
    public static class AnalyticsSender
    {
        public static void TrySendAnalytics(this WeaponUpgradeManager _, WeaponUpgradeData upgradeData, Dictionary<WeaponUpgradeData, IWeaponStatistics> dictionaryOfUpgrades)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            var parameters = new Dictionary<string, object>()
            {
                { AnalyticsParameterNames.WeaponUpgrades_WeaponName, upgradeData.WeaponDefinition.WeaponName },
                { AnalyticsParameterNames.WeaponUpgrades_WeaponRarity, upgradeData.WeaponUpgradeRarity.ToString() },
                { AnalyticsParameterNames.WeaponUpgrades_UpgradeCost, upgradeData.UpgradeCost },
            };

            if (dictionaryOfUpgrades.TryGetValue(upgradeData, out var statistics))
            {
                var index = 1;
                foreach (var (statName, upgradeValue) in statistics.GetNonZeroFields())
                {
                    if (index > 4)
                    {
                        break;
                    }
                    
                    var upgradeStat = AnalyticsParameterNames.GetWeaponUpgradeStat(index);
                    parameters.Add(upgradeStat.Item1, statName);
                    parameters.Add(upgradeStat.Item2, upgradeValue);
                    index++;
                }
            }

            AnalyticsManager.Instance.SendCustomData(AnalyticsEventNames.WeaponUpgradeBought, parameters, true);
#endif
        }
        
        public static void TrySendAnalytics(this LevelHandler _, string levelName)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            var parameters = new Dictionary<string, object>()
            {
                { AnalyticsParameterNames.LevelName, levelName },
                { AnalyticsParameterNames.RoomsBeaten, FloorManager.Instance.RoomsDoneCounter.RoomsBeaten },
                { AnalyticsParameterNames.RoomsToBeat, FloorManager.Instance.RoomsDoneCounter.RoomsInitialized },
            };

            AnalyticsManager.Instance.SendCustomData(AnalyticsEventNames.LevelCompleted, parameters, true);
#endif
        }

        public static void TrySendAnalytics(this PerkShop _, PerkScriptableObject perk)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            var parameters = new Dictionary<string, object>()
            {
                { AnalyticsParameterNames.PerkName, perk.perkName },
            };

            AnalyticsManager.Instance.SendCustomData(AnalyticsEventNames.PerkBought, parameters);
#endif
        }

        public static void TrySendAnalytics(this BlakeHeroCharacter _, GameObject killer)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            if (killer == null)
            {
                return;
            }
        
            var parameters = new Dictionary<string, object>()
            {
                { AnalyticsParameterNames.Killer, killer.name },
                { AnalyticsParameterNames.ItemName, killer.GetComponent<AIController>()?.Weapon?.name },
                { AnalyticsParameterNames.PlacementName, ReferenceManager.RoomManager.GetActiveRoom().name }
            };
            
            AnalyticsManager.Instance.SendCustomData(AnalyticsEventNames.HeroDead, parameters);
#endif
        }
        
        public static void TrySendAnalytics(this WeaponPickup _, WeaponDefinition weaponDefinition)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            var parameters = new Dictionary<string, object>()
            {
                { AnalyticsParameterNames.ItemName, weaponDefinition.WeaponName }
            };

            AnalyticsManager.Instance.SendCustomData(AnalyticsEventNames.WeaponPickup, parameters);
#endif
        }
    }
}