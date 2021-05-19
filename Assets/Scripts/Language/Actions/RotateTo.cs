using System.Collections;
using Language.Variables;

namespace Language.Actions {
    public class RotateTo : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
        };
        
        public RotateTo(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            ((Value<float>) Context.Arguments[0]).TryTransferValueTo(Context.Base.VariableDict[nameof(Rotation)]);
            
            return null;
        }
    }
}
