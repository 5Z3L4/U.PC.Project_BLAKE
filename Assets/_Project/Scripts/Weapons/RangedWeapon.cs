using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Weapons.Definition;
using _Project.Scripts.Weapons.Statistics;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Weapons
{
    [RequireComponent(typeof(AudioSource))]
    public class RangedWeapon : Weapon
    {
        [SerializeField]
        private Transform bulletsSpawnPoint;
        [SerializeField]
        public ParticleSystem muzzleFlashEffect;
        [SerializeField]
        public ParticleSystem shootEffect;
        [SerializeField]
        private bool infinityAmmo = false;

        private RangedWeaponDefinition rangedWeaponDefinition;
        private RangedWeaponStatistics baseWeaponStats;
        private RangedWeaponStatistics weaponUpgrades;
        private RangedWeaponStatistics currentWeaponStats;
        
        private float lastFireTime;
        private float negativeSpreadThreshold;
        private float positiveSpreadThreshold;
        private float currentSpread;
        private float masterShootDelayTime => currentWeaponStats.WaitingTimeForNextShoot + shootDelayTime;

        //Enemy only
        private float effectDuration = 0f;
        private float shootDelayTime = 0f;
        private bool isTryingToShoot = false;

        public ThrowableWeapon throwableWeapon;
        
        public float Range => currentWeaponStats.Range;
        public int BulletsLeft { get; set; }
        public RangedWeaponStatistics CurrentRangedWeaponStatistics => currentWeaponStats;
        public bool HasFullMagazine => BulletsLeft >= currentWeaponStats.MagazineSize;


        protected override void Awake()
        {
            base.Awake();

            SetupWeaponDefinition();
        }

        private void OnEnable()
        {
            TryStopEnemyMuzzleFlashVFX();
        }

        public override void PrimaryAttack()
        {
            CastPrimaryAttack().Forget();
        }

        private async UniTaskVoid CastPrimaryAttack()
        {
            isTryingToShoot = true;
            
            if (weaponOwnerIsEnemy)
            {
                CastEnemyWeaponVFX();
                await UniTask.Delay(TimeSpan.FromSeconds(shootDelayTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            if (audioSource != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(audioSource.clip);
            }

            Shot();

            lastFireTime = Time.time;

            weaponsManager?.BroadcastOnPrimaryAttack();
            isTryingToShoot = false;
        }

        private void CastEnemyWeaponVFX()
        {
            muzzleFlashEffect.Play();
            DOVirtual.DelayedCall(effectDuration, TryStopEnemyMuzzleFlashVFX);
        }

        private void TryStopEnemyMuzzleFlashVFX()
        {
            muzzleFlashEffect.Clear();
            muzzleFlashEffect.Stop();
        }

        private void CastWeaponShootVFX()
        {
            shootEffect.Play();
        }

        public override bool CanPrimaryAttack()
        {
            if (Time.time - lastFireTime < masterShootDelayTime) return false;
            if (isTryingToShoot) return false;
            if(BulletsLeft <= 1 && !weaponsManager.throwOnNoAmmo)
            {
                StartCoroutine(UnequipSelf());
                return BulletsLeft == 1;
            }
            

            return true;
        }

        private IEnumerator UnequipSelf(bool throwingWeapon = false)
        {
            yield return new WaitForEndOfFrame();
            if (audioSource != null && audioSource.isPlaying && !throwingWeapon)
            {
                var detachedAudio = new GameObject("DetachedAudioSource");
                var detachedSource = detachedAudio.AddComponent<AudioSource>();
                detachedSource.clip = audioSource.clip;
                detachedSource.volume = audioSource.volume;
                detachedSource.pitch = audioSource.pitch;
                detachedSource.spatialBlend = audioSource.spatialBlend;
                detachedSource.minDistance = audioSource.minDistance;
                detachedSource.maxDistance = audioSource.maxDistance;
                detachedSource.transform.position = audioSource.transform.position;
                detachedSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;

                detachedSource.Play();
                Destroy(detachedAudio, detachedSource.clip.length);
            }
            if (Owner.TryGetComponent(out WeaponsManager weaponsManager))
            {
                weaponsManager.Unequip(weaponsManager.ActiveWeaponIndex);
                weaponsManager.SetActiveIndex(weaponsManager.ActiveWeaponIndex - 1);
            }
        }

        private bool Shot()
        {
            if (BulletsLeft == 0)
            {
                return TryThrowWeapon();
            }

            var list = GetCalculatedProjectilesAngles();
            for (var index = 0; index < list.Count; index++)
            {
                var bulletSpreadValue = list[index];

                if (currentWeaponStats.SpreadType == SpreadType.StaticSeries)
                {
                    DOVirtual.DelayedCall(index / 10f, () => CreateBullet(bulletSpreadValue));
                }
                else
                {
                    CreateBullet(bulletSpreadValue);
                }
            }

            if (!weaponOwnerIsEnemy)
            {
                SpreadWeaponSound();
            }

            if (!infinityAmmo) BulletsLeft--;

            return true;
        }

        private void SpreadWeaponSound()
        {
            var collidersFoundNumber = Physics.OverlapSphere(Owner.transform.position,
                currentWeaponStats.LoudnessRange, currentWeaponStats.EnemyLayerMask);
            foreach (var enemy in collidersFoundNumber)
            {
                enemy.GetComponent<AIController>().TryChangeCombatState(CombatState.Chase);
            }
        }

        private bool TryThrowWeapon()
        {
            if(!weaponsManager.throwOnNoAmmo) return false;
            //Spawn and throw weapon
            var spawnedThrowable = Instantiate(throwableWeapon, bulletsSpawnPoint.position, transform.rotation);
            spawnedThrowable.transform.parent = null;
            spawnedThrowable.SetupVFX(rangedWeaponDefinition.WeaponGFX);
            spawnedThrowable.SetupBullet(0, transform.parent.gameObject);
            StartCoroutine(UnequipSelf(true));
            return true;
        }

        private void CreateBullet(float bulletSpreadValue)
        {
            //TODO: Add pooling
            var bulletPrefab = rangedWeaponDefinition.BasicBullet;
            var bullet = Instantiate(bulletPrefab, bulletsSpawnPoint.position, transform.rotation);
            CastWeaponShootVFX();
                
            bullet.SetupBullet(bulletSpreadValue, transform.parent.gameObject, currentWeaponStats.Range, currentWeaponStats.BulletType);
        }

        private List<float> GetCalculatedProjectilesAngles()
        {
            var projectilesAngles = new List<float>();
            
            if (currentWeaponStats.SpreadType == SpreadType.Undefined)
            {
                Debug.LogError($"SpreadType in {rangedWeaponDefinition.WeaponName} is Undefined!");
            }

            else if (currentWeaponStats.SpreadType == SpreadType.NoSpread)
            {
                projectilesAngles.Add(0f);
            }
            
            else if (currentWeaponStats.SpreadType is SpreadType.Static)
            {
                var projectileAngle = Random.Range(negativeSpreadThreshold, positiveSpreadThreshold);
                projectilesAngles.Add(projectileAngle);
            }
            
            else if (currentWeaponStats.SpreadType is SpreadType.StaticSeries)
            {
                for (var i = 0; i < currentWeaponStats.ProjectilesPerShot; i++)
                {
                    var projectileAngle = Random.Range(negativeSpreadThreshold, positiveSpreadThreshold);
                    projectilesAngles.Add(projectileAngle);
                }
            }

            else if (currentWeaponStats.SpreadType == SpreadType.StaticMultiShot)
            {
                var anglePerProjectile = currentWeaponStats.Spread / (currentWeaponStats.ProjectilesPerShot - 1);

                for (var i = 0; i < currentWeaponStats.ProjectilesPerShot; i++)
                {
                    var projectileAngle = negativeSpreadThreshold + anglePerProjectile * i;
                    projectilesAngles.Add(projectileAngle);
                }
            }

            else if (currentWeaponStats.SpreadType == SpreadType.NoSpreadThenStatic)
            {
                if (Time.time - lastFireTime < currentWeaponStats.SpreadResetThreshold)
                { 
                    var projectileAngle = Random.Range(negativeSpreadThreshold, positiveSpreadThreshold);
                    projectilesAngles.Add(projectileAngle);
                }
                else
                {
                    projectilesAngles.Add(0f);
                }
            }

            else if (currentWeaponStats.SpreadType == SpreadType.GraduallyIncrease)
            {
                if (Time.time - lastFireTime > masterShootDelayTime + currentWeaponStats.SpreadResetThreshold)
                {
                    ResetSpread();
                }
                
                var projectileAngle = Random.Range(negativeSpreadThreshold, positiveSpreadThreshold);
                projectilesAngles.Add(projectileAngle);
                
                if (currentSpread < currentWeaponStats.SpreadThreshold)
                {
                    currentSpread += currentWeaponStats.SpreadStep;
                    UpdateSpreadThresholds();
                }
            }

            return projectilesAngles;
        }

        public override WeaponInstanceInfo GenerateWeaponInstanceInfo(bool randomize)
        {
            if(randomize)
            {
                return new RangedWeaponInstanceInfo
                {
                    bulletsLeft = Random.Range(currentWeaponStats.MagazineSize / 2, currentWeaponStats.MagazineSize + 1)
                };
            }

            return new RangedWeaponInstanceInfo
            {
                bulletsLeft = BulletsLeft
            };
        }

        private void SetupWeaponDefinition()
        {
            if (WeaponDefinition is not RangedWeaponDefinition definition)
            {
                Debug.LogError("Wrong WeaponDefinition is attached to the weapon!");
                return;
            }
            
            rangedWeaponDefinition = definition;

            baseWeaponStats = rangedWeaponDefinition.GetWeaponStatistics();

            if (weaponOwnerIsEnemy)
            {
                effectDuration = rangedWeaponDefinition.EffectDuration; 
                shootDelayTime = rangedWeaponDefinition.ShootDelayTime;
            }

            BulletsLeft = baseWeaponStats.MagazineSize;
            RestoreRangedWeaponStatistics();
        }

        public override void CalculateWeaponStatsWithUpgrades(WeaponDefinition weaponDefinition, IWeaponStatistics weaponStatistics)
        {
            if (rangedWeaponDefinition is not null)
            {
                if(weaponDefinition != rangedWeaponDefinition)
                {
                    return;
                }
            }
            else if (weaponDefinition != (RangedWeaponDefinition)WeaponDefinition)
            {
                return;
            }

            var statistics = (RangedWeaponStatistics)weaponStatistics;
            weaponUpgrades = statistics;

            RestoreRangedWeaponStatistics();
        }

        public void ApplyRangedWeaponStatistics(RangedWeaponStatistics rangedWeaponStatistics)
        {
            currentWeaponStats = rangedWeaponStatistics;
            ResetSpread();
        }

        public void RestoreRangedWeaponStatistics()
        {
            currentWeaponStats = baseWeaponStats + weaponUpgrades;
            ResetSpread();
        }

        private void ResetSpread()
        {
            currentSpread = currentWeaponStats.Spread;
            UpdateSpreadThresholds();
        }

        private void UpdateSpreadThresholds()
        {
            negativeSpreadThreshold = -(currentSpread / 2);
            positiveSpreadThreshold = currentSpread / 2;
        }

        public override void LoadWeaponInstanceInfo(WeaponInstanceInfo weaponInstanceInfo)
        {
            if (weaponInstanceInfo is RangedWeaponInstanceInfo rangedWeaponInstanceInfo)
            {
                BulletsLeft = Mathf.Min(rangedWeaponInstanceInfo.bulletsLeft, currentWeaponStats.MagazineSize);
            }
            
            base.LoadWeaponInstanceInfo(weaponInstanceInfo);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !drawDebugGizmos)
            {
                return;
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Owner.transform.position, currentWeaponStats.LoudnessRange);
        }
        
        public void SetInfiniteAmmo(bool setInfinite)
        {
            infinityAmmo = setInfinite;
        }
#endif
    
    }
}
