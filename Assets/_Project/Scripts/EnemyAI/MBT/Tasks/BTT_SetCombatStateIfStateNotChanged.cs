using System.Collections;
using MBT;
using UnityEngine;

namespace _Project.Scripts.EnemyAI.MBT.Tasks
{
    [AddComponentMenu("")]
    [MBTNode(name = "Tasks/SetCombatStateIfStateNotChanged")]
    public class BTT_SetCombatStateIfStateNotChanged : BTT_SetCombatState
    {
        public CombatState currentState;

        public override NodeResult Execute()
        {
            StartCoroutine("SetNewValue");
            return NodeResult.success;
        }
        
        private IEnumerator SetNewValue()
        {
            yield return new WaitForEndOfFrame();
            if (currentState == CombatStateReference.GetVariable().Value)
            {
                CombatStateReference.GetVariable().Value = NewCombatState;
            }
        }
    }
}
