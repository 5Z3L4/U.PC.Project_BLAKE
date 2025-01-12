using _Project.Scripts.Interfaces;
using UnityEngine;

namespace _Project.Scripts.Weapons.Upgrades.Bullet
{
    public class BulletExplosion : MonoBehaviour
    {
        [SerializeField]
        private float explosionRadius;

        [SerializeField] 
        private ParticleSystem particles;

        public void Explode(GameObject instigator)
        {
            Destroy(gameObject, particles.main.duration);
            particles.Play();

            var bulletPos = transform.position;
            var colliders = Physics.OverlapSphere(bulletPos, explosionRadius);
            foreach (var coll in colliders)
            {
                var isDamageable = coll.transform.TryGetComponent<IDamageable>(out var damageable);
                if (!isDamageable)
                {
                    continue;
                }

                var enemyPos = coll.transform.position;
                var rayTarget = new Vector3(enemyPos.x, bulletPos.y, enemyPos.z);
                Physics.Linecast(bulletPos, rayTarget, out var hit);
                if (hit.transform != coll.transform)
                {
                    continue;
                }
                
                damageable?.TryTakeDamage(instigator, 1);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
