using UnityEngine;

namespace _Project.Scripts.Weapons
{
    public class WeaponLaser : MonoBehaviour
    {
        [SerializeField]
        private Transform attackPointTransform;
        [SerializeField]
        private LayerMask mask;

        private RangedWeapon rangedWeapon;
        private LineRenderer lineRenderer;
        private float laserRange;
        private void Awake()
        {
            rangedWeapon = GetComponentInParent<RangedWeapon>();
            lineRenderer = GetComponent<LineRenderer>();
            laserRange = rangedWeapon.Range - 2f;
        }

        private void Start()
        {
            var laserLength = new Vector3(0f, 0f, laserRange);

            transform.position = attackPointTransform.position;
            lineRenderer.SetPosition(1, laserLength);
        }

        private void Update()
        {
            if(Physics.Raycast(new Ray(transform.TransformPoint(lineRenderer.GetPosition(0)), transform.TransformDirection(lineRenderer.GetPosition(1)-lineRenderer.GetPosition(0))), out RaycastHit hit, rangedWeapon.Range-2f, mask))
            {
                lineRenderer.SetPosition(1, new Vector3(0, 0, hit.distance));
            } else
            {
                lineRenderer.SetPosition(1, new Vector3(0,0, laserRange));
            }
        }
    }
}
