using System.Collections.Generic;
using _Project.Scripts.Analytics;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.PointsSystem;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.Scripts
{
    public class BlakeHeroCharacter : BlakeCharacter
    {
        private void Awake()
        {
            ReferenceManager.BlakeHeroCharacter = this;

            defaultHealth = ReferenceManager.SceneHandler.isNormalDifficulty ? 3 : 1;
            health = defaultHealth;
            respawnCounter = 0;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            onDeath += EnemyDeathMediator.Instance.PlayerCurrencyController.LosePointsOnDeath;
        }

        private void OnDestroy()
        {
            ReferenceManager.BlakeHeroCharacter = null;
        }

        public override void Die(GameObject killer)
        {
            TrySendAnalytics(killer);
        
            explosionParticleInstantiated = Instantiate(explosionParticle, transform.position, quaternion.identity);
            gameObject.SetActive(false);
            Invoke("Respawn", 2f);

            base.Die(killer);
        }

        private void TrySendAnalytics(GameObject killer)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            if (killer == null)
            {
                return;
            }
        
            var parameters = new Dictionary<string, object>()
            {
                { AnalyticsParameterNames.KILLER, killer.name },
                { AnalyticsParameterNames.ITEM_NAME, killer.GetComponent<AIController>()?.Weapon?.name },
                { AnalyticsParameterNames.PLACEMENT_NAME, ReferenceManager.RoomManager.GetActiveRoom().name }
            };
            
            AnalyticsManager.Instance.SendCustomData(AnalyticsEventNames.HERO_DEAD, parameters);
#endif
        }
    }
}
