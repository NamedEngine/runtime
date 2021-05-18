using UnityEngine;

namespace Language.Operators {
    public class DoesObjExist : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] { new Value<string>() },
            new IValue[] { new ClassRef() }
        };

        public DoesObjExist(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override bool InternalGet() {
            return EngineAPI.GetObjectByName((Value<string>) Arguments[0], ((ClassRef) Arguments[1]).ClassName) == null;
        }
    }
}
