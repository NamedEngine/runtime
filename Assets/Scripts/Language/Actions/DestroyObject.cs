using System.Collections;

namespace Language.Actions {
    public class DestroyObject : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new ClassRef()},
        };

        public DestroyObject(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var objName = ((Value<string>) Context.Arguments[0]).Get();
            var className = ((ClassRef) Context.Arguments[1]).ClassName;
            var destroyed = Context.Base.EngineAPI.DestroyObject(objName, className);
            if (!destroyed) {
                throw new LogicException(nameof(DestroyObject), 
                    $"Could not find object with name \"{objName}\" and type \"{className}\"");
            }

            return null;
        }
    }
}
