using _Project.Scripts;
using _Project.Scripts.EnemyAI;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Weapons;
using DG.Tweening;
using MBT;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyCharacter))]
public class AIController : MonoBehaviour
{
    public Weapon Weapon;

    [HideInInspector]
    public NavMeshAgent NavMeshAgent;

    [SerializeField] private float alarmEnemiesInRoomDelay;
    private GameObject playerRef;
    private Animator animator;
    private EnemyCharacter character;
    private Tween _alarmEnemies;

    public CombatStateReference CombatStateReference;
    public BoolReference HasLineOfSightReference;

    public Waypoints Waypoints;

    private void Awake()
    {
        UpdatePlayerRef();
        Weapon.Owner = gameObject.GetComponent<BlakeCharacter>();

        NavMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        character = GetComponent<EnemyCharacter>();

        character.onDeath += Die;
        GetComponent<EnemyFOV>().OnCanSeePlayerChanged += OnCanSeePlayerChanged;
    }

    private void Start()
    {
        ReferenceManager.BlakeHeroCharacter.onDeath += OnPlayerDeath;
    }
    
    private void OnDestroy()
    {
        if (ReferenceManager.BlakeHeroCharacter is not null)
        {
            ReferenceManager.BlakeHeroCharacter.onDeath -= OnPlayerDeath;
        }
    }

    private void Die(BlakeCharacter blakeCharacter)
    {
        this.enabled = false;
        NavMeshAgent.isStopped = true;
        GetComponent<MBTExecutor>().enabled = false;
    }

    private void Update()
    {
        FaceThePlayer();

        animator.SetFloat("Direction", BlakeAnimatorHelper.CalculateDirection(NavMeshAgent.velocity, transform));
        animator.SetFloat("Speed", NavMeshAgent.velocity.magnitude);
    }

    public void UpdatePlayerRef()
    {
        playerRef = ReferenceManager.BlakeHeroCharacter?.gameObject;
    }

    public EnemyCharacter GetEnemyScript()
    {
        return character;
    }

    public void SetWaypoints(Waypoints waypoints)
    {
        Waypoints = waypoints;
    }

    private void OnCanSeePlayerChanged(bool newCanSeePlayer, CombatState stateToApply = CombatState.Alarmed, bool instantAlarmEnemies = false)
    {
        HasLineOfSightReference.Value = newCanSeePlayer;
        CombatStateReference.GetVariable().Value = stateToApply;
        
        if (newCanSeePlayer)
        {
            _alarmEnemies = DOVirtual.DelayedCall(instantAlarmEnemies ? 0f : alarmEnemiesInRoomDelay,
                () => ChangeStateForEveryInRoom(CombatState.Chase), false).SetLink(gameObject);
            
            if (IsInvoking("ClearPlayerFocus"))
            {
                CancelInvoke("ClearPlayerFocus");
            }
        }
        else
        {
            if (stateToApply == CombatState.Patrol)
            {
                _alarmEnemies.Kill();
                _alarmEnemies = null;
            }
            
            Invoke("ClearPlayerFocus", 10f);
        }
    }

    private void ChangeStateForEveryInRoom(CombatState combatState)
    {
        if (!character.IsAlive) return;
        
        var enemiesInRoom = character.SpawnedInRoom.SpawnedEnemiesList;
        foreach (var enemy in enemiesInRoom)
        {
            if(enemy.AIController == this) continue;
            
            enemy.AIController.TryChangeCombatState(combatState);
        }

        _alarmEnemies = null;
    }
    
    public void TryChangeCombatState(CombatState combatState)
    {
        if (!character.IsAlive) return;
        
        CombatStateReference.GetVariable().Value = combatState;
        if (IsInvoking("ClearPlayerFocus"))
        {
            CancelInvoke("ClearPlayerFocus");
        }
    }

    private void FaceThePlayer()
    {
        if (playerRef == null)
        {
            return;
        }

        if (CombatStateReference.GetVariable().Value != CombatState.Patrol)
        {
            Vector3 direction = (playerRef.transform.position - gameObject.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, lookRotation, Time.deltaTime * 40f);
        }
    }

    private void ClearPlayerFocus()
    {
        CombatStateReference.GetVariable().Value = CombatState.Patrol;
    }

    private void OnPlayerDeath(BlakeCharacter blakeCharacter)
    {
        ClearPlayerFocus();
    }
}
