using System.Collections;
using UnityEngine;

public abstract class Action : Chainable {
    protected Action(IValue[][] argTypes, GameObject gameObject, IValue[] values, bool constraintReference) : base(argTypes, gameObject, values, constraintReference) { }
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = true;
        return ActionLogic();
    }

    protected abstract IEnumerator ActionLogic();
}