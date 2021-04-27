using System.Collections;

public abstract class Action : Chainable {
    protected Action(IValue[][] argTypes, IValue[] values, bool constraintReference) : base(argTypes, values, constraintReference) { }
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = true;
        return ActionLogic();
    }

    protected abstract IEnumerator ActionLogic();
}