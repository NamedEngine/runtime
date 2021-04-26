using System.Collections;

public abstract class Condition : Chainable {
    protected Condition(IValue[][] argTypes,IValue[] values) : base(argTypes, values) { }
    
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = ConditionLogic();
        return null;
    }

    protected abstract bool ConditionLogic();
}