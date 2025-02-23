using System.Collections.Generic;
using _Project.Scripts.Patterns;
using Unity.Services.Analytics;
using UnityEngine;

namespace _Project.Scripts.Analytics
{
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        [Header("Globals:")]
        [SerializeField] private bool statisticSenderEnabled = true;
        [SerializeField] private bool instantSendInfo;
        [Header("Analytics Enabled:")]
        [SerializeField] private bool heroDead;
        [SerializeField] private bool weaponPickup;
        [SerializeField] private bool levelCompleted;
        [SerializeField] private bool perkBought;
        [SerializeField] private bool weaponUpgradeBought;

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

        public bool StatisticSenderEnabled
        {
            get => statisticSenderEnabled;
            set => statisticSenderEnabled = value;
        }

        private void Start()
        {
            AnalyticsService.Instance.StartDataCollection();
        }

        public void SendCustomData(string eventName, Dictionary<string, object> parameters, bool forceSendInfo = false)
        {
            if (!StatisticSenderEnabled)
            {
                return;
            }
            
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
        }
    }
}
