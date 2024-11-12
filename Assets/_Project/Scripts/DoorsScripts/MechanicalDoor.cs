using _Project.Scripts.GlobalHandlers;
using UnityEngine;

public class MechanicalDoor : Door, IInteractable, IAltInteractable
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private RoomConnector connector;
    [SerializeField]
    private Transform uiHolder;
    [SerializeField]
    private bool interactable = false;
    [SerializeField]
    private bool open = false;

    private Room roomToPeek;

    private void Awake()
    {
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        LockDoor();
    }

    private void Start()
    {
        CloseDoor();
    }
    public override void UnlockDoor()
    {
        interactable = true;
    }

    public override void LockDoor()
    {
        interactable = false;
        CloseDoor();
    }

    public void Interact(GameObject interacter)
    {
        if (!CanInteract()) return;

        if(open)
        {
            CloseDoor();
            connector.GetConnector().CloseDoor();
        } 
        else
        {
            OpenDoor();
            connector.GetConnector().OpenDoor();
        }
    }


    public bool CanInteract()
    {
        return interactable;
    }

    private void SetRoomToPeek()
    {
        if (connector.GetConnector().GetRoom() != ReferenceManager.RoomManager.GetActiveRoom())
        {
            roomToPeek = connector.GetConnector().GetRoom();
        }
        else
        {
            roomToPeek = connector.GetRoom();
        }
    }


    public void AltInteract(GameObject interacter)
    {
        if (!CanAltInteract()) return;
        ReferenceManager.PlayerInputController.EnablePeeking();
        roomToPeek.Peek();
    }

    public bool CanAltInteract()
    {
        SetRoomToPeek();
        return (roomToPeek.HavePeekingCam());
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
    public override void OpenDoor()
    {
        if (IsAnyAnimationPlaying())
        {
            return;
        }
        
        animator.SetBool("closed", false);
        open = true;
    }

    public override void CloseDoor()
    {
        if (IsAnyAnimationPlaying())
        {
            return;
        }
        
        animator.SetBool("closed", true);
        open = false;
    }

    public Vector3 GetPositionForUI()
    {
        return uiHolder.position;
    }
    
    private bool IsAnyAnimationPlaying()
    {
        var currentState = animator.GetCurrentAnimatorStateInfo(0);
        return currentState.normalizedTime < 1 && animator.GetCurrentAnimatorClipInfo(0).Length > 0;
    }

}
