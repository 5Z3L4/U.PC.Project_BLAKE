using System;
using Cysharp.Threading.Tasks;
using GameFramework.Abilities;

namespace _Project.Scripts.Abilities
{
    public class BasicAttackHold : WeaponAbility
    {
        public override bool CanActivateAbility()
        {
            return base.CanActivateAbility();
        }
        
        protected override void AbilitySkill()
        {
            HoldShoot().Forget();
        }

        private async UniTaskVoid HoldShoot()
        {
            while (IsInputPressed)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
                if (CanActivateAbility() && IsWeaponActive())
                {
                    weaponSource.PrimaryAttack();
                }
            }
        }
        
        private bool IsWeaponActive()
        {
            return weaponSource.WeaponsManager.CurrentActiveWeapon == weaponSource;
        }
    }
}
