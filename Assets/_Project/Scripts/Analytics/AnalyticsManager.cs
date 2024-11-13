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
    }
    public static class AnalyticsParameterNames
    {
        public const string PLACEMENT_NAME = "placementName";
        public const string ITEM_NAME = "itemName";
        public const string KILLER = "killer";
        public const string LEVEL_NAME = "LevelName";
        public const string ROOMS_BEATEN = "RoomsBeaten";
        public const string ROOMS_TO_BEAT = "RoomsToBeat";
    }
    
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        [Header("Analytics Enabled:")]
        [SerializeField] private bool heroDead;
        [SerializeField] private bool weaponPickup;
        [SerializeField] private bool levelCompleted;

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
                    };
                }

                return _enabledAnalytics;
            }
        }

        private void Start()
        {
            AnalyticsService.Instance.StartDataCollection();
        }

        public void SendCustomData(string eventName, Dictionary<string, object> parameters)
        {
            if (!EnabledAnalytics.TryGetValue(eventName, out var isEnabled) || !isEnabled)
            {
                return;
            }
            
            AnalyticsService.Instance.CustomData(eventName, parameters);
            AnalyticsService.Instance.Flush();
        }

        private void OnDestroy()
        {
            AnalyticsService.Instance.StopDataCollection();
        }
    }
}
