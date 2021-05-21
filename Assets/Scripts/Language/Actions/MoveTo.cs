using System.Collections;
using Language.Variables;

namespace Language.Actions {
    public class MoveTo : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };
        
        public MoveTo(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            Context.Arguments[0].TryTransferValueTo(Context.Base.VariableDict[nameof(CenterX)]);
            Context.Arguments[1].TryTransferValueTo(Context.Base.VariableDict[nameof(CenterY)]);

            return null;
        }
    }
}
