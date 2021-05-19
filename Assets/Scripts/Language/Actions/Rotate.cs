using System.Collections;
using Language.Variables;

namespace Language.Actions {
    public class Rotate : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
        };
        
        public Rotate(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var deltaRotation = ((Value<float>) Context.Arguments[0]).Get();
            var rotation = (Variable<float>) Context.Base.VariableDict[nameof(Rotation)];
            
            rotation.Set(rotation + deltaRotation);

            return null;
        }
    }
}
