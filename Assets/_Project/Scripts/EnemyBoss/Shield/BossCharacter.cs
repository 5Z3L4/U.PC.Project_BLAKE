using System;
using UnityEngine;

namespace _Project.Scripts.EnemyBoss.Shield
{
    public class BossCharacter : EnemyCharacter
    {
        [SerializeField] private string bossName;

        public string BossName => bossName;
        
        private void Start()
        {
            defaultHealth = Math.Max(Health, defaultHealth);
            Health = Math.Max(Health, defaultHealth);
        }
    }
}
