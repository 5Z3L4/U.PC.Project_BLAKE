using _Project.Scripts.Analytics;
using _Project.Scripts.Weapons.Definition;
using QuickOutline.Scripts;
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

        [SerializeField] 
        private bool addAmmoWhenWalkedOn = true;

        [HideInInspector]
        public WeaponInstanceInfo WeaponInstanceInfo;

        private GameObject weaponGFX;

        private Color _weaponPickupColorOnGround = Color.red;
        private Outline.Mode _weaponPickupModeOnGround = Outline.Mode.OutlineAll;

        private void Start()
        {
            weaponGFX = Instantiate(WeaponDefinition.WeaponGFX, pickupGameObject.transform.position, pickupGameObject.transform.rotation, pickupGameObject.transform);
            SetOutlineVisibilityOnGround();

            foreach (var customCollider in GetComponentsInChildren<WeaponPickupCustomCollider>())
            {
                customCollider.OnObjectTriggerEnter += TryAddAmmoIfWalkedOn;
            }
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
        
            this.TrySendAnalytics(WeaponDefinition);

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
            
            SetOutlineVisibilityOnGround();
        }
        
        private void SetOutlineVisibilityOnGround()
        {
            var outline = weaponGFX.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                outline.OutlineColor = _weaponPickupColorOnGround;
                outline.OutlineMode = _weaponPickupModeOnGround;
            }
        }

        private void TryAddAmmoIfWalkedOn(Collider other)
        {
            if (!addAmmoWhenWalkedOn)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }
            
            var weaponsManager = other.GetComponent<WeaponsManager>();
            if(weaponsManager == null)
            {
                Debug.LogError("WeaponsManager is not valid");
                return;
            }

            for (var i = weaponsManager.Weapons.Count - 1; i >= 0; i--)
            {
                var weapon = weaponsManager.Weapons[i];
                if (weapon == null)
                {
                    continue;
                }
                
                if (weapon.WeaponDefinition != WeaponDefinition)
                {
                    continue;
                }

                if (weapon is not RangedWeapon rangedWeapon)
                {
                    continue;
                }

                if (rangedWeapon.HasFullMagazine)
                {
                    continue;
                }

                if (WeaponInstanceInfo is not RangedWeaponInstanceInfo rangedWeaponInstanceInfo)
                {
                    rangedWeaponInstanceInfo = rangedWeapon.GenerateWeaponInstanceInfo(true) as RangedWeaponInstanceInfo;
                }

                rangedWeaponInstanceInfo.bulletsLeft += rangedWeapon.BulletsLeft;
                rangedWeapon.LoadWeaponInstanceInfo(rangedWeaponInstanceInfo);
                Destroy(gameObject);
            }
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