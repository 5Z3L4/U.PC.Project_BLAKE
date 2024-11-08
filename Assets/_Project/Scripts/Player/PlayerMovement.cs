using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.GlobalHandlers;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const string DIRECTION = "Direction";
    private const string SPEED = "Speed";
    
    [SerializeField] 
    private float playerSpeed;
    [SerializeField]
    private float additionalPlayerSpeed = 0;
    [SerializeField] 
    private float dashForce;

    [SerializeField] 
    private float dashCooldown;
    
    [SerializeField] 
    private float dashDuration;

    [SerializeField] 
    private Transform gunHandlerTransform;

    private Vector2 movementAxis;
    private Vector2 mousePosition;
    private float angleRotationDifference;
    private Rigidbody rigidbodyCache;
    private Animator animator;

    private float dashCooldownReduction = 0;
    private float dashCooldownCountdown;
    private float dashDurationCountdown;
    private bool dashPerformed;
    private float minVelocityMagnitude = 0.1f;

    private Camera mainCamera;
    public event Action OnDashPerformed;
    public event Action<Dash> OnDashAdded;
    public event Action<Dash> OnDashRemoved;
    public event Action<Room> OnPeek;

    private List<Dash> dashes = new List<Dash>() { new Dash() };
    
    public float DashCooldown
    {
        get
        {
            return Math.Max(dashCooldown - dashCooldownReduction, 0.5f);
        }
    }

    public float DashCooldownCountdown => dashCooldownCountdown;

    public List<Dash> Dashes => dashes;

    private void Awake()
    {
        rigidbodyCache = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Peek(Room room)
    {
        OnPeek?.Invoke(room);
    }

    private void OnEnable()
    {
        StartCoroutine(SetMainCamera());
    }

    private void Start()
    {
        ReferenceManager.PlayerInputController.movementEvent += MovementHandler;
        ReferenceManager.PlayerInputController.mousePositionEvent += MousePositionHandler;
        ReferenceManager.PlayerInputController.dashEvent += Dash;
        ReferenceManager.BlakeHeroCharacter.onDeath += Die;
        ReferenceManager.BlakeHeroCharacter.onRespawn += Respawn;
    }

    private void Update()
    {
        Rotation();
        DashCountdown();

        animator.SetFloat(DIRECTION, BlakeAnimatorHelper.CalculateDirection(rigidbodyCache.velocity, transform));
        animator.SetFloat(SPEED, rigidbodyCache.velocity.magnitude);
    }


   void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();
    }

    private float CalculateSpeed()
    {
        return playerSpeed + additionalPlayerSpeed;
    }

    public void AddAdditionalSpeed(float speed)
    {
        additionalPlayerSpeed += speed;
    }

    public void RemoveAdditionalSpeed(float speed)
    {
        additionalPlayerSpeed -= speed;
    }

    public void AddDashCooldownReduction(float reduction)
    {
        dashCooldownReduction += reduction;
    }

    public void RemoveDashCooldownReduction(float reduction)
    {
        dashCooldownReduction -= reduction;
    }

    private void MovementHandler(Vector2 dir)
    {
        movementAxis = dir;
    }
    private void MousePositionHandler(Vector2 dir)
    {
        mousePosition = dir;
    }

    private void MovePlayer()
    {
        movementAxis = movementAxis.normalized;
        Vector3 direction = new Vector3(movementAxis.x, 0, movementAxis.y);
        Vector3 isometricDirection = direction.ToIsometric();
        
        rigidbodyCache.AddForce(isometricDirection * CalculateSpeed(), ForceMode.VelocityChange);
    }
    
   private void Rotation()
   {
        if (mainCamera == null) return;
       Plane playerPlane = new Plane(Vector3.up, gunHandlerTransform.position);
       Ray ray = mainCamera.ScreenPointToRay(mousePosition);
       
       
       if (playerPlane.Raycast(ray, out var hitDistance))
       {
           Vector3 targetPoint = ray.GetPoint(hitDistance);
           Quaternion playerRotation = transform.rotation;
           Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position).normalized;
           targetRotation.x = 0;
           targetRotation.z = 0;

           if (!ShouldPlayerRotate(playerRotation, targetRotation)) return;
           rigidbodyCache.MoveRotation(Quaternion.Slerp(playerRotation, targetRotation, 2000f * Time.deltaTime).normalized);
       }
   }

   private bool ShouldPlayerRotate(Quaternion playerRotation, Quaternion targetRotation)
   {
       //without this function, rigidbody calculating smth all the time
       var angle = Quaternion.Angle(playerRotation, targetRotation);
       if (angleRotationDifference == angle) return false;
       
       angleRotationDifference = angle;
       return true;

   }
    private void Dash()
    {
        var rbVelocity = rigidbodyCache.velocity;
        if (rbVelocity.magnitude < minVelocityMagnitude)
        {
            return;
        }

        bool canDash = false;
        foreach (Dash dash in dashes)
        {
            if (dash.CanUse())
            {
                dash.dashTimer = DashCooldown;
                canDash = true;
                break;
            }
        }
        if (!canDash) return;

        var force = rigidbodyCache.velocity.normalized * CalculateSpeed() * dashForce;
        rigidbodyCache.AddForce(force, ForceMode.Impulse);

        OnDashPerformed?.Invoke();
        SetDashCountdowns();
    }
   
   private void SetDashCountdowns()
   {
       dashCooldownCountdown = DashCooldown;
       dashDurationCountdown = dashDuration;
   }

    private void DashCountdown()
    {
        foreach (Dash dash in dashes)
        {
            if (dash.dashTimer > 0)
            {
                dash.dashTimer -= Time.deltaTime;
            }
        }

        if (dashDurationCountdown > 0)
        {
            dashDurationCountdown -= Time.deltaTime;
        }
    }

    public void AddDashes( int dashesToAdd)
    {
        for(int i = 0; i < dashesToAdd; i++)
        {
            Dash newDash = new Dash();
            dashes.Add(newDash);
            OnDashAdded?.Invoke(newDash);
        }
    }

    public void RemoveDashes(int dashesToRemove)
    {
        for(int i = 0; i < dashesToRemove; i++)
        {
            if(dashes.Count > 0)
            {
                var dashToRemove = dashes[dashes.Count - 1];
                OnDashRemoved?.Invoke(dashToRemove);
                dashes.Remove(dashToRemove);
            }
        }
    }

   private void SpeedControl()
   {
       if (dashDurationCountdown > 0) return;
       
       var rbVelocity = rigidbodyCache.velocity;

       if (rbVelocity.magnitude < minVelocityMagnitude)
       {
           rigidbodyCache.velocity = Vector3.zero;
           return;
       }
       
       
       Vector3 currentVelocity = new Vector3(rbVelocity.x, 0, rbVelocity.z);
       if (currentVelocity.magnitude > CalculateSpeed())
       {
           currentVelocity = currentVelocity.normalized * CalculateSpeed();
           rigidbodyCache.velocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
       }
   }
   
   private void Die(BlakeCharacter blakeCharacter)
    {
        ReferenceManager.PlayerInputController.enabled = false;
        this.enabled = false;
    }

    private void Respawn()
    {
        ReferenceManager.PlayerInputController.enabled = true;
        this.enabled = true;
    }

   private IEnumerator SetMainCamera()
   {
       while (Camera.main == null)
       {
           yield return new WaitForSeconds(0.1f);
       }

       mainCamera = Camera.main;
   }

#if UNITY_EDITOR
    public void SetDashValue(float dashValue)
    {
        dashCooldown = dashValue;
    }
#endif
}

public class Dash
{
    public float dashTimer = 0;

    public bool CanUse()
    {
        return dashTimer <= 0;
    }
}
