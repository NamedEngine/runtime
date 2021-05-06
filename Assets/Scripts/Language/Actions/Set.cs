using System.Collections;
using UnityEngine;

namespace Language.Actions {
    public class SetInt : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Variable<int>()},
        };

        public SetInt(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<int>) Arguments[1]).Set((Value<int>) Arguments[0]);
            return null;
        }
    }
    
    public class SetFloat : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Variable<float>()},
        };

        public SetFloat(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<float>) Arguments[1]).Set((Value<float>) Arguments[0]);
            return null;
        }
    }
    
    public class SetBool : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()},
            new IValue[] {new Variable<bool>()},
        };

        public SetBool(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<bool>) Arguments[1]).Set((Value<bool>) Arguments[0]);
            return null;
        }
    }
    
    public class SetString : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new Variable<string>()},
        };

        public SetString(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<string>) Arguments[1]).Set((Value<string>) Arguments[0]);
            return null;
        }
    }
}
