using _Project.Scripts.GlobalHandlers;
using UnityEngine;

namespace _Project.Scripts.Weapons.Upgrades.Bullet
{
    public class BossHomingBullet : BasicBullet
    {
        [SerializeField] private float rotationSpeed;
        
        protected override void UpdatePosition()
        {
            base.UpdatePosition();

            var target = ReferenceManager.BlakeHeroCharacter.transform;
            var direction = (target.position - transform.position).normalized;
            var targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
