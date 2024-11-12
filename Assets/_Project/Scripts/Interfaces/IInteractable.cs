using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interacter);
    bool CanInteract();
    GameObject GetGameObject();
    Vector3 GetPositionForUI();
}

public interface IAltInteractable
{
    void AltInteract(GameObject interacter);
    bool CanAltInteract();
    GameObject GetGameObject();
    Vector3 GetPositionForUI();
}
