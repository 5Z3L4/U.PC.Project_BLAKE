using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.EnemyAI.Visuals
{
    public enum AnimationType
    {
        PunchPosition = 1,
        PunchScale = 2,
    }
    
    public class EnemyStateVisuals : MonoBehaviour
    {
        [SerializeField] private GameObject _yellowMarker;
        [SerializeField] private GameObject _redMarker;
        
        private bool _isAttacking;
        private Sequence _tweenSequence;
        
        public void TryChangeEnemyVisuals(CombatState combatState)
        {
            if (_isAttacking)
            {
                return;
            }
            
            switch (combatState)
            {
                case CombatState.Patrol:
                    OnPatrolState();
                    break;
                case CombatState.Alarmed:
                    OnAlarmedState();
                    break;
                case CombatState.Chase:
                case CombatState.Attack:
                case CombatState.Strafe:
                    OnAttackingState();
                    break;
                case CombatState.Undefined:
                default:
                    break;
            }
        }

        private void OnPatrolState()
        {
            _isAttacking = false;
            _tweenSequence?.Kill(true);
        }

        private void OnAlarmedState()
        {
            AnimateVisual(_yellowMarker, AnimationType.PunchPosition).Forget();
        }

        private void OnAttackingState()
        {
            if (!_isAttacking)
            {
                _isAttacking = true;
                AnimateVisual(_redMarker, AnimationType.PunchScale, 2f).Forget();
            }
        }

        private async UniTask AnimateVisual(GameObject marker, AnimationType type, float duration = 1f)
        {
            HideAnotherMarkers(marker);
            _tweenSequence?.Kill(true);
            _tweenSequence = DOTween.Sequence();
            _tweenSequence.Join(marker.transform.DOLookAt(Camera.main.transform.position, duration, AxisConstraint.Y));
            switch (type)
            {
                case AnimationType.PunchPosition:
                    _tweenSequence.Join(marker.transform.DOPunchPosition(Vector3.up, duration));
                    break;
                case AnimationType.PunchScale:
                    _tweenSequence.Join(marker.transform.DOPunchScale(Vector3.one, duration));
                    break;
            }
            
            marker.transform.rotation = Quaternion.Euler(new Vector3(0, 230, 0));
            
            marker.SetActive(true);
            _tweenSequence.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            marker.SetActive(false);
            _tweenSequence = null;
        }

        private void HideAnotherMarkers(GameObject marker)
        {
            if (marker != _yellowMarker)
            {
                _yellowMarker.SetActive(false);
            }
            
            if (marker != _redMarker)
            {
                _redMarker.SetActive(false);
            }
        }

        private void OnDisable()
        {
            _isAttacking = false;
        }
    }
}
