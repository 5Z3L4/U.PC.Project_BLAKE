using MBT;
using UnityEngine;

[AddComponentMenu("")]
public class CombatStateVariable : Variable<CombatState>
{
    protected override bool ValueEquals(CombatState val1, CombatState val2)
    {
        return val1 == val2;
    }
}

[System.Serializable]
public class CombatStateReference : VariableReference<CombatStateVariable, CombatState>
{
    // You can create additional constructors and Value getter/setter
    // See FloatVariable.cs as example

    // If your variable is reference type you might want constant validation
    // protected override bool isConstantValid
    // {
    //     get { return constantValue != null; }
    // }
}

public enum CombatState
{
    Undefined = 0,
    Patrol = 1,
    Chase = 2,
    Attack = 3,
    Strafe = 4,
    Alarmed = 5
}