using _Project.Scripts.Weapons.Definition;
using _Project.Scripts.Weapons.Statistics;
using _Project.Scripts.Weapons.Upgrades;
using UnityEngine;

namespace _Project.Scripts.Weapons
{
    public abstract class Weapon : MonoBehaviour
    {
        [SerializeField] protected bool drawDebugGizmos;
        
        [SerializeField] public WeaponDefinition WeaponDefinition;

        //BlakeCharacter
        private BlakeCharacter owner;
        public BlakeCharacter Owner
        {
            get => owner;
            set
            {
                if (owner == value) return;
                owner = value;
                OnOwnerChanged();
            }
        }

        protected AudioSource audioSource;
        protected WeaponsManager weaponsManager;
        protected bool weaponOwnerIsEnemy;

        public WeaponsManager WeaponsManager => weaponsManager;

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public abstract void PrimaryAttack();
        
        public abstract void CalculateWeaponStatsWithUpgrades(WeaponDefinition weaponDefinition, IWeaponStatistics weaponStatistics);

        protected virtual void OnOwnerChanged()
        {
            if (Owner == null) { return; }

            weaponsManager = owner.GetComponent<WeaponsManager>();
            weaponOwnerIsEnemy = owner is EnemyCharacter;

            if (!weaponOwnerIsEnemy)
            {
                owner.GetComponent<PlayerWeaponUpgradeManager>().OnWeaponUpgradeChanged +=
                    CalculateWeaponStatsWithUpgrades;
            }
        }

        public virtual bool CanPrimaryAttack() => true;

        public virtual bool CanAttack()
        {
            var viewportPosition = Camera.main.WorldToViewportPoint(owner.transform.position);
            return (!(viewportPosition.x <= 0) && !(viewportPosition.x >= 1) && 
                    !(viewportPosition.y <= 0) && !(viewportPosition.y >= 1));
        }

        public virtual WeaponInstanceInfo GenerateWeaponInstanceInfo(bool randomize = false) => new WeaponInstanceInfo();

        public virtual void LoadWeaponInstanceInfo(WeaponInstanceInfo weaponInstanceInfo)
        {
            weaponsManager.OnWeaponUpdate();
        }
    }
}