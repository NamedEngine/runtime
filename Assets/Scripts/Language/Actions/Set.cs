using System.Collections;

namespace Language.Actions {
    public class SetInt : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Variable<int>()},
        };

        public SetInt(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<int>) Context.Arguments[1]).Set((Value<int>) Context.Arguments[0]);
            return null;
        }
    }
    
    public class SetFloat : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Variable<float>()},
        };

        public SetFloat(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<float>) Context.Arguments[1]).Set((Value<float>) Context.Arguments[0]);
            return null;
        }
    }
    
    public class SetBool : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()},
            new IValue[] {new Variable<bool>()},
        };

        public SetBool(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<bool>) Context.Arguments[1]).Set((Value<bool>) Context.Arguments[0]);
            return null;
        }
    }
    
    public class SetString : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new Variable<string>()},
        };

        public SetString(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<string>) Context.Arguments[1]).Set((Value<string>) Context.Arguments[0]);
            return null;
        }
    }
}
