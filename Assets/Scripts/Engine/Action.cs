
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Action : Chainable {
    protected Action(IValue[][] argTypes, IValue[] values) : base(argTypes, values) { }
    protected override IEnumerator InternalLogic(out bool shouldNotify) {
        shouldNotify = true;
        return ActionLogic();
    }

    abstract protected IEnumerator ActionLogic();
}