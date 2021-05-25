using System;
using System.Linq;

public class LogicTypeConstraints {
    public readonly IValue[][] ArgTypes;
    public LogicTypeConstraints(IValue[][] argTypes) {
        ArgTypes = argTypes;
    }

    public IValue[] CheckArgs(IValue[] arguments, object caller) {
        var mandatoryArgs = LogicUtils.MandatoryParamsNum(ArgTypes);

        if (ArgTypes.Length < arguments.Length || arguments.Length < mandatoryArgs) {
            var message = "Argument count mismatch in " + caller.GetType() + " operator: got " + arguments.Length + ", expected ";
            if (mandatoryArgs == ArgTypes.Length) {
                message += mandatoryArgs;
            } else {
                message += "from " + mandatoryArgs + " to " + ArgTypes.Length;
            }
            throw new ArgumentException(message);
        }

        // padding with nulls so we will always have fixed number of Values in descendants
        IValue[] newValues;
        if (arguments.Length < ArgTypes.Length) {
            newValues = new IValue[ArgTypes.Length];
            for (int i = 0; i < arguments.Length; i++) {
                newValues[i] = arguments[i];
            }

            for (int i = arguments.Length; i < ArgTypes.Length; i++) {
                newValues[i] = null;
            }
        } else {
            newValues = arguments;
        }

        for (int i = 0; i < newValues.Length; i++) {
            var typeMatch = ArgTypes[i]
                .Select(t => t.Cast(newValues[i]))
                .Any(b => b);
            if (typeMatch) {
                continue;
            }

            if (newValues[i] is null) {
                throw new ArgumentException("Argument error in " + caller.GetType() + " operator: argument #" + i + " is null");
            }

            throw new ArgumentException("Argument type mismatch in " + caller.GetType() + " operator: argument #" + i + " has inappropriate type");
        }

        return newValues.Select(v => v?.PrepareForCast()).ToArray();
    }
}