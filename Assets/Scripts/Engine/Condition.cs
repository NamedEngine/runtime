using System.Collections;

public abstract class Condition : Chainable {
    protected Condition(IValue[][] argTypes, IValue[] values, bool constraintReference) : base(argTypes, values, constraintReference) { }
    
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = ConditionLogic();
        return null;
    }

    protected abstract bool ConditionLogic();
}