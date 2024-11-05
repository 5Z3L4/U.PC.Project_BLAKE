using _Project.Scripts.Interfaces;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.EnemyBoss.Shield
{
    public class ShieldController : MonoBehaviour, IDamageable
    {
        [SerializeField] private ShieldManager shieldManager;
        [SerializeField] private GameObject shieldVisual;
        
        private Tween _tween;
        
        public bool TryTakeDamage(GameObject instigator, int damage)
        {
            HideCurrentShieldForTime();
            return true;
        }

        public bool CanTakeDamage(GameObject instigator)
        {
            return shieldManager.BlakeCharacter != instigator.GetComponent<BlakeCharacter>();
        }

        private void HideCurrentShieldForTime()
        {
            if (_tween == null)
            {
                shieldVisual.SetActive(false);
                _tween = DOVirtual.DelayedCall(shieldManager.ShieldRespawnTime, Reset, false);
            }
        }

        public void Reset()
        {
            _tween.Kill();
            _tween = null;
            shieldVisual.SetActive(true);
        }
    }
}
