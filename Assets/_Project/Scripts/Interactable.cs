using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Interactable : MonoBehaviour, IInteractable
{
    //Position where UI Element should appear
    [SerializeField]
    protected Transform uiHolder;

    //Can player interact with this object
    [SerializeField]
    protected bool interactable = true;

    /// <summary>
    /// Interact with object
    /// </summary>
    public abstract void Interact(GameObject interacter);


    /// <returns>If object is interactable</returns>
    public bool CanInteract()
    {
        return interactable;
    }

    /// <returns>This gameObject</returns>
    public GameObject GetGameObject()
    {
        if (this == null) return null;
        return this.gameObject;
    }

    /// <returns>Position where UI should appear</returns>
    public Vector3 GetPositionForUI()
    {
        return uiHolder.position;
    }
}
