using System;
using System.Linq;

public class LogicTypeConstraints {
    public readonly IValue[][] ArgTypes;
    public LogicTypeConstraints(IValue[][] argTypes) {
        ArgTypes = argTypes;
    }

    public void CheckArgs(IValue[] arguments, object caller) {
        var mandatoryArgs = ArgTypes
            .Select(types => types.Any(t => t.Cast(null)))
            .Count(nullable => nullable == false);

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
        if (arguments.Length < ArgTypes.Length) {
            var newValues = new IValue[ArgTypes.Length];
            for (int i = 0; i < arguments.Length; i++) {
                newValues[i] = arguments[i];
            }

            for (int i = arguments.Length; i < ArgTypes.Length; i++) {
                newValues[i] = null;
            }
        }

        for (int i = 0; i < arguments.Length; i++) {
            var typeMatch = ArgTypes[i]
                .Select(t => t.Cast(arguments[i]))
                .Any(b => b);
            if (typeMatch) {
                continue;
            }

            if (arguments[i] is null) {
                throw new ArgumentException("Argument error in " + caller.GetType() + " operator: argument #" + i + " is mandatory");
            }

            throw new ArgumentException("Argument type mismatch in " + caller.GetType() + " operator: argument #" + i + " has inappropriate type");
        }
    }
}