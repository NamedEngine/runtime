using System.Collections;

public abstract class Condition : Chainable {
    protected Condition(IValue[][] argTypes, ConstrainableContext context, bool constraintReference)
        : base(argTypes, context, constraintReference) { }
    
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = ConditionLogic();
        return null;
    }

    protected abstract bool ConditionLogic();
}
