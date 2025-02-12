using _Project.Scripts.Interfaces;
using UnityEngine;

namespace _Project.Scripts.Weapons.Upgrades.Bullet
{
    [RequireComponent(typeof(Rigidbody))]
    public class BasicBullet : MonoBehaviour, IBullet
    {
        [SerializeField]
        private float bulletSpeed;

        [SerializeField, Tooltip("How many enemies bullet should penetrate 0 = destroy at first hit")]
        private int penetrateAmount;
        
        [SerializeField]
        private BulletExplosion explosionPrefab;

        private GameObject instigator;
        private BulletType bulletType;

        protected Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            UpdatePosition();
        }

        protected virtual void UpdatePosition()
        {
            rb.velocity = transform.forward * bulletSpeed;
        }

        /// <summary>
        /// Use it on instantiate to declare base stats which are weapon related
        /// </summary>
        /// <param name="xSpread">Spread range (it declares range of (-xSpread, xSpread))</param>
        public void SetupBullet(float xSpread, GameObject instigator, float range, BulletType bulletType)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + xSpread, 0);
            
            this.instigator = instigator;
            this.bulletType = bulletType;

            if (range > 0)
            {
                var destroyTime = range / bulletSpeed;
                Destroy(gameObject, destroyTime);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject == instigator)
            {
                return;
            }
            
            var damageable = collision.gameObject.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                Destroy(gameObject);
                return;
            }

            damageable.TryTakeDamage(instigator, 1);
            if (collision.gameObject.CompareTag("Shield"))
            {
                Destroy(gameObject);
                return;
            }
            
            if (bulletType == BulletType.Explosive)
            {
                Destroy(gameObject);
            }

            if (penetrateAmount > 0)
            {
                penetrateAmount--;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void OnDestroy()
        {
            switch (bulletType)
            {
                case BulletType.Undefined:
                    Debug.LogError($"BulletType in {name} is Undefined!");
                    break;
                case BulletType.Basic:
                    break;
                case BulletType.Explosive:
                    var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                    explosion.Explode(instigator);
                    break;
            }
        }
    }
}
