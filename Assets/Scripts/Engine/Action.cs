using System.Collections;

public abstract class Action : Chainable {
    protected Action(IValue[][] argTypes, IValue[] values) : base(argTypes, values) { }
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = true;
        return ActionLogic();
    }

    protected abstract IEnumerator ActionLogic();
}