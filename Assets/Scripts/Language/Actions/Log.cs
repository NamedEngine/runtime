using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Language.Actions {
    public class Log : Action {
        static readonly IValue[][] ArgTypes = new[] {
            new IValue[] {new Value<string>()},
        }.Concat(Enumerable.Repeat(new IValue[] {
            new Value<string>(),
            new Value<int>(),
            new Value<float>(),
            new Value<bool>(),
            new NullValue(),
        }, 100).ToArray());
        
        public Log(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        // ReSharper disable Unity.PerformanceAnalysis
        protected override IEnumerator ActionLogic() {
            string ValueToString(IValue val) {
                switch (val) {
                    case Value<int> intVal:
                        return intVal.ToString();
                    case Value<float> floatVal:
                        return floatVal.ToString();
                    case Value<bool> boolVal:
                        return boolVal.ToString();
                    case Value<string> strVal:
                        return strVal;
                    default:
                        throw new Exception("This should not be possible!");
                }
            }
            
            var format = (Value<string>) Context.Arguments[0];
            var other = Context.Arguments
                .Skip(1)
                .Where(arg => arg != null)
                .Select(ValueToString)
                .Select(param => (object) param)
                .ToArray();
            var message = string.Format(format, other);
            Debug.Log(message);

            return null;
        }
    }
}
