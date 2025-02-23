using System;
using System.Collections.Generic;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Weapons.Definition;
using GameFramework.Abilities;
using UnityEngine;
using System.Linq;

namespace _Project.Scripts.Weapons
{
    public class WeaponsManager : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> attachSockets = new List<GameObject>();

        public event Action<Weapon> OnWeaponUpdateEvent;
        public event Action<Weapon> OnWeaponChangedEvent;
        public event Action<Weapon> OnPlayerPickupWeaponEvent;
        public event Action<Weapon> OnPrimaryAttack;

        private int activeWeaponIndex = 0;
        public int ActiveWeaponIndex { get => activeWeaponIndex; }
        public Weapon CurrentActiveWeapon => Weapons[activeWeaponIndex];

        private List<Weapon> weapons = new List<Weapon>() { null, null };
        public List<Weapon> Weapons { get => weapons; }

        public WeaponDefinition defaultWeapon;
        private AbilityManager abilityManager;

        public bool throwOnNoAmmo = true;
        private bool haveThirdWeaponSlot = false;

        public Dictionary<string, int> weaponKills = new Dictionary<string, int>();

        private void Awake()
        {
            if (defaultWeapon == null)
            {
                Debug.LogWarning("Default equipment item is not valid");
                return;
            }

            abilityManager = GetComponent<AbilityManager>();

            Equip(defaultWeapon, 0);

            if (ReferenceManager.PlayerInputController != null)
            {
                ReferenceManager.PlayerInputController.changeWeaponEvent += SetActiveIndex;
            }
        }

        public void AddKillToWeapon(WeaponDefinition weapon)
        {
            if(weaponKills.ContainsKey(weapon.WeaponName))
            {
                weaponKills[weapon.WeaponName]++;
            } else
            {
                weaponKills.Add(weapon.WeaponName, 1);
            }
        }

        public string GetBestWeaponName()
        {
            string usedWeapons = "None";
            if (weaponKills.Count > 0)
                usedWeapons = weaponKills.OrderByDescending(pair => pair.Value).First().Key;
            return usedWeapons;
        }

        public void Equip(WeaponDefinition weaponDefinition, int index)
        {
            if (weaponDefinition == null) return;
            if (index < 0 || index >= weapons.Count) return;

            Unequip(index);
            SpawnWeapon(weaponDefinition, index);

            if (index == activeWeaponIndex)
            {
                GiveWeaponAbilities();
                OnWeaponChangedEvent?.Invoke(weapons[activeWeaponIndex]);
            }
        }

        public void Unequip(int index)
        {
            if (index < 0 || index >= weapons.Count) return;

            if (weapons[index] != null)
            {
                if (index == activeWeaponIndex)
                {
                    RemoveWeaponAbilities();
                }

                Destroy(weapons[index].gameObject);
                weapons[index] = null;
            }
        }

        public void SetActiveIndex(int index)
        {
            if (index < 0 || index >= weapons.Count) return;
            if (weapons[index] == null) return;
            if (index == activeWeaponIndex) return;

            if (weapons[activeWeaponIndex] != null)
            {
                weapons[activeWeaponIndex].gameObject.SetActive(false);
                RemoveWeaponAbilities();
            }

            if (weapons[index] != null)
            {
                activeWeaponIndex = index;
                weapons[activeWeaponIndex].gameObject.SetActive(true);
                GiveWeaponAbilities();

                OnWeaponChangedEvent?.Invoke(weapons[activeWeaponIndex]);
            }
        }

        public void BroadcastOnPrimaryAttack()
        {
            OnPrimaryAttack?.Invoke(weapons[activeWeaponIndex]);
        }

        public void OnPlayerPickupWeapon()
        {
            OnPlayerPickupWeaponEvent?.Invoke(weapons[activeWeaponIndex]);
        }

        public void OnWeaponUpdate()
        {
            OnWeaponUpdateEvent?.Invoke(weapons[activeWeaponIndex]);
        }

        public int GetFreeIndex()
        {
            int freeIndex = -1;

            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] == null)
                {
                    freeIndex = i;
                    break;
                }
            }
            return freeIndex;
        }

        private void SpawnWeapon(WeaponDefinition weaponDefinition, int index)
        {
            Transform socketTransform = transform;
            foreach (var socket in attachSockets)
            {
                if (socket.name == weaponDefinition.AttachSocketName)
                {
                    socketTransform = socket.transform;
                    break;
                }
            }

            Vector3 spawnLocation = socketTransform.position + weaponDefinition.LocationOffset;
            Quaternion spawnRotation = weaponDefinition.Rotation;

            var weapon = Instantiate(weaponDefinition.WeaponPrefab, spawnLocation, spawnRotation, transform);
            weapon.transform.localScale = weaponDefinition.Scale;
            weapon.SetActive(index == activeWeaponIndex);

            if (weapon.TryGetComponent(out Weapon weaponComp))
            {
                weaponComp.Owner = gameObject.GetComponent<BlakeCharacter>();
            }

            weapons[index] = weapon.GetComponent<Weapon>();
        }

        private void GiveWeaponAbilities()
        {
            foreach (var ability in weapons[activeWeaponIndex].WeaponDefinition.AbilitiesToGrant)
            {
                abilityManager.GiveAbility(ability, weapons[activeWeaponIndex]);
            }
        }

        private void RemoveWeaponAbilities()
        {
            foreach (var ability in weapons[activeWeaponIndex].WeaponDefinition.AbilitiesToGrant)
            {
                abilityManager.RemoveAbility(ability);
            }
        }

        public void AddThirdWeaponSlot()
        {
            if (!haveThirdWeaponSlot)
            {
                weapons.Add(null);
                haveThirdWeaponSlot = true;
            }
        }

        public void RemoveThirdWeaponSlot()
        {
            if (haveThirdWeaponSlot)
            {
                if (activeWeaponIndex == 2) SetActiveIndex(1);
                Equip(null, 2);
                weapons.RemoveAt(2);
                haveThirdWeaponSlot = false;
            }
        }

    }
}