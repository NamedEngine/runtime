using System.Collections;
using Language.Variables;
using UnityEngine;

namespace Language.Actions {
    public class MoveTo : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };
        
        public MoveTo(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            Arguments[0].TryTransferValueTo(VariableDict[nameof(CenterX)]);
            Arguments[1].TryTransferValueTo(VariableDict[nameof(CenterY)]);

            return null;
        }
    }
}
