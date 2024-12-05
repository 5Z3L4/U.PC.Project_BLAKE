using System;

using UnityEngine;

using TypeReferences;

using GameFramework.System;
using GameFramework.System.Attributes;
using UnityEngine.InputSystem;

namespace GameFramework.Abilities
{
    [CreateAssetMenu(menuName = "GameFramework/Abilities/AbilityDefinition")]
    public class AbilityDefinition : ScriptableObject
    {
        [Inherits(typeof(Ability), IncludeBaseType = true, ShowNoneElement = false, ShortName = true)]
        public TypeReference AbilityClass = new(typeof(Ability));

        [SerializeField, HideInInspector]
        private TypeReference oldAbilityClass = new(typeof(Ability));

        [SerializeReference]
        private Ability abilityInstance;
        public Ability AbilityInstance { get => abilityInstance; }


        private void OnValidate()
        {
            if(AbilityClass.Type != oldAbilityClass.Type)
            {
                abilityInstance = (Ability)Activator.CreateInstance(AbilityClass);
                abilityInstance.AbilityDefinition = this;

                oldAbilityClass.Type = AbilityClass.Type;
            }
        }

        [Tooltip("Describes ability.")]
        [State] public State[] AbilityStates;

        [Tooltip("Abilities with these states will be canceled.")]
        [State] public State[] AbilitiesToCancel;

        [Tooltip("Abilities with these states will be blocked.")]
        [State] public State[] AbilitiesToBlock;

        [Tooltip("Abilities with these states will be added to ability manager while ability is active.")]
        [State] public State[] StatesToAdd;

        [Tooltip("States required to activate this ability.")]
        [State] public State[] RequiredStates;

        [Tooltip("States that blocks activation of this ability.")]
        [State] public State[] BlockedStates;
    }
}