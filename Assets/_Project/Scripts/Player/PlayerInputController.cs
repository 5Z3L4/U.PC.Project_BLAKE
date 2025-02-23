using System;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Patterns;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Player
{
    public class PlayerInputController : Singleton<PlayerInputController>, PlayerInputSystem.IGameplayActions, PlayerInputSystem.IRoomPeekingActions, PlayerInputSystem.IPauseActions
    {
        private enum InputState
        {
            Gameplay,
            Peeking
        }
        
        private PlayerInputSystem inputSystem;
        private InputState inputState = InputState.Gameplay;

        protected override void Awake()
        {
            base.Awake();
            
            inputSystem = new PlayerInputSystem();
            inputSystem.Gameplay.SetCallbacks(this);
            inputSystem.RoomPeeking.SetCallbacks(this);
            inputSystem.Pause.SetCallbacks(this);
            ReferenceManager.PlayerInputController = this;
        }

        private void OnDestroy()
        {
            inputSystem.Gameplay.RemoveCallbacks(this);
            inputSystem.RoomPeeking.RemoveCallbacks(this);
            inputSystem.Pause.RemoveCallbacks(this);
            inputSystem.Dispose();

            if (ReferenceManager.PlayerInputController != null)
            {
                ReferenceManager.PlayerInputController = null;
            }
        }

        private void Start()
        {
            SetUpControls();
        }

        void SetUpControls()
        {
            SetGameplayState();
            //Shooting
        }

        public event Action<Vector2> movementEvent;
        public event Action<Vector2> mousePositionEvent;

        public event Action onShootBasicStartEvent;
        public event Action shootBasicEvent;
        public event Action onShootBasicCancelEvent;
        public event Action onShootStrongStartEvent;
        public event Action shootStrongEvent;
        public event Action onShootStrongCancelEvent;
        public event Action<int> changeWeaponEvent;
        public event Action interactEvent;
        public event Action altInteractEvent;
        public event Action onMapPressEvent; 
        public event Action onMapReleaseEvent;
        public event Action dashEvent;
        public event Action escapeButtonEvent;

        public event Action onPeekingCancel;

        public void OnMovement(InputAction.CallbackContext context)
        {
            movementEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnChangeWeapon(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                changeWeaponEvent?.Invoke((int)context.ReadValue<float>() - 1);
            }
        }

        public void OnShootBasic(InputAction.CallbackContext context)
        {
            if (context.started) onShootBasicStartEvent?.Invoke();
            if (context.canceled) onShootBasicCancelEvent?.Invoke();

            if(context.performed)
            {
                shootBasicEvent?.Invoke();
            }
        }
        
        public void OnShootStrong(InputAction.CallbackContext context)
        {
            if (context.started) onShootStrongStartEvent?.Invoke();
            if (context.canceled) onShootStrongCancelEvent?.Invoke();

            if(context.performed)
            {
                shootStrongEvent?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                interactEvent?.Invoke();
            }
        }

        public void OnAltInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                altInteractEvent?.Invoke();
            }
        }

        public void OnMap(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                onMapPressEvent?.Invoke();
            }
            else if (context.canceled)
            {
                onMapReleaseEvent?.Invoke();
            }
        }

        public void OnMousePosition(InputAction.CallbackContext context)
        {
            mousePositionEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                dashEvent?.Invoke();
            }
        }

        public void OnEscapeButton(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                escapeButtonEvent?.Invoke();
            }
        }

        public void IsGamePaused(bool isPaused)
        {
            if(isPaused)
            {
                SetPauseState();
            } 
            else
            {
                EnableInputSystem();
            }
        }
    
        public void EnableInputSystem()
        {
            switch(inputState)
            {
                case InputState.Gameplay:
                    SetGameplayState();
                    break;
                case InputState.Peeking:
                    SetPeekingState();
                    break;
            }
        }

        public void DisableInputSystem()
        {
            inputSystem.Disable();
        }

        private void OnEnable()
        {
            EnableInputSystem();
        }

        private void OnDisable()
        {
            DisableInputSystem();
        }

        public void SetPeekingState()
        {
            inputSystem.Disable();
            inputSystem.RoomPeeking.Enable();
            inputState = InputState.Peeking;
        }
        public void SetGameplayState()
        {
            inputSystem.Disable();
            inputSystem.Gameplay.Enable();
            inputState = InputState.Gameplay;
        }

        public void SetPauseState()
        {
            inputSystem.Disable();
            inputSystem.Pause.Enable();
        }
        
        public void OnCancel(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                SetGameplayState();
                onPeekingCancel?.Invoke();
            }
        }
    }
}
