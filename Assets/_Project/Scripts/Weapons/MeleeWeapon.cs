using System;
using _Project.Scripts.Interfaces;
using _Project.Scripts.Weapons.Definition;
using _Project.Scripts.Weapons.Statistics;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

namespace _Project.Scripts.Weapons
{
    public class MeleeWeapon : Weapon
    {
        [SerializeField]
        public ParticleSystem weaponFlashEffect;
        
        private PlayableDirector playableDirector;
        private Transform characterTransform;

        private MeleeWeaponDefinition meleeWeaponDefinition;
        private MeleeWeaponStatistics baseWeaponStats;
        private MeleeWeaponStatistics weaponUpgrades;
        private MeleeWeaponStatistics currentWeaponStats;
        
        private Collider[] raycastCollidersFound;
        private int maxSpreadRangePerSide;
        private float lastAttackTime;
        
        private float masterShootDelayTime => currentWeaponStats.AttackDelayTime + shootDelayTime;
        
        //Enemy only
        private float effectDuration = 0f;
        private float shootDelayTime = 0f;
        private bool isTryingToAttack = false;

        private void OnDisable()
        {
            transform.localRotation = quaternion.identity;
            playableDirector.Stop();
        }

        protected override void Awake()
        {
            base.Awake();
            SetupWeaponDefinition();

            playableDirector = GetComponent<PlayableDirector>();
            characterTransform = transform.GetComponentInParent<BlakeCharacter>().transform;
        }

        public override bool CanPrimaryAttack()
        {
            if (Time.time - lastAttackTime < masterShootDelayTime) return false;
            if (isTryingToAttack) return false;
            
            return true;
        }

        public override void PrimaryAttack()
        {
            CastPrimaryAttack().Forget();
        }

        private async UniTaskVoid CastPrimaryAttack()
        {
            isTryingToAttack = true;
            
            if (weaponOwnerIsEnemy)
            {
                CastEnemyWeaponVFX();
                await UniTask.Delay(TimeSpan.FromSeconds(shootDelayTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            
            playableDirector.Stop();
            playableDirector.Play();
            audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            audioSource.Play();
            
            MakeRaycast();

            lastAttackTime = Time.time;
            isTryingToAttack = false;
        }
        
        private void CastEnemyWeaponVFX()
        {
            weaponFlashEffect.Play();
            DOVirtual.DelayedCall(effectDuration, TryStopEnemyMuzzleFlashVFX);
        }

        private void TryStopEnemyMuzzleFlashVFX()
        {
            weaponFlashEffect.Clear();
            weaponFlashEffect.Stop();
        }

        private void MakeRaycast()
        {
            var characterForwardDir = characterTransform.forward;
            characterForwardDir.y = 0;

            var collidersFoundNumber = Physics.OverlapSphereNonAlloc(characterTransform.position,
                currentWeaponStats.SphereCastRadius, raycastCollidersFound, currentWeaponStats.EnemyLayerMask);
            for (var i = 0; i < collidersFoundNumber; i++)
            {
                var colliderFound = raycastCollidersFound[i];
                if (colliderFound is null)
                {
                    break;
                }
                
                var targetDir = colliderFound.transform.position - characterTransform.position;
                targetDir.y = 0;
                    
                var angle = Vector3.Angle(characterForwardDir, targetDir);

                if (angle > maxSpreadRangePerSide)
                {
                    continue;
                }
                
                var damageable = colliderFound.transform.GetComponentInParent<IDamageable>();
                damageable?.TryTakeDamage(transform.parent.gameObject, 1);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !drawDebugGizmos)
            {
                return;
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(characterTransform.position, currentWeaponStats.SphereCastRadius);
        }
#endif
        
        private void SetupWeaponDefinition()
        {
            if (WeaponDefinition is not MeleeWeaponDefinition definition)
            {
                Debug.LogError("Wrong WeaponDefinition is attached to the weapon!");
                return;
            }
            
            meleeWeaponDefinition = definition;

            baseWeaponStats = meleeWeaponDefinition.GetWeaponStatistics();
            
            if (weaponOwnerIsEnemy)
            { 
                effectDuration = meleeWeaponDefinition.EffectDuration; 
                shootDelayTime = meleeWeaponDefinition.AttackDelayTime;
            }
            
            raycastCollidersFound = new Collider[meleeWeaponDefinition.MaxNumberOfEnemies];
            RestoreMeleeWeaponStatistics();
        }
        
        public override void CalculateWeaponStatsWithUpgrades(WeaponDefinition weaponDefinition, IWeaponStatistics weaponStatistics)
        {
            if (meleeWeaponDefinition is not null)
            {
                if(weaponDefinition != meleeWeaponDefinition)
                {
                    return;
                }
            }
            else if (weaponDefinition != (RangedWeaponDefinition)WeaponDefinition)
            {
                return;
            }

            var statistics = (MeleeWeaponStatistics)weaponStatistics;
            weaponUpgrades = statistics;

            RestoreMeleeWeaponStatistics();
        }

        public void ApplyMeleeWeaponStatistics(MeleeWeaponStatistics meleeWeaponStatistics)
        {
            currentWeaponStats = meleeWeaponStatistics;

            ResetSpread();
        }

        public void RestoreMeleeWeaponStatistics()
        {
            currentWeaponStats = baseWeaponStats + weaponUpgrades;
            
            ResetSpread();
        }
        
        private void ResetSpread()
        {
            maxSpreadRangePerSide = baseWeaponStats.MaxSpreadRange / 2;
        }

        public override void LoadWeaponInstanceInfo(WeaponInstanceInfo weaponInstanceInfo)
        {
        
        }
    }
}
