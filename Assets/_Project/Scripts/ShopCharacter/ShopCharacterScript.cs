using _Project.Scripts.GlobalHandlers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopCharacterScript : MonoBehaviour, IInteractable
{
    [SerializeField]
    private GameObject shopMenu;
    [SerializeField]
    private Transform uiPos;
    private bool isOpen;
    public bool CanInteract()
    {
        return true;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public Vector3 GetPositionForUI()
    {
        return uiPos.position;
    }

    public void Interact(GameObject interacter)
    {
        if(!isOpen)
        {
            OpenShop();
        }
    }

    private void OpenShop()
    {
        shopMenu.SetActive(true);
        GameHandler.Instance.PauseWithoutUI();
        isOpen = true;
    }

    public void CloseShop()
    {
        shopMenu.SetActive(false);
        GameHandler.Instance.CloseAllCanvasAndUnpause();
        isOpen = false;
    }

    private void OnEnable()
    {
        if (ReferenceManager.PlayerInputController == null) return;
        ReferenceManager.PlayerInputController.escapeButtonEvent += CloseShop;
    }

    private void OnDisable()
    {
        if (ReferenceManager.PlayerInputController == null) return;
        ReferenceManager.PlayerInputController.escapeButtonEvent -= CloseShop;
    }


}
