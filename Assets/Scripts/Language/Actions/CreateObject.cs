using System.Collections;

namespace Language.Actions {
    public class CreateObject : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new ClassRef()},
        };

        public CreateObject(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var objName = ((Value<string>) Context.Arguments[0]).Get();
            var res = Context.Base.EngineAPI.CreateObject(objName, ((ClassRef) Context.Arguments[1]).ClassName);
            if (res == null) {
                throw new LogicException(nameof(CreateObject), $"Object with name \"{objName}\" already exists!");
            }

            return null;
        }
    }
}
