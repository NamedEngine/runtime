using System.Collections;
using UnityEngine;

public abstract class Condition : Chainable {
    protected Condition(IValue[][] argTypes, GameObject gameObject, IValue[] values, bool constraintReference) : base(argTypes, gameObject, values, constraintReference) { }
    
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = ConditionLogic();
        return null;
    }

    protected abstract bool ConditionLogic();
}