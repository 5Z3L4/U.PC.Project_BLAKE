using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.EnemyBoss.Shield
{
    public class ShieldManager : MonoBehaviour
    {
        [SerializeField] private BlakeCharacter blakeCharacter;
        [SerializeField] private float shieldRespawnTime;
        [SerializeField] private float shieldsSingleSpinTime;
        [SerializeField] private List<ShieldController> shields;
        
        private Tween _tween;

        public BlakeCharacter BlakeCharacter => blakeCharacter;
        public float ShieldRespawnTime => shieldRespawnTime;
        
        private void OnEnable()
        {
            var singleAngle = 360 / shields.Count;
            for (var i = 0; i < shields.Count; i++)
            {
                var shield = shields[i];
                shield.transform.localRotation = Quaternion.Euler(0, singleAngle * i, 0);
                shield.Reset();
            }
            
            RotateShields();
        }

        private void RotateShields()
        {
            if (_tween == null)
            {
                _tween = transform.DORotate(new Vector3(0, 360, 0), shieldsSingleSpinTime, RotateMode.FastBeyond360).SetLoops(-1)
                    .SetEase(Ease.Linear).SetRelative(true);
            }
        }

        private void OnDisable()
        {
            _tween.Kill();
            _tween = null;
        }
    }
}
