using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class Condition : Chainable {
    protected Condition(IValue[][] argTypes,IValue[] values) : base(argTypes, values) { }
    
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = ConditionLogic();
        return null;
    }

    abstract protected bool ConditionLogic();
}