namespace Language.Operators {
    public class DoesObjExist : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] { new Value<string>() },
            new IValue[] { new ClassRef() }
        };

        public DoesObjExist(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return Context.Base.EngineAPI.GetObjectByName((Value<string>) Context.Arguments[0], ((ClassRef) Context.Arguments[1]).ClassName) == null;
        }
    }
}
