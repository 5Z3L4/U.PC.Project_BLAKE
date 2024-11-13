using System.Collections.Generic;
using _Project.Scripts.Analytics;
using _Project.Scripts.Weapons.Definition;
using UnityEngine;

namespace _Project.Scripts.Weapons
{
    public class WeaponPickup : Interactable
    {
        public WeaponDefinition WeaponDefinition;

        [SerializeField] 
        private GameObject pickupGameObject;

        [SerializeField] 
        private float rotateForce = 12f;

        [HideInInspector]
        public WeaponInstanceInfo WeaponInstanceInfo;

        private GameObject weaponGFX;

        private void Start()
        {
            weaponGFX = Instantiate(WeaponDefinition.WeaponGFX, pickupGameObject.transform.position, pickupGameObject.transform.rotation, pickupGameObject.transform);
        }

        private void Update()
        {
            pickupGameObject.transform.Rotate(Vector3.up * (Time.deltaTime * rotateForce));
        }

        public override void Interact(GameObject interacter)
        {
            WeaponsManager weaponsManager = interacter.GetComponent<WeaponsManager>();
            if(weaponsManager == null)
            {
                Debug.LogWarning("WeaponsManager is not valid");
                return;
            }
            int index = 0;
            index = weaponsManager.ActiveWeaponIndex == 0 ? 1 : weaponsManager.ActiveWeaponIndex;
            for (int i = 0; i < weaponsManager.Weapons.Count; i++)
            {
                if (weaponsManager.Weapons[i] == null)
                {
                    index = i;
                    break;
                }
            }

            WeaponDefinition weaponDefinition = null;
            if(weaponsManager.Weapons[index] != null)
            {
                weaponDefinition = weaponsManager.Weapons[index].WeaponDefinition;
            }

            WeaponInstanceInfo weaponInstanceInfoToSave = null;
            Weapon oldWeapon = weaponsManager.Weapons[index];
            if (oldWeapon != null)
            {
                weaponInstanceInfoToSave = oldWeapon.GenerateWeaponInstanceInfo();
            }

            weaponsManager.Equip(WeaponDefinition, index);
            if(weaponsManager.ActiveWeaponIndex == 0) weaponsManager.SetActiveIndex(index);
        
            TrySendAnalytics();

            if (WeaponInstanceInfo != null)
            {
                weaponsManager.Weapons[index].LoadWeaponInstanceInfo(WeaponInstanceInfo);
            }

            WeaponInstanceInfo = weaponInstanceInfoToSave;
            weaponsManager.OnPlayerPickupWeapon();

            if (weaponDefinition != null)
            {
                WeaponDefinition = weaponDefinition;
                ChangeVisuals(weaponDefinition);
                return;
            }

            PlayerInteractables playerInteractables = interacter.GetComponent<PlayerInteractables>();
            if (playerInteractables != null)
            {
                playerInteractables.RemoveInteractable(this);
            }
            Destroy(gameObject);
        }

        private void ChangeVisuals(WeaponDefinition newWeapon)
        {
            if (weaponGFX != null)
            {
                Destroy(weaponGFX);
            }

            weaponGFX = Instantiate(newWeapon.WeaponGFX, pickupGameObject.transform);
            weaponGFX.transform.localPosition = newWeapon.PickupLocationOffset;
            weaponGFX.transform.localRotation = newWeapon.PickupRotation;
        }
    
        private void TrySendAnalytics()
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            var parameters = new Dictionary<string, object>()
            {
                { AnalyticsParameterNames.ITEM_NAME, WeaponDefinition.WeaponName }
            };

            AnalyticsManager.Instance.SendCustomData(AnalyticsEventNames.HERO_DEAD, parameters);
#endif
        }
    }

    public class WeaponInstanceInfo
    {

    }

    public class RangedWeaponInstanceInfo : WeaponInstanceInfo
    {
        public int bulletsLeft;
    }
}