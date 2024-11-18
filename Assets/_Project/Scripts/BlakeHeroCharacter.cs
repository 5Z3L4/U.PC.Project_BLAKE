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
            this.TrySendAnalytics(killer);
        
            explosionParticleInstantiated = Instantiate(explosionParticle, transform.position, quaternion.identity);
            gameObject.SetActive(false);
            Invoke("Respawn", 2f);

            base.Die(killer);
        }
    }
}
