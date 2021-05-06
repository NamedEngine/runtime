using UnityEngine;

namespace Language.Operators {
    public class DoesObjExist : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] { new Value<string>() },
            new IValue[] { new ClassRef() }
        };

        public DoesObjExist(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] arguments,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, arguments, constraintReference) { }

        protected override bool InternalGet() {
            return EngineAPI.GetObjectByName((Value<string>) Arguments[0], ((ClassRef) Arguments[1]).ClassName) == null;
        }
    }
}