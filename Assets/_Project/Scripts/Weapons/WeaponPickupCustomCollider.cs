using System;
using UnityEngine;

namespace _Project.Scripts.Weapons
{
    public class WeaponPickupCustomCollider : MonoBehaviour
    {
        public event Action<Collider> OnObjectTriggerEnter;
        
        private void OnTriggerEnter(Collider other)
        {
            OnObjectTriggerEnter?.Invoke(other);
        }
    }
}
