using _Project.Scripts.Analytics;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Player.GFX;
using _Project.Scripts.PointsSystem;
using _Project.Scripts.SceneHandlers;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.Scripts
{
    public class BlakeHeroCharacter : BlakeCharacter
    {
        [SerializeField] private WhiteAlphaColorBlender whiteAlphaColorBlender;
        
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
            OnDamageTaken += LightUpPlayer;
            SceneHandler.Instance.OnMainMenuLoad += DestroyOnMainMenuLoad;
        }

        private void OnDestroy()
        {
            if (SceneHandler.Instance != null)
            {
                SceneHandler.Instance.OnMainMenuLoad -= DestroyOnMainMenuLoad;
            }
            
            ReferenceManager.BlakeHeroCharacter = null;
        }

        private void LightUpPlayer(GameObject _)
        {
            whiteAlphaColorBlender.LightUpPlayer(timeBetweenDamages);
        }

        private void DestroyOnMainMenuLoad()
        {
            Destroy(gameObject);
        }

        public override void Die(GameObject killer)
        {
            this.TrySendAnalytics(killer);
            string killerName = "";
            if(killer.TryGetComponent(out EnemyPointsData enemy))
            {
                killerName = Regex.Replace(enemy.EnemyTypeEnum.ToString(), "(?<!^)([A-Z])", " $1");
            }
            FindAnyObjectByType<SummaryScreen>(FindObjectsInactive.Include).SetKiller(killerName);
            explosionParticleInstantiated = Instantiate(explosionParticle, transform.position, quaternion.identity);
            gameObject.SetActive(false);
            Invoke("Respawn", 2f);

            base.Die(killer);
        }
    }
}
