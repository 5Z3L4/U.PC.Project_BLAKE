using _Project.Scripts.EnemyBoss.Shield;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Weapons;
using MBT;
using UnityEngine;

namespace _Project.Scripts.EnemyAI.MBT.Services
{
    [AddComponentMenu("")]
    [MBTNode("Services/ShootBoss")]
    public class BTS_Shoot_Boss : BTS_Shoot
    {
        public BossCharacter BossCharacter;

        private BossAiController BossAiController
        {
            get
            {
                if (AIController is BossAiController bossAiController)
                {
                    return bossAiController;
                }
                else
                {
                    Debug.LogError($"AiController is not BossAiController, enemy won't shoot!", AIController.gameObject);
                    return null;
                }
            }
        }

        public override void Task()
        {
            if(BossAiController == null) return;
            
            var specialWeapon = BossAiController.SpecialWeapon;
            if (SpecialWeaponConditionsMet() && CheckIfCanAttack(specialWeapon))
            {
                specialWeapon.PrimaryAttack();
                return;
            }
            
            var weapon = BossAiController.Weapon;
            if (CheckIfCanAttack(weapon))
            {
                weapon.PrimaryAttack();
            }
        }

        private bool CheckIfCanAttack(Weapon weapon)
        {
            if (weapon == null) return false;
            if (!HasLineOfSightReference.Value) return false;
            if (!weapon.CanAttack()) return false;
            if (!weapon.CanPrimaryAttack()) return false;
            
            return true;
        }

        private bool SpecialWeaponConditionsMet()
        {
            var halfHealth = (float)BossCharacter.DefaultHealth / 2;
            var blakePos = ReferenceManager.BlakeHeroCharacter.transform.position;
            var distance = Vector3.Distance(BossAiController.transform.position, blakePos);
            
            if (BossCharacter.Health >= halfHealth) return false;
            if (distance > BossAiController.MaxDistanceToShootSpecialWeapon) return false;
            if (BossAiController.IsSpecialWeaponOnCooldown) return false;
            if (Random.value > BossAiController.SpecialWeaponShootChance) return false;
            
            return true;
        }
    }
}