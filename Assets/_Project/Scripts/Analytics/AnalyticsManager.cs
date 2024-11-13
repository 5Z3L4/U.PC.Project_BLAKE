using System.Collections.Generic;
using _Project.Scripts.Patterns;
using Unity.Services.Analytics;
using UnityEngine;

namespace _Project.Scripts.Analytics
{
    public static class AnalyticsEventNames
    {
        public const string HERO_DEAD = "HeroDead";
        public const string WEAPON_PICKUP = "WeaponPickup";
        public const string LEVEL_COMPLETED = "LevelCompleted";
        public const string PERK_BOUGHT = "PerkBought";
    }
    public static class AnalyticsParameterNames
    {
        public const string PLACEMENT_NAME = "placementName";
        public const string ITEM_NAME = "itemName";
        public const string KILLER = "killer";
        public const string LEVEL_NAME = "LevelName";
        public const string ROOMS_BEATEN = "RoomsBeaten";
        public const string ROOMS_TO_BEAT = "RoomsToBeat";
        public const string PERK_NAME = "PerkName";
    }
    
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        [Header("Analytics Enabled:")]
        [SerializeField] private bool heroDead;
        [SerializeField] private bool weaponPickup;
        [SerializeField] private bool levelCompleted;
        [SerializeField] private bool perkBought;

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
                        { AnalyticsEventNames.HERO_DEAD, heroDead },
                        { AnalyticsEventNames.WEAPON_PICKUP, weaponPickup },
                        { AnalyticsEventNames.LEVEL_COMPLETED, levelCompleted },
                        { AnalyticsEventNames.PERK_BOUGHT, perkBought },
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
            AnalyticsService.Instance.Flush();
            AnalyticsService.Instance.StopDataCollection();
        }
    }
}
