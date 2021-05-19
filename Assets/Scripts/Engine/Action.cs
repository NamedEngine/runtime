using System.Collections;

public abstract class Action : Chainable {
    protected Action(IValue[][] argTypes, ConstrainableContext context, bool constraintReference) : base(argTypes,
        context, constraintReference) { }

    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = true;
        return ActionLogic();
    }

    protected abstract IEnumerator ActionLogic();
}
