using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

public class DummyAnd : LogicOperator<bool> {
    static readonly IValue[][] ArgTypes = {
        new IValue[]{new LogicVariable<bool>()},
        new IValue[]{new LogicVariable<bool>()},
    };
    
    public DummyAnd(IValue[] values) : base(ArgTypes, values) { }

    protected override bool InternalGet() {
        return Arguments[0] as LogicValue<bool> && Arguments[1] as LogicValue<bool>;
    }
}

public class DummyOr : LogicOperator<bool> {
    static readonly IValue[][] ArgTypes = {
        new IValue[]{new LogicVariable<bool>()},
        new IValue[]{new LogicVariable<bool>()},
    };

    public DummyOr(IValue[] values) : base(ArgTypes, values) { }

    protected override bool InternalGet() {
        return Arguments[0] as LogicValue<bool> || Arguments[1] as LogicValue<bool>;
    }
}

public class DummyPlus : LogicOperator<int> {
    static readonly IValue[][] ArgTypes = {
        new IValue[]{new LogicVariable<int>()},
        new IValue[]{new LogicVariable<int>()},
    };

    public DummyPlus(IValue[] values) : base(ArgTypes, values) { }


    protected override int InternalGet() {
        return (Arguments[0] as LogicValue<int>) + (Arguments[1] as LogicValue<int>);
    }
}

public class DummyToInt : LogicOperator<int> {
    static readonly IValue[][] ArgTypes = {
        new IValue[] {
            new LogicVariable<int>(),
            new LogicVariable<float>(),
            new LogicVariable<bool>(),
            new LogicVariable<string>(),
        },
    };

    public DummyToInt(IValue[] values) : base(ArgTypes, values) { }

    protected override int InternalGet() {
        switch (Arguments[0]) {
            case LogicValue<int> intVal:
                return intVal;
            case LogicValue<float> floatVal:
                return Convert.ToInt32(floatVal);
            case LogicValue<bool> boolVal:
                return Convert.ToInt32(boolVal);
            case LogicValue<string> strVal:
                return Convert.ToInt32(strVal);
            default:
                throw new Exception("This should not be possible!");
        }
    }
}

public class DummyToString : LogicOperator<string> {
    static readonly IValue[][] ArgTypes = {
        new IValue[] {
            new LogicVariable<int>(),
            new LogicVariable<float>(),
            new LogicVariable<bool>(),
            new LogicVariable<string>(),
        },
    };

    public DummyToString(IValue[] values) : base(ArgTypes, values) { }

    protected override string InternalGet() {
        switch (Arguments[0]) {
            case LogicValue<int> intVal:
                return intVal.ToString();
            case LogicValue<float> floatVal:
                return floatVal.ToString();
            case LogicValue<bool> boolVal:
                return boolVal.ToString();
            case LogicValue<string> strVal:
                return strVal;
            default:
                throw new Exception("This should not be possible!");
        }
    }
}