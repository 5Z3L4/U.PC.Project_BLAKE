using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.EnemyAI
{
    public class EnemyFOV : MonoBehaviour
    {
        public float Radius;
        [Range(0, 360)] public float Angle;

        [SerializeField] private LayerMask TargetMask;
        [SerializeField] private LayerMask ObstacleMask;
        [SerializeField] private Transform raycastOrigin;
        [SerializeField] private float fovCheckDelay;
        [SerializeField] private float maxBackVisibilityDistance;
        [SerializeField] private float findPlayerFromBehindDelay;
        

        //create one Collider because there will be only one player
        private Collider[] overlapSphereColliders = new Collider[1];
        private CancellationTokenSource cts;
        private AIController _aiController;
        private bool _canSeePlayer;

        [HideInInspector]
        public bool CanSeePlayer { 
            get => _canSeePlayer;
            set
            {
                if (_canSeePlayer == false)
                {
                    _canSeePlayer = value;
                }
            }
        }

        public delegate void CanSeePlayerChanged(bool newCanSeePlayer, CombatState stateToApply = CombatState.Alarmed, bool instantAlarmEnemies = false);
        public event CanSeePlayerChanged OnCanSeePlayerChanged;

        private void OnEnable()
        {
            FOVRoutine().Forget();
            _aiController = GetComponent<AIController>();
        }

        private void OnDisable()
        {
            _canSeePlayer = false;
        }

        private async UniTaskVoid FOVRoutine()
        {
            cts = new CancellationTokenSource();
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(fovCheckDelay), cancellationToken: cts.Token);
                await FieldOfViewCheck(cts.Token);
            }
        }

        private async UniTask FieldOfViewCheck(CancellationToken cToken)
        {
            Physics.OverlapSphereNonAlloc(transform.position, Radius, overlapSphereColliders, TargetMask);
            
            if (overlapSphereColliders[0] is null)
            {
                if (CanSeePlayer)
                {
                    OnCanSeePlayerChanged?.Invoke(_canSeePlayer = false, CombatState.Patrol);
                }
                
                return;
            }

            if (CanSeePlayer && _aiController.CombatStateReference.GetVariable().Value != CombatState.Alarmed)
            {
                return;
            }

            var target = overlapSphereColliders[0].transform;
            var directionToTarget = (target.position - transform.position).normalized;
            var distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (!Physics.Raycast(raycastOrigin.position, directionToTarget, distanceToTarget, ObstacleMask))
            {
                if (CanSeePlayer)
                {
                    return;
                }
                
                //front of enemy 
                if (Vector3.Angle(transform.forward, directionToTarget) < Angle / 2)
                {
                    OnCanSeePlayerChanged?.Invoke(CanSeePlayer = true);
                }
                //back of enemy
                else if(distanceToTarget < maxBackVisibilityDistance)
                {
                    CanSeePlayer = true;
                    await UniTask.Delay(TimeSpan.FromSeconds(findPlayerFromBehindDelay), cancellationToken: cToken);
                    OnCanSeePlayerChanged?.Invoke(CanSeePlayer);
                }
            }
            else if (_aiController.CombatStateReference.GetVariable().Value == CombatState.Alarmed && CanSeePlayer)
            {
                OnCanSeePlayerChanged?.Invoke(_canSeePlayer = false, CombatState.Patrol);
                _canSeePlayer = false;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.collider.CompareTag("Player"))
            {
                return;
            }
            
            if (StateShouldBeChanged())
            {
                OnCanSeePlayerChanged?.Invoke(CanSeePlayer = true, CombatState.Chase, true);
            }
        }

        private bool StateShouldBeChanged()
        {
            return _aiController.CombatStateReference.GetVariable().Value is CombatState.Undefined
                or CombatState.Patrol or CombatState.Alarmed;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxBackVisibilityDistance);
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
#endif
        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }
    }
}
