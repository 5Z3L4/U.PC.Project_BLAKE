using GameFramework.Abilities;
using UnityEngine;

namespace _Project.Scripts.Weapons.Definition
{
    [CreateAssetMenu(fileName = "Weapon definition", menuName = "Project BLAKE/Weapon")]
    public class WeaponDefinition : ScriptableObject
    {
        public WeaponType WeaponType;
        
        public GameObject WeaponPrefab;
    
        public GameObject WeaponGFX;

        public string WeaponName;
    
        [Range(0f, 1f)]
        public float DropRate = 0.6f;

        public AbilityDefinition[] AbilitiesToGrant;
    
        //[mt] to remove because we would probably have only one attachSocket
        public string AttachSocketName = "GunHandler";

        [Header("On Character position")]
        public Vector3 LocationOffset;
    
        public Quaternion Rotation;

        public Vector3 Scale = Vector3.one;
        
        [Header("Pickup position")]
        public Vector3 PickupLocationOffset = Vector3.zero;

        public Quaternion PickupRotation;
    }
}
