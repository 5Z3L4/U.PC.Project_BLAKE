using _Project.Scripts.GlobalHandlers;
using UnityEngine;

namespace _Project.Scripts.SceneHandlers
{
    public class NextLevelInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private Transform UIHolderTransform;
        [SerializeField]
        private Room room;
        public bool CanInteract()
        {
            return room.IsBeaten;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public Vector3 GetPositionForUI()
        {
            if (UIHolderTransform == null) return transform.position;
            return UIHolderTransform.position;
        }

        public void Interact(GameObject interacter)
        {
            ReferenceManager.LevelHandler.GoToNextLevel();
        }
    }
}
