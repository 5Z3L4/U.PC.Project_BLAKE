using System;
using System.Collections.Generic;

using UnityEngine;
using _Project.Scripts.Player;
using SolidUtilities.UnityEngineInternals;

using GameFramework.System;
using _Project.Scripts.Abilities;

namespace GameFramework.Abilities
{
    public partial class AbilityManager : MonoBehaviour
    {
        [SerializeField] 
        private List<AbilityDefinition> startupAbilities = new();

        protected List<Ability> availableAbilities = new();
        protected Dictionary<State, int> stateCountArray = new();
        protected List<State> activeStates = new();
        protected Dictionary<State, StateDelegate> stateEventArray = new();

        public delegate void StateDelegate(State state, int newCount);

        private partial void Awake();

        #region Abilities

        public partial void GiveAbility(AbilityDefinition abilityDefinition, object sourceObject = null);
        public partial void RemoveAbility(AbilityDefinition abilityDefinition);

        public partial bool TryActivateAbility(Type abilityClass);

        #endregion

        #region States

        public State[] ActiveStates { get => activeStates.ToArray(); }
        public partial void UpdateStates(State state, int countDelta);
        public partial void RegisterStateEvent(State state, StateDelegate stateDelegate);

        #endregion
    }

    public partial class AbilityManager : MonoBehaviour
    {
        private partial void Awake()
        {
            for (int i = 0; i < startupAbilities.Count; i++)
            {
                GiveAbility(startupAbilities[i]);
            }
        }

        private void Start()
        {
            PlayerInputController.Instance.onShootBasicStartEvent += OnShootBasicPressed;
            PlayerInputController.Instance.onShootBasicCancelEvent += OnShootBasicReleased;
            PlayerInputController.Instance.onShootStrongStartEvent += OnShootStrongPressed;
            PlayerInputController.Instance.onShootStrongCancelEvent += OnShootStrongReleased;
        }

        private void OnDestroy()
        {
            PlayerInputController.Instance.onShootBasicStartEvent -= OnShootBasicPressed;
            PlayerInputController.Instance.onShootBasicCancelEvent -= OnShootBasicReleased;
            PlayerInputController.Instance.onShootStrongStartEvent -= OnShootStrongPressed;
            PlayerInputController.Instance.onShootStrongCancelEvent -= OnShootStrongReleased;
        }

        #region Abilities

        public partial void GiveAbility(AbilityDefinition abilityDefinition, object sourceObject)
        {
            if (abilityDefinition == null) { Debug.LogError("AbilityDefinition is not vaild"); return; }

            Ability newAbility = abilityDefinition.AbilityInstance.ShallowCopy();
            availableAbilities.Add(newAbility);


            newAbility.SetupAbility(this, sourceObject);
            newAbility.OnGiveAbility();
        }

        public partial void RemoveAbility(AbilityDefinition abilityDefinition)
        {
            if (abilityDefinition == null) { Debug.LogError("AbilityDefinition is not vaild"); return; }

            for(int i = 0; i < availableAbilities.Count; i++)
            {
                if (availableAbilities[i].AbilityDefinition == abilityDefinition)
                {
                    if(availableAbilities[i].IsActive)
                    {
                        availableAbilities[i].OnAbilityEnded += (bool wasCanceled) =>
                        {
                            RemoveAbility(abilityDefinition);
                        };
                        return;
                    }

                    availableAbilities.RemoveAt(i);
                    return;
                }
            }
        }

        private void OnShootBasicPressed()
        {
            for (int i = 0; i < availableAbilities.Count; i++)
            {
                if (availableAbilities[i] is BasicAttack || availableAbilities[i] is BasicAttackHold)
                {
                    availableAbilities[i].IsInputPressed = true;

                    if (availableAbilities[i].IsActive)
                    {
                        availableAbilities[i].InputPressed();
                    }
                    else
                    {
                        TryActivateAbility(availableAbilities[i].GetType());
                    }
                }
            }
        }

        private void OnShootBasicReleased()
        {
            for (int i = 0; i < availableAbilities.Count; i++)
            {
                if (availableAbilities[i] is BasicAttack || availableAbilities[i] is BasicAttackHold)
                {
                    availableAbilities[i].IsInputPressed = false;

                    if (availableAbilities[i].IsActive)
                    {
                        availableAbilities[i].InputReleased();
                    }
                }
            }
        }

        private void OnShootStrongPressed()
        {
            for (int i = 0; i < availableAbilities.Count; i++)
            {
                if (availableAbilities[i] is RevolverStrong || availableAbilities[i] is SniperStrong)
                {
                    availableAbilities[i].IsInputPressed = true;

                    if (availableAbilities[i].IsActive)
                    {
                        availableAbilities[i].InputPressed();
                    }
                    else
                    {
                        TryActivateAbility(availableAbilities[i].GetType());
                    }
                }
            }
        }

        private void OnShootStrongReleased()
        {
            for (int i = 0; i < availableAbilities.Count; i++)
            {
                if (availableAbilities[i] is RevolverStrong || availableAbilities[i] is RevolverStrong)
                {
                    availableAbilities[i].IsInputPressed = false;

                    if (availableAbilities[i].IsActive)
                    {
                        availableAbilities[i].InputReleased();
                    }
                }
            }
        }

        public partial bool TryActivateAbility(Type abilityClass)
        {
            if (abilityClass == null) { Debug.LogError("AbilityClass is not valid"); return false; }

            for (int i = 0; i < availableAbilities.Count; i++)
            {
                if (availableAbilities[i].AbilityDefinition.AbilityClass.Type == abilityClass)
                {
                    if (!availableAbilities[i].CanActivateAbility()) return false;

                    availableAbilities[i].ActivateAbility();
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region States

        public partial void RegisterStateEvent(State state, StateDelegate stateDelegate)
        {
            if(!stateEventArray.ContainsKey(state))
            {
                stateEventArray.Add(state, stateDelegate);
                return;
            }

            stateEventArray[state] += stateDelegate;
        }

        public partial void UpdateStates(State state, int countDelta)
        {
            if (countDelta != 0)
            {
                if(stateCountArray.ContainsKey(state))
                {
                    stateCountArray[state] = Math.Max(stateCountArray[state] + countDelta, 0);
                }
                else
                {
                    stateCountArray.Add(state, Math.Max(countDelta, 0));
                }

                if(activeStates.Contains(state))
                {
                    if (stateCountArray[state] == 0)
                    {
                        activeStates.Remove(state);
                    }
                }
                else
                {
                    if (stateCountArray[state] != 0)
                    {
                        activeStates.Add(state);
                    }
                }

                if (stateEventArray.ContainsKey(state))
                {
                    List<StateDelegate> invalidDelegates = null;
                    Delegate[] delegates = stateEventArray[state].GetInvocationList();
                    for (int i = 0; i < delegates.Length; i++)
                    {
                        if (delegates[i] is not StateDelegate stateDelegate) continue;
                        
                        if (stateDelegate.Target is MonoBehaviour monoBehaviour)
                        {
                            if (monoBehaviour == null)
                            {
                                invalidDelegates ??= new List<StateDelegate>();
                                invalidDelegates.Add(stateDelegate);
                                continue;
                            }
                        }

                        stateDelegate.Invoke(state, stateCountArray[state]);
                    }

                    if (invalidDelegates == null) return;

                    for (int i = 0; i < invalidDelegates.Count; i++)
                    {
                        stateEventArray[state] -= invalidDelegates[i];
                    }
                }
            }
        }

        #endregion

    }
}