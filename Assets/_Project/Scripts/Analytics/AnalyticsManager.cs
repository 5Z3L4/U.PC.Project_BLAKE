using System.Collections.Generic;
using _Project.Scripts.Patterns;
using _Project.Scripts.Weapons.Upgrades;
using Unity.Services.Analytics;
using UnityEngine;

namespace _Project.Scripts.Analytics
{
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        [Header("Analytics Enabled:")]
        [SerializeField] private bool heroDead;
        [SerializeField] private bool weaponPickup;
        [SerializeField] private bool levelCompleted;
        [SerializeField] private bool perkBought;
        [SerializeField] private bool weaponUpgradeBought;

        [Space] 
        [SerializeField] private bool instantSendInfo;

        private Dictionary<string, bool> _enabledAnalytics;

        private Dictionary<string, bool> EnabledAnalytics
        {
            get
            {
                if (_enabledAnalytics == null || _enabledAnalytics.Count == 0)
                {
                    _enabledAnalytics = new Dictionary<string, bool>
                    {
                        { AnalyticsEventNames.HeroDead, heroDead },
                        { AnalyticsEventNames.WeaponPickup, weaponPickup },
                        { AnalyticsEventNames.LevelCompleted, levelCompleted },
                        { AnalyticsEventNames.PerkBought, perkBought },
                        { AnalyticsEventNames.WeaponUpgradeBought, weaponUpgradeBought },
                    };
                }

                return _enabledAnalytics;
            }
        }

        private void Start()
        {
            AnalyticsService.Instance.StartDataCollection();
        }

        public void SendCustomData(string eventName, Dictionary<string, object> parameters, bool forceSendInfo = false)
        {
            if (!EnabledAnalytics.TryGetValue(eventName, out var isEnabled) || !isEnabled)
            {
                return;
            }
            
            AnalyticsService.Instance.CustomData(eventName, parameters);

            if (forceSendInfo || instantSendInfo)
            {
                AnalyticsService.Instance.Flush();
            }
        }

        private void OnDestroy()
        {
            AnalyticsService.Instance?.Flush();
            AnalyticsService.Instance?.StopDataCollection();
        }
    }
    
    
    public static class AnalyticsEventNames
    {
        public const string HeroDead = "HeroDead";
        public const string WeaponPickup = "WeaponPickup";
        public const string LevelCompleted = "LevelCompleted";
        public const string PerkBought = "PerkBought";
        public const string WeaponUpgradeBought = "WeaponUpgradeBought";
    }
    
    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberCanBePrivate.Global
    public static class AnalyticsParameterNames
    {
        public const string PlacementName = "placementName";
        public const string ItemName = "itemName";
        public const string Killer = "killer";
        public const string PerkName = "PerkName";
        
        public const string LevelName = "LevelName";
        public const string RoomsBeaten = "RoomsBeaten";
        public const string RoomsToBeat = "RoomsToBeat";
        
        public const string WeaponUpgrades_WeaponName = "WeaponUpgrades_WeaponName";
        public const string WeaponUpgrades_WeaponRarity = "WeaponUpgrades_WeaponRarity";
        public const string WeaponUpgrades_UpgradeCost = "WeaponUpgrades_UpgradeCost";
        public const string WeaponUpgrades_UpgradedStatName1 = "WeaponUpgrades_UpgradedStatName1";
        public const string WeaponUpgrades_UpgradedStatValue1 = "WeaponUpgrades_UpgradedStatValueFloat1";
        public const string WeaponUpgrades_UpgradedStatName2 = "WeaponUpgrades_UpgradedStatName2";
        public const string WeaponUpgrades_UpgradedStatValue2 = "WeaponUpgrades_UpgradedStatValueFloat2";
        public const string WeaponUpgrades_UpgradedStatName3 = "WeaponUpgrades_UpgradedStatName3";
        public const string WeaponUpgrades_UpgradedStatValue3 = "WeaponUpgrades_UpgradedStatValueFloat3";
        public const string WeaponUpgrades_UpgradedStatName4 = "WeaponUpgrades_UpgradedStatName4";
        public const string WeaponUpgrades_UpgradedStatValue4 = "WeaponUpgrades_UpgradedStatValueFloat4";

        public static (string, string) GetWeaponUpgradeStat(int index)
        {
            return index switch
            {
                1 => (WeaponUpgrades_UpgradedStatName1, WeaponUpgrades_UpgradedStatValue1),
                2 => (WeaponUpgrades_UpgradedStatName2, WeaponUpgrades_UpgradedStatValue2),
                3 => (WeaponUpgrades_UpgradedStatName3, WeaponUpgrades_UpgradedStatValue3),
                4 => (WeaponUpgrades_UpgradedStatName4, WeaponUpgrades_UpgradedStatValue4),
                _ => ("", "")
            };
        }
    }
    // ReSharper restore InconsistentNaming
    // ReSharper restore MemberCanBePrivate.Global
}
