using _Project.Scripts.Interfaces;
using _Project.Scripts.Weapons.Upgrades.Bullet;
using UnityEngine;

public class ThrowableWeapon : MonoBehaviour, IBullet
{
    private GameObject instigator;
    private GameObject weaponVFX;

    public float bulletSpeed;

    private Rigidbody rb;
    public void SetupBullet(float xSpread, GameObject instigator, float range = 0, BulletType bulletType = BulletType.Basic)
    {
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        this.instigator = instigator;
        Instantiate(weaponVFX, transform.position, transform.rotation, this.transform);
        Destroy(gameObject, 999f);
    }

    public void SetupVFX(GameObject VFX)
    {
        weaponVFX = VFX;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    //TODO: Add magic function handler
    private void Update()
    {
        rb.velocity = transform.forward * bulletSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var damageable = collision.gameObject.GetComponentInParent<IDamageable>();
        if (damageable != null && collision.gameObject != instigator)
        {
            damageable.TryTakeDamage(instigator, 1);
        }
            Destroy(gameObject);
    }
}
