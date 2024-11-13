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
    }
    
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        [SerializeField] private bool heroDead;
        [SerializeField] private bool weaponPickup;

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
