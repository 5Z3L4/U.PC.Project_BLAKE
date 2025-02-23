using System.Collections.Generic;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.SceneHandlers;
using UnityEngine;

public class PlayerInteractables : MonoBehaviour
{
    [SerializeField]
    private float interactRadius = 10f;
    [SerializeField]
    private LayerMask collectMask;
    [SerializeField]
    private LayerMask raycastMask;
    //List of Interactable objects nearby
    [SerializeField]
    private List<IInteractable> interactables = new List<IInteractable>();
    [SerializeField]
    private List<IAltInteractable> altInteractables = new List<IAltInteractable>();
    [SerializeField]
    private Transform playerHead;

    //Object of UI to show above
    [SerializeField]
    private GameObject interactUI;
    [SerializeField]
    private GameObject altInteractUI;

    private void Start()
    {
        ReferenceManager.PlayerInputController.interactEvent += Interact;
        ReferenceManager.PlayerInputController.altInteractEvent += AltInteract;
        InvokeRepeating("CheckForInteractables", 0f, 0.2f);
    }

    /// <summary>
    /// Add Interactable to list
    /// </summary>
    /// <param name="interactable"></param>
    public void AddInteractable(IInteractable interactable)
    {
        interactables.Add(interactable);
    }

    public void AddAltInteractable(IAltInteractable interactable)
    {
        altInteractables.Add(interactable);
    }

    /// <summary>
    /// Remove Interactable from list
    /// </summary>
    /// <param name="interactable"></param>
    public void RemoveInteractable(IInteractable interactable)
    {
        interactables.Remove(interactable);
    }

    public void RemoveAltInteractable(IAltInteractable interactable)
    {
        altInteractables.Remove(interactable);
    }

    private void Update()
    {
        SetUI();
        SetAltUI();
    }

    private void CheckForInteractables()
    {
        interactables.Clear();
        altInteractables.Clear();
        Collider[] cols = Physics.OverlapSphere(transform.position, interactRadius, collectMask, QueryTriggerInteraction.Collide);
        foreach(var col in cols)
        {
            if(col.GetComponent<IInteractable>() != null)
            {
                interactables.Add(col.GetComponent<IInteractable>());
            }
            if(col.GetComponent<IAltInteractable>() != null)
            {
                altInteractables.Add(col.GetComponent<IAltInteractable>());
            }
        }
    }

    private IInteractable GetClosestInteractable()
    {
        if (SceneHandler.Instance.IsAnySceneLoading)
        {
            interactables.Clear();
            altInteractables.Clear();
            return null;
        }
        
        if (interactables.Count > 0)
        {
            IInteractable closest = null;
            float closestDistance = float.MaxValue;

            List<IInteractable> invalidInteractables = new List<IInteractable>();

            foreach (IInteractable interactable in interactables)
            {
                if(interactable == null)
                {
                    invalidInteractables.Add(interactable);
                    continue;
                }
                if (!interactable.CanInteract()) continue;
                if (interactable.GetGameObject() == null) { invalidInteractables.Add(interactable); continue; }

                Vector3 direction = interactable.GetGameObject().transform.position - playerHead.position;
                Vector3 rayOrigin = playerHead.position;

                Collider objCollider = interactable.GetGameObject().GetComponent<Collider>();
                if (objCollider != null && objCollider.bounds.Contains(playerHead.position))
                {
                    float distance = Vector3.Distance(gameObject.transform.position, interactable.GetGameObject().transform.position);
                    if (distance < closestDistance)
                    {
                        closest = interactable;
                        closestDistance = distance;
                    }
                    continue;
                }
                if (Physics.Raycast(new Ray(rayOrigin, direction), out RaycastHit hit, interactRadius, raycastMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.transform.gameObject == interactable.GetGameObject())
                    {
                        float distance = Vector3.Distance(gameObject.transform.position, interactable.GetGameObject().transform.position);
                        if (distance < closestDistance)
                        {
                            closest = interactable;
                            closestDistance = distance;
                        }
                    }
                }

            }

            for(var i = 0; i < invalidInteractables.Count; i++)
            {
                interactables.Remove(invalidInteractables[i]);
            }

            return closest;
        }
        
        return null;
    }

    private IAltInteractable GetClosestAltInteractable()
    {
        if (altInteractables.Count > 0)
        {
            IAltInteractable closest = null;
            float closestDistance = float.MaxValue;

            List<IAltInteractable> invalidInteractables = new List<IAltInteractable>();

            foreach (IAltInteractable interactable in altInteractables)
            {
                if (interactable == null)
                {
                    invalidInteractables.Add(interactable);
                    continue;
                }
                if (!interactable.CanAltInteract()) continue;
                if (interactable.GetGameObject() == null) { invalidInteractables.Add(interactable); continue; }
                Vector3 direction = interactable.GetGameObject().transform.position - playerHead.position;
                Vector3 rayOrigin = playerHead.position;

                Collider objCollider = interactable.GetGameObject().GetComponent<Collider>();
                if (objCollider != null && objCollider.bounds.Contains(playerHead.position))
                {
                    float distance = Vector3.Distance(gameObject.transform.position, interactable.GetGameObject().transform.position);
                    if (distance < closestDistance)
                    {
                        closest = interactable;
                        closestDistance = distance;
                    }
                    continue;
                }
                if (Physics.Raycast(new Ray(rayOrigin, direction), out RaycastHit hit, interactRadius, raycastMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.transform.gameObject == interactable.GetGameObject())
                    {
                        float distance = Vector3.Distance(gameObject.transform.position, interactable.GetGameObject().transform.position);
                        if (distance < closestDistance)
                        {
                            closest = interactable;
                            closestDistance = distance;
                        }
                    }
                }
            }

            for (int i = 0; i < invalidInteractables.Count; i++)
            {
                altInteractables.Remove(invalidInteractables[i]);
            }

            return closest;
        }
        return null;
    }

    private void Interact()
    {
        IInteractable closest = GetClosestInteractable();
        if (closest == null) return;

        closest.Interact(gameObject);
    }

    private void AltInteract()
    {
        IAltInteractable closest = GetClosestAltInteractable();
        if (closest == null) return;

        closest.AltInteract(gameObject);
    }

    //Set UI Element above Interactable
    private void SetUI()
    {
        if (interactUI == null) return;

        IInteractable closest = GetClosestInteractable();
        if (closest == null)
        {
            interactUI.SetActive(false);
            return;
        }

        interactUI.SetActive(true);
        interactUI.transform.position = closest.GetPositionForUI();
        interactUI.transform.LookAt(Camera.main.transform);
    }

    private void SetAltUI()
    {
        if (altInteractUI == null) return;

        IAltInteractable closest = GetClosestAltInteractable();
        if (closest == null)
        {
            altInteractUI.SetActive(false);
            return;
        }

        altInteractUI.SetActive(true);
        altInteractUI.transform.position = closest.GetPositionForUI() - Vector3.up;
        altInteractUI.transform.LookAt(Camera.main.transform);
    }

    public void SetInteractUIReference(GameObject interactUIReference, GameObject altInteractUIReference)
    {
        interactUI = interactUIReference;
        altInteractUI = altInteractUIReference;
    }
}
