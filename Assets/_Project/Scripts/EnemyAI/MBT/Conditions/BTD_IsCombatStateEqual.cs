using MBT;
using UnityEngine;

[AddComponentMenu("")]
[MBTNode(name = "Conditions/Is Combat State Equal")]
public class BTD_IsCombatStateEqual : Condition
{
    public Abort Abort;
    public bool Invert = false;
    public CombatStateReference CombatStateReference = new CombatStateReference();
    public CombatState CombatState;

    public override bool Check()
    {
        return CombatState == CombatStateReference.GetVariable().Value;
    }

    public override void OnAllowInterrupt()
    {
        if (Abort != Abort.None)
        {
            ObtainTreeSnapshot();
            CombatStateReference.GetVariable().AddListener(OnVariableChange);
        }
    }

    public override void OnDisallowInterrupt()
    {
        if (Abort != Abort.None)
        {
            CombatStateReference.GetVariable().RemoveListener(OnVariableChange);
        }
    }

    private void OnVariableChange(CombatState oldValue, CombatState newValue)
    {
        EvaluateConditionAndTryAbort(Abort);
    }

    public override bool IsValid()
    {
        return !CombatStateReference.isInvalid;
    }
}
