using _Project.Scripts.EnemyBoss.Shield;
using _Project.Scripts.Weapons;
using UnityEngine;

namespace _Project.Scripts.EnemyAI
{
    [RequireComponent(typeof(BossCharacter))]
    public class BossAiController : AIController
    {
        [Space(20)]
        public Weapon SpecialWeapon;
        
        [SerializeField, Range(0f,1f)] private float specialWeaponShootChance;
        [SerializeField] private float specialWeaponCooldown;
        [SerializeField] private int maxDistanceToShootSpecialWeapon;
        
        private float _specialWeaponCooldownTimestamp;

        public float SpecialWeaponShootChance => specialWeaponShootChance;
        public float MaxDistanceToShootSpecialWeapon => maxDistanceToShootSpecialWeapon;
        public bool IsSpecialWeaponOnCooldown
        {
            get
            {
                var isOnCooldown = Time.time <= _specialWeaponCooldownTimestamp;
                if (!isOnCooldown)
                {
                    _specialWeaponCooldownTimestamp = Time.time + specialWeaponCooldown;
                }
            
                return isOnCooldown;
            }
        }
        
        protected override void Awake()
        {
            if (SpecialWeapon != null)
            {
                SpecialWeapon.Owner = gameObject.GetComponent<BlakeCharacter>();
            }
            
            base.Awake();
        }
    }
}
