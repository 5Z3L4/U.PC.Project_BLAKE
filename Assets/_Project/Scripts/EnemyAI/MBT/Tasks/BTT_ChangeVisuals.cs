using _Project.Scripts.EnemyAI.Visuals;
using MBT;
using UnityEngine;

namespace _Project.Scripts.EnemyAI.MBT.Tasks
{
    [AddComponentMenu("")]
    [MBTNode(name = "Tasks/ChangeVisuals")]
    public class BTT_ChangeVisuals : Leaf
    {
        public CombatStateReference CombatStateReference = new CombatStateReference();
        public EnemyStateVisuals EnemyVisuals;

        public override NodeResult Execute()
        {
            EnemyVisuals.TryChangeEnemyVisuals(CombatStateReference.GetVariable().Value);
            return NodeResult.success;
        }
    }
}
